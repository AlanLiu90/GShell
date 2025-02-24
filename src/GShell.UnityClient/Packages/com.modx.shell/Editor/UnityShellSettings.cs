using System;
using UnityEditor;
using UnityEngine;

namespace GShell
{
    [Serializable]
    public class DllCompileSettings
    {
        public BuildTarget BuildTarget = BuildTarget.Android;
        public bool Development;
        public string OutputDir;
    }

    public enum SearchPathType
    {
        System,
        Unity,
        Custom,
    }

    [Serializable]
    public class DynamicDllCompileSettings
    {
        public string[] SearchPaths;
        public string[] References;
        public string[] Usings;
        public string ScriptClassName = "Script";
    }

    public enum Runtime
    {
        IL2CPP,
        Mono,
    }

    [CreateAssetMenu(fileName = "ShellSettings", menuName = "ScriptableObjects/ShellSettings")]
    public class UnityShellSettings : ScriptableObject
    {
        public DllCompileSettings DllCompileSettings = new DllCompileSettings();
        public DynamicDllCompileSettings DynamicDllCompileSettings = new DynamicDllCompileSettings();
        public Runtime Runtime;
        public string ToolPath;
        public string ExecuteURL;
        public ExtraDataItem[] ExtraDatas;
    }
}
