using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OffensivePipeline
{
    public class ToolConfig
    {
        public ToolConfig(string name, string description, string gitLink, string solutionPath, string language, string plugins, string authUser, string authToken)
        {
            this.name = name;
            this.description = description;
            this.gitLink = gitLink;
            this.solutionPath = solutionPath;
            this.language = language;
            this.plugins = plugins.Split(',').Select(s => s.Trim()).ToList<string>();
            this.authUser = authUser;
            this.authToken = authToken;
        }

        public string name { get; set; }
        public string description { get; set; }
        public string gitLink { get; set; }
        public string solutionPath { get; set; }
        public string language { get; set; }
        public List<string> plugins { get; set; }
        public string authUser { get; set; }
        public string authToken { get; set; }
    }
}
