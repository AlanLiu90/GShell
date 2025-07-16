using System;
using Microsoft.CodeAnalysis;

namespace GShell.Core
{
    public interface ILogger
    {
        void LogDiagnostic(Diagnostic diagnostic);
        void LogError(Exception exception);
    }
}
