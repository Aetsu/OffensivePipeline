using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OffensivePipeline.Modules
{
    public interface iModule
    {
        public ToolConfig _tool { get; set; }
        public ModuleOutput _moduleOutput { get; set; }
        public string Name { get; }

        ModuleOutput CheckStart();
        ModuleOutput Run();
    }
}
