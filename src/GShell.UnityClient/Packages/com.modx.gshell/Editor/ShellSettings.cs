using System;

namespace GShell
{
    [Serializable]
    public struct ExtraDataItem
    {
        public string Key;
        public string Value;
    }

    public class ShellSettings
    {
        public string TargetFramework;
        public string[] AssemblySearchPaths;
        public string[] References;
        public string[] Usings = new string[0];
        public string[] SourceFileSearchPaths;
        public string ScriptClassName;
        public string Runtime;
        public string ExecuteURL;
        public string[] ExtraAssemblies;
        public ExtraDataItem[] ExtraDataItems;
        public string AuthenticationType;
        public string AuthenticationData;
    }
}
