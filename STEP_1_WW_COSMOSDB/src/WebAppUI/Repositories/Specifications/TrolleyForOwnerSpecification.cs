using CnAppForAzureDev.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Repositories.Specifications
{
    public class TrolleyForOwnerSpecification : SpecificationBase<Trolley>
    {
        public TrolleyForOwnerSpecification(string ownerId)
            : base(trolley => trolley.OwnerId == ownerId)
        {
        }
    }
}
