﻿using CnAppForAzureDev.Extensions;
using CnAppForAzureDev.Managers;
using CnAppForAzureDev.ViewModels;
using CnAppForAzureDev.ViewServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Controllers
{
    //[Authorize(Policy = Constants.AuthorizationPolicies.AccessTrolley)]
    public class TrolleyController : BaseController
    {
        private readonly ITrolleyManager _trolleyManager;
        private readonly ITrolleyViewService _trolleyViewService;

        public TrolleyController(ITrolleyViewService trolleyViewService, ITrolleyManager trolleyManager, IndustryManager industryManager) :
            base(industryManager)
        {
            _trolleyViewService = trolleyViewService ?? throw new ArgumentNullException(nameof(trolleyViewService));
            _trolleyManager = trolleyManager ?? throw new ArgumentNullException(nameof(trolleyManager));
        }

        [HttpPost]
        public async Task<IActionResult> AddFromCatalogToTrolley(CatalogItemViewModel catalogItemViewModel)
        {
            var trolleyViewModel = await GetTrolleyViewModelAsync();
            await _trolleyManager.AddToTrolleyAsync(trolleyViewModel.Id, catalogItemViewModel.Id, 1);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddFromPantryToTrolley(PantryItemViewModel pantryItemViewModel)
        {
            var trolleyViewModel = await GetTrolleyViewModelAsync();
            await _trolleyManager.AddToTrolleyAsync(trolleyViewModel.Id, pantryItemViewModel.CatalogItemId, 1);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var userIsInBusinessCustomerStockerRole = User.IsInBusinessCustomerStockerRole();

            var viewModel = new TrolleyIndexViewModel
            {
                AspController = userIsInBusinessCustomerStockerRole ? "Pantry" : "CatalogItem",
                Trolley = await GetTrolleyViewModelAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromTrolley(string id)
        {
            var trolleyViewModel = await GetTrolleyViewModelAsync();
            await _trolleyManager.RemoveFromTrolleyAsync(trolleyViewModel.Id, id);
            return RedirectToAction("Index");
        }

        private async Task<TrolleyViewModel> GetTrolleyViewModelAsync()
        {
            //TODO : Uncomment when authentication is enabled
            //var userId = User.FindFirstValue(Constants.ClaimTypes.ObjectIdentifier);
            var userId = Constants.ClaimTypes.MockUser;
            return await _trolleyViewService.GetOrCreateTrolleyForOwnerAsync(userId);
        }
    }
}
