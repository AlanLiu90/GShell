namespace GShell.Core
{
    public interface IShellContext
    {
        string SessionId { get; }
        int SubmissionId { get; }
        (byte[]? RawAssembly, string? ScriptName, bool HasErrors) Compile(string code);
    }
}
