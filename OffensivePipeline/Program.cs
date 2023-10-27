using McMaster.Extensions.CommandLineUtils;
using System.Reflection;
using OffensivePipeline.Modules;
using System.Configuration;

namespace OffensivePipeline
{
    public class Summary
    {
        public ToolConfig tool;
        public List<ModuleOutput> lModuleOutput;
        public bool status;
    }

    internal class Program
        {
        static void listTools()
        {
            List<ToolConfig> lTools = YmlHelpers.ReadYmls();
            int index = 1;
            foreach (var tool in lTools)
            {
                LogHelpers.PrintYellow($"[{index}/{lTools.Count}] {tool.name} - <{tool.language}>:");
                Console.WriteLine($"\t> Description: {tool.description}");
                Console.WriteLine($"\t> Link: {tool.gitLink}");
                LogHelpers.PrintBlue($"\t> Plugins: {string.Join(", ", tool.plugins)}");
                index++;
            }
        }

        static void cleanTools()
        {
            string message = "\t[+] Cleaning...";
            LogHelpers.PrintBlue(message);
            LogHelpers.LogToFile("cleanTools", "INFO", message);
            if (Directory.Exists(Conf.gitToolsPath))
            {
                LogHelpers.PrintGray($"\t\t> Folder {Conf.gitToolsPath}");
                Helpers.DeleteReadOnlyDirectory(Conf.gitToolsPath);
            }
            if (Directory.Exists(Conf.outputPath))
            {
                LogHelpers.PrintGray($"\t\t> Folder {Conf.outputPath}");
                Helpers.DeleteReadOnlyDirectory(Conf.outputPath);
            }
            if (File.Exists(Conf.logFile))
            {
                LogHelpers.PrintGray($"\t\t> Log file {Conf.logFile}");
                File.Delete(Conf.logFile);
            }
            Directory.CreateDirectory(Conf.gitToolsPath);
            Directory.CreateDirectory(Conf.outputPath);
        }

        public static void LaunchPipeline(string toolName=null, string toolArguments=null)
        {
            List<ToolConfig> lTools = new List<ToolConfig>();
            if (toolName != null)
            {
                lTools = YmlHelpers.ReadYmls(toolName, toolArguments);
            }
            else
            {
                lTools = YmlHelpers.ReadYmls();
            }
            List<Summary> lSummary = new List<Summary>();
            foreach (var tool in lTools)
            {
                Summary summary = LoadTool(tool);
                if (!summary.status)
                {
                    string message = $"Error processing {tool.name}";
                    LogHelpers.PrintError(message);
                    LogHelpers.LogToFile("LaunchPipeline", "ERROR", message);
                }
                lSummary.Add(summary);
            }

            ShowSummary(lSummary);
        }
        private static object LoadModule(string namespaceName, ToolConfig tool, ModuleOutput moduleOutput)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var type = assembly.GetTypes()
            .First(t => t.FullName == $"OffensivePipeline.Modules.{namespaceName}");
            if (type != null)
            {
                return Activator.CreateInstance(type, tool, moduleOutput);
            }
            else
            {
                return null;
            }
        }

        private static void ShowSummary(List<Summary> lSummary)
        {
            Console.WriteLine("\n\n-----------------------------------------------------------------");
            LogHelpers.PrintMagenta("\t\tSUMMARY\n");
            foreach(Summary s in lSummary)
            {
                Console.WriteLine($" - {s.tool.name}");
                foreach(ModuleOutput m in s.lModuleOutput)
                {
                    if (m.Status)
                    {
                        LogHelpers.PrintOk($"\t - {m.Name}: OK");
                    }
                    else
                    {
                        LogHelpers.PrintError($"\t - {m.Name}: ERROR");
                    }
                }
            }
            Console.WriteLine("\n-----------------------------------------------------------------");
        }

