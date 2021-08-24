using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.PrintCalculator.Models
{
    public class CustomizeImageModel
    {
        public CustomizeImageModel()
        {
            PaperType = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.PrintCalculator.PrintOptions.PaperType")]
        public IList<SelectListItem> PaperType { get; set; }
        [NopResourceDisplayName("Plugins.Misc.PrintCalculator.PrintOptions.WidthInCm")]
        public double WidthInCm { get; set; }
        [NopResourceDisplayName("Plugins.Misc.PrintCalculator.PrintOptions.HeightInCm")]
        public double HeightInCm { get; set; }

        public string Reference { get; set; }
        public Dictionary<string, decimal> Options;
        public string Thumbnail { get; set; }
    }
}
