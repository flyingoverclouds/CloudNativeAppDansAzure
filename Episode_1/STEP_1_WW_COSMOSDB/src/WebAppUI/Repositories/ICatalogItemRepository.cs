using CnAppForAzureDev.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Repositories
{
    public interface ICatalogItemRepository : IRepository<CatalogItem>
    {
        Task<CatalogItem> AddAsync(string ownerId, string productName, string productPictureUrl);
    }
}
