using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
namespace CnAppForAzureDev.Entities
{
    //TODO:DEMO TABLEENTITY
    public class CatalogItem : TableEntity, IEntityBase
    {
        public CatalogItem()
        {
        }
        public CatalogItem(string partionkey, string rowkey)
        {
            this.PartitionKey = partionkey;
            this.RowKey = rowkey;
        }
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductPictureUrl { get; set; }

        public string ProductAllergyInfo { get; set; }

        public string OwnerId { get; set; }
        public string Id { get; set; }
    }
}
