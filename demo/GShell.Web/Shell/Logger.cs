using System;
using System.Threading.Channels;
using GShell.Core;
using Microsoft.CodeAnalysis;

namespace GShell.Web.Shell
{
    internal sealed class Logger : ILogger
    {
        private readonly Channel<string> mOutputChannel;

        private static readonly string mRedFormat = "\x1b[31m{0}\x1b[0m";
        private static readonly string mYellowFormat = "\x1b[33m{0}\x1b[0m";

        public Logger(Channel<string> outputChannel)
        {
            mOutputChannel = outputChannel;
        }

        public void LogDiagnostic(Diagnostic diagnostic)
        {
            string format = diagnostic.Severity switch
            {
                DiagnosticSeverity.Error => mRedFormat,
                DiagnosticSeverity.Warning => mYellowFormat,
                _ => "{0}",
            };

            WriteLog(diagnostic.ToString(), format);
        }

        public void LogError(Exception exception)
        {
            WriteLog(exception.ToString(), mRedFormat);
        }

        private void WriteLog(string str, string colorFormat)
        {
            mOutputChannel.Writer.WriteAsync(string.Format(colorFormat, str) + Environment.NewLine);
        }
    }
}
