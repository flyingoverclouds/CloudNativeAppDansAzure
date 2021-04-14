using CnAppForAzureDev.Entities;

namespace CnAppForAzureDev.Repositories.Specifications
{
    public class PantryForOwnerSpecification : SpecificationBase<Pantry>
    {
        public PantryForOwnerSpecification(string ownerId)
            : base(pantry => pantry.OwnerId == ownerId)
        {
        }
    }
}
