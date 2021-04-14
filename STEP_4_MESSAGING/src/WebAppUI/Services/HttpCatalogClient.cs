using CnAppForAzureDev.Entities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Services
{
    public class HttpCatalogClient : IHttpCatalogClient
    {
        private readonly HttpClient _httpClient;
        private IConfiguration _configuration;
        public HttpCatalogClient(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_configuration["CatalogItemBaseAddressUrl"]);
            _httpClient.DefaultRequestHeaders.Add("Accept", "Application/json");
        }
        
        public async Task<CatalogItem> AddAsync(CatalogItem entity)
        {
            CatalogItem newItem = null;
            string url = $"{_httpClient.BaseAddress}/add";
            try
            {
                string jsonData = JsonConvert.SerializeObject(entity);
                StringContent stringContent = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                
                // L'instruction ici est synchrone et bloque l'interface graphique
                var response = await _httpClient.PostAsync(url, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var newJsonData = await response.Content.ReadAsStringAsync();
                    newItem = JsonConvert.DeserializeObject<CatalogItem>(newJsonData);
                }
            }
            catch (HttpRequestException httpRequestEx)
            {
                // swallow the error 
                // TODO : Log the error Application Insight
                var message = httpRequestEx.Message;
            }
            return newItem;
        }

        public async Task<CatalogItem> GetItemAsync(string id)
        {
            string url = $"{_httpClient.BaseAddress}?id={id}";
            var response = await _httpClient.GetAsync(url);
            CatalogItem item = null;
            if (response.IsSuccessStatusCode)
            {
                var newJsonData = await response.Content.ReadAsStringAsync();
                item = JsonConvert.DeserializeObject<CatalogItem>(newJsonData);
            }
            return item;
        }

        public async Task<List<CatalogItem>> ListAsync()
        {
            string items = null;
            string url = $"{_httpClient.BaseAddress}/list";
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
    }
}
