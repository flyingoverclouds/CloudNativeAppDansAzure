namespace CnAppForAzureDev.Entities
{
    public class PantryItem : IEntityBase
    {
        public string CatalogItemId { get; set; }
        public string OwnerId { get; set; }
        public string Id { get; set; }
    }
}
