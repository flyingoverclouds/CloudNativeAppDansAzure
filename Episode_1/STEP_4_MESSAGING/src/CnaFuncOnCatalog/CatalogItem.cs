using Repositories.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnaFuncOnCatalog
{
    public class CatalogItem : ICatalogItem
    {
        public CatalogItem()
        {
        }

        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductPictureUrl { get; set; }

        public string ProductAllergyInfo { get; set; }

        public string OwnerId { get; set; }
        public string Id { get; set; }
    }
}
