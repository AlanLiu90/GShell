using System;
using GShell.Core;
using Microsoft.CodeAnalysis;

namespace GShell
{
    internal sealed class Logger : ILogger
    {
        public void LogDiagnostic(Diagnostic diagnostic)
        {
            ConsoleColor color = diagnostic.Severity switch
            {
                DiagnosticSeverity.Error => ConsoleColor.Red,
                DiagnosticSeverity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.White,
            };

            WriteLog(diagnostic.ToString(), color);
        }

        public void LogError(Exception exception)
        {
            WriteLog(exception.ToString(), ConsoleColor.Red);
        }

        private void WriteLog(string str, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ForegroundColor = oldColor;
        }
    }
}
