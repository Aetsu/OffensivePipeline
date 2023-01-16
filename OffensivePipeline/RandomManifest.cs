using OffensivePipeline.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OffensivePipeline
{
    internal class RandomManifest : iModule
    {
        public string Name => "RandomManifest";
        public ToolConfig _tool { get; set; }
        public ModuleOutput _moduleOutput { get; set; }

        public ModuleOutput CheckStart()
        {
            return _moduleOutput;
        }

        public ModuleOutput Run()
        {
            return _moduleOutput;
        }
    }
}
