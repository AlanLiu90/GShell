using System;

namespace GShell.Web.Shell
{
    [Serializable]
    internal struct ExtraDataItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    internal class ShellSettings
    {
        public required string TargetFramework { get; set; }
        public required string[] SearchPaths { get; set; }
        public required string[] References { get; set; }
        public required string[] Usings { get; set; }
        public required string ScriptClassName { get; set; }
        public required string Runtime { get; set; }
        public required string ExecuteURL { get; set; }
        public required string[] ExtraAssemblies { get; set; }
        public required ExtraDataItem[] ExtraDataItems { get; set; }
    }
}
