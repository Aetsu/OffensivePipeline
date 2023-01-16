using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OffensivePipeline
{
    public class ModuleOutput
    {
        public string Name { get; set; }
        public string? OutputPath { get; set; }
        public bool Status { get; set; }

        public ModuleOutput()
        {
            this.Status = true;
        }
    }
}
