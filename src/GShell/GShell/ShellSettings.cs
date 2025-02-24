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
        public string[] SearchPaths;
        public string[] References;
        public string[] Usings = new string[0];
        public string ScriptClassName;
        public string Runtime;
        public string ExecuteURL;
        public ExtraDataItem[] ExtraData;
    }
}
