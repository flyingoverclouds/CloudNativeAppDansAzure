using System.Threading.Tasks;

namespace CnAppForAzureDev.Managers
{
    public interface ICatalogItemManager
    {
        Task AddToCatalogAsync(string ownerId, string productName, string productPictureUrl);

        Task RemoveFromCatalogAsync(string id);
    }
}
