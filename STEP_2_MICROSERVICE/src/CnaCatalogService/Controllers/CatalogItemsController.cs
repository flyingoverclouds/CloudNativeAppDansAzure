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
    // TODO : Add Swagger
    // TODO : Add Authentication
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogItemsController : ControllerBase
    {
        IConfiguration _configuration;
        ICatalogItemRepository _catalogItemRepository;
        public CatalogItemsController(IConfiguration configuration, 
                                    ICatalogItemRepository catalogItemRepository)
        {
            _configuration = configuration;
            _catalogItemRepository = catalogItemRepository;
        }
        [HttpGet()]
        public async Task<IEnumerable<CatalogItem>> ListAsync()
        {            
            return await _catalogItemRepository.ListAsync(); ;
        }
        [HttpPost]
        public async Task<CatalogItem> AddAsync([FromBody] CatalogItem item)
        {
            

            return await _catalogItemRepository.AddAsync(item);
        }
        
    }
}