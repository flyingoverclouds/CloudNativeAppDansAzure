using CnAppForAzureDev.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Services
{
    public interface IHttpCatalogClient
    {
        Task<List<CatalogItem>> ListAsync();
        Task<CatalogItem> AddAsync(CatalogItem entity);
        Task<CatalogItem> GetItemAsync(string id);
    }
}
