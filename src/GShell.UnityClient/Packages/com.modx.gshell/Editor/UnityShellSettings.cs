using System;
using UnityEditor;
using UnityEngine;

namespace GShell
{
    [Serializable]
    public class AssemblyCompilationSettings
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
    public class DynamicCodeCompileSettings
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

    public enum AuthenticationType
    {
        None,
        Basic,
        JWT,
    }

    [Serializable]
    public class AuthenticationSettings
    {
        public AuthenticationType Type;
        public string Data;
    }

    [CreateAssetMenu(fileName = "ShellSettings", menuName = "ScriptableObjects/ShellSettings")]
    public class UnityShellSettings : ScriptableObject
    {
        public AssemblyCompilationSettings AssemblyCompilationSettings = new AssemblyCompilationSettings();
        public DynamicCodeCompileSettings DynamicCodeCompileSettings = new DynamicCodeCompileSettings();
        public Runtime Runtime;
        public string Command;
        public string ExecuteURL;
        public string[] ExtraAssemblies;
        public ExtraDataItem[] ExtraDataItems;
        public AuthenticationSettings AuthenticationSettings = new AuthenticationSettings();
    }
}
