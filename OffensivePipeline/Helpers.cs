using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace OffensivePipeline
{
    static class Helpers
    {
        public static int ExecuteCommand(string command)
        {
            int exitCode = 0;
            try
            {
                ProcessStartInfo processInfo;
                Process process;
                processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;
                process = Process.Start(processInfo);
                //process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                exitCode = process.ExitCode;
                if (!String.IsNullOrEmpty(output))
                {
                    LogToFile("ExecuteCommand", "INFO", output);
                }
                if (!String.IsNullOrEmpty(error))
                {
                    LogToFile("ExecuteCommand", "ERROR", error);
                }
                //Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
                //Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
                //Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
                process.Close();
            } catch (Exception ex)
            {
                Console.WriteLine("         Error -> Executing command <" + command + "> - " + ex.ToString());
                LogToFile("ExecuteCommand", "ERROR", command + "-> " + ex.ToString());
                return 1;
            }
            
            return exitCode;
        }

        public static string GetRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path;
        }

        public static int DownloadResources(string url, string outputName, string outputPath)
        {
            WebClient client = new WebClient();
            try
            {

                client.DownloadFile(url, Path.Combine(new string[] { Directory.GetCurrentDirectory(), outputPath, outputName }));
            }
            catch (Exception ex)
            {
                Console.WriteLine("         Error -> Downloading <" + url + "> - " + ex.ToString());
                LogToFile("DownloadResources", "ERROR", url + "-> " + ex.ToString());
                return 1;
            }
            return 0;
        }

        public static int UnzipFile(string filePath, string outputFolder)
        {
            try
            {
                ZipFile.ExtractToDirectory(filePath, outputFolder);

            }
            catch (Exception ex)
            {
                Console.WriteLine("         Error -> Unzipping <" + filePath + "> - " + ex.ToString());
                LogToFile("UnzipFile", "ERROR", filePath + "-> " + ex.ToString());
                return 1;
            }
            return 0;
        }


        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static void CalculateMD5Files(string folder)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("     Calculating md5...");
            Console.ResetColor();
            string[] fileList = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
            List<string> md5List = new List<string>();
            foreach (string filename in fileList)
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        md5List.Add(filename + " - " + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
                    }
                }
            }
            File.WriteAllLines(Path.Combine(new string[] { folder, "md5.txt" }), md5List);
        }

        public static void LogToFile(string source, string logType, string messsage)
        {
            string FilePath = "log.txt";
            DateTime localDate = DateTime.Now;
            using var fileStream = new FileStream(FilePath, FileMode.Append);
            using var writter = new StreamWriter(fileStream);
            writter.WriteLine("{0} | {1} | {2} -- {3}", localDate.ToString(), logType, source, messsage);
        }

    }
}
