using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.ViewModels
{
    public class CatalogItemViewModel
    {
        public string Id { get; set; }

        public string OwnerId { get; set; }

        public string ProductId { get; set; }

        public string ProductPictureUrl { get; set; }

        public string ProductName { get; set; }

        public string ProductAllergyInfo { get; set; }
    }
}
