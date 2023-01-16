using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OffensivePipeline.Modules
{
    internal class ConfuserEx : iModule
    {
        private string template = @"<project outputDir=""{{OUTPUT_DIR}}"" baseDir=""{{BASE_DIR}}"" xmlns=""http://confuser.codeplex.com"">
        <packer id=""compressor"" />
        <module path=""{{EXE_FILE}}"" />
        {{DLL_LIST}}
        </project>";

        private string previousFolder;


        public string Name => "ConfuserEx";
        public ToolConfig _tool { get; set; }
        public ModuleOutput _moduleOutput { get; set; }


        public ConfuserEx(ToolConfig tool, ModuleOutput moduleOutput)
        {
            _tool = tool;
            _moduleOutput = moduleOutput;
            previousFolder = _moduleOutput.OutputPath;
            _moduleOutput.OutputPath = Path.Combine(_moduleOutput.OutputPath, Name);
            Helpers.CheckFolder(_moduleOutput.OutputPath);
        }


        ModuleOutput iModule.CheckStart()
        {
            string confuserExZip = "ConfuserEx.zip";
            string message;
            LogHelpers.PrintBlue("\t[+] Checking requirements...");
            if (!File.Exists(Conf.confuserExFile))
            {
                _moduleOutput.Status = Helpers.DownloadResources(ConfigurationManager.AppSettings["ConfuserExUrl"], confuserExZip, Conf.resourcesPath);
                message = $"\t[+] Downloading ConfuserEx from {ConfigurationManager.AppSettings["ConfuserExUrl"]}";
                LogHelpers.PrintYellow(message);
                LogHelpers.LogToFile($"{Name} - CheckStart", "INFO", message);
                if (_moduleOutput.Status)
                {
                    message = "\t\t[+] Download OK - ConfuserEx";
                    LogHelpers.PrintOk(message);
                    LogHelpers.LogToFile($"{Name} - CheckStart", "INFO", message);
                    _moduleOutput.Status = Helpers.UnzipFile(
                        Path.Combine(Conf.resourcesPath, confuserExZip),
                        Conf.confuserExFolder);
                    if (_moduleOutput.Status)
                    {
                        File.Delete(Path.Combine(Conf.resourcesPath, confuserExZip));
                    }
                    else
                    {
                        _moduleOutput.Status = false;
                        message = $"\t\t[+] Error deleting ConfuserEx.zip";
                        LogHelpers.PrintError(message);
                        LogHelpers.LogToFile($"{Name} - CheckStart", "ERROR", message);
                    }
                    if (!File.Exists(Conf.confuserExFile))
                    {
                        _moduleOutput.Status = false;
                        message = $"\t\t[+] File not found {Conf.confuserExFile}";
                        LogHelpers.PrintError(message);
                        LogHelpers.LogToFile($"{Name} - CheckStart", "ERROR", message);
                    }
                }
                else
                {
                    _moduleOutput.Status = false;
                    message = "\t\t[+] Download ERROR - {";
                    LogHelpers.PrintError(message);
                    LogHelpers.LogToFile($"{Name} - CheckStart", "ERROR", message);
                }

            }
            return _moduleOutput;
        }

        private void Confuse()
        {
            SolveDependences();
            string message;
            string[] exeList = Directory.GetFiles(previousFolder, "*.exe");
            foreach (string exe in exeList)
            {
                string text = File.ReadAllText(Conf.confuserTemplateFile);
                text = text.Replace("{{BASE_DIR}}", previousFolder);
                text = text.Replace("{{OUTPUT_DIR}}", _moduleOutput.OutputPath);
                text = text.Replace("{{EXE_FILE}}", exe);
                string crprojPath = Path.Combine(_moduleOutput.OutputPath, exe + ".crproj");
                File.WriteAllText(crprojPath, text);
                message = $"\tConfusing...";
                LogHelpers.PrintBlue(message);
                LogHelpers.LogToFile($"{Name} - Confuse", "INFO", message);
                if (!Helpers.ExecuteCommand($"{Conf.confuserExFile} {crprojPath}"))
                {
                    message = $"Confusing: {exe}";
                    LogHelpers.PrintError(message);
                    LogHelpers.LogToFile($"{Name} - Confuse", "ERROR", message);
                    _moduleOutput.Status = false;
                    return;
                }
                else
                {
                    message = "\t\t[+] No errors!";
                    LogHelpers.PrintOk(message);
                    LogHelpers.LogToFile($"{Name} - BuildTool", "INFO", message);
                }
            }
            string[] dllList = Directory.GetFiles(previousFolder, "*.dll");
            foreach (string dll in dllList)
            {
                File.Copy(dll, Path.Combine(_moduleOutput.OutputPath, Path.GetFileName(dll)), true);
            }
            return;
        }

        private void SolveDependences()
        {
            try
            {
                SolutionFile s = SolutionFile.Parse(Path.Combine(Conf.gitToolsPath, _tool.solutionPath));
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
                            if (File.Exists(Path.Combine(
                                Path.GetDirectoryName(_tool.solutionPath),
                                referenceFile)))
                            {
                                File.Copy(
                                    Path.Combine(Path.GetDirectoryName(_tool.solutionPath), referenceFile),
                                    Path.Combine(_moduleOutput.OutputPath, Path.GetFileName(referenceFile)),
                                    true);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string message = $"SolveDependences - - {e}";
                LogHelpers.PrintError(message);
                LogHelpers.LogToFile("ConfuserEx - SolveDependences", "ERROR", message);
            }

        }
        ModuleOutput iModule.Run()
        {
            Confuse();
            return _moduleOutput;
        }
    }
}
