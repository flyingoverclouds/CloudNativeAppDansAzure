using CnAppForAzureDev.ViewModels;
using System.Threading.Tasks;

namespace CnAppForAzureDev.ViewServices
{
    public interface ITrolleyViewService
    {
        Task<TrolleyViewModel> GetOrCreateTrolleyForOwnerAsync(string ownerId);
    }
}
