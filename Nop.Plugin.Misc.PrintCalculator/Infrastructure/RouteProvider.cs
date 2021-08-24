using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.PrintCalculator.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.PrintCalculator.FileUpload", "Print/FileUpload",
                 new { controller = "PrintCalculator", action = "FileUpload"});

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.PrintCalculator.ImageUpload", "Print/ImageUpload",
                 new { controller = "PrintCalculator", action = "ImageUpload" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.PrintCalculator.Upload", "Print/Upload",
                new { controller = "PrintCalculator", action = "Upload" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.PrintCalculator.UploadImg", "Print/UploadImg",
                new { controller = "PrintCalculator", action = "UploadImg" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.PrintCalculator.Custmoize", "Print/Customize",
                new { controller = "PrintCalculator", action = "Customize" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.PrintCalculator.CustmoizeImage", "Print/CustomizeImage",
                new { controller = "PrintCalculator", action = "CustomizeImage" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.PrintCalculator.Create", "Print/Create",
                new { controller = "PrintCalculator", action = "Create" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.PrintCalculator.CreateImage", "Print/CreateImage",
                new { controller = "PrintCalculator", action = "CreateImage" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 2;
    }
}