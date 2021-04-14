using CnAppForAzureDev.ViewModels;
using System.Threading.Tasks;

namespace CnAppForAzureDev.ViewServices
{
    public interface IPantryViewService
    {
        Task<PantryViewModel> GetOrCreatePantryForOwnerAsync(string ownerId);
    }
}
