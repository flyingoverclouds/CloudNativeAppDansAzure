using System;
using System.Collections.Generic;
using System.Text;

namespace ProductManagementFunction
{
    public class ProductEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        
        public string CatalogItemsServiceUrl { get; set; }
        public Int64 Id { get; set; }
        public Int64 OwnerId { get; set; }
        public string ProductAllergyInfo { get; set; }
        public Int64 ProductId { get; set; }
        public string ProductName { get; set; }

        public string ProductPictureUrl { get; set; }

    }
}
