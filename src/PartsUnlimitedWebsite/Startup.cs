using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PartsUnlimited.Areas.Admin;
using PartsUnlimited.Models;
using PartsUnlimited.Queries;
using PartsUnlimited.Recommendations;
using PartsUnlimited.Search;
using PartsUnlimited.Security;
using PartsUnlimited.Telemetry;
using PartsUnlimited.WebsiteConfiguration;

namespace PartsUnlimited
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IServiceCollection service { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            service = services;
            var sqlConnectionString = Configuration.GetConnectionString("DefaultConnectionString");
            var useInMemoryDatabase = string.IsNullOrWhiteSpace(sqlConnectionString);

            // Add EF services to the services container
            if (useInMemoryDatabase)
            {
                services.AddDbContext<PartsUnlimitedContext>(options => options.UseInMemoryDatabase("PartsUnlimited"));
            }
            else
            {
                services.AddDbContext<PartsUnlimitedContext>(options => options.UseSqlServer(sqlConnectionString));
            }


            // Add Identity services to the services container
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<PartsUnlimitedContext>()
                .AddDefaultTokenProviders();

            // Configure admin policies
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(AdminConstants.Role,
                    authBuilder =>
                    {
                        authBuilder.RequireClaim(AdminConstants.ManageStore.Name, AdminConstants.ManageStore.Allowed);
                    });

            });

            // Add implementations
            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddScoped<IOrdersQuery, OrdersQuery>();
            services.AddScoped<IRaincheckQuery, RaincheckQuery>();

            services.AddSingleton<ITelemetryProvider, EmptyTelemetryProvider>();
            services.AddScoped<IProductSearch, StringContainsProductSearch>();

            SetupRecommendationService(services);

            services.AddScoped<IWebsiteOptions>(p =>
            {
                var telemetry = p.GetRequiredService<ITelemetryProvider>();
                return new ConfigurationWebsiteOptions(Configuration.GetSection("WebsiteOptions"), telemetry);
            });

            services.AddScoped<IApplicationInsightsSettings>(p =>
            {
                return new ConfigurationApplicationInsightsSettings(Configuration.GetSection("Keys:ApplicationInsights"));
            });

            services.AddApplicationInsightsTelemetry(Configuration);

            // Associate IPartsUnlimitedContext
            services.AddScoped<IPartsUnlimitedContext>(provider => provider.GetRequiredService<PartsUnlimitedContext>());

            // We need access to these settings in a static extension method, so DI does not help us :(
            ContentDeliveryNetworkExtensions.Configuration = new ContentDeliveryNetworkConfiguration(Configuration.GetSection("CDN"));

            // Add MVC services to the services container
            services.AddControllersWithViews();
            services.AddRazorPages();

            // Add session related services
            services.AddSession();
        }

        private void SetupRecommendationService(IServiceCollection services)
        {
            var azureMlConfig = new AzureMLFrequentlyBoughtTogetherConfig(Configuration.GetSection("Keys:AzureMLFrequentlyBoughtTogether"));

            // If keys are not available for Azure ML recommendation service, register an empty recommendation engine
            if (string.IsNullOrEmpty(azureMlConfig.AccountKey) || string.IsNullOrEmpty(azureMlConfig.ModelName))
            {
                services.AddSingleton<IRecommendationEngine, EmptyRecommendationsEngine>();
            }
            else
            {
                services.AddSingleton<IAzureMLAuthenticatedHttpClient, AzureMLAuthenticatedHttpClient>();
                services.AddSingleton<IAzureMLFrequentlyBoughtTogetherConfig>(azureMlConfig);
                services.AddScoped<IRecommendationEngine, AzureMLFrequentlyBoughtTogetherRecommendationEngine>();
            }
        }

        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Development' or is not defined
        //The allowed values are Development,Staging and Production
        public void ConfigureDevelopment(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Display custom error page in production when error occurs
            //During development use the ErrorPage middleware to display error information in the browser
            app.UseDeveloperExceptionPage();

            Configure(app, env);
        }

        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Staging'
        //The allowed values are Development,Staging and Production
        public void ConfigureStaging(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler("/Home/Error");
            Configure(app, env);
        }

        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Production'
        //The allowed values are Development,Staging and Production
        public void ConfigureProduction(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler("/Home/Error");
            Configure(app, env);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            AppBuilderLoginProviderExtensions.AddLoginProviders(service, new ConfigurationLoginProviders(Configuration.GetSection("Authentication")));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areaRoute",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });
        }
    }
}