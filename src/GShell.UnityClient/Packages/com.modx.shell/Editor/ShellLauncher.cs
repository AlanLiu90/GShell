using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GShell
{
    public class ShellLauncher : EditorWindow
    {
        private const string TempSettingPath = "Library/ShellSettings.json";
        private const string PrefsKey = "GShellSettingGUID";

        private UnityShellSettings mSettings;

        [MenuItem("MODX/Shell Launcher")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShellLauncher>();
            window.titleContent = new GUIContent("Shell Launcher");
        }

        private void OnEnable()
        {
            if (EditorPrefs.HasKey(PrefsKey))
            {
                string guid = EditorPrefs.GetString(PrefsKey);
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                    mSettings = AssetDatabase.LoadAssetAtPath<UnityShellSettings>(path);
            }
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            mSettings = (UnityShellSettings)EditorGUILayout.ObjectField("Configuration", mSettings, typeof(UnityShellSettings), false);

            if (EditorGUI.EndChangeCheck())
                SavePrefs();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            GUI.enabled = mSettings != null && mSettings.AssemblyCompilationSettings.BuildTarget != BuildTarget.NoTarget;

            if (GUILayout.Button("Compile Scripts"))
                CompileScripts();

            GUI.enabled = true;

            EditorGUILayout.Space();

            GUI.enabled = mSettings != null;

            if (GUILayout.Button("Launch"))
                Launch();

            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        private void SavePrefs()
        {
            if (mSettings != null)
            {
                var guid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(mSettings));
                EditorPrefs.SetString(PrefsKey, guid.ToString());
            }
        }

        private void CompileScripts()
        {
            if (mSettings == null)
            {
                Debug.LogError("Configuration is empty");
                return;
            }

            var settings = mSettings.AssemblyCompilationSettings;

            if (string.IsNullOrEmpty(settings.OutputDir))
            {
                Debug.LogError("'OutputDir' in DllCompileSettings is empty");
                return;
            }

            if (!CanBuildPlayer(settings.BuildTarget))
                Debug.LogWarning("The build target is not supported. If the build fails please install the corresponding Editor module.");

            string outputDir = Path.Combine(settings.OutputDir, settings.BuildTarget.ToString());
            CompileScripts(outputDir, settings.BuildTarget, settings.Development);
        }

        private static void CompileScripts(string buildDir, BuildTarget target, bool developmentBuild)
        {
            var group = BuildPipeline.GetBuildTargetGroup(target);

            ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
            scriptCompilationSettings.group = group;
            scriptCompilationSettings.target = target;
            scriptCompilationSettings.options = developmentBuild ? ScriptCompilationOptions.DevelopmentBuild : ScriptCompilationOptions.None;
            Directory.CreateDirectory(buildDir);
            ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, buildDir);
#if UNITY_2022
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
            Debug.Log("Compilation finished");

            if ((scriptCompilationResult.assemblies == null || scriptCompilationResult.assemblies.Count == 0) && 
                scriptCompilationResult.typeDB == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to compile scripts.", "OK");
                return;
            }
        }

        private void Launch()
        {
            if (mSettings == null)
            {
                Debug.LogError("Configuration is empty");
                return;
            }

            var searchPaths = new List<string>();

            var dllCompileSettings = mSettings.AssemblyCompilationSettings;

            if (dllCompileSettings.BuildTarget != BuildTarget.NoTarget)
            {
                if (string.IsNullOrEmpty(dllCompileSettings.OutputDir))
                {
                    Debug.LogError("'OutputDir' in DllCompileSettings is empty");
                    return;
                }

                string dllDir = Path.Combine(dllCompileSettings.OutputDir, dllCompileSettings.BuildTarget.ToString());
                if (!Directory.Exists(dllDir) || !Directory.EnumerateFiles(dllDir).Any())
                {
                    Debug.LogError("The DLLs have not been compiled yet");
                    return;
                }

                searchPaths.Add(Path.GetFullPath(dllDir));
            }

            if (string.IsNullOrEmpty(mSettings.Command))
            {
                Debug.LogError("ToolPath is empty");
                return;
            }

            if (string.IsNullOrEmpty(mSettings.ExecuteURL))
            {
                Debug.LogError("ExecuteURL is empty");
                return;
            }

            if (mSettings.DynamicCodeCompileSettings.SearchPaths != null)
            {
                foreach (var searchPath in mSettings.DynamicCodeCompileSettings.SearchPaths)
                {
                    var dir = searchPath;

                    if (!Directory.Exists(dir))
                    {
                        Debug.LogError("The directory doesn't exist: " + dir);
                        return;
                    }

                    searchPaths.Add(Path.GetFullPath(dir));
                }
            }

            searchPaths.Add(Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEngine"));
            searchPaths = searchPaths.Select(x => x.Replace("\\", "/")).ToList();

            if (mSettings.ExtraDataItems != null)
            {
                foreach (var data in mSettings.ExtraDataItems)
                {
                    if (string.IsNullOrEmpty(data.Key))
                    {
                        Debug.LogError("Key is empty in ExtraData");
                        return;
                    }
                }
            }

            if (!ParseCommand(mSettings.Command, out var fileName, out var arguments))
            {
                Debug.LogError("Failed to parse Command");
                return;
            }

            // Save settings
            var settings = new ShellSettings();

#if UNITY_2021_1_OR_NEWER
            settings.TargetFramework = "netstandard2.1";
#else
            settings.TargetFramework = "netstandard2.0";
#endif

            settings.SearchPaths = searchPaths.ToArray();
            settings.References = mSettings.DynamicCodeCompileSettings.References ?? Array.Empty<string>();
            settings.Usings = mSettings.DynamicCodeCompileSettings.Usings ?? Array.Empty<string>();
            settings.ScriptClassName = mSettings.DynamicCodeCompileSettings.ScriptClassName;
            settings.Runtime = mSettings.Runtime.ToString();
            settings.ExecuteURL = mSettings.ExecuteURL;
            settings.ExtraAssemblies = mSettings.ExtraAssemblies ?? Array.Empty<string>();
            settings.ExtraDataItems = mSettings.ExtraDataItems ?? Array.Empty<ExtraDataItem>();
            settings.AuthenticationType = mSettings.AuthenticationSettings.Type.ToString();
            settings.AuthenticationData = mSettings.AuthenticationSettings.Data;

            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(TempSettingPath, json);

            // Start GShell
            string settingPath = TempSettingPath;

            Debug.LogFormat("Start process: {0} {1} {2}", fileName, arguments, settingPath);

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = false,
                FileName = fileName,
                Arguments = $"{arguments} {settingPath}",
                WorkingDirectory = Directory.GetCurrentDirectory(),
            };

            Process.Start(startInfo);
        }

        private static bool ParseCommand(string cmd, out string fileName, out string arguments)
        {
            fileName = null;
            arguments = null;

            cmd = cmd?.Trim();
            if (string.IsNullOrEmpty(cmd))
                return false;

            if (cmd.StartsWith("\"", StringComparison.Ordinal))
            {
                int index = cmd.IndexOf('"', 1);
                if (index < 0)
                    return false;

                fileName = cmd.Substring(1, index - 1);
                arguments = cmd.Substring(index + 1);
            }
            else
            {
                int index = cmd.IndexOf(' ', 0);
                if (index >= 0)
                {
                    fileName = cmd.Substring(0, index);
                    arguments = cmd.Substring(index + 1);
                }
                else
                {
                    fileName = cmd;
                    arguments = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(fileName))
                return false;

            var dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir))
                fileName = Path.GetFullPath(fileName);

            return true;
        }

        private static bool CanBuildPlayer(BuildTarget target)
        {
#if UNITY_2021_3_OR_NEWER
            var group = BuildPipeline.GetBuildTargetGroup(target);
            return CanBuildPlayer(target, group, GetBuildWindowExtension(target, group));
#else
            return true;
#endif
        }

