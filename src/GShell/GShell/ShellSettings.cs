using System;

namespace GShell
{
    [Serializable]
    public struct ExtraDataItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ShellSettings
    {
        public string TargetFramework { get; set; }
        public string[] SearchPaths { get; set; }
        public string[] References { get; set; }
        public string[] Usings { get; set; }
        public string ScriptClassName { get; set; }
        public string Runtime { get; set; }
        public string ExecuteURL { get; set; }
        public ExtraDataItem[] ExtraData { get; set; }
        public string AuthenticationType { get; set; }
        public string AuthenticationData { get; set; }
    }
}
