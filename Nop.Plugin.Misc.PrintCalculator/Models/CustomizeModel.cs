
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.PrintCalculator.Models
{
    public class CustomizeModel
    {
        public CustomizeModel()
        {
            PaperSize = new List<SelectListItem>();
            PaperType = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.PrintCalculator.PrintOptions.PaperSize")]
        public IList<SelectListItem> PaperSize { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PrintCalculator.PrintOptions.PaperType")]
        public IList<SelectListItem> PaperType { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PrintCalculator.PrintOptions.PrintColor")]
        public bool PrintColor { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PrintCalculator.PrintOptions.Duplex")]
        public bool Duplex { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PrintCalculator.PrintOptions.Stapling")]
        public bool Stapling { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PrintCalculator.PrintOptions.PageCount")]
        public int PageCount { get; set; }
        public string Reference { get; set; }
        public Dictionary<string, decimal> Options;
        public string Thumbnail { get; set; }
    }
}
