using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using CnaCatalogService.Entities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Repositories.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CnaCatalogService.Repositories
{
    /// <summary>
    /// Etape 1 Utilisation de l'API CosmosDb pour accèder à une table d'un compte de stockage
    /// Etape 2 Découpage de l'application en micro-service
    /// </summary>
    /// <remarks> Vous devez avoir aupréalablement crée 
    /// Un compte de stockage
    /// </remarks>
    public class CatalogItemRepository : ICatalogItemRepository, IRepository<CatalogItem>
    {

        private readonly CloudTable _catalog;
        private readonly BlobContainerClient _blobContainerClient;

        private readonly StorageSharedKeyCredential _storageSharedKeyCredential;

        private string _containerName;
        private List<CatalogItem> _catalogItems;
        private IConfiguration _configuration;
        
        private readonly IHttpClientFactory _httpClientFactory;
        /// <summary>
        /// CCTOR
        /// </summary>
        /// <param name="configuration"></param>
        public CatalogItemRepository(IConfiguration configuration, 
                                     CloudTable catalog,
                                     BlobServiceClient blobServiceClient,
                                     StorageSharedKeyCredential storageSharedKeyCredential,
                                     IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _catalog = catalog;
          
            _containerName = configuration["ContainerName"];

            // Instancie les informations de connexion au Blob
            _storageSharedKeyCredential = storageSharedKeyCredential;
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        }
        private string GetFileNameFromUrl(string productPictureUrl)
        {
            var IndexOf = productPictureUrl.LastIndexOf('/') + 1;
            var BlobName = productPictureUrl.Remove(0, IndexOf);
            return BlobName;
        }
        private async Task<Stream> DownloadPictureFromUrlAsync(string productPictureUrl)
        {
            
            Stream stream = null;
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(productPictureUrl);

                if (response.IsSuccessStatusCode)
                {
                    stream = await response.Content.ReadAsStreamAsync();

                }
            }
            catch (HttpRequestException)
            {

                // swallow the error 
                // TODO : Log the error Application Insight
            }

            return stream;
        }
        /// <summary>
        /// Télécharger une image à partir d'une URL et 
        /// la sauvegarder dans le Blob
        /// </summary>
        /// <param name="productPictureUrl">ULR de l'image à télécharger</param>
        /// <returns>Le nom du fichier téléchargé</returns>
        public async Task<string> UploadPictureToBlobAsync(string productPictureUrl)
        {
            var stream = await DownloadPictureFromUrlAsync(productPictureUrl);
            if (stream != null)
            {                
                var BlobName = GetFileNameFromUrl(productPictureUrl);
                await _blobContainerClient.UploadBlobAsync(BlobName, stream);
                return BlobName;
            }
            return null;
        }
        /// <summary>
        /// Création d'une signature d'accès partagé
        /// </summary>
        /// <remarks>Cette signature permet de limiter les accès au compte de stockage</remarks>
        /// <see cref="https://docs.microsoft.com/fr-fr/azure/storage/common/storage-sas-overview"/>
        /// <param name="containername">Nom du container</param>
        /// <param name="blobname">nom du blob</param>
        /// <param name="startson">début de validité de la clé SAS</param>
        /// <param name="expireson">fin de validité de la clé SAS</param>
        /// <returns></returns>
        private string CreateSasKey(string containername, string blobname, DateTimeOffset startson, DateTimeOffset expireson)
        {
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containername,
                BlobName = blobname,
                Resource = "b",
            };
            sasBuilder.StartsOn = startson;
            sasBuilder.ExpiresOn = expireson;
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            string sasToken = sasBuilder.ToSasQueryParameters(_storageSharedKeyCredential).ToString();
            return $"{_blobContainerClient.GetBlockBlobClient(blobname).Uri}?{sasToken}";
        }
        public Task<CatalogItem> AddAsync(CatalogItem entity)
        {
            return AddAsync(entity.OwnerId, entity.ProductName, entity.ProductPictureUrl);
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

            string sasKey = null;
            // Pousse l'image dans le Blob 
            var NameFileWithExtention = await UploadPictureToBlobAsync(productPictureUrl);
            if (NameFileWithExtention != null)
            {

                DateTimeOffset StartsOn = DateTimeOffset.UtcNow;
                // TODO: put in config file
                DateTimeOffset ExpiresOn = DateTimeOffset.UtcNow.AddYears(1);
                //Crée une clé à accès partagé
                sasKey = CreateSasKey(_containerName, NameFileWithExtention, StartsOn, ExpiresOn);
            }
            else
            {
                // Si Jamais l'image et l'url ne sont pas valide, utilise une image de remplacement
                sasKey = _configuration["AlternateImageUrl"];
            }

            CatalogItem newItem = new CatalogItem("1", Guid.NewGuid().ToString())
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                ProductId = Guid.NewGuid().ToString(),
                ProductName = productName,
                ProductPictureUrl = sasKey,
                PictureName = NameFileWithExtention
            };

            // Insertion d'un nouveau élèment dans la table
            TableOperation tableOperation = TableOperation.InsertOrReplace(newItem);
            var tableItem = await _catalog.ExecuteAsync(tableOperation);

            // Mettre à jour la liste en mémoire
            _catalogItems.Add(newItem);
            return newItem;
        }

        public Task<CatalogItem> GetAsync(string id)
        {
            var q = _catalogItems.Where(item => item.Id == id).Select(i => i).FirstOrDefault();
            return Task.FromResult(q);
        }


        public Task<List<CatalogItem>> ListAsync()
        {
            int MaxItems = Convert.ToInt32(_configuration["MaxItems"]);
            // Création d'une requête sur la table "catalog"
            IQueryable<CatalogItem> catalogQuery = _catalog.CreateQuery<CatalogItem>();
            // Indique le nombre d'élèments à charger
            // Exécute la requête, c'est uniquement à ce moment la que les données
            // sont rapatriées du compte de stockage
            var items = catalogQuery.ToList<CatalogItem>().Take<CatalogItem>(MaxItems);
            _catalogItems = items.ToList<CatalogItem>();
            return Task.FromResult(_catalogItems);
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
