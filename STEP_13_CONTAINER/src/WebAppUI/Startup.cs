using CnAppForAzureDev.Entities;
using CnAppForAzureDev.Extensions;
using CnAppForAzureDev.Managers;
using CnAppForAzureDev.Repositories;
using CnAppForAzureDev.ViewServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repositories.Services.Interfaces;

namespace CnAppForAzureDev
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
            services.AddControllersWithViews();

            ConfigureOptions(Configuration, services);
            ConfigureViewServices(services);
            ConfigureManagers(services);
            ConfigureRepositories(services);            
            ConfigureMvc(services);
            

            services.AddSingleton<IdentityService, IdentityService>();
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "en-US", "fr" };
                options.SetDefaultCulture(supportedCultures[1])
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
            });
        }
        private void ConfigureMvc(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                //var requireHttpsFilter = new RequireHttpsAttribute(); 
                //options.Filters.Add(requireHttpsFilter);
                options.EnableEndpointRouting = false;
            })
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();
        }
        private void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
        {
            services.AddScoped<IndustryManager, IndustryManager>();
            
        }
       
        private void ConfigureManagers(IServiceCollection services)
        {
            services.AddTransient<ICatalogItemManager, CatalogItemManager>();
            services.AddTransient<ITrolleyManager, TrolleyManager>();
            services.AddTransient<IPantryManager, PantryManager>();
        }
        private void ConfigureViewServices(IServiceCollection services)
        {
            services.AddTransient<ICatalogItemViewService, CatalogItemViewService>();
            services.AddTransient<ITrolleyViewService, TrolleyViewService>();
            services.AddTransient<IPantryViewService, PantryViewService>();
        }
        private void ConfigureRepositories(IServiceCollection services)
        {
       
            services.AddSingleton<ICatalogItemRepository, CatalogItemRepository>();
            services.AddScoped<IRepository<Pantry>, DbRepository<Pantry>>();
            services.AddScoped<IRepository<Trolley>, DbRepository<Trolley>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseCors(options => options.AllowAnyOrigin());

            app.UseRequestLocalization();
            
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
