using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OffensivePipeline
{
    public static class Conf
    {
        public static string gitToolsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Git");
        public static string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
        public static string modulesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules");
        public static string ymlsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Tools");
        public static string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"log.txt");
        public static string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Resources");
        //BuildPath
        public static string nugetPath = Path.Combine(resourcesPath, "nuget.exe");
        public static string templateBuildPath = Path.Combine(resourcesPath, "template_build.bat");
        //ConfuserEx
        public static string confuserExFolder = Path.Combine(resourcesPath, "ConfuserEx");
        public static string confuserExFile = Path.Combine(confuserExFolder, "Confuser.CLI.exe");
        public static string confuserTemplateFile = Path.Combine(resourcesPath, "template_confuserEx.crproj.template");
    }
}
