using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace GShell.Core
{
    public abstract class ShellBase
    {
        protected readonly ShellContext mContext;

        private static readonly Regex mDirectivePattern = new Regex(@"\s*#(exit|quit|reset)\s*");

        public ShellBase(ShellContext context)
        {
            mContext = context;
        }

        public async Task<ShellExitCode> RunAsync(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                await WriteAsync("> ", cancellationToken);

                string line = await ReadLineAsync(cancellationToken);
                if (!string.IsNullOrEmpty(line))
                {
                    string str = line;

                    while (!IsCompleteSubmission(str))
                    {
                        await WriteAsync("* ");

                        line = await ReadLineAsync(cancellationToken);
                        str += Environment.NewLine + line;
                    }

                    var result = ProcessDirective(str);
                    if (result.HasValue)
                        return result.Value;

                    var (rawAssembly, scriptClassName, _) = mContext.Compile(str);
                    if (rawAssembly == null)
                        continue;

                    try
                    {
                        var success = await ProcessAsync(rawAssembly, scriptClassName!, cancellationToken);
                        if (!success)
                            return ShellExitCode.ExecutionError;
                    }
                    catch (Exception ex)
                    {
                        await WriteLineAsync($"Failed to process: {ex}", cancellationToken);
                        return ShellExitCode.ExecutionError;
                    }
                }
            }
        }

        protected abstract ValueTask<string> ReadLineAsync(CancellationToken cancellationToken = default);

        protected abstract ValueTask WriteAsync(string s, CancellationToken cancellationToken = default);

        protected virtual ValueTask WriteLineAsync(string s, CancellationToken cancellationToken = default)
        {
            return WriteAsync(s + Environment.NewLine, cancellationToken);
        }

        protected abstract Task<bool> ProcessAsync(byte[] rawAssembly, string scriptClassName, CancellationToken cancellationToken = default);

        private static bool IsCompleteSubmission(string input)
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(input, ShellContext.ParseOptions);
            return SyntaxFactory.IsCompleteSubmission(syntaxTree);
        }

        private static ShellExitCode? ProcessDirective(string input)
        {
            var match = mDirectivePattern.Match(input);
            if (match.Success)
            {
                switch (match.Groups[1].Value)
                {
                    case "exit":
                    case "quit":
                        return ShellExitCode.Exit;

                    case "reset":
                        return ShellExitCode.Reset;
                }
            }

            return null;
        }
    }
}
