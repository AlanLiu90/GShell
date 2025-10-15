using System.Diagnostics;
using System.IO;
using UnityEditor;

public class HTTPServer
{
    [MenuItem("Demo/Start HTTP Server")]
    private static void Run()
    {
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            CreateNoWindow = false,
            FileName = "dotnet",
            Arguments = $"run --project ../HttpServer/HttpServer.csproj",
            WorkingDirectory = Directory.GetCurrentDirectory(),
        };

        Process.Start(startInfo);
    }
}
