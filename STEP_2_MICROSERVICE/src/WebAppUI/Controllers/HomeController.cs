using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CnAppForAzureDev.Models;
using CnAppForAzureDev.Managers;
using CnAppForAzureDev.ViewServices;
using CnAppForAzureDev.ViewModels;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CnAppForAzureDev.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICatalogItemViewService _catalogItemViewService;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, 
                              ICatalogItemViewService catalogItemViewService, 
                              IConfiguration configuration,
                              IndustryManager manager):base(manager)
        {
            _logger = logger;
            _catalogItemViewService = catalogItemViewService;
            _configuration = configuration;
        }
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
        public async Task<IActionResult> Index()
        {
            
            var items = await _catalogItemViewService.GetCatalogItemsAsync(User);
            int MaxItemsOnHomePage = Convert.ToInt32(_configuration["MaxItemsOnHomePage"]);
            var viewModel = new CatalogItemIndexViewModel { Items = items.Take(MaxItemsOnHomePage) };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
