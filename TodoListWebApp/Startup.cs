using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoListWebApp.Models;
using TodoListWebApp.Utils;
using Microsoft.EntityFrameworkCore;
using TodoListWebApp.Services;
using Microsoft.AspNetCore.Http;

namespace TodoListWebApp
{
    public partial class Startup
    {
        public static string ClientId;
        public static string ClientSecret;
        public static string Authority;
        public static string GraphResourceId;
        public static string TodoListResourceId;

        public Startup(IHostingEnvironment env)
        {
            // Setup configuration sources.
            Configuration = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json")
               .Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddCookieAuthentication();

            // Configure the OWIN pipeline to use OpenID Connect auth.
            services.AddOpenIdConnectAuthentication(options => 
            {
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                options.ClientId = Configuration["AzureAD:ClientId"];
                options.Authority = String.Format(Configuration["AzureAd:AuthorityFormat"], AzureADConstants.Common);
                options.PostLogoutRedirectUri = Configuration["AzureAd:RedirectUri"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // instead of using the default validation (validating against a single issuer value, as we do in line of business apps), 
                    // we inject our own multitenant validation logic
                    ValidateIssuer = false
                };
                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = OnAuthenticationFailed,
                    OnAuthorizationCodeReceived = OnAuthorizationCodeReceived,
                    OnTokenValidated = OnTokenValidated,
                    OnRedirectToIdentityProvider = OnRedirectToIdentityProvider
                };
            });
            
            // Add Authentication services.
            services.AddAuthentication(options => 
            {
                options.DefaultAuthenticationScheme = OpenIdConnectAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            // Expose Azure AD configuration to controllers
            services.AddOptions();
            services.Configure<AzureADConfig>(Configuration.GetChildren().Where(c => c.Key.Equals("AzureAd")).First());

            // Register the db context
            services.AddDbContext<TodoListWebAppContext>(options => options.UseSqlite(Configuration["Data:ConnectionString"]));

            // Expose the HttpContext to dependent services
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Expose a token caching service to controllers, using a db implementation
            services.AddScoped<IAzureAdTokenService, DbTokenCache>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Add the console logger.
            loggerFactory.AddConsole(Microsoft.Extensions.Logging.LogLevel.Debug);

            // Configure error handling middleware.
            app.UseDeveloperExceptionPage();
            //app.UseExceptionHandler("/Home/Error");

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            app.UseAuthentication();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
