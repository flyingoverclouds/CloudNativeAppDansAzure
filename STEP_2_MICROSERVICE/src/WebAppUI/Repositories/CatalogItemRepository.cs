using CnAppForAzureDev.Entities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Repositories
{
    /// <summary>
    /// Etape 1 Utilisation de l'API CosmosDb pour accèder à une table d'un compte de stockage
    /// </summary>
    /// <remarks> Vous devez avoir aupréalablement crée :
    /// Un compte de stockage
    /// </remarks>
    public class CatalogItemRepository : ICatalogItemRepository, IRepository<CatalogItem>
    {
        
        private List<CatalogItem> _catalogItems;
        private IConfiguration _configuration;
        static HttpClient _httpClient;
        /// <summary>
        /// CCTOR
        /// </summary>
        /// <param name="configuration"></param>
        public CatalogItemRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            
        }
        
       
        public Task<CatalogItem> AddAsync(CatalogItem entity)
        {
            throw new NotImplementedException();
        }
        
       
        /// <summary>
        /// Ajoute un élèment dans le catalogue
        /// </summary>
        /// <param name="ownerId">Propriétaire du produit</param>
        /// <param name="productName">Nom du produit</param>
        /// <param name="productPictureUrl">ULR de l'image du produit</param>
        /// <returns></returns>
        public async Task<CatalogItem> AddAsync(string ownerId, string productName, string productPictureUrl)
        {
            //TODO:DEMO AddAsync
            CatalogItem newItem = new CatalogItem
            {
                OwnerId = ownerId,
                ProductName = productName,
                ProductPictureUrl = productPictureUrl
            };
            newItem = await AddRemoteAsync(newItem, _configuration["CatalogItemsServiceUrl"]);
            _catalogItems.Add(newItem);
            return newItem;
        }
        private async Task<CatalogItem> AddRemoteAsync(CatalogItem item, string url)
        {
            //TODO:DEMO ADDREMOTE ASYNC
            CatalogItem newItem = null;
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
            }
            try
            {
                string jsonData = JsonConvert.SerializeObject(item);
                StringContent stringContent = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var newJsonData = await response.Content.ReadAsStringAsync();
                    newItem = JsonConvert.DeserializeObject<CatalogItem>(newJsonData);
                }
            }
            catch (HttpRequestException)
            {
                // swallow the error 
                // TODO : Log the error Application Insight
            }
            return newItem;
        }
        public Task<CatalogItem> GetAsync(string id)
        {
            var q = _catalogItems.Where(item => item.Id == id).Select(i => i).FirstOrDefault();
            return Task.FromResult(q);
        }

       
        public async Task<List<CatalogItem>> ListAsync()
        {            
            // HACK - deactivate UI data caching to test catalog update
            //if (_catalogItems==null)
            {
                string CatalogItemsServiceUrl = _configuration["CatalogItemsServiceUrl"];
                // Invoque le micro-service catalogue via Http
                _catalogItems = await ListRemoteAsync(CatalogItemsServiceUrl);
            }            
            
            return _catalogItems;
        }
        private async Task<List<CatalogItem>> ListRemoteAsync(string url)
        {

            string items = null;
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
            }
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    items = await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException)
            {
                // swallow the error 
                // TODO : Log the error Application Insight
            }

            if (items != null)
            {
                return JsonConvert.DeserializeObject<List<CatalogItem>>(items);
            }
            return null;
        }


        public Task RemoveAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(CatalogItem entity)
        {
            throw new NotImplementedException();
        }

        Task<List<CatalogItem>> IRepository<CatalogItem>.ListAsync(ISpecification<CatalogItem> specification)
        {
            throw new NotImplementedException();
        }
    }
}
