using Bus.Services.Helper;
using CnAppForAzureDev.Entities;
using CnAppForAzureDev.Extensions;
using CnAppForAzureDev.Managers;
using CnAppForAzureDev.Repositories;
using CnAppForAzureDev.ViewModels;
using CnAppForAzureDev.ViewServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Controllers
{
    //[Authorize(Policy = Constants.AuthorizationPolicies.AccessCatalog)]
    public class CatalogItemController : BaseController
    {
        private readonly ICatalogItemManager _catalogItemManager;
        private readonly ICatalogItemViewService _catalogItemViewService;
        private readonly IStringLocalizer<CatalogItemController> _localizer;
        private IConfiguration _configuration;        
        private ICatalogItemRepository _repository;

        public CatalogItemController(IConfiguration configuration,
            ICatalogItemViewService catalogItemViewService,
            ICatalogItemManager catalogItemManager,
            IStringLocalizer<CatalogItemController> localizer,
            IndustryManager industryManager,
            CnaServicesBus cnaServicesBus,
            ICatalogItemRepository catalogItemRepository)
            : base(industryManager)
        {
            _repository = catalogItemRepository;
            _configuration = configuration;
            _catalogItemViewService = catalogItemViewService ?? throw new ArgumentNullException(nameof(catalogItemViewService));
            _catalogItemManager = catalogItemManager ?? throw new ArgumentNullException(nameof(catalogItemManager));
            _localizer = localizer;
                        
        }
             
        [HttpPost]
        public async Task<IActionResult> AddToCatalog(CatalogItemViewModel catalogItemViewModel)
        {
            //var ownerId = User.FindFirstValue(Constants.ClaimTypes.OwnerIdentifier);
            var ownerId = "1";
            
            await _catalogItemManager.AddToCatalogAsync(ownerId, catalogItemViewModel.ProductName, catalogItemViewModel.ProductPictureUrl);
            return RedirectToAction("Index");
        }
        
        private async Task<IActionResult> SynchronizedIndexAsync(int id=0)
        {
            var userIsInBusinessCustomerManagerRole = User.IsInBusinessCustomerManagerRole();
            var userIsInEmployeeRole = User.IsInEmployeeRole();
            var userIsInPartnerRole = User.IsInPartnerRole();
            IEnumerable<CatalogItemViewModel> Items = null;
            if (userIsInPartnerRole )
            {
                // En attendant que je trouve une solution pour notifier l'interface utilisateur
                // récupère la liste des items dans un cache local
                Items = await _catalogItemViewService.GetCatalogItemsWhenNewItemWasAddedAsync(User);
            }
            else
            {
                Items = await _catalogItemViewService.GetCatalogItemsAsync(User);
            }
            
            
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
                Items = Items
            };
           
            if (userIsInPartnerRole)
            {
                return View("PartnerLanding", viewModel);
            }

            return View(viewModel);
        }
        //[HttpGet()]
        //[Route("[Controller]/refresh")]
        //public async Task<IActionResult> Refresh()
        //{
        //    var controller = new CatalogItemController(_configuration, _catalogItemViewService, _catalogItemManager, _localizer, _industryManager, _cnaServicesBus, _repository);
        //    controller.ControllerContext=new ControllerContext(this.ControllerContext);
        //    return await controller.Index();
        //}
        public async Task<IActionResult> Index()
        {
            
            return await SynchronizedIndexAsync();

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