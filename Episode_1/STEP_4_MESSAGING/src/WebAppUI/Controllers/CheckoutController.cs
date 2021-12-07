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
    //[Authorize(Policy = Constants.AuthorizationPolicies.AccessCheckout)]
    public class CheckoutController : BaseController
    {
        private readonly ITrolleyManager _trolleyManager;
        private readonly ITrolleyViewService _trolleyViewService;

        public CheckoutController(ITrolleyViewService trolleyViewService, ITrolleyManager trolleyManager, IndustryManager industryManager) :
            base(industryManager)
        {
            _trolleyViewService = trolleyViewService ?? throw new ArgumentNullException(nameof(trolleyViewService));
            _trolleyManager = trolleyManager ?? throw new ArgumentNullException(nameof(trolleyManager));
        }

        public async Task<IActionResult> Index()
        {
            var userIsInBusinessCustomerStockerRole = User.IsInBusinessCustomerStockerRole();

            var userId = User.FindFirstValue(Constants.ClaimTypes.ObjectIdentifier);
            var trolleyViewModel = await _trolleyViewService.GetOrCreateTrolleyForOwnerAsync(userId);

            if (trolleyViewModel == null || !trolleyViewModel.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new TrolleyIndexViewModel
            {
                AspController = userIsInBusinessCustomerStockerRole ? "Pantry" : "CatalogItem",
                Trolley = trolleyViewModel
            };

            await _trolleyManager.RemoveAllItemFromTrolleyAsync(trolleyViewModel.Id);

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Finish()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
