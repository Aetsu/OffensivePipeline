using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;



namespace OffensivePipeline
{
    internal class GitHelpers
    {
        public static bool CloneLocalTool(ToolConfig tool)
        {
            bool status = CloneChecks(tool);
            if (status)
            {
                string toolPath = Path.Combine(Conf.gitToolsPath, tool.name);
                status = Helpers.CopyDirectory(tool.gitLink, toolPath, true);
                if (status)
                {
                    if (!File.Exists(Path.Combine(Conf.gitToolsPath, tool.solutionPath)))
                    {
                        status = false;
                        LogHelpers.PrintError($"CloneLocalTool: Folder not found {toolPath}");
                    }
                    else
                    {
                        LogHelpers.PrintOk($"\t\t Repository {tool.name} copied into {toolPath}");
                    }
                }
            }
            return status;
        }
        public static bool CloneChecks(ToolConfig tool)
        {
            bool status = true;
            try
            {
                if (!Directory.Exists(Conf.gitToolsPath))
                {
                    DirectoryInfo di = Directory.CreateDirectory(Conf.gitToolsPath);
                }
            }
            catch (Exception e)
            {
                LogHelpers.PrintError($"DownloadRepository - Creating {Conf.gitToolsPath} folder - {e}");
                status = false;
            }
            if (status)
            {
                string toolPath = Path.Combine(Conf.gitToolsPath, tool.name);
                try
                {
                    if (Directory.Exists(toolPath))
                    {
                        Helpers.DeleteReadOnlyDirectory(toolPath);
                    }
                }
                catch (Exception e)
                {
                    LogHelpers.PrintError($"CloneChecks - Deleting {toolPath} folder - {e}");
                    status = false;
                }
            }

            return status;
        }
        public static bool DownloadRepository(ToolConfig tool)
        {
            bool status = CloneChecks(tool);
            if (status)
            {
                try
                {
                    string toolPath = Path.Combine(Conf.gitToolsPath, tool.name);
                    if (!Directory.Exists(toolPath))
                    {
                        LogHelpers.PrintYellow($"    Clonnig repository: {tool.name} into {toolPath}");
                        CloneOptions co = new CloneOptions();
                        if (tool.authToken != "")
                        {
                            string gitUser = tool.authUser, gitToken = tool.authToken;
                            co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = gitUser, Password = gitToken };
                        }
                        co.RecurseSubmodules = true;
                        _ = Repository.Clone(tool.gitLink, toolPath, co);
                        if (!File.Exists(Path.Combine(Conf.gitToolsPath, tool.solutionPath)))
                        {
                            status = false;
                            LogHelpers.PrintError($"DownloadRepository: Solution not found {Path.Combine(Conf.gitToolsPath, tool.solutionPath)}");
                        }
                        else
                        {
                            LogHelpers.PrintOk($"\t\t Repository {tool.name} cloned into {toolPath}");
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelpers.PrintError($"DownloadRepository: {tool.name} - {e}");
                    status = false;
                }
            }
            return status;
        }
    }
}
