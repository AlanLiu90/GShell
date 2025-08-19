using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GShell.Core;

namespace GShell
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var logger = new Logger();

            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("No configuration");
                    return;
                }

                var path = args[0];
                var json = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<ShellSettings>(json);

                var targetFramework = GetTargetFramework(settings.TargetFramework);
                var searchPaths = settings.SearchPaths;
                var references = settings.References;
                var usings = settings.Usings;
                var scriptClassName = settings.ScriptClassName;
                var extraAssemblies = settings.ExtraAssemblies;
                var extraDataItems = settings.ExtraDataItems;
                var additionalAttributeType = GetAdditionalAttributeType(settings.Runtime);
                var authenticationData = GetAuthenticationData(settings.AuthenticationType, settings.AuthenticationData);

                PrintInfo(targetFramework, searchPaths, references, usings, extraAssemblies, extraDataItems);

                bool exit = false;
                while (!exit)
                {
                    var context = new ShellContext(
                        targetFramework,
                        searchPaths,
                        references,
                        usings,
                        scriptClassName,
                        additionalAttributeType,
                        logger
                    );

                    var shell = new GShell(
                        context,
                        settings.ExecuteURL,
                        extraAssemblies,
                        extraDataItems.ToDictionary(x => x.Key, x => x.Value),
                        authenticationData
                    );

                    var ret = await shell.Run();
                    switch (ret)
                    {
                        case ShellExitCode.Exit:
                            exit = true;
                            break;

                        case ShellExitCode.Reset:
                        case ShellExitCode.ExecutionError:
                        default:
                            // 重置状态
                            Console.WriteLine("Reset session");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
            }

            Console.WriteLine("Press any key to close this window . . .");
            Console.ReadKey();
        }

        private static void PrintInfo(
            TargetFramework targetFramework,
            string[] searchPaths,
            string[] references,
            string[] usings,
            string[] extraAssemblies,
            ExtraDataItem[] extraDataItems)
        {
            var sb = new StringBuilder();

            sb.AppendLine("TargetFramework:");
            sb.AppendFormat("  {0}", targetFramework);
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("SearchPath:");

            foreach (var item in searchPaths)
            {
                sb.AppendFormat("  {0}", item);
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("Reference:");

            foreach (var item in references)
            {
                sb.AppendFormat("  {0}", item);
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("Using:");

            foreach (var item in usings)
            {
                sb.AppendFormat("  {0}", item);
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("ExtraAssemblies:");

            foreach (var assembly in extraAssemblies)
            {
                sb.AppendFormat("  {0}", assembly);
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("ExtraDataItems:");

            foreach (var data in extraDataItems)
            {
                sb.AppendFormat("  {0}: {1}", data.Key, data.Value);
                sb.AppendLine();
            }

            sb.AppendLine();

            Console.WriteLine(sb.ToString());
        }

        private static TargetFramework GetTargetFramework(string targetFramework)
        {
            switch (targetFramework)
            {
                case "netstandard2.0":
                    return TargetFramework.NetStandard20;

                case "netstandard2.1":
                    return TargetFramework.NetStandard21;

                default:
                    throw new NotSupportedException($"No support for {targetFramework}");
            }
        }

        private static AdditionalAttributeType GetAdditionalAttributeType(string runtime)
        { 
            switch (runtime)
            {
                case "Mono":
                    return AdditionalAttributeType.SecurityPermission;

                default:
                    return AdditionalAttributeType.None;
            }
        }

        private static AuthenticationData GetAuthenticationData(string type, string data)
        {
            if (!Enum.TryParse<AuthenticationType>(type, out var authType))
                throw new NotSupportedException($"No support for {type}");
            
            AuthenticationData authData;

            switch (authType)
            {
                case AuthenticationType.Basic:
                case AuthenticationType.JWT:
                    authData = JsonSerializer.Deserialize<AuthenticationData>(data);
                    authData.Type = authType;
                    break;

                case AuthenticationType.None:
                default:
                    authData = null;
                    break;
            }

            return authData;
        }
    }
}
