using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GShell.Core;
using Newtonsoft.Json;

namespace GShell
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("No configuration");
                    return;
                }

                var path = args[0];
                var json = File.ReadAllText(path);
                var settings = JsonConvert.DeserializeObject<ShellSettings>(json);

                var searchPaths = settings.SearchPaths;
                var references = settings.References;
                var usings = settings.Usings;
                var scriptClassName = settings.ScriptClassName;
                var extraData = settings.ExtraData;
                var additionalAttributeType = GetAdditionalAttributeType(settings.Runtime);

                PrintInfo(searchPaths, references, usings, extraData);

                bool exit = false;
                while (!exit)
                {
                    var context = new ShellContext(searchPaths, references, usings, scriptClassName, additionalAttributeType);
                    var shell = new GShell(context, settings.ExecuteURL, extraData.ToDictionary(x => x.Key, x => x.Value));

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
                Console.WriteLine(ex);
            }

            Console.WriteLine("Press any key to close this window . . .");
            Console.ReadKey();
        }

        private static void PrintInfo(IEnumerable<string> searchPaths,
            IEnumerable<string> references,
            IEnumerable<string> usings,
            ExtraDataItem[] extraDatas)
        {
            var sb = new StringBuilder();

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
            sb.AppendLine("ExtraData:");

            foreach (var data in extraDatas)
            {
                sb.AppendFormat("  {0}: {1}", data.Key, data.Value);
                sb.AppendLine();
            }

            sb.AppendLine();

            Console.WriteLine(sb.ToString());
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
    }
}
