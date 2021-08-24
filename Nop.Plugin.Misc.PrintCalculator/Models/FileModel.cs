using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.PrintCalculator.Models
{
    [Serializable]
    public class FileModel
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Location { get; set; }
        public string Thumbnail { get; set; }
        public int PageCount { get; set; }
        public bool Readable { get; set; }
    }
}
