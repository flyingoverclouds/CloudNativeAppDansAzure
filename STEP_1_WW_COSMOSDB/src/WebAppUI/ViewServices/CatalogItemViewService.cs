using CnAppForAzureDev.Entities;
using CnAppForAzureDev.Managers;
using CnAppForAzureDev.Repositories;
using CnAppForAzureDev.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CnAppForAzureDev.ViewServices
{
    public class CatalogItemViewService : ICatalogItemViewService
    {
        private readonly ICatalogItemRepository _catalogItemRepository;
        private readonly IndustryManager _manager;

        public CatalogItemViewService(ICatalogItemRepository catalogItemRepository, IndustryManager manager)
        {
            _catalogItemRepository = catalogItemRepository ?? throw new ArgumentNullException(nameof(catalogItemRepository));
            _manager = manager;
        }
        
        //Vue des Elements du catalogue
        public async Task<IEnumerable<CatalogItemViewModel>> GetCatalogItemsAsync(ClaimsPrincipal user)
        {
            var catalogItems = await _catalogItemRepository.ListAsync();

            return catalogItems.Select(catalogItem =>
            {

                return new CatalogItemViewModel
                {
                    Id = catalogItem.Id,
                    OwnerId = catalogItem.OwnerId,
                    ProductId = catalogItem.ProductId,
                    ProductName = catalogItem.ProductName,
                    ProductPictureUrl = catalogItem.ProductPictureUrl,
                    ProductAllergyInfo = catalogItem.ProductAllergyInfo
                };
            });

        }
    }
}
