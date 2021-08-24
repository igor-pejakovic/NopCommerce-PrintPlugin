using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.PrintCalculator.Models
{
    [Serializable]
    public class PrintJobModel
    {
        public FileModel FileModel { get; set; }
        public decimal Quantity { get; set; }
        public Dictionary<string, string> Options = new Dictionary<string, string>();
    }
}
