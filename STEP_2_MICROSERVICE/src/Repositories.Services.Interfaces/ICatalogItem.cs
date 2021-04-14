using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories.Services.Interfaces
{
    public interface ICatalogItem : IEntityBase
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductPictureUrl { get; set; }

        public string ProductAllergyInfo { get; set; }
        public string PictureName { get; set; }
       
    }
}
