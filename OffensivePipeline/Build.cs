using LibGit2Sharp;
using System;
using System.IO;
using Microsoft.Build.Construction;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace OffensivePipeline
{
    static class Build
    {
        public static string DownloadRepository(string toolName, string url)
        {
            string path = "GitTools";
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            string toolPath = Path.Combine(new string[] { path, toolName });
            if (!Directory.Exists(toolPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("     Clonnig the repository: {0}", toolName);
                Console.ResetColor();
                Repository.Clone(url, toolPath);
                using (var repo = new Repository(toolPath))
                {
                    var commit = repo.Head.Tip;
                    Console.WriteLine(@"        Last commit: {0}", commit.Author.When.ToLocalTime());
                }
            }
            return toolPath;
        }
        public static string BuildTool(string solutionPath, string toolName)
        {
            string finalPath = string.Empty;
            string text = System.IO.File.ReadAllText(Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Resources", "template_build.bat" }));
            string buildOptions = "/p:Platform=\"Any CPU\"";
            solutionPath = Path.Combine(new string[] { Directory.GetCurrentDirectory(), "GitTools", solutionPath });
            string outputDir = Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Output" });
            if (File.Exists(solutionPath))
            {

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("     Solving dependences with nuget...");
                Console.ResetColor();
                if (Helpers.ExecuteCommand(@"Resources\nuget.exe restore " + solutionPath) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("         Error -> nuget.exe: {0}", solutionPath);
                    Console.ResetColor();
                }
                finalPath = Path.Combine(new string[] { outputDir, toolName + "_" + Helpers.GetRandomString() });

                text = text.Replace("{{SOLUTION_PATH}}", solutionPath);
                text = text.Replace("{{BUILD_OPTIONS}}", buildOptions);
                text = text.Replace("{{OUTPUT_DIR}}", finalPath);
                string batPath = Path.Combine(new string[] { Path.GetDirectoryName(solutionPath), "buildSolution.bat" });
                File.WriteAllText(batPath, text);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("     Building solution...");
                Console.ResetColor();
                if (Helpers.ExecuteCommand(batPath) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("         Error -> msbuild.exe: {0}", solutionPath);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("         No errors!");
                    Console.ResetColor();
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
                            if (File.Exists(Path.Combine(new string[] { Path.GetDirectoryName(solutionPath), referenceFile })))
                            {
                                File.Copy(
                                    Path.Combine(new string[] { Path.GetDirectoryName(solutionPath), referenceFile }),
                                    Path.Combine(new string[] { finalPath, Path.GetFileName(referenceFile) }),
                                    true);
                            }
                        }
                    }

                }
            }
            return finalPath;
        }

        public static void Confuse(string folder)
        {
            string[] exeList = Directory.GetFiles(folder, "*.exe");
            foreach (string exe in exeList)
            {
                string text = File.ReadAllText(Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Resources", "template_confuserEx.crproj" }));
                text = text.Replace("{{BASE_DIR}}", folder);
                text = text.Replace("{{OUTPUT_DIR}}", Path.Combine(new string[] { folder, "Confused" }));
                text = text.Replace("{{EXE_FILE}}", exe);
                string crprojPath = Path.Combine(new string[] { folder, exe + ".crproj" });
                System.IO.File.WriteAllText(crprojPath, text);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("     Confusing " + exe + "...");
                Console.ResetColor();
                if (Helpers.ExecuteCommand(
                    Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Resources", "ConfuserEx", "Confuser.CLI.exe" }) + " " + crprojPath) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("         Error -> ConfuserEx: {0}", exe);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("         No errors!");
                    Console.ResetColor();
                }
            }
        }
    }
}
