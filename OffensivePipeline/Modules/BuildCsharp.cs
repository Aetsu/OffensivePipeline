using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Build.Construction;
using System.Configuration;

namespace OffensivePipeline.Modules
{
    public class BuildCsharp : iModule
    {
        private string buildOptions = ConfigurationManager.AppSettings["BuildCsharpOptions"];
        public string Name => "BuildCsharp";
        public ToolConfig _tool { get; set; }
        public ModuleOutput _moduleOutput { get; set; }


        public BuildCsharp(ToolConfig tool, ModuleOutput moduleOutput)
        {
            _tool = tool;
            _moduleOutput = moduleOutput;
        }

        ModuleOutput iModule.CheckStart()
        {
            string message;
            LogHelpers.PrintBlue("\t[+] Checking requirements...");
            if (!File.Exists(Conf.nugetPath))
            {
                message = $"\t[*] Downloading nuget.exe from {ConfigurationManager.AppSettings["NugetUrl"]}";
                LogHelpers.PrintYellow(message);
                LogHelpers.LogToFile($"{Name} - CheckStart", "INFO", message);
                //Download nuget.exe
                _moduleOutput.Status = Helpers.DownloadResources(ConfigurationManager.AppSettings["NugetUrl"], "nuget.exe", Conf.resourcesPath);
                if (_moduleOutput.Status)
                {
                    message = "\t\t[+] Download OK - nuget.exe";
                    LogHelpers.PrintOk(message);
                    LogHelpers.LogToFile($"{Name} - CheckStart", "INFO", message);
                }
                else
                {
                    message = "\t\t[+] Download ERROR - nuget.exe";
                    LogHelpers.PrintError(message);
                    LogHelpers.LogToFile($"{Name} - CheckStart", "ERROR", message);
                    _moduleOutput.Status = false;
                    return _moduleOutput;
                }
            }
            else
            {
                message = "\t\t[+] Download OK - nuget.exe";
                LogHelpers.PrintOk(message);
                LogHelpers.LogToFile($"{Name} - CheckStart", "INFO", message);
            }
            if (!File.Exists(ConfigurationManager.AppSettings["BuildCSharpTools"]))
            {
                message = $"\t\t[-] File not found: {ConfigurationManager.AppSettings["BuildCSharpTools"]}";
                LogHelpers.PrintError(message);
                LogHelpers.LogToFile($"{Name} - CheckStart", "ERROR", message);
                _moduleOutput.Status = false;
            }
            else
            {
                message = $"\t\t[+] Path found - {ConfigurationManager.AppSettings["BuildCSharpTools"]}";
                LogHelpers.PrintOk(message);
                LogHelpers.LogToFile($"{Name} - CheckStart", "INFO", message);
            }
            return _moduleOutput;
        }

        ModuleOutput iModule.Run()
        {
            string message;
            //_moduleOutput.OutputPath = Path.Combine(Directory.GetCurrentDirectory(), Conf.outputsPath, $"{_tool.name}_{Helpers.GetRandomString()}");
            string text = File.ReadAllText(Conf.templateBuildPath);

            string solutionPath = Path.Combine(Directory.GetCurrentDirectory(), Conf.gitToolsPath, _tool.solutionPath);
            if (File.Exists(solutionPath))
            {
                message = $"\tSolving dependences with nuget...";
                LogHelpers.PrintBlue(message);
                LogHelpers.LogToFile($"{Name} - BuildTool", "INFO", message);
                Console.ResetColor();
                if (!Helpers.ExecuteCommand($"{Path.Combine(Directory.GetCurrentDirectory(), Conf.nugetPath)} restore {solutionPath}"))
                {
                    message = $"BuildTool: {Path.Combine(Directory.GetCurrentDirectory(), Conf.nugetPath)} - {solutionPath}";
                    LogHelpers.PrintError(message);
                    LogHelpers.LogToFile($"{Name} - BuildTool", "ERROR", message);
                    _moduleOutput.Status = false;
                    return _moduleOutput;
                }
                text = text.Replace("{{MSBUILD_PATH}}", ConfigurationManager.AppSettings["BuildCSharpTools"]);
                text = text.Replace("{{SOLUTION_PATH}}", solutionPath);
                text = text.Replace("{{BUILD_OPTIONS}}", buildOptions);
                text = text.Replace("{{OUTPUT_DIR}}", _moduleOutput.OutputPath);
                text = text.Replace("{{OUTPUT_FILENAME}}", Helpers.GetRandomString());

                string batPath = Path.Combine(Path.GetDirectoryName(solutionPath), "buildSolution.bat");
                File.WriteAllText(batPath, text);
                LogHelpers.PrintBlue("\tBuilding solution...");
                if (!Helpers.ExecuteCommand(batPath))
                {
                    message = $"BuildTool: msbuild.exe: {solutionPath}";
                    LogHelpers.PrintError(message);
                    LogHelpers.LogToFile($"{Name} - BuildTool", "ERROR", message);
                    _moduleOutput.Status = false;
                    return _moduleOutput;
                }
                else
                {
                    message = "\t\t[+] No errors!";
                    LogHelpers.PrintOk(message);
                    LogHelpers.LogToFile($"{Name} - BuildTool", "INFO", message);
                }

                //Gets all references to the project to obfuscate it with confuser
                SolutionFile s = SolutionFile.Parse(solutionPath);
                foreach (ProjectInSolution p in s.ProjectsInOrder)
                {
                    if (File.Exists(p.AbsolutePath))
                    {
                        XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
                        XDocument projDefinition = XDocument.Load(p.AbsolutePath);
                        IEnumerable<string> references = projDefinition
                            .Element(msbuild + "Project")
                            .Elements(msbuild + "ItemGroup")
                            .Elements(msbuild + "Reference")
                            .Elements(msbuild + "HintPath")
                            .Select(refElem => refElem.Value);
                        foreach (string reference in references)
                        {
                            string referenceFile = reference.Replace(@"..\", "");
                            if (File.Exists(Path.Combine(Path.GetDirectoryName(solutionPath), referenceFile)))
                            {
                                File.Copy(
                                    Path.Combine(Path.GetDirectoryName(solutionPath), referenceFile),
                                    Path.Combine(_moduleOutput.OutputPath, Path.GetFileName(referenceFile)),
                                    true);
                            }
                        }
                    }
                }
            }
            message = $"\t\t[+] Output folder: {_moduleOutput.OutputPath}";
            LogHelpers.PrintOk(message);
            LogHelpers.LogToFile($"{Name} - BuildTool", "INFO", message);
            return _moduleOutput;
        }
    }
}
