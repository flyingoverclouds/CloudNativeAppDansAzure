using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Entities
{
    public interface IEntityBase
    {
        public string Id { get; set; }
        public string OwnerId { get; set; }
    }
}
