using CnAppForAzureDev.Entities;
using CnAppForAzureDev.Managers;
using CnAppForAzureDev.Repositories;
using CnAppForAzureDev.Repositories.Specifications;
using CnAppForAzureDev.ViewModels;
using Repositories.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.ViewServices
{
    public class TrolleyViewService : ITrolleyViewService
    {
        
        private readonly ICatalogItemRepository _catalogItemRepository;

        private readonly IndustryManager _manager;
        private readonly IRepository<Trolley> _trolleyRepository;

        public TrolleyViewService(IRepository<Trolley> trolleyRepository, ICatalogItemRepository catalogItemRepository, IndustryManager manager)
        {
            _trolleyRepository = trolleyRepository ?? throw new ArgumentNullException(nameof(trolleyRepository));
            _catalogItemRepository = catalogItemRepository ?? throw new ArgumentNullException(nameof(catalogItemRepository));
            _manager = manager;
        }

        public async Task<TrolleyViewModel> GetOrCreateTrolleyForOwnerAsync(string ownerId)
        {
            var listSpecification = new TrolleyForOwnerSpecification(ownerId);

            var trolley = (await _trolleyRepository.ListAsync(listSpecification)).FirstOrDefault();

            if (trolley == null)
            {
                trolley = await CreateTrolleyForOwnerAsync(ownerId);
            }

            return await CreateTrolleyViewModelFromTrolleyAsync(trolley);
        }

        private async Task<Trolley> CreateTrolleyForOwnerAsync(string ownerId)
        {
            var trolley = new Trolley { OwnerId = ownerId };

            await _trolleyRepository.AddAsync(trolley);
            return trolley;
        }

        private async Task<TrolleyViewModel> CreateTrolleyViewModelFromTrolleyAsync(Trolley trolley)
        {
            var trolleyViewModel = new TrolleyViewModel { Id = trolley.Id, OwnerId = trolley.OwnerId };

            foreach (var trolleyItem in trolley.Items)
            {
                var trolleyItemViewModel = new TrolleyItemViewModel
                {
                    Id = trolleyItem.Id,
                    CatalogItemId = trolleyItem.CatalogItemId,
                    Quantity = trolleyItem.Quantity
                };

                var catalogItem = await _catalogItemRepository.GetAsync(trolleyItem.CatalogItemId);

                //var newItem = _manager.GetIndustry().ConvertItem(catalogItem);

                trolleyItemViewModel.ProductId = catalogItem.ProductId;
                trolleyItemViewModel.ProductName = catalogItem.ProductName;
                trolleyItemViewModel.ProductPictureUrl = catalogItem.ProductPictureUrl;
                trolleyItemViewModel.ProductAllergyInfo = catalogItem.ProductAllergyInfo;
                trolleyViewModel.Items.Add(trolleyItemViewModel);
            }

            return trolleyViewModel;
        }
    }
}
