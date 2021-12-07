using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CnaCatalogService.Entities;
using CnaCatalogService.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace CnaCatalogService.Controllers
{

    // TODO : Add Authentication
    [Produces("application/json")]
    [Route("api/catalog/items")]
    [ApiController]
    public class CatalogItemsController : ControllerBase
    {
        IConfiguration _configuration;
        CatalogItemRepository _catalogItemRepository;
        public CatalogItemsController(IConfiguration configuration,
                                      CatalogItemRepository catalogItemRepository)
        {
            _configuration = configuration;
            _catalogItemRepository = catalogItemRepository;
        }
        [HttpGet("list", Order = 1)]
        public async Task<IEnumerable<CatalogItem>> ListAsync()
        {
            return await _catalogItemRepository.ListAsync(); ;
        }

        // Logiquement n'a plus de raison d'être remplacée par ServiceBus
        [HttpPost("add", Order = 2)]
        [ApiExplorerSettings(IgnoreApi =true)]
        public async Task<CatalogItem> AddAsync([FromBody] CatalogItem item)
        {

            return await _catalogItemRepository.AddAsync(item);
        }

        [HttpGet()]
        public async Task<CatalogItem>GetItemAsync([FromQuery]string id)
        {
            
            return await _catalogItemRepository.GetAsync(id);
        }
        
    }
}