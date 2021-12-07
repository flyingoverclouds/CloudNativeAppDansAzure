using CnAppForAzureDev.Entities;
using CnAppForAzureDev.Repositories;
using Repositories.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Managers
{
    public class PantryManager : IPantryManager
    {
        private readonly IRepository<Pantry> _pantryRepository;

        public PantryManager(IRepository<Pantry> pantryRepository)
        {
            _pantryRepository = pantryRepository ?? throw new ArgumentNullException(nameof(pantryRepository));
        }

        public async Task AddToPantryAsync(string id, string catalogItemId)
        {
            var pantry = await _pantryRepository.GetAsync(id);
            pantry.AddItem(catalogItemId);
            await _pantryRepository.UpdateAsync(pantry);
        }

        public async Task RemoveFromPantryAsync(string id, string itemId)
        {
            var pantry = await _pantryRepository.GetAsync(id);
            pantry.RemoveItem(itemId);
            await _pantryRepository.UpdateAsync(pantry);
        }
    }
}
