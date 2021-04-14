using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Bus.Services.Helper;
using CnaCatalogService.Entities;
using CnaCatalogService.Services;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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
    public class CatalogItemRepository :  IRepository<CatalogItem>
    {

        private readonly CloudTable _catalog;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly StorageSharedKeyCredential _storageSharedKeyCredential;
        IQueueClient _receiveQueueclient;

        private string _containerName;
        private List<CatalogItem> _catalogItems;
        private IConfiguration _configuration;
        private CnaServicesBus _cnaServicesBus;
        private ITopicClient _topicClient;
      
        private readonly IHttpClientFactory _httpClientFactory;
        /// <summary>
        /// CCTOR
        /// </summary>
        /// <param name="configuration"></param>
        public CatalogItemRepository(IConfiguration configuration, 
                                     CloudTable catalog,
                                     BlobServiceClient blobServiceClient,
                                     StorageSharedKeyCredential storageSharedKeyCredential,                                     
                                     IHttpClientFactory httpClientFactory,
                                     CnaServicesBus cnaServicesBus)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _catalog = catalog;
            _cnaServicesBus = cnaServicesBus;
            _containerName = configuration["ContainerName"];   
            // Inscrit la méthode qui sera appelée lorsqu'un nouveau message de type CatalogItem
            // est envoyé à la file d'attente de Azure ServiceBus
            string QueueName = _configuration["QueueName"];       
            //TODO:DEMO RECEIVE QUEUE
            _receiveQueueclient= _cnaServicesBus.RegisterReceiverToQueue(ProcessNewCatalogItemToAddAsync,QueueName);

            // Cette rubrique de Azure service bus est utilisée afin de prévenir les différents recepteurs 
            // que l'item a bien été persisté.
            string TopicName = configuration["TopicName"];
            _topicClient = _cnaServicesBus.CreateTopic(TopicName);
            
            

            // Instancie les informations de connexion au Blob
            _storageSharedKeyCredential = storageSharedKeyCredential;
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        }
        public async Task ProcessNewCatalogItemToAddAsync(Message message, CancellationToken token)
        {
            //TODO:DEMO RECEIVE MESSAGE
            //  récupère le message
            var MessageReceive = Encoding.UTF8.GetString(message.Body);

            // Si le message n'est pas valide la méthode ExceptionReceivedHandler est invoquée
            var newItem = JsonConvert.DeserializeObject<CatalogItem>(MessageReceive);
            
            // Persiste le nouvel Item
            await this.AddAsync(newItem);
            
            //Complétez le message afin qu'il ne soit pas reçu à nouveau.
            //Ceci ne peut être fait que si le client de la file d'attente est créé en mode ReceiveMode.PeekLock (qui est le mode par défaut).
            if (_receiveQueueclient != null) await _receiveQueueclient.CompleteAsync(message.SystemProperties.LockToken);

            // Note : Utilisez le jeton d'annulation passé si nécessaire pour déterminer si le client de la file d'attente a déjà été fermé.
            // Si queueClient a déjà été fermé, vous pouvez choisir de ne pas appeler les appels CompleteAsync() ou AbandonAsync() etc. 
            // pour éviter des exceptions inutiles.

        }


        private string GetFileNameFromUrl(string productPictureUrl)
        {
            var IndexOf = productPictureUrl.LastIndexOf('/') + 1;
            var BlobName = productPictureUrl.Remove(0, IndexOf);
            return BlobName;
        }
        
        /// <summary>
        /// Télécharger une image à partir d'une URL et 
        /// la sauvegarder dans le Blob
        /// </summary>
        /// <param name="productPictureUrl">ULR de l'image à télécharger</param>
        /// <returns>Le nom du fichier téléchargé</returns>
        public async Task<string> UploadPictureToBlobAsync(string productPictureUrl)
        {
            // Prend du temps c'est pourquoi l'appel au service catalogue devra se faire en asynchrone
            var stream = await Helper.DownloadPictureFromUrlAsync(productPictureUrl, _httpClientFactory.CreateClient());
            if (stream != null)
            {
                string BlobName = null;
                try
                {
                    BlobName = GetFileNameFromUrl(productPictureUrl);
                    
                    await _blobContainerClient.UploadBlobAsync(BlobName, stream);
                }
                catch (Exception ex)
                {
                    //Ne fait rien si le blob exist déjà
                    //Log in App insights
                    
                    
                }
                
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
        

        /// <summary>
        /// Ajoute un élèment dans le catalogue
        /// </summary>
        /// <param name="ownerId">Propriétaire du produit</param>
        /// <param name="productName">Nom du produit</param>
        /// <param name="productPictureUrl">ULR de l'image du produit</param>
        /// <returns></returns>
        public async Task<CatalogItem> AddAsync(CatalogItem entity)
        {

            string sasKey = null;
            // Pousse l'image dans le Blob             
            var NameFileWithExtention = await UploadPictureToBlobAsync(entity.ProductPictureUrl);
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
                string AlternateImageUrl= _configuration["AlternateImageUrl"];
                sasKey = AlternateImageUrl;
                NameFileWithExtention = GetFileNameFromUrl(AlternateImageUrl);
            }

            CatalogItem newItem = new CatalogItem("1", Guid.NewGuid().ToString())
            {
                Id = entity.Id,
                OwnerId = entity.OwnerId,
                ProductId = entity.ProductId,
                ProductName = entity.ProductName,
                ProductPictureUrl = sasKey,
                PictureName = NameFileWithExtention
            };

            // Insertion d'un nouveau élèment dans la table
            TableOperation tableOperation = TableOperation.InsertOrReplace(newItem);
            var tableResult = await _catalog.ExecuteAsync(tableOperation);
            var savedItem = (CatalogItem)tableResult.Result;

            //Notifie  aux recepteurs que l'Item a bien été crée
            Message message = _cnaServicesBus.CreateMessage<CatalogItem>(savedItem);
            await _topicClient.SendAsync(message);


            // Mettre à jour la liste en mémoire attention elle n'est pas thread safe
            // et cela ne sert à rien pour l'instant
            _catalogItems.Add(savedItem);

            //le retour aussi ne sert à rien
            return savedItem;
        }

        public Task<CatalogItem> GetAsync(string id)
        {
            //TODO : rechercher autrement avec cognitive search
            var q = _catalogItems.Where(item => item.Id == id).Select(i => i).FirstOrDefault();
            return Task.FromResult(q);
        }


        public Task<List<CatalogItem>> ListAsync()
        {
            // TRODO: A passer dans le body
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
