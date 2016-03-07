using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoListWebApp.Models;
using TodoListWebApp.Services;
using TodoListWebApp.Utils;

namespace TodoListWebApp
{
    public partial class Startup
    {
        public Startup()
        {
            // Setup configuration sources.
            Configuration = new ConfigurationBuilder()
               .AddJsonFile("config.json")
               .AddEnvironmentVariables()
               .AddUserSecrets()
               .Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add MVC services to the services container.
            services.AddMvc();

            // Add Authentication services.
            services.AddAuthentication(sharedOptions => sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            // Expose Azure AD configuration to controllers
            services.Configure<AzureADConfig>(Configuration.GetChildren().Where(c => c.Key == "AzureAd").First());

            // Register the db context
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<TodoListWebAppContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            // Expose a token caching service to controllers, using a db implementation
            services.AddTransient<ITokenCache, DbTokenCache>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Add the console logger.
            loggerFactory.AddConsole(LogLevel.Debug);

            // Configure error handling middleware.
            app.UseExceptionHandler("/Home/Error");

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Configure the OpenIdConnect pipeline and required services.
            ConfigureAuth(app);

            // Create or migrate the db during deployment
            var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            serviceScope.ServiceProvider.GetService<TodoListWebAppContext>().Database.Migrate();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
