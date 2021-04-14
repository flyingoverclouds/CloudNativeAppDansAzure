using System.Threading.Tasks;

namespace CnAppForAzureDev.Managers
{
    public interface IPantryManager
    {
        Task AddToPantryAsync(string id, string catalogItemId);

        Task RemoveFromPantryAsync(string id, string itemId);
    }
}
