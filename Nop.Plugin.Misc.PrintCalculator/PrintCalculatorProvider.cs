using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.PrintCalculator
{
    public class PrintCalculatorProvider : BasePlugin, IMiscPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public PrintCalculatorProvider(ISettingService settingService, IWebHelper webHelper, ILocalizationService localizationService)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _localizationService = localizationService;
        }
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PrintCalculator/Configure";
        }
        public string GetPublicViewComponentName()
        {
            return "PrintCalculator";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {

            //locales
            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.PrintCalculator.PrintOptions.DragAndDrop"] = "Drag and drop your file here",
                ["Plugins.Misc.PrintCalculator.PrintOptions.Or"] = "Or",
                ["Plugins.Misc.PrintCalculator.PrintOptions.SelectFile"] = "Select a file",
                ["Plugins.Misc.PrintCalculator.PrintOptions.PaperSize"] = "Paper size",
                ["Plugins.Misc.PrintCalculator.PrintOptions.PaperType"] = "Paper type",
                ["Plugins.Misc.PrintCalculator.PrintOptions.Duplex"] = "Two-sided printing",
                ["Plugins.Misc.PrintCalculator.PrintOptions.Stapling"] = "Stapling",
                ["Plugins.Misc.PrintCalculator.PrintOptions.PrintColor"] = "Print in color",
                ["Plugins.Misc.PrintCalculator.PrintOptions.NextStep"] = "You may specify the quantity in the next step.",
                ["Plugins.Misc.PrintCalculator.PrintOptions.Submit"] = "Next step",
                ["Plugins.Misc.PrintCalculator.PrintOptions.SupportedFormatsText"] = "Supported formats: ",
                ["Plugins.Misc.PrintCalculator.PrintOptions.SupportedFormats"] = ".pdf .jpg .jpeg .png .tiff .tif .bmp .gif",
                ["Plugins.Misc.PrintCalculator.PrintOptions.PageCount"] = "Number of pages",
                ["Plugins.Misc.PrintCalculator.PrintOptions.Price"] = "Price",
                ["Plugins.Misc.PrintCalculator.PrintOptions.MaxSize"] = "15MB",
                ["Plugins.Misc.PrintCalculator.PrintOptions.MaxSizeText"] = "Maximum file size: ",
                ["Plugins.Misc.PrintCalculator.PrintOptions.MaxSizeAlert"] = "File size must be under 15MB",
                ["Plugins.Misc.PrintCalculator.PrintOptions.WidthInCm"] = "Width in cm: ",
                ["Plugins.Misc.PrintCalculator.PrintOptions.HeightInCm"] = "Height in cm: ",
                ["Plugins.Misc.PrintCalculator.PrintOptions.ImageType"] = "Image type"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.PrintCalculator.PrintOptions");
        }
    }
}
