using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using CnaCatalogService.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            string AccountKey = Configuration["AccountKey"];
            services.AddHttpClient();
            services.AddControllers();
            services.AddSingleton((s) =>
            {                
                var StorageCredentials = new StorageCredentials(AccountName, AccountKey);
                // Instancie le compte de stockage    
                var storageAccount=new CloudStorageAccount(StorageCredentials, true);                
                CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();                 
                return cloudTableClient.GetTableReference(Configuration["CatalogName"]);
            });
            services.AddSingleton((s) =>
            {                
                return new StorageSharedKeyCredential(AccountName, AccountKey);
            });
            services.AddSingleton((s) =>
            {
                var storageSharedKeyCredential = new StorageSharedKeyCredential(AccountName, AccountKey);
                Uri BlobUri = new Uri($"https://{AccountName}.blob.core.windows.net/");
                // Instancie le client Blob qui permettra de récupérer une référence sur le container "images"
                // Remarque : Le container doit être déjà crée dans le compte de stockage
                // https://docs.microsoft.com/fr-fr/azure/storage/blobs/storage-quickstart-blobs-portal
                return new BlobServiceClient(BlobUri, storageSharedKeyCredential);
            });

            services.AddSingleton<ICatalogItemRepository, CatalogItemRepository>();
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
            });
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(options => options.AllowAnyOrigin());
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
