using Bus.Services.Helper;
using CnAppForAzureDev.Entities;
using CnAppForAzureDev.Managers;
using CnAppForAzureDev.Repositories;
using CnAppForAzureDev.ViewModels;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CnAppForAzureDev.ViewServices
{
    public class CatalogItemViewService : ICatalogItemViewService
    {
        private readonly ICatalogItemRepository _catalogItemRepository;
        private readonly IndustryManager _manager;
        
        public CatalogItemViewService(IConfiguration configuration, ICatalogItemRepository catalogItemRepository, 
                                      IndustryManager manager)
        {
            _catalogItemRepository = catalogItemRepository ?? throw new ArgumentNullException(nameof(catalogItemRepository));
            _manager = manager;
            
        }

        
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

        public async Task<IEnumerable<CatalogItemViewModel>> GetCatalogItemsWhenNewItemWasAddedAsync(ClaimsPrincipal user)
        {
            var catalogItems = await _catalogItemRepository.ListWhenNewItemWasAdded();

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
