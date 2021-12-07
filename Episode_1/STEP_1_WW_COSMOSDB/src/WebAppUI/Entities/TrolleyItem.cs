namespace CnAppForAzureDev.Entities
{
    public class TrolleyItem : IEntityBase
    {
        public string CatalogItemId { get; set; }

        public int Quantity { get; set; }
        public string OwnerId { get; set; }
        public string Id { get; set; }

    }
}