using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OffensivePipeline.Modules
{
    internal class RandomAssemblyInfo : iModule
    {
        public string Name => "RandomAssemblyInfo";
        public ToolConfig _tool { get; set; }
        public ModuleOutput _moduleOutput { get; set; }

        private string _regexAssemblyTitle = @"\[assembly: AssemblyTitle\(.*\)]";
        private string _regexAssemblyDescription = @"\[assembly: AssemblyDescription\(.*\)]";
        private string _regexAssemblyConfiguration = @"\[assembly: AssemblyConfiguration\(.*\)]";
        private string _regexAssemblyCompany = @"\[assembly: AssemblyCompany\(.*\)]";
        private string _regexAssemblyProduct = @"\[assembly: AssemblyProduct\(.*\)]";
        private string _regexAssemblyCopyright = @"\[assembly: AssemblyCopyright\(.*\)]";
        private string _regexAssemblyTrademark = @"\[assembly: AssemblyTrademark\(.*\)]";
        private string _regexAssemblyCulture = @"\[assembly: AssemblyCulture\(.*\)]";

        public RandomAssemblyInfo(ToolConfig tool, ModuleOutput moduleOutput)
        {
            _tool = tool;
            _moduleOutput = moduleOutput;
        }
        public ModuleOutput CheckStart()
        {
            return _moduleOutput;
        }

        private List<string> FindFiles()
        {
            List<string> lFiles = new List<string>();
            foreach (
                string file in Directory.EnumerateFiles(
                    Path.Combine(Directory.GetCurrentDirectory(), Conf.gitToolsPath, _tool.name), "AssemblyInfo.cs", SearchOption.AllDirectories))
            {
                lFiles.Add(file);
            }
            return lFiles;
        }

        public ModuleOutput Run()
        {
            List<string> lFiles = FindFiles();
            string message;
            foreach (string file in lFiles)
            {
                if (File.Exists(file))
                {
                    message = $"\tReplacing strings in {file}";
                    LogHelpers.PrintBlue(message);
                    LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                    string fContent = File.ReadAllText(file);
                    fContent = File.ReadAllText(file);
                    string randomName = Helpers.GetRandomString();
                    foreach (Match match in Regex.Matches(fContent, _regexAssemblyTitle))
                    {
                        string replacement = $"[assembly: AssemblyTitle(\"{randomName}\")]";
                        message = $"\t\t{match.Value} -> {replacement}";
                        LogHelpers.PrintGray(message);
                        LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                        fContent = fContent.Replace(match.Value, replacement);
                    }
                    foreach (Match match in Regex.Matches(fContent, _regexAssemblyDescription))
                    {
                        string replacement = $"[assembly: AssemblyDescription(\"\")]";
                        message = $"\t\t{match.Value} -> {replacement}";
                        LogHelpers.PrintGray(message);
                        LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                        fContent = fContent.Replace(match.Value, replacement);
                    }
                    foreach (Match match in Regex.Matches(fContent, _regexAssemblyConfiguration))
                    {
                        string replacement = $"[assembly: AssemblyConfiguration(\"\")]";
                        message = $"\t\t{match.Value} -> {replacement}";
                        LogHelpers.PrintGray(message);
                        LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                        fContent = fContent.Replace(match.Value, replacement);
                    }
                    foreach (Match match in Regex.Matches(fContent, _regexAssemblyCompany))
                    {
                        string replacement = $"[assembly: AssemblyCompany(\"\")]";
                        message = $"\t\t{match.Value} -> {replacement}";
                        LogHelpers.PrintGray(message);
                        LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                        fContent = fContent.Replace(match.Value, replacement);
                    }
                    foreach (Match match in Regex.Matches(fContent, _regexAssemblyProduct))
                    {
                        string replacement = $"[assembly: AssemblyProduct(\"{randomName}\")]";
                        message = $"\t\t{match.Value} -> {replacement}";
                        LogHelpers.PrintGray(message);
                        LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                        fContent = fContent.Replace(match.Value, replacement);
                    }
                    foreach (Match match in Regex.Matches(fContent, _regexAssemblyCopyright))
                    {
                        var random = new Random();

                        string replacement = $"[assembly: AssemblyCopyright(\"Copyright ©  {RandomNumberGenerator.GetInt32(2018, 2022)}\")]";
                        message = $"\t\t{match.Value} -> {replacement}";
                        LogHelpers.PrintGray(message);
                        LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                        fContent = fContent.Replace(match.Value, replacement);
                    }
                    foreach (Match match in Regex.Matches(fContent, _regexAssemblyTrademark))
                    {
                        string replacement = $"[assembly: AssemblyTrademark(\"\")]";
                        message = $"\t\t{match.Value} -> {replacement}";
                        LogHelpers.PrintGray(message);
                        LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                        fContent = fContent.Replace(match.Value, replacement);
                    }
                    foreach (Match match in Regex.Matches(fContent, _regexAssemblyCulture))
                    {
                        string replacement = $"[assembly: AssemblyCulture(\"\")]";
                        message = $"\t\t{match.Value} -> {replacement}";
                        LogHelpers.PrintGray(message);
                        LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                        fContent = fContent.Replace(match.Value, replacement);
                    }
                    File.WriteAllText(file, fContent);
                }
            }
           
            return _moduleOutput;
        }
    }
}