#if UNITY_2021_3_OR_NEWER
        private static object GetBuildWindowExtension(BuildTarget target, BuildTargetGroup targetGroup)
        {
            var moduleManagerType = typeof(EditorUserBuildSettings).Assembly.GetType("UnityEditor.Modules.ModuleManager");

            var methods = moduleManagerType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.Name == "GetTargetStringFrom");

#if UNITY_2023_3_OR_NEWER
            var module = methods.Where(x => x.GetParameters().Length == 1).First().Invoke(null, new object[] { target });
#else
            var module = methods.Where(x => x.GetParameters().Length == 2).First().Invoke(null, new object[] { targetGroup, target });
#endif

            var getBuildWindowExtension = moduleManagerType.GetMethod("GetBuildWindowExtension", BindingFlags.Static | BindingFlags.NonPublic);
            return getBuildWindowExtension.Invoke(null, new object[] { module });
        }

        private static bool CanBuildPlayer(BuildTarget target, BuildTargetGroup targetGroup, object buildWindowExtension)
        {
            if (!BuildPipeline.IsBuildTargetSupported(targetGroup, target))
                return false;

            if (buildWindowExtension == null)
                return false;

            var enabledBuildButton = buildWindowExtension.GetType().GetMethod("EnabledBuildButton", BindingFlags.Instance | BindingFlags.Public);
            return (bool)enabledBuildButton.Invoke(buildWindowExtension, null);
        }
#endif
    }
}
