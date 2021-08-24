using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.PrintCalculator.Models
{
    [Serializable]
    public class ImageModel
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Location { get; set; }
        public string Thumbnail { get; set; }
        public bool Readable { get; set; }
        public float Ratio { get; set; }
    }
}
