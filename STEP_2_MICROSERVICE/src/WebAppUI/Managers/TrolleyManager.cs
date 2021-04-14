using CnAppForAzureDev.Entities;
using CnAppForAzureDev.Repositories;
using Repositories.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Managers
{
    public class TrolleyManager : ITrolleyManager
    {
        private readonly IRepository<Trolley> _trolleyRepository;

        public TrolleyManager(IRepository<Trolley> trolleyRepository)
        {
            _trolleyRepository = trolleyRepository ?? throw new ArgumentNullException(nameof(trolleyRepository));
        }

        public async Task AddToTrolleyAsync(string id, string catalogItemId, int quantity)
        {
            var trolley = await _trolleyRepository.GetAsync(id);
            trolley.AddItem(catalogItemId, quantity);
            await _trolleyRepository.UpdateAsync(trolley);
        }

        public async Task RemoveFromTrolleyAsync(string id, string itemId)
        {
            var trolley = await _trolleyRepository.GetAsync(id);
            trolley.RemoveItem(itemId);
            await _trolleyRepository.UpdateAsync(trolley);
        }

        public async Task RemoveAllItemFromTrolleyAsync(string id)
        {
            var trolley = await _trolleyRepository.GetAsync(id);

            var trolleyItemIds = trolley.Items.Select(x => x.Id).ToList();

            foreach (var itemId in trolleyItemIds)
            {
                trolley.RemoveItem(itemId);
            }

            await _trolleyRepository.UpdateAsync(trolley);
        }
    }
}
