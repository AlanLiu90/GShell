using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            mSettings = (UnityShellSettings)EditorGUILayout.ObjectField("配置", mSettings, typeof(UnityShellSettings), false);

            if (EditorGUI.EndChangeCheck())
                SavePrefs();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            GUI.enabled = mSettings != null && mSettings.DllCompileSettings.BuildTarget != BuildTarget.NoTarget;

            if (GUILayout.Button("编译dll"))
                CompileDlls();

            GUI.enabled = true;

            EditorGUILayout.Space();

            GUI.enabled = mSettings != null;

            if (GUILayout.Button("启动"))
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

        private void CompileDlls()
        {
            if (mSettings == null)
            {
                Debug.LogError("配置为空");
                return;
            }

            var settings = mSettings.DllCompileSettings;

            if (string.IsNullOrEmpty(settings.OutputDir))
            {
                Debug.LogError("dll的编译输出路径没有配置");
                return;
            }

            string outputDir = Path.Combine(settings.OutputDir, settings.BuildTarget.ToString());
            CompileDll(outputDir, settings.BuildTarget, settings.Development);
        }

        private static void CompileDll(string buildDir, BuildTarget target, bool developmentBuild)
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
            Debug.Log("编译完成");
        }

        private void Launch()
        {
            if (mSettings == null)
            {
                Debug.LogError("配置为空");
                return;
            }

            var searchPaths = new List<string>();

            var dllCompileSettings = mSettings.DllCompileSettings;

            if (dllCompileSettings.BuildTarget != BuildTarget.NoTarget)
            {
                if (string.IsNullOrEmpty(dllCompileSettings.OutputDir))
                {
                    Debug.LogError("dll的编译输出路径没有配置");
                    return;
                }

                string dllDir = Path.Combine(dllCompileSettings.OutputDir, dllCompileSettings.BuildTarget.ToString());
                if (!Directory.Exists(dllDir) || !Directory.EnumerateFiles(dllDir).Any())
                {
                    Debug.LogError("dll还未编译");
                    return;
                }

                searchPaths.Add(Path.GetFullPath(dllDir));
            }

            if (string.IsNullOrEmpty(mSettings.ToolPath))
            {
                Debug.LogError("ToolPath未配置");
                return;
            }

            if (string.IsNullOrEmpty(mSettings.ExecuteURL))
            {
                Debug.LogError("ExecuteURL未配置");
                return;
            }

            if (mSettings.DynamicDllCompileSettings.SearchPaths != null)
            {
                foreach (var searchPath in mSettings.DynamicDllCompileSettings.SearchPaths)
                {
                    var dir = searchPath;

                    if (!Directory.Exists(dir))
                    {
                        Debug.LogError("目录不存在: " + dir);
                        return;
                    }

                    searchPaths.Add(Path.GetFullPath(dir));
                }
            }

            searchPaths.Add(Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEngine"));
            searchPaths = searchPaths.Select(x => x.Replace("\\", "/")).ToList();

            if (mSettings.ExtraDatas != null)
            {
                foreach (var data in mSettings.ExtraDatas)
                {
                    if (string.IsNullOrEmpty(data.Key))
                    {
                        Debug.Log("ExtraData配置的键值为空");
                        return;
                    }
                }
            }

            // 保存配置表
            var settings = new ShellSettings();

#if UNITY_2021_1_OR_NEWER
            settings.TargetFramework = "netstandard2.1";
#else
            settings.TargetFramework = "netstandard2.0";
#endif

            settings.SearchPaths = searchPaths.ToArray();
            settings.References = mSettings.DynamicDllCompileSettings.References ?? Array.Empty<string>();
            settings.Usings = mSettings.DynamicDllCompileSettings.Usings ?? Array.Empty<string>();
            settings.ScriptClassName = mSettings.DynamicDllCompileSettings.ScriptClassName;
            settings.Runtime = mSettings.Runtime.ToString();
            settings.ExecuteURL = mSettings.ExecuteURL;
            settings.ExtraData = mSettings.ExtraDatas ?? Array.Empty<ExtraDataItem>();
            settings.AuthenticationType = mSettings.AuthenticationSettings.Type.ToString();
            settings.AuthenticationData = mSettings.AuthenticationSettings.Data;

            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(TempSettingPath, json);

            // 启动 GShell
            string toolPath = mSettings.ToolPath;
            string settingPath = TempSettingPath;

            toolPath = Path.Combine(Directory.GetCurrentDirectory(), toolPath);

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                toolPath = toolPath.Replace("/", "\\");
                settingPath = settingPath.Replace("/", "\\");
            }

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = false,
                FileName = toolPath,
                Arguments = settingPath,
                WorkingDirectory = Directory.GetCurrentDirectory(),
            };

            Process.Start(startInfo);
        }

#if false
        private string GetRealPath(SearchPath searchPath)
        {
            switch (searchPath.Type)
            {
                case SearchPathType.System:
                    var path = Path.Combine(EditorApplication.applicationContentsPath, "MonoBleedingEdge/lib/mono/unityaot");

#if UNITY_2022_1_OR_NEWER
                    switch (Application.platform)
                    {
                        case RuntimePlatform.WindowsEditor:
                            path += "-win32";
                            break;

                        case RuntimePlatform.OSXEditor:
                            path += "macos";
                            break;

                        case RuntimePlatform.LinuxEditor:
                            path += "linux";
                            break;
                    }
#endif

                    return path;

                case SearchPathType.Unity:
                    return Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEngine");

                default:

                    return searchPath.Path;
            }
        }
#endif
    }
}
