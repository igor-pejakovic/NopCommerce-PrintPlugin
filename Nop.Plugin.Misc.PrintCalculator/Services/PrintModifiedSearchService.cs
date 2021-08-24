using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.PrintCalculator.Services
{
    public class PrintModifiedSearchService : ProductService
    {
        #region Fields

        protected readonly CatalogSettings _catalogSettings;
        protected readonly CommonSettings _commonSettings;
        protected readonly IAclService _aclService;
        protected readonly ICustomerService _customerService;
        protected readonly IDateRangeService _dateRangeService;
        protected readonly ILanguageService _languageService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        protected readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;
        protected readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        protected readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        protected readonly IRepository<ProductPicture> _productPictureRepository;
        protected readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        protected readonly IRepository<ProductReview> _productReviewRepository;
        protected readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
        protected readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        protected readonly IRepository<ProductTag> _productTagRepository;
        protected readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        protected readonly IRepository<RelatedProduct> _relatedProductRepository;
        protected readonly IRepository<Shipment> _shipmentRepository;
        protected readonly IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
        protected readonly IRepository<TierPrice> _tierPriceRepository;
        protected readonly IRepository<Warehouse> _warehouseRepository;
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IStoreService _storeService;
        protected readonly IWorkContext _workContext;
        protected readonly LocalizationSettings _localizationSettings;

        #endregion

        #region Ctor

        public PrintModifiedSearchService(CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            IAclService aclService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IRepository<DiscountProductMapping> discountProductMappingRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<Product> productRepository,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IRepository<ProductAttributeMapping> productAttributeMappingRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductPicture> productPictureRepository,
            IRepository<ProductProductTagMapping> productTagMappingRepository,
            IRepository<ProductReview> productReviewRepository,
            IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<Shipment> shipmentRepository,
            IRepository<StockQuantityHistory> stockQuantityHistoryRepository,
            IRepository<TierPrice> tierPriceRepository,
            IRepository<Warehouse> warehouseRepository,
            IStaticCacheManager staticCacheManager,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            LocalizationSettings localizationSettings)
            : base(catalogSettings, commonSettings, aclService, customerService, dateRangeService, languageService, localizationService, productAttributeParser, productAttributeService,
                 crossSellProductRepository, discountProductMappingRepository, localizedPropertyRepository, productRepository, productAttributeCombinationRepository, productAttributeMappingRepository,
                 productCategoryRepository, productManufacturerRepository, productPictureRepository, productTagMappingRepository, productReviewRepository, productReviewHelpfulnessRepository,
                 productSpecificationAttributeRepository, productTagRepository, productWarehouseInventoryRepository, relatedProductRepository, shipmentRepository, stockQuantityHistoryRepository,
                 tierPriceRepository, warehouseRepository, staticCacheManager, storeService, storeMappingService, workContext, localizationSettings)
        {
            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
            _aclService = aclService;
            _customerService = customerService;
            _dateRangeService = dateRangeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _crossSellProductRepository = crossSellProductRepository;
            _discountProductMappingRepository = discountProductMappingRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _productRepository = productRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productPictureRepository = productPictureRepository;
            _productTagMappingRepository = productTagMappingRepository;
            _productReviewRepository = productReviewRepository;
            _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _productTagRepository = productTagRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _relatedProductRepository = relatedProductRepository;
            _shipmentRepository = shipmentRepository;
            _stockQuantityHistoryRepository = stockQuantityHistoryRepository;
            _tierPriceRepository = tierPriceRepository;
            _warehouseRepository = warehouseRepository;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _workContext = workContext;
            _localizationSettings = localizationSettings;
        }

        #endregion

        //public override async Task<IPagedList<Product>> SearchProductsAsync(int pageIndex = 0,
        //    int pageSize = int.MaxValue,
        //    IList<int> categoryIds = null,
        //    IList<int> manufacturerIds = null,
        //    int storeId = 0,
        //    int vendorId = 0,
        //    int warehouseId = 0,
        //    ProductType? productType = null,
        //    bool visibleIndividuallyOnly = false,
        //    bool excludeFeaturedProducts = false,
        //    decimal? priceMin = null,
        //    decimal? priceMax = null,
        //    int productTagId = 0,
        //    string keywords = null,
        //    bool searchDescriptions = false,
        //    bool searchManufacturerPartNumber = true,
        //    bool searchSku = true,
        //    bool searchProductTags = false,
        //    int languageId = 0,
        //    IList<SpecificationAttributeOption> filteredSpecOptions = null,
        //    ProductSortingEnum orderBy = ProductSortingEnum.Position,
        //    bool showHidden = false,
        //    bool? overridePublished = null)
        //{
        //    //some databases don't support int.MaxValue
        //    if (pageSize == int.MaxValue)
        //        pageSize = int.MaxValue - 1;

        //    var productsQuery = _productRepository.Table;

        //    if (!showHidden)
        //        // Ignorise ako iam admin comment noSearch
        //        productsQuery = productsQuery.Where(p => p.Published).Where(p => p.AdminComment != "noSearch");
        //    else if (overridePublished.HasValue)
        //        productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

        //    //apply store mapping constraints
        //    productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

        //    //apply ACL constraints
        //    if (!showHidden)
        //    {
        //        var customer = await _workContext.GetCurrentCustomerAsync();
        //        productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
        //    }

        //    productsQuery =
        //        from p in productsQuery
        //        where !p.Deleted &&
        //            (vendorId == 0 || p.VendorId == vendorId) &&
        //            (
        //                warehouseId == 0 ||
        //                (
        //                    !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
        //                        _productWarehouseInventoryRepository.Table.Any(pwi => pwi.Id == warehouseId && pwi.ProductId == p.Id)
        //                )
        //            ) &&
        //            (productType == null || p.ProductTypeId == (int)productType) &&
        //            (showHidden == false || LinqToDB.Sql.Between(DateTime.UtcNow, p.AvailableStartDateTimeUtc ?? DateTime.MinValue, p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)) &&
        //            (priceMin == null || p.Price >= priceMin) &&
        //            (priceMax == null || p.Price <= priceMax)
        //        select p;

        //    if (!string.IsNullOrEmpty(keywords))
        //    {
        //        var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

        //        //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
        //        var searchLocalizedValue = languageId > 0 && langs.Count() >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);

        //        IQueryable<int> productsByKeywords;

        //        productsByKeywords =
        //                from p in _productRepository.Table
        //                where p.Name.Contains(keywords) ||
        //                    (searchDescriptions &&
        //                        (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
        //                    (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
        //                    (searchSku && p.Sku == keywords)
        //                select p.Id;

        //        //search by SKU for ProductAttributeCombination
        //        if (searchSku)
        //        {
        //            productsByKeywords = productsByKeywords.Union(
        //                from pac in _productAttributeCombinationRepository.Table
        //                where pac.Sku == keywords
        //                select pac.ProductId);
        //        }

        //        if (searchProductTags)
        //        {
        //            productsByKeywords = productsByKeywords.Union(
        //                from pptm in _productTagMappingRepository.Table
        //                join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
        //                where pt.Name == keywords
        //                select pptm.ProductId
        //            );

        //            if (searchLocalizedValue)
        //            {
        //                productsByKeywords = productsByKeywords.Union(
        //                from pptm in _productTagMappingRepository.Table
        //                join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
        //                where lp.LocaleKeyGroup == nameof(ProductTag) &&
        //                      lp.LocaleKey == nameof(ProductTag.Name) &&
        //                      lp.LocaleValue.Contains(keywords)
        //                select lp.EntityId);
        //            }
        //        }

        //        if (searchLocalizedValue)
        //        {
        //            productsByKeywords = productsByKeywords.Union(
        //                        from lp in _localizedPropertyRepository.Table
        //                        let checkName = lp.LocaleKey == nameof(Product.Name) &&
        //                                        lp.LocaleValue.Contains(keywords)
        //                        let checkShortDesc = searchDescriptions &&
        //                                        lp.LocaleKey == nameof(Product.ShortDescription) &&
        //                                        lp.LocaleValue.Contains(keywords)
        //                        let checkProductTags = searchProductTags &&
        //                                        lp.LocaleKeyGroup == nameof(ProductTag) &&
        //                                        lp.LocaleKey == nameof(ProductTag.Name) &&
        //                                        lp.LocaleValue.Contains(keywords)
        //                        where
        //                            (lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId) && (checkName || checkShortDesc) ||
        //                            checkProductTags

        //                        select lp.EntityId);
        //        }

        //        productsQuery =
        //            from p in productsQuery
        //            from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
        //            select p;
        //    }

        //    if (categoryIds is not null)
        //    {
        //        if (categoryIds.Contains(0))
        //            categoryIds.Remove(0);

        //        if (categoryIds.Any())
        //        {
        //            var productCategoryQuery =
        //                from pc in _productCategoryRepository.Table
        //                where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
        //                    categoryIds.Contains(pc.CategoryId)
        //                select pc;

        //            productsQuery =
        //                from p in productsQuery
        //                where productCategoryQuery.Any(pc => pc.ProductId == p.Id)
        //                select p;
        //        }
        //    }

        //    if (manufacturerIds is not null)
        //    {
        //        if (manufacturerIds.Contains(0))
        //            manufacturerIds.Remove(0);

        //        if (manufacturerIds.Any())
        //        {
        //            var productManufacturerQuery =
        //                from pm in _productManufacturerRepository.Table
        //                where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
        //                    manufacturerIds.Contains(pm.ManufacturerId)
        //                select pm;

        //            productsQuery =
        //                from p in productsQuery
        //                where productManufacturerQuery.Any(pm => pm.ProductId == p.Id)
        //                select p;
        //        }
        //    }

        //    if (productTagId > 0)
        //    {
        //        productsQuery =
        //            from p in productsQuery
        //            join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
        //            where ptm.ProductTagId == productTagId
        //            select p;
        //    }

        //    if (filteredSpecOptions?.Count > 0)
        //    {
        //        var specificationAttributeIds = filteredSpecOptions
        //            .Select(sao => sao.SpecificationAttributeId)
        //            .Distinct();

        //        foreach (var specificationAttributeId in specificationAttributeIds)
        //        {
        //            var optionIdsBySpecificationAttribute = filteredSpecOptions
        //                .Where(o => o.SpecificationAttributeId == specificationAttributeId)
        //                .Select(o => o.Id);

        //            var productSpecificationQuery =
        //                from psa in _productSpecificationAttributeRepository.Table
        //                where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
        //                select psa;

        //            productsQuery =
        //                from p in productsQuery
        //                where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
        //                select p;
        //        }
        //    }

        //    return await productsQuery.OrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
        //}

        public override async Task<IPagedList<Product>> SearchProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;

            // Misc.PrintCalculator Added Special ocmment to ignore on product search
            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published).Where(p => p.AdminComment != "noSearch");
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            if (!showHidden)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                    (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.Id == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType) &&
                    (showHidden || LinqToDB.Sql.Between(DateTime.UtcNow, p.AvailableStartDateTimeUtc ?? DateTime.MinValue, p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)) &&
                    (priceMin == null || p.Price >= priceMin) &&
                    (priceMax == null || p.Price <= priceMax)
                select p;

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);

                IQueryable<int> productsByKeywords;

                productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(keywords) ||
                            (searchDescriptions &&
                                (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                            (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                            (searchSku && p.Sku == keywords)
                        select p.Id;

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);
                }

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                        where pt.Name == keywords
                        select pptm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(ProductTag) &&
                              lp.LocaleKey == nameof(ProductTag.Name) &&
                              lp.LocaleValue.Contains(keywords)
                        select lp.EntityId);
                    }
                }

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                                from lp in _localizedPropertyRepository.Table
                                let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkShortDesc = searchDescriptions &&
                                                lp.LocaleKey == nameof(Product.ShortDescription) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkProductTags = searchProductTags &&
                                                lp.LocaleKeyGroup == nameof(ProductTag) &&
                                                lp.LocaleKey == nameof(ProductTag.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                where
                                    lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc) ||
                                    checkProductTags

                                select lp.EntityId);
                }

                productsQuery =
                    from p in productsQuery
                    from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            DisplayOrder = pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                            manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (productTagId > 0)
            {
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where ptm.ProductTagId == productTagId
                    select p;
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            return await productsQuery.OrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
        }
    }
}
