using CnAppForAzureDev.Extensions;
using CnAppForAzureDev.Managers;
using CnAppForAzureDev.ViewModels;
using CnAppForAzureDev.ViewServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Controllers
{
    //[Authorize(Policy = Constants.AuthorizationPolicies.AccessCatalog)]
    public class CatalogItemController : BaseController
    {
        private readonly ICatalogItemManager _catalogItemManager;
        private readonly ICatalogItemViewService _catalogItemViewService;
        private readonly IStringLocalizer<CatalogItemController> _localizer;

        public CatalogItemController(
            ICatalogItemViewService catalogItemViewService,
            ICatalogItemManager catalogItemManager,
            IStringLocalizer<CatalogItemController> localizer,
            IndustryManager industryManager)
            : base(industryManager)
        {
            _catalogItemViewService = catalogItemViewService ?? throw new ArgumentNullException(nameof(catalogItemViewService));
            _catalogItemManager = catalogItemManager ?? throw new ArgumentNullException(nameof(catalogItemManager));
            _localizer = localizer;
        }

        //[Authorize(Policy = Constants.AuthorizationPolicies.AddToCatalog)]
        [HttpPost]
        public async Task<IActionResult> AddToCatalog(CatalogItemViewModel catalogItemViewModel)
        {
            //var ownerId = User.FindFirstValue(Constants.ClaimTypes.OwnerIdentifier);
            var ownerId = "1";
            
            await _catalogItemManager.AddToCatalogAsync(ownerId, catalogItemViewModel.ProductName, catalogItemViewModel.ProductPictureUrl);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            
            var userIsInBusinessCustomerManagerRole = User.IsInBusinessCustomerManagerRole();
            var userIsInEmployeeRole = User.IsInEmployeeRole();
            var userIsInPartnerRole = User.IsInPartnerRole();
            var viewModel = new CatalogItemIndexViewModel
            {
                FormAspArea = string.Empty,
                FormAspController =
                    userIsInPartnerRole || userIsInEmployeeRole ? "CatalogItem" : userIsInBusinessCustomerManagerRole ? "Pantry" : "Trolley",
                FormAspAction =
                    userIsInPartnerRole || userIsInEmployeeRole ? "RemoveFromCatalog" :
                    userIsInBusinessCustomerManagerRole ? "AddToPantry" : "AddFromCatalogToTrolley",
                FormSubmitButtonIconCssClass =
                    userIsInPartnerRole || userIsInEmployeeRole ? "fa fa-minus" :
                    userIsInBusinessCustomerManagerRole ? "fa fa-plus" : "fa fa-shopping-cart",
                FormSubmitButtonText =
                    userIsInPartnerRole || userIsInEmployeeRole ? _localizer["Remove from catalog"] :
                    userIsInBusinessCustomerManagerRole ? _localizer["Add to pantry"] : _localizer["Add to cart"],
                Items = await _catalogItemViewService.GetCatalogItemsAsync(User)
            };

            if (userIsInPartnerRole)
            {
                return View("PartnerLanding", viewModel);
            }


            return View(viewModel);
        }

        //[Authorize(Policy = Constants.AuthorizationPolicies.RemoveFromCatalog)]
        [HttpPost]
        public async Task<IActionResult> RemoveFromCatalog(string id)
        {
            await _catalogItemManager.RemoveFromCatalogAsync(id);
            return RedirectToAction("Index");
        }
    }
}