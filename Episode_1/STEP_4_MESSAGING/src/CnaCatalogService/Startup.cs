using Azure.Storage;
using Azure.Storage.Blobs;
using Bus.Services.Helper;
using CnaCatalogService.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;


namespace CnaCatalogService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string AccountName = Configuration["AccountName"];
            // TODO : keep the account key in a KeyVault (for future step)
            string AccountKey = Configuration["AccountKey"];
            //TODO : keep in keyvault
            string ServiceBusConnectionString = Configuration["ServiceBusConnectionString"];
            
            // Ajout du Service Azure ServiceBus
            services.AddSingleton<CnaServicesBus>(s =>
            {                                
                return new CnaServicesBus(ServiceBusConnectionString);
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {

                    Title = $"CNA Catalog",
                    Version = "v1.0.0",
                    Description = "API de gestion du catalogue",
                    License = new OpenApiLicense { Name = "MIT", Url = new Uri(Configuration["OpenApiLicenseUrl"]) },                    
                                        
                });
                
            });

            services.AddHttpClient();
            services.AddControllers();
            // Ajout du Client Table
            services.AddSingleton((s) =>
            {
                CloudStorageAccount storageAccount = null;
                bool UseCosmosDb = bool.Parse(Configuration["UseCosmosDb"]);
                if (UseCosmosDb)
                {
                    // Instancie le compte de stockage    
                    string CosmosDbConnectionString = Configuration["CosmosDbConnectionString"];
                    storageAccount = CloudStorageAccount.Parse(CosmosDbConnectionString);
                }
                else
                {
                    var StorageCredentials = new StorageCredentials(AccountName, AccountKey);
                    storageAccount = new CloudStorageAccount(StorageCredentials, true);
                }

                CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();                 
                return cloudTableClient.GetTableReference(Configuration["CatalogName"]);
            });
            services.AddSingleton((s) =>
            {                
                return new StorageSharedKeyCredential(AccountName, AccountKey);
            });
            // Ajout du Client Blob
            services.AddSingleton((s) =>
            {
                var storageSharedKeyCredential = new StorageSharedKeyCredential(AccountName, AccountKey);
                Uri BlobUri = new Uri($"https://{AccountName}.blob.core.windows.net/");
                // Instancie le client Blob qui permettra de récupérer une référence sur le container "images"
                // Remarque : Le container doit être déjà crée dans le compte de stockage
                // https://docs.microsoft.com/fr-fr/azure/storage/blobs/storage-quickstart-blobs-portal
                return new BlobServiceClient(BlobUri, storageSharedKeyCredential);
            });

            services.AddSingleton<CatalogItemRepository, CatalogItemRepository>();

            // Permet d'être appeler par n'importe quelle origine
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
            });
            services.AddApplicationInsightsTelemetry();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CNA Catalog v1");
                
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(options => options.AllowAnyOrigin());
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
