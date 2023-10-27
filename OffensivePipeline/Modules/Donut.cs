using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Donut;
using Donut.Structs;

namespace OffensivePipeline.Modules
{
    internal class Donut : iModule
    {
        public string Name => "Donut";
        public ToolConfig _tool { get; set; }
        public ModuleOutput _moduleOutput { get; set; }
        
        private string previousFolder;
        
        public Donut(ToolConfig tool, ModuleOutput moduleOutput)
        {
            _tool = tool;
            _moduleOutput = moduleOutput;
            previousFolder = _moduleOutput.OutputPath;
            _moduleOutput.OutputPath = Path.Combine(_moduleOutput.OutputPath, Name);
            Helpers.CheckFolder(_moduleOutput.OutputPath);
        }

        public ModuleOutput CheckStart()
        {
            return _moduleOutput;
        }

        public ModuleOutput Run()
        {
            string message;
            string[] exeList = Directory.GetFiles(previousFolder, "*.exe");
            foreach (string exe in exeList)
            {
                try
                {
                    message = $"\tGenerating shellcode...";
                    LogHelpers.PrintBlue(message);
                    LogHelpers.LogToFile($"{Name} - Donut", "INFO", message);
                    DonutConfig config = new DonutConfig();
                    config.Arch = 3; //Target architecture for loader : 1=x86, 2=amd64, 3=x86+amd64(default).
                    config.Bypass = 3; //Behavior for bypassing AMSI/WLDP : 1=None, 2=Abort on fail, 3=Continue on fail.(default)
                    config.InputFile = exe;
                    config.Payload = Path.Combine(_moduleOutput.OutputPath, $"{_tool.name}.bin");
                    if (_tool.toolArguments != "") {
                        LogHelpers.PrintOk($"\t - Arguments passed to shellcode : \"{_tool.toolArguments}\"");
                        config.Args = _tool.toolArguments;
                    }
                    int ret = Generator.Donut_Create(ref config);
                    message = "\t\t[+] No errors!";
                    LogHelpers.PrintOk(message);
                    LogHelpers.LogToFile($"{Name} - Donut", "INFO", message);
                    //Console.WriteLine(Helper.GetError(ret));
                }
                catch (Exception e)
                {
                    message = $"Donut: {exe}";
                    LogHelpers.PrintError(message);
                    LogHelpers.LogToFile($"{Name} - Donut", "ERROR", message);
                    _moduleOutput.Status = false;
                }
            }

            return _moduleOutput;
        }
    }
}
