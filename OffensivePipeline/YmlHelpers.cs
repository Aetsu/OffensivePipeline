using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.RepresentationModel;

namespace OffensivePipeline
{
    public class YmlHelpers
    {
        public static List<ToolConfig> ReadYmls()
        {
            List<ToolConfig> lTools = new List<ToolConfig>();

            if (!Directory.Exists(Conf.ymlsPath))
            {
                LogHelpers.PrintError($"ReadYmls: Folder not found <{Conf.ymlsPath}>");
                LogHelpers.LogToFile("ReadYmls", "ERROR", $"Folder not found <{Conf.ymlsPath}>");
            }
            else
            {
                foreach (string tool in Directory.GetFiles(Conf.ymlsPath, "*.yml"))
                {
                    string outputFolder = string.Empty;
                    string toolPath = string.Empty;
                    string text = File.ReadAllText(tool);
                    var stringReader = new StringReader(text);
                    var yaml = new YamlStream();
                    yaml.Load(stringReader);
                    var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                    foreach (var entry in mapping.Children)
                    {
                        var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("tool")];
                        foreach (YamlMappingNode item in items)
                        {
                            try
                            {
                                lTools.Add(new ToolConfig(
                                item.Children[new YamlScalarNode("name")].ToString(),
                                    item.Children[new YamlScalarNode("description")].ToString(),
                                    item.Children[new YamlScalarNode("gitLink")].ToString(),
                                    item.Children[new YamlScalarNode("solutionPath")].ToString(),
                                    item.Children[new YamlScalarNode("language")].ToString(),
                                    item.Children[new YamlScalarNode("plugins")].ToString(),
                                    item.Children[new YamlScalarNode("authUser")].ToString(),
                                    item.Children[new YamlScalarNode("authToken")].ToString(),
                                    item.Children[new YamlScalarNode("toolArguments")].ToString()
                                    ));
                            }
                            catch (Exception e)
                            {
                                LogHelpers.PrintError($"ReadYmls: <{item.Children[new YamlScalarNode("name")]}> - {e}");
                                LogHelpers.LogToFile("ReadYmls", "ERROR", $"Tool: <{item.Children[new YamlScalarNode("name")]}> - {e}");
                            }
                        }
                    }
                }
            }
            return lTools;
        }

        public static List<ToolConfig> ReadYmls(string ymlName, string overrideArguments)
        {
            List<ToolConfig> lTools = new List<ToolConfig>();

            if (!Directory.Exists(Conf.ymlsPath))
            {
                LogHelpers.PrintError($"ReadYmls: Folder not found <{Conf.ymlsPath}>");
                LogHelpers.LogToFile("ReadYmls", "ERROR", $"Folder not found <{Conf.ymlsPath}>");
            }
            else
            { 
                string outputFolder = string.Empty;
                string toolPath = string.Empty;
                string text = File.ReadAllText(Path.Combine(Conf.ymlsPath, $"{ymlName}.yml"));
                var stringReader = new StringReader(text);
                var yaml = new YamlStream();
                yaml.Load(stringReader);
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                foreach (var entry in mapping.Children)
                {
                    var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("tool")];
                    foreach (YamlMappingNode item in items)
                    {
                        try
                        {
                            lTools.Add(new ToolConfig(
                            item.Children[new YamlScalarNode("name")].ToString(),
                                item.Children[new YamlScalarNode("description")].ToString(),
                                item.Children[new YamlScalarNode("gitLink")].ToString(),
                                item.Children[new YamlScalarNode("solutionPath")].ToString(),
                                item.Children[new YamlScalarNode("language")].ToString(),
                                item.Children[new YamlScalarNode("plugins")].ToString(),
                                item.Children[new YamlScalarNode("authUser")].ToString(),
                                item.Children[new YamlScalarNode("authToken")].ToString(),
                                (overrideArguments!=null) ? overrideArguments : item.Children[new YamlScalarNode("toolArguments")].ToString()
                                ));
                        }
                        catch (Exception e)
                        {
                            LogHelpers.PrintError($"ReadYmls: <{item.Children[new YamlScalarNode("name")]}> - {e}");
                            LogHelpers.LogToFile("ReadYmls", "ERROR", $"Tool: <{item.Children[new YamlScalarNode("name")]}> - {e}");
                        }
                    }
                }

            }
            return lTools;
        }
    }
}
