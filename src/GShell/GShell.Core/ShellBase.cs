using System;
using System.Text.RegularExpressions;
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

        public async Task<ShellExitCode> Run()
        {
            while (true)
            {
                Console.Write("> ");

                string line = await Console.In.ReadLineAsync();
                if (!string.IsNullOrEmpty(line))
                {
                    string str = line;

                    while (!IsCompleteSubmission(str))
                    {
                        Console.Write("* ");

                        line = await Console.In.ReadLineAsync();
                        str += Environment.NewLine + line;
                    }

                    var result = ProcessDirective(str);
                    if (result.HasValue)
                        return result.Value;

                    var (rawAssembly, scriptClassName) = mContext.Compile(str);
                    if (rawAssembly == null)
                        continue;

                    try
                    {
                        var success = await Process(rawAssembly, scriptClassName);
                        if (!success)
                            return ShellExitCode.ExecutionError;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to process: {0}", ex);
                        return ShellExitCode.ExecutionError;
                    }
                }
            }
        }

        protected abstract Task<bool> Process(byte[] rawAssembly, string scriptClassName);

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
