using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace OffensivePipeline
{
    internal class Helpers
    {
        public static string GetRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path;
        }

        public static bool DownloadResources(string url, string outputName, string outputPath)
        {
            bool status = true;
            WebClient client = new WebClient();
            try
            {
                string f = Path.Combine(Directory.GetCurrentDirectory(), outputPath, outputName);
                client.DownloadFile(url, f);
                if (!File.Exists(f))
                {
                    LogHelpers.PrintError($"DownloadResources: File not found <{f}>");
                    LogHelpers.LogToFile("DownloadResources", "ERROR", $"File not found <{f}>");
                    status = false;
                }
            }
            catch (Exception ex)
            {
                LogHelpers.PrintError($"DownloadResources: <{url}> - {ex}");
                LogHelpers.LogToFile("DownloadResources", "ERROR", $"{url} - {ex}");
                status = false;
            }
            return status;
        }

        public static bool UnzipFile(string filePath, string outputFolder)
        {
            bool status = true;
            try
            {
                ZipFile.ExtractToDirectory(filePath, outputFolder);

            }
            catch (Exception ex)
            {
                string message = $"UnzipFile: <{filePath}> - {ex}";
                LogHelpers.PrintError(message);
                LogHelpers.LogToFile("UnzipFile", "ERROR", message);
                status = false;
            }
            return status;
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

        public static void CalculateSha256Files(string folder)
        {
            string[] fileList = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
            List<string> sha256l = new List<string>();
            foreach (string filename in fileList)
            {
                using (var sha256 = SHA256.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var hash = sha256.ComputeHash(stream);
                        sha256l.Add(filename + " - " + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
                    }
                }
            }
            File.WriteAllLines(Path.Combine(new string[] { folder, "sha256.txt" }), sha256l);
        }

        /// <summary>
        /// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, they are flagged as normal and then deleted.
        /// </summary>
        /// <param name="directory">The name of the directory to remove.</param>
        public static void DeleteReadOnlyDirectory(string directory)
        {
            foreach (var subdirectory in Directory.EnumerateDirectories(directory))
            {
                DeleteReadOnlyDirectory(subdirectory);
            }
            foreach (var fileName in Directory.EnumerateFiles(directory))
            {
                var fileInfo = new FileInfo(fileName);
                fileInfo.Attributes = FileAttributes.Normal;
                fileInfo.Delete();
            }
            Directory.Delete(directory);
        }

        public static bool ExecuteCommand(string command)
        {
            bool status = true;
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
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                int exitCode = process.ExitCode;
                if (!String.IsNullOrEmpty(output))
                {
                    LogHelpers.LogToFile("ExecuteCommand", "INFO", output);
                }
                if (!String.IsNullOrEmpty(error))
                {
                    LogHelpers.LogToFile("ExecuteCommand", "ERROR", error);
                }
                process.Close();
            }
            catch (Exception ex)
            {
                LogHelpers.PrintError($"ExecuteCommand: <{ command}> - {ex.ToString()}");
                LogHelpers.LogToFile("ExecuteCommand", "ERROR", $"{command} - {ex.ToString()}");
                status = false;
            }

            return status;
        }

        public static bool CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            bool status = true;
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
            {
                LogHelpers.PrintError($"CopyDirectory: Source directory not found: {dir.FullName}");
                LogHelpers.LogToFile("CopyDirectory", "ERROR", $"Source directory not found: {dir.FullName}");
                return false;
            }
            try
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                Directory.CreateDirectory(destinationDir);

                foreach (FileInfo file in dir.GetFiles())
                {
                    string targetFilePath = Path.Combine(destinationDir, file.Name);
                    file.CopyTo(targetFilePath);
                }
                if (recursive)
                {
                    foreach (DirectoryInfo subDir in dirs)
                    {
                        string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                        CopyDirectory(subDir.FullName, newDestinationDir, true);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelpers.PrintError($"CopyDirectory: {e}");
                LogHelpers.LogToFile("CopyDirectory", "ERROR", $"{e}");

            }

            return status;
        }

        public static void CheckFolder(string folderPath)
        {
            bool exists = Directory.Exists(folderPath);

            if (!exists)
                Directory.CreateDirectory(folderPath);
        }
    }
}
