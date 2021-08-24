using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.PrintCalculator.Models;
using Nop.Services.Plugins;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Common;
using Nop.Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf;
using System.Drawing;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Logging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using Nop.Services.Seo;
using System.Drawing.Imaging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Core;

namespace Nop.Plugin.Misc.PrintCalculator.Controllers
{
    //[AutoValidateAntiforgeryToken]

    public class PrintCalculatorController : BasePluginController
    {
        private readonly ILogger _logger;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        
        private readonly INotificationService _notificationService;
        private readonly ICategoryService _categoryService;

        private readonly IStoreContext _storeContext = EngineContext.Current.Resolve<IStoreContext>();


        private readonly string _fileUploadsDirectory = "printUploads";
        private readonly string _jobDescriptionDirectory = "printJobDescription";

        private PrintCalculatorSettings _printCalculatorSettings;


        public PrintCalculatorController(ILogger logger, 
            IProductService productService, 
            IPictureService pictureService, 
            IPermissionService permissionService, 
            ISettingService settingService, 
            INotificationService notificationService,
            ICategoryService categoryService)
        {
            _logger = logger;
            _productService = productService;
            _pictureService = pictureService;
            _permissionService = permissionService;
            _settingService = settingService;
            _notificationService = notificationService;
            _categoryService = categoryService;
        }


        [AuthorizeAdmin] //confirms access to the admin panel
        [Area(AreaNames.Admin)] //specifies the area containing a controller or action
        public async Task<IActionResult> Configure()
        {
            _printCalculatorSettings = await _settingService.LoadSettingAsync<PrintCalculatorSettings>((await _storeContext.GetCurrentStoreAsync()).Id);
            //prepare model
            var model = new ConfigurationModel()
            {
                PrintCategoryID = _printCalculatorSettings.PrintCategoryID
            };

            return View("~/Plugins/Misc.PrintCalculator/Views/Configure.cshtml", model);
        }

        [AuthorizeAdmin] //confirms access to the admin panel
        [Area(AreaNames.Admin)] //specifies the area containing a controller or action
        [HttpPost, ActionName("Configure")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            _printCalculatorSettings = await _settingService.LoadSettingAsync<PrintCalculatorSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

            //save settings
            _printCalculatorSettings.PrintCategoryID = model.PrintCategoryID;
            await _settingService.SaveSettingAsync(_printCalculatorSettings);

            _notificationService.SuccessNotification("PrintCalculator settings changed.");

            return await Configure();
        }

        public async Task<IActionResult> FileUpload()
        {
            return View("~/Plugins/Misc.PrintCalculator/Views/FileUpload.cshtml");
        }

        public async Task<IActionResult> ImageUpload()
        {
            return View("~/Plugins/Misc.PrintCalculator/Views/ImageUpload.cshtml");
        }

        private bool CheckExtensions(IFormFile file)
        {
            if (file == null)
                return false;

            if (file.FileName == null)
                return false;

            var extension = Path.GetExtension(file.FileName);
            var supportedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".tiff", ".tif", ".pdf", ".bmp", ".gif" };

            bool extensionCheck = false;
            supportedExtensions.ForEach(x =>
            {
                if (string.Equals(x, extension, StringComparison.OrdinalIgnoreCase))
                    extensionCheck = true;
            });

            if (!extensionCheck)
                return false;

            return true;
        }

        private async Task<string> GenerateNewFile(string extension, IFormFile file)
        {
            try
            {
                string filename = Guid.NewGuid() + extension;

                using (FileStream output = System.IO.File.Create(Path.Combine(_fileUploadsDirectory, filename)))
                {
                    await file.CopyToAsync(output);
                }

                return filename;

            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Print file upload error", e.Message);
                return null;
            }
        }