        private static Summary LoadTool(ToolConfig tool)
        {
            Summary summary = new Summary();
            summary.lModuleOutput = new List<ModuleOutput>();
            summary.tool = tool;
            summary.status = true;
            ModuleOutput moduleOutput = new ModuleOutput();
            LogHelpers.PrintMagenta($"\n[+] Loading tool: {tool.name}");
            Uri uriResult;
            bool isUrl = Uri.TryCreate(tool.gitLink, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUrl) //is remote
            {
                moduleOutput.Status = GitHelpers.DownloadRepository(tool);
            }
            else //is local
            {
                moduleOutput.Status = GitHelpers.CloneLocalTool(tool);
            }
            if (moduleOutput.Status)
            {
                Helpers.CheckFolder(Conf.outputPath);
                string outputDir = Path.Combine(Directory.GetCurrentDirectory(), Conf.outputPath);
                moduleOutput.OutputPath = Path.Combine(outputDir, $"{tool.name}_{Helpers.GetRandomString()}");
                string outputAux = moduleOutput.OutputPath;
                foreach (string module in tool.plugins)
                {
                    moduleOutput.Name = module;
                    if (moduleOutput.Status)
                    {
                        try
                        {
                            iModule instancedModule  = (iModule)LoadModule(module, tool, moduleOutput);
                            Console.WriteLine($"\n    [+] Load {module} module");
                            moduleOutput = instancedModule.CheckStart();
                            if (moduleOutput.Status)
                            {
                                moduleOutput = instancedModule.Run();
                            }
                        }
                        catch (Exception e)
                        {
                            string message = $"LoadTool - Error in module: {module} - {e}";
                            LogHelpers.PrintError(message);
                            LogHelpers.LogToFile("Main - LoadTool", "ERROR", message);
                        }
                    }
                    Console.WriteLine();
                    ModuleOutput moduleOutputAux = new ModuleOutput();
                    moduleOutputAux.Name = moduleOutput.Name;
                    moduleOutputAux.Status = moduleOutput.Status;
                    moduleOutputAux.OutputPath = moduleOutput.OutputPath;
                    summary.status = moduleOutputAux.Status;
                    summary.lModuleOutput.Add(moduleOutputAux);
                }
                if (Directory.Exists(outputAux))
                {
                    Console.WriteLine($"\n    [+] Generating Sha256 hashes");
                    Helpers.CalculateSha256Files(outputAux);
                    string str = $"\t\tOutput file: {outputAux}";
                    LogHelpers.PrintGray(str);
                    LogHelpers.LogToFile($"Main - LoadTool", "INFO", str);
                }
            }
            return summary;
        }

        public static void ShowBanner()
        {
            string banner = @"
                                                                                                   ooo
                                                                                           .osooooM M
      ___   __  __                _           ____  _            _ _                      +y.     M M
     / _ \ / _|/ _| ___ _ __  ___(_)_   _____|  _ \(_)_ __   ___| (_)_ __   ___           :h  .yoooMoM
    | | | | |_| |_ / _ \ '_ \/ __| \ \ / / _ \ |_) | | '_ \ / _ \ | | '_ \ / _ \          oo  oo
    | |_| |  _|  _|  __/ | | \__ \ |\ V /  __/  __/| | |_) |  __/ | | | | |  __/          oo  oo
     \___/|_| |_|  \___|_| |_|___/_| \_/ \___|_|   |_| .__/ \___|_|_|_| |_|\___|          oo  oo
                                                     |_|                            MoMoooy.  h:
                                                                                    M M     .y+
                                                                                    M Mooooso.
                                                                                    ooo
   
                                                                    @aetsu
            " + $"\t\t\t\t\t\t\t\t\tv{ConfigurationManager.AppSettings["Version"]}\n";
            Console.WriteLine(banner);
        }


        static void Main(string[] args)
        {
            ShowBanner();
            var app = new CommandLineApplication();
            app.Name = "OffensivePipeline";
            app.HelpOption("-?|-h|--help");
            app.ExtendedHelpText = @"
Examples:
 - List all tools:
    OffensivePipeline.exe list
 - Load seatbelt tool:
    OffensivePipeline.exe t seatbelt [-a/--args] [args]
 - Load all tools:
    OffensivePipeline.exe all
";

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            app.Command("list", (command) =>
            {
                command.Description = "List all tools";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    listTools();
                    Console.WriteLine();
                    return 0;
                });
            });

            app.Command("all", (command) =>
            {
                command.Description = "Load all tools";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    LaunchPipeline();
                    Console.WriteLine();
                    return 0;
                });
            });

            app.Command("t", (command) =>
            {
                command.Description = "Load the specified tool";
                command.HelpOption("-?|-h|--help");
                var toolArgument = command.Argument("[tool]", "Tool to build.");
                var toolArguments = command.Option("-a|--args", "Command-line arguments to pass to the Donut shellcode, will override the yaml value", CommandOptionType.SingleValue);
                
                command.OnExecute(() =>
                {
                    if (toolArgument.Value != null)
                    {
                        LaunchPipeline(toolArgument.Value, toolArguments.Value());
                    }
                    Console.WriteLine();
                    return 0;
                });
            });
            app.Command("clean", (command) =>
            {
                command.Description = "Clean all tools";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    cleanTools();
                    Console.WriteLine();
                    return 0;
                });
            });
            app.Execute(args);
        }
    }
}