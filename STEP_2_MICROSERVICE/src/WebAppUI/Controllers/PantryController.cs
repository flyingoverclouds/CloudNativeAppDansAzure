using CnAppForAzureDev.Extensions;
using CnAppForAzureDev.Managers;
using CnAppForAzureDev.ViewModels;
using CnAppForAzureDev.ViewServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Controllers
{
    //[Authorize(Policy = Constants.AuthorizationPolicies.AccessPantry)]
    public class PantryController : BaseController
    {
        private readonly IPantryManager _pantryManager;
        private readonly IPantryViewService _pantryViewService;
        private readonly IStringLocalizer<PantryController> _localizer;
        public PantryController(IPantryViewService pantryViewService, 
            IPantryManager pantryManager, IStringLocalizer<PantryController> localizer,
            IndustryManager industryManager) :
            base(industryManager)
        {
            _pantryViewService = pantryViewService ?? throw new ArgumentNullException(nameof(pantryViewService));
            _pantryManager = pantryManager ?? throw new ArgumentNullException(nameof(pantryManager));
            _localizer = localizer;
        }

        //[Authorize(Policy = Constants.AuthorizationPolicies.AddToPantry)]
        [HttpPost]
        public async Task<IActionResult> AddToPantry(CatalogItemViewModel catalogItemViewModel)
        {
            var pantryViewModel = await GetPantryViewModelAsync();
            await _pantryManager.AddToPantryAsync(pantryViewModel.Id, catalogItemViewModel.Id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var userIsInBusinessAdministratorRole = User.IsInBusinessCustomerManagerRole();

            var viewModel = new PantryIndexViewModel
            {
                FormAspArea = "",
                FormAspController = userIsInBusinessAdministratorRole ? "Pantry" : "Trolley",
                FormAspAction = userIsInBusinessAdministratorRole ? "RemoveFromPantry" : "AddFromPantryToTrolley",
                FormSubmitButtonIconCssClass = userIsInBusinessAdministratorRole ? "fa fa-minus" : "fa fa-shopping-cart",
                FormSubmitButtonText = userIsInBusinessAdministratorRole ? _localizer["Remove from pantry"] : _localizer["Add to cart"],
                Pantry = await GetPantryViewModelAsync()
            };

            return View(viewModel);
        }

        //[Authorize(Policy = Constants.AuthorizationPolicies.RemoveFromPantry)]
        [HttpPost]
        public async Task<IActionResult> RemoveFromPantry(string id)
        {
            var pantryViewModel = await GetPantryViewModelAsync();
            await _pantryManager.RemoveFromPantryAsync(pantryViewModel.Id, id);
            return RedirectToAction("Index");
        }

        private async Task<PantryViewModel> GetPantryViewModelAsync()
        {

            //TODO : Uncomment when authentication is enabled
            //var organizationId = User.FindFirstValue(Constants.ClaimTypes.TenantIdentifier);
            var organizationId = Constants.ClaimTypes.MockUser;

            return await _pantryViewService.GetOrCreatePantryForOwnerAsync(organizationId);
        }
    }
}