        private async Task<string> GenerateBase64Thumbnail(string imagePath)
        {
            Image img = Image.FromFile(imagePath);
            int thumbWidth = 300;
            int ratio = img.Height / img.Width;
            int thumbHeight = ratio * thumbWidth;
            Image thumb = img.GetThumbnailImage(thumbWidth, thumbHeight, null, IntPtr.Zero);
            using (MemoryStream m = new MemoryStream())
            {
                thumb.Save(m, ImageFormat.Png);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return "data:image/png;base64," + base64String;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if(CheckExtensions(file) == false)
            {
                return BadRequest();
            }
            var extension = Path.GetExtension(file.FileName);
            var model = new FileModel();

            Directory.CreateDirectory(_fileUploadsDirectory);
            Directory.CreateDirectory(_jobDescriptionDirectory);

            model.Extension = extension;
            model.Readable = true;

            string filename = await GenerateNewFile(extension, file);
            if (filename == null)
            {
                return StatusCode(500);
            }
            
            model.Name = filename;
            model.Extension = extension;
            model.Location = Path.Combine(_fileUploadsDirectory, filename);


            string existingPath = Path.Combine(_fileUploadsDirectory, model.Name);

            switch (extension)
            {
                case ".pdf":
                    var fs = System.IO.File.OpenRead(existingPath);
                    PdfDocument inputDocument = PdfReader.Open(fs, PdfDocumentOpenMode.Import);
                    if (inputDocument.SecuritySettings.PermitExtractContent == false)
                    {
                        return BadRequest();
                    }
                    model.PageCount = inputDocument.PageCount;
                    await fs.DisposeAsync();
                    break;
                default:
                    model.Thumbnail = await GenerateBase64Thumbnail(existingPath);
                    model.PageCount = 1;
                    break;
            }

            try
            {
                string descriptionFileName = model.Name + ".desc";

                using FileStream createStream = System.IO.File.Create(Path.Combine(_jobDescriptionDirectory, descriptionFileName));
                await JsonSerializer.SerializeAsync(createStream, model);
                await createStream.DisposeAsync();
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Print job description save error", e.Message);
                return StatusCode(500);
            }

            return Ok("Customize?filename=" + model.Name);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImg(IFormFile file)
        {
            if (CheckExtensions(file) == false)
            {
                return BadRequest();
            }

            var model = new ImageModel();
            var extension = Path.GetExtension(file.FileName);


            Directory.CreateDirectory(_fileUploadsDirectory);
            Directory.CreateDirectory(_jobDescriptionDirectory);

            model.Extension = extension;
            model.Readable = true;

            string filename = await GenerateNewFile(extension, file);
            if (filename == null)
            {
                return StatusCode(500);
            }

            model.Name = filename;

            string existingPath = Path.Combine(_fileUploadsDirectory, model.Name);
            model.Thumbnail = await GenerateBase64Thumbnail(existingPath);

            try
            {
                string descriptionFileName = model.Name + ".desc";

                using FileStream createStream = System.IO.File.Create(Path.Combine(_jobDescriptionDirectory, descriptionFileName));
                await JsonSerializer.SerializeAsync(createStream, model);
                await createStream.DisposeAsync();
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Image job description save error", e.Message);
                return StatusCode(500);
            }

            return Ok("CustomizeImage?filename=" + model.Name);
        }

        [HttpGet]
        public async Task<IActionResult> Customize(string filename)
        {
            var fileModel = await RetrieveFileAsync(filename);
            if (fileModel.Readable == false)
                return NotFound();

            var model = new CustomizeModel()
            {
                PageCount = fileModel.PageCount,
                Reference = filename,
                Thumbnail = fileModel.Thumbnail
            };
            model.PaperSize.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("A4 297x210mm", "a4"));
            model.PaperSize.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("A5 210x148mm", "a5"));
            model.PaperSize.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("A3 420x297mm", "a3"));
            model.PaperType.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Obični 80gr/m²", "80plain"));
            model.PaperType.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Mat 250gr/m²", "250matte"));
            model.PaperType.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Mat 130gr/m²", "130matte"));
            model.PaperType.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Sjajni 250gr/m²", "250glossy"));
            model.Options = OptionCost;

            return View("~/Plugins/Misc.PrintCalculator/Views/Customize.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> CustomizeImage(string filename)
        {
            var fileModel = await RetrieveImageFileAsync(filename);
            if (fileModel.Readable == false)
                return NotFound();

            var model = new CustomizeImageModel()
            {
                Reference = filename,
                Thumbnail = fileModel.Thumbnail,
                WidthInCm = 50.00,
                HeightInCm = 50*fileModel.Ratio
            };
            
            model.PaperType.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Sjajni foto papir", "glossy"));
            model.PaperType.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Mat foto papir", "matte"));
            model.PaperType.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Kanvas platno", "canvas"));
            model.PaperType.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Platno na drvenom ramu", "framedCanvas"));
            model.Options = ImageOptionCost;

            return View("~/Plugins/Misc.PrintCalculator/Views/CustomizeImage.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection fc)
        {
            if (string.IsNullOrEmpty(fc["name"]))
                return BadRequest();

            var productService = EngineContext.Current.Resolve<IProductService>();
            var urlService = EngineContext.Current.Resolve<IUrlRecordService>();

            var model = await RetrieveFileAsync(fc["name"]);

            if(model.Readable == false)
            {
                return BadRequest();
            }

            model.Readable = false;

            try
            {
                string descriptionFileName = model.Name + ".desc";

                using FileStream createStream = System.IO.File.Create(Path.Combine(_jobDescriptionDirectory, descriptionFileName));
                await JsonSerializer.SerializeAsync(createStream, model);
                await createStream.DisposeAsync();
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Print job description save error", e.Message);
                return StatusCode(500);
            }

            var options = new List<string>();
            decimal basePrice = .1M;
            foreach(var keyValue in fc)
            {
                if (OptionCost.ContainsKey(keyValue.Value))
                {
                    basePrice *= OptionCost[keyValue.Value];
                    options.Add(keyValue.Value);
                }
                if (keyValue.Value.Contains("true"))
                {
                    basePrice *= OptionCost[keyValue.Key];
                    options.Add(keyValue.Key);
                }
            }

            if (options.Contains("Duplex"))
            {
                model.PageCount = model.PageCount % 2 == 0 ? model.PageCount : model.PageCount + 1;
            }

            basePrice *= model.PageCount;

            if (options.Contains("Stapling"))
            {
                basePrice += 0.5M;
            }

            string shortDesc = "";
            foreach(var option in options)
            {
                shortDesc += Description[option];
                shortDesc += "<br />";
            }

            if(!options.Contains("PrintColor"))
            {
                shortDesc += "✔️ Crno bijela štampa";
                shortDesc += "<br />";
            }

            var newProd = new Product()
            {
                Name = $"Štampa - Broj stranica: " + model.PageCount,
                Price = basePrice,
                ShortDescription = shortDesc,
                FullDescription = "<div style='display: none'>" + JsonSerializer.Serialize(fc) + "</div>",
                Published = true,
                VisibleIndividually = true,
                IsShipEnabled = true,
                ShowOnHomepage = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                AdminComment = "noSearch",
                ProductType = ProductType.SimpleProduct,
                OrderMaximumQuantity = 100,
                OrderMinimumQuantity = 1
            };

            var productPicture = await _pictureService.InsertPictureAsync(Convert.FromBase64String(fc["thumbnail"].FirstOrDefault().Remove(0, "data:image/png;base64,".Length)), "image/png", "thumb_" + model.Name);

            await productService.InsertProductAsync(newProd);

            var storeID = (await _storeContext.GetCurrentStoreAsync()).Id;

            PrintCalculatorSettings printCalculatorSettings = await _settingService.LoadSettingAsync<PrintCalculatorSettings>(storeID);

            ProductCategory productCategory = new ProductCategory()
            {
                CategoryId = printCalculatorSettings.PrintCategoryID,
                ProductId = newProd.Id
            };

            await _categoryService.InsertProductCategoryAsync(productCategory);

            var seoRecord = new Core.Domain.Seo.UrlRecord
            {
                EntityName = "Product",
                EntityId = newProd.Id,
                IsActive = true,
                Slug = Path.GetFileNameWithoutExtension("print-" + model.Name),
            };

            await urlService.InsertUrlRecordAsync(seoRecord);

            ProductPicture picture = new ProductPicture()
            {
                PictureId = productPicture.Id,
                ProductId = newProd.Id
            };

            await _productService.InsertProductPictureAsync(picture);

            return RedirectToRoute("Product", new { SeName = seoRecord.Slug });
        }

        [HttpPost]
        public async Task<IActionResult> CreateImage(IFormCollection fc)
        {
            if (string.IsNullOrEmpty(fc["name"]))
                return BadRequest();

            var productService = EngineContext.Current.Resolve<IProductService>();
            var urlService = EngineContext.Current.Resolve<IUrlRecordService>();

            var model = await RetrieveImageFileAsync(fc["name"]);

            if (model.Readable == false)
            {
                return BadRequest();
            }

            model.Readable = false;

            try
            {
                string descriptionFileName = model.Name + ".desc";

                using FileStream createStream = System.IO.File.Create(Path.Combine(_jobDescriptionDirectory, descriptionFileName));
                await JsonSerializer.SerializeAsync(createStream, model);
                await createStream.DisposeAsync();
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Print job description save error", e.Message);
                return StatusCode(500);
            }

            var options = new List<string>();
            decimal basePrice = .004M;
            foreach (var keyValue in fc)
            {
                if (ImageOptionCost.ContainsKey(keyValue.Value))
                {
                    basePrice *= ImageOptionCost[keyValue.Value];
                    options.Add(keyValue.Value);
                }
                if (keyValue.Value.Contains("true"))
                {
                    basePrice *= ImageOptionCost[keyValue.Key];
                    options.Add(keyValue.Key);
                }
            }

            basePrice *= Decimal.Parse(fc["WidthInCm"]) * Decimal.Parse(fc["HeightInCm"]);

            string shortDesc = "";
            foreach (var option in options)
            {
                shortDesc += ImageDescription[option];
                shortDesc += "<br />";
                shortDesc += "" + fc["WidthInCm"] + "cm x " + fc["HeightInCm"] + "cm<br />";
            }


            var newProd = new Product()
            {
                Name = $"Štampa slike",
                Price = basePrice,
                ShortDescription = shortDesc,
                FullDescription = "<div style='display: none'>" + JsonSerializer.Serialize(fc) + "</div>",
                Published = true,
                VisibleIndividually = true,
                IsShipEnabled = true,
                ShowOnHomepage = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                AdminComment = "noSearch",
                ProductType = ProductType.SimpleProduct,
                OrderMaximumQuantity = 100,
                OrderMinimumQuantity = 1
            };

            var productPicture = await _pictureService.InsertPictureAsync(Convert.FromBase64String(fc["thumbnail"].FirstOrDefault().Remove(0, "data:image/png;base64,".Length)), "image/png", "thumb_" + model.Name);



            await productService.InsertProductAsync(newProd);

            var storeID = (await _storeContext.GetCurrentStoreAsync()).Id;

            PrintCalculatorSettings printCalculatorSettings = await _settingService.LoadSettingAsync<PrintCalculatorSettings>(storeID);

            ProductCategory productCategory = new ProductCategory()
            {
                CategoryId = printCalculatorSettings.PrintCategoryID,
                ProductId = newProd.Id
            };

            await _categoryService.InsertProductCategoryAsync(productCategory);

            var seoRecord = new Core.Domain.Seo.UrlRecord
            {
                EntityName = "Product",
                EntityId = newProd.Id,
                IsActive = true,
                Slug = Path.GetFileNameWithoutExtension("printImage-" + model.Name),
            };

            await urlService.InsertUrlRecordAsync(seoRecord);

            ProductPicture picture = new ProductPicture()
            {
                PictureId = productPicture.Id,
                ProductId = newProd.Id
            };

            await _productService.InsertProductPictureAsync(picture);

            return RedirectToRoute("Product", new { SeName = seoRecord.Slug });
        }

        private async Task<FileModel> RetrieveFileAsync(string fileName)
        {
            string descriptionFileName = fileName + ".desc";

            using FileStream readStream = System.IO.File.OpenRead(Path.Combine(_jobDescriptionDirectory, descriptionFileName));
            var model = JsonSerializer.DeserializeAsync<FileModel>(readStream);

            return await model;
        }

        private async Task<ImageModel> RetrieveImageFileAsync(string fileName)
        {
            string descriptionFileName = fileName + ".desc";

            using FileStream readStream = System.IO.File.OpenRead(Path.Combine(_jobDescriptionDirectory, descriptionFileName));
            var model = JsonSerializer.DeserializeAsync<ImageModel>(readStream);

            return await model;
        }

        private readonly Dictionary<string, decimal> OptionCost = new Dictionary<string, decimal>()
        {
            {"BasePrice", 0.1M },
            {"Duplex", 0.8M },
            {"PrintColor", 2.8M },
            {"Stapling", 0.5M },
            {"a4", 1M },
            {"a3", 2M },
            {"a5", 0.6M },
            {"80plain", 1M },
            {"130matte", 2M },
            {"250matte", 3.5M },
            {"250glossy", 3.5M }
        };

        private readonly Dictionary<string, decimal> ImageOptionCost = new Dictionary<string, decimal>()
        {
            {"BasePrice", 0.004M },
            {"glossy", 1.5M },
            {"matte", 1.5M },
            {"canvas", 2.5M },
            {"framedCanvas", 6.5M }
        };

        private readonly Dictionary<string, string> Description = new Dictionary<string, string>()
        {
            {"Duplex", "✔️ Obostrana štampa" },
            {"PrintColor", "✔️ Štampa u boji" },
            {"Stapling", "✔️ Heftanje" },
            {"80plain", "Obični papir - 80gr/m²" },
            {"130matte", "Mat papir - 130gr/m²" },
            {"250matte", "Mat papir - 250gr/m²" },
            {"250glossy", "Sjajni papir - 250gr/m²" },
            {"a4", "Veličina papira: A4 - 210x297mm" },
            {"a3", "Veličina papira: A3 - 297x497mm" },
            {"a5", "Veličina papira: A5 - 148x210mm" },
        };

        private readonly Dictionary<string, string> ImageDescription = new Dictionary<string, string>()
        {
            {"matte", "Mat foto papir" },
            {"glossy", "Sjajni foto papir" },
            {"canvas", "Kanvas platno" },
            {"framedCanvas", "Platno na drvenom ramu" },
        };
    }
}
