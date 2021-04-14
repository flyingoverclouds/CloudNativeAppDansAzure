using CnAppForAzureDev.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CnAppForAzureDev.ViewServices
{
    public interface ICatalogItemViewService
    {
        Task<IEnumerable<CatalogItemViewModel>> GetCatalogItemsAsync(ClaimsPrincipal user);
        Task<IEnumerable<CatalogItemViewModel>> GetCatalogItemsWhenNewItemWasAddedAsync(ClaimsPrincipal user);
    }
}
