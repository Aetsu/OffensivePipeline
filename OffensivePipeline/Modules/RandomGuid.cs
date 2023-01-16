using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OffensivePipeline.Modules
{
    internal class RandomGuid : iModule
    {
        public string Name => "RandomGuid";
        public ToolConfig _tool { get; set; }
        public ModuleOutput _moduleOutput { get; set; }

        private string _regexStr = @"[(]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[)]?";
        public RandomGuid(ToolConfig tool, ModuleOutput moduleOutput)
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
            lFiles.Add(Path.Combine(Directory.GetCurrentDirectory(), Conf.gitToolsPath, _tool.solutionPath));
            foreach (
                string file in Directory.EnumerateFiles(
                    Path.Combine(Directory.GetCurrentDirectory(), Conf.gitToolsPath, _tool.name), "*.csproj", SearchOption.AllDirectories))
            {
                lFiles.Add(file);
            }
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
            Regex rx = new Regex(_regexStr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            string message;
            List<string> lGuid = new List<string>();
            Hashtable hTable = new Hashtable();
            string fContent;
            message = $"\tSearching GUIDs...";
            LogHelpers.PrintBlue(message);
            LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
            foreach (string file in lFiles) //first get all GUIDs
            {
                message = $"\t\t> {file}";
                LogHelpers.PrintGray(message);
                LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                if (File.Exists(file))
                {
                    fContent = File.ReadAllText(file);
                    MatchCollection matches = rx.Matches(fContent);
                    
                    //get all guid
                    foreach (Match match in matches)
                    {
                        lGuid.Add(match.Value);
                    }
                }
            }
            //replace all GUID
            message = $"\tReplacing GUIDs...";
            LogHelpers.PrintBlue(message);
            LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
            foreach (string file in lFiles) 
            {
                //remove duplicates and generate new guid
                foreach (string guid in lGuid.Distinct())
                {
                    try
                    {
                        hTable.Add(guid, Guid.NewGuid());
                    }
                    catch { }
                }
                //replace guid
                fContent = File.ReadAllText(file);
                message = $"\t\tFile {file}:";
                LogHelpers.PrintGray(message);
                LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                foreach (DictionaryEntry de in hTable)
                {
                    message = $"\t\t\t> Replacing GUID {de.Key.ToString()} with {de.Value.ToString()}";
                    LogHelpers.PrintGray(message);
                    LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                    fContent = fContent.Replace(de.Key.ToString(), de.Value.ToString());
                }
                File.WriteAllText(file, fContent);
                if (File.Exists(file))
                {
                    message = "\t\t[+] No errors!";
                    LogHelpers.PrintOk(message);
                    LogHelpers.LogToFile($"{Name} - Run", "INFO", message);
                }
                else
                {
                    _moduleOutput.Status = false;
                    message = $"\t\t[+] File not found {file}";
                    LogHelpers.PrintError(message);
                    LogHelpers.LogToFile($"{Name} - Run", "ERROR", message);
                }

            }
            return _moduleOutput;
        }
    }
}
