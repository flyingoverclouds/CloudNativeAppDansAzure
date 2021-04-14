using Bus.Services.Helper;
using CnAppForAzureDev.Entities;
using CnAppForAzureDev.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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
        private readonly IHttpCatalogClient _httpCatalogClient;

        private CnaServicesBus _cnaServicesBus;
        private IQueueClient _queueClient;
        private ISubscriptionClient _subscriptionClient;

        /// <summary>
        /// CCTOR
        /// </summary>
        /// <param name="configuration"></param>
        public CatalogItemRepository(IConfiguration configuration, 
                                     IHttpCatalogClient httpCatalogClient,
                                     CnaServicesBus cnaServicesBus)
        {
            _configuration = configuration;
            _httpCatalogClient = httpCatalogClient;
            _cnaServicesBus = cnaServicesBus;
            // Création d'une file d'attente pour envoyer les messages
            string QueueName = _configuration["QueueName"];            
            // Création d'une file d'attente pour envoyer des messages à serviceBus
            _queueClient = _cnaServicesBus.CreateQueueClient(QueueName);
            
            string TopicName = configuration["TopicName"];
            string SubscriptionName = configuration["SubscriptionName"];

            _subscriptionClient = _cnaServicesBus.AddReceiverToSubscriptionTopic(ProcessNewCatalogItemCreatedAsync, TopicName, SubscriptionName);

        }
        public async Task ProcessNewCatalogItemCreatedAsync(Message message, CancellationToken token)
        {

            //  récupère le message
            var MessageReceive = Encoding.UTF8.GetString(message.Body);

            // Si le message n'est pas valide la méthode ExceptionReceivedHandler est invoquée
            var newItem = JsonConvert.DeserializeObject<CatalogItem>(MessageReceive);

            //Complétez le message afin qu'il ne soit pas reçu à nouveau.
            //Ceci ne peut être fait que si le client de la file d'attente est créé en mode ReceiveMode.PeekLock (qui est le mode par défaut).
            if (_subscriptionClient != null) await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            //TODO: Comment rafraichir la page avec l'item ajouté ?
            //TODO: Comment notifier l'utilisateur ?
            

            // Note : Utilisez le jeton d'annulation passé si nécessaire pour déterminer si le client de la file d'attente a déjà été fermé.
            // Si queueClient a déjà été fermé, vous pouvez choisir de ne pas appeler les appels CompleteAsync() ou AbandonAsync() etc. 
            // pour éviter des exceptions inutiles.

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
            CatalogItem newItem = new CatalogItem
            {
                Id = Guid.NewGuid().ToString(),                
                ProductId = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                ProductName = productName,
                ProductPictureUrl = productPictureUrl
            };
            // Step 2 :Invoque le microservice catalogue via Http
            // newItem = await _httpCatalogClient.AddAsync(newItem);
            
            var strMessage = JsonConvert.SerializeObject(newItem);
            // Step 4 Crée un nouveau message            
            // Envoi le message à la file d'attente
            Message message = new Message(Encoding.UTF8.GetBytes(strMessage));            
            await _queueClient.SendAsync(message);
            

            // Ajoute le nouvel Item dans un cache local
            _catalogItems.Add(newItem);

            return newItem;
        }
     
        public Task<CatalogItem> GetAsync(string id)
        {
            var q = _catalogItems.Where(item => item.Id == id).Select(i => i).FirstOrDefault();
            return Task.FromResult(q);
        }

       
        public async Task<List<CatalogItem>> ListAsync()
        {            
            // Invoque le microservice catalogue via Http
            // Remarque cette méthode est invoquée régulierement il est possible
            // TODO : Ajouter un cache local afin d'éviter de soliciter le service catalogue
            // inutilement
             _catalogItems = await _httpCatalogClient.ListAsync();

            return _catalogItems;
        }
        public Task<List<CatalogItem>> ListWhenNewItemWasAdded()
        {
            // La méthode AddAsync
            // En ajoutant un nouveau produit de manière asynchrone, il ne sera pas
            // forcement disponible imédiatement

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
