using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace OffensivePipeline
{
    class Program
    {
        static void AnalyzeTools()
        {
            string[] toolList = Directory.GetFiles("Tools", "*.yml");
            foreach (string tool in toolList)
            {
                string outputFolder = string.Empty;
                string toolPath = string.Empty;
                string text = File.ReadAllText(tool);
                var stringReader = new StringReader(text);
                var yaml = new YamlStream();
                yaml.Load(stringReader);
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                foreach (var entry in mapping.Children)
                {
                    var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("tool")];
                    foreach (YamlMappingNode item in items)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n[+] Name: {0}", item.Children[new YamlScalarNode("name")]);
                        Console.ResetColor();
                        Console.WriteLine("     - Description: {0}\n     - Git link: {1}\n     - Solution file: {2}\n",
                            item.Children[new YamlScalarNode("description")],
                            item.Children[new YamlScalarNode("gitLink")],
                            item.Children[new YamlScalarNode("solutionPath")]);

                        try
                        {
                            toolPath = Build.DownloadRepository(item.Children[new YamlScalarNode("name")].ToString()
                            , item.Children[new YamlScalarNode("gitLink")].ToString());
                            outputFolder = Build.BuildTool(
                                item.Children[new YamlScalarNode("solutionPath")].ToString(),
                                item.Children[new YamlScalarNode("name")].ToString());
                            if (Helpers.ExecuteCommand("RMDIR \"" + toolPath + "\" /S /Q") != 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error -> RMDIR: {0}", toolPath);
                                Console.ResetColor();
                            }
                            Build.Confuse(outputFolder);
                            Helpers.CalculateMD5Files(outputFolder);
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("     Output folder: {0}", outputFolder);
                            Console.ResetColor();
                        } 
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error analyzing: <{0}> -> {1}", item.Children[new YamlScalarNode("name")], ex.ToString());
                        }
                    }
                }
            }
        }

        static void AnalyzeTools(string toolName)
        {
            string outputFolder = string.Empty;
            string toolPath = string.Empty;
            if (!File.Exists(@"Tools\" + toolName + ".yml"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("- Error: {0} tool not supported", toolName);
                Console.ResetColor();
                return;
            }
            string text = File.ReadAllText(@"Tools\" + toolName + ".yml");
            var stringReader = new StringReader(text);
            var yaml = new YamlStream();
            yaml.Load(stringReader);
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var entry in mapping.Children)
            {
                var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("tool")];
                foreach (YamlMappingNode item in items)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n[+] Name: {0}", item.Children[new YamlScalarNode("name")]);
                    Console.ResetColor();
                    Console.WriteLine("     - Description: {0}\n     - Git link: {1}\n     - Solution file: {2}\n",
                        item.Children[new YamlScalarNode("description")],
                        item.Children[new YamlScalarNode("gitLink")],
                        item.Children[new YamlScalarNode("solutionPath")]);

                    toolPath = Build.DownloadRepository(item.Children[new YamlScalarNode("name")].ToString()
                        , item.Children[new YamlScalarNode("gitLink")].ToString());
                    outputFolder = Build.BuildTool(
                            item.Children[new YamlScalarNode("solutionPath")].ToString(),
                            item.Children[new YamlScalarNode("name")].ToString());
                    //if (Helpers.ExecuteCommand("RMDIR \"" + toolPath + "\" /S /Q") != 0)
                    //{
                    //    Console.ForegroundColor = ConsoleColor.Red;
                    //    Console.WriteLine("Error -> RMDIR: {0}", toolPath);
                    //    Console.ResetColor();
                    //}
                    Build.Confuse(outputFolder);
                    Helpers.CalculateMD5Files(outputFolder);
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("     Output folder: {0}", outputFolder);
                    Console.ResetColor();
                }
            }

        }
        static void CheckStart()
        {
            int error = 0;
            if (!File.Exists(Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Resources", "nuget.exe" })))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("   [*] Downloading nuget.exe...");
                Console.ResetColor();
                //Download nuget.exe
                error = Helpers.DownloadResources(@"https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", "nuget.exe", "Resources");
                if (error != 0)
                {
                    System.Environment.Exit(1);
                }
            }
            if (!Directory.Exists(Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Resources", "ConfuserEx" })))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("   [*] Downloading ConfuserEx...");
                Console.ResetColor();
                //Download ConfuserEx
                error = Helpers.DownloadResources(@"https://github.com/mkaring/ConfuserEx/releases/download/v1.4.1/ConfuserEx-CLI.zip", "ConfuserEx.zip", "Resources");
                if (error != 0)
                {
                    System.Environment.Exit(1);
                }
                error = Helpers.UnzipFile(
                    Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Resources", "ConfuserEx.zip" }),
                    Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Resources", "ConfuserEx" }));
                if (error != 0)
                {
                    System.Environment.Exit(1);
                }
                try
                {
                    File.Delete(Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Resources", "ConfuserEx.zip" }));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error deleting <" + Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Resources", "ConfuserEx.zip" }) + "> - " + ex.ToString());
                }
            }
        }

        static void ListTools()
        {
            string[] toolList = Directory.GetFiles("Tools", "*.yml");
            foreach (string tool in toolList)
            {
                string text = File.ReadAllText(tool);
                var stringReader = new StringReader(text);
                var yaml = new YamlStream();
                yaml.Load(stringReader);
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                foreach (var entry in mapping.Children)
                {
                    var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("tool")];
                    foreach (YamlMappingNode item in items)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n   [+] Name: {0}", item.Children[new YamlScalarNode("name")]);
                        Console.ResetColor();
                        Console.WriteLine("     - Description: {0}\n     - Git: {1}",
                            item.Children[new YamlScalarNode("description")],
                            item.Children[new YamlScalarNode("gitLink")]);
                    }
                }
            }
            Console.WriteLine();
        }

        static void Main(string[] args)
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
            ";
            Console.WriteLine(banner);

            var app = new CommandLineApplication();
            app.Name = "OffensivePipeline";
            app.HelpOption("-?|-h|--help");

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            app.Command("list", (command) =>
            {
                command.Description = "List all supported tools";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    ListTools();
                    return 0;
                });
            });

            app.Command("all", (command) =>
            {
                command.Description = "Build and obfuscate all tools";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    CheckStart();
                    AnalyzeTools();
                    return 0;
                });
            });

            app.Command("t", (command) =>
            {
                command.Description = "Build and obfuscate the specified tool";
                command.HelpOption("-?|-h|--help");
                var toolArgument = command.Argument("[tool]", "Tool to build.");
                command.OnExecute(() =>
                {
                    if (toolArgument.Value != null)
                    {
                        CheckStart();
                        AnalyzeTools(toolArgument.Value);
                    }

                    return 0;
                });
            });
            app.Execute(args);
        }
    }
}
