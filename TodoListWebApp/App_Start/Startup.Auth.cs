using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using TodoListWebApp.Utils;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using TodoListWebApp.Models;
using TodoListWebApp.Services;

namespace TodoListWebApp
{
    public partial class Startup
    {
        public void ConfigureAuth(IApplicationBuilder app)
        {
            // Configure the OWIN pipeline to use cookie auth.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
            });

            // Configure the OWIN pipeline to use OpenID Connect auth.
            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AutomaticChallenge = true,
                ClientId = Configuration["AzureAD:ClientId"],
                Authority = String.Format(Configuration["AzureAd:AuthorityFormat"], AzureADConstants.Common),
                PostLogoutRedirectUri = Configuration["AzureAd:RedirectUri"],
                TokenValidationParameters = new TokenValidationParameters
                {
                    // instead of using the default validation (validating against a single issuer value, as we do in line of business apps), 
                    // we inject our own multitenant validation logic
                    ValidateIssuer = false
                },
                Events = new OpenIdConnectEvents
                {
                    OnAuthenticationFailed = OnAuthenticationFailed,
                    OnAuthorizationCodeReceived = OnAuthorizationCodeReceived,
                    OnAuthenticationValidated = OnAuthenticationValidated
                }
            });
        }

        // Inject custom logic for validating which users we allow to sign in
        // Here we check that the user (or their tenant admin) has signed up for the application.
        private Task OnAuthenticationValidated(AuthenticationValidatedContext context)
        {
            // Retrieve the db service
            TodoListWebAppContext db = (TodoListWebAppContext)context.HttpContext.ApplicationServices.GetService(typeof(TodoListWebAppContext));

            // retriever caller data from the incoming principal
            string issuer = context.AuthenticationTicket.Principal.FindFirst(AzureADConstants.Issuer).Value;
            string UPN = context.AuthenticationTicket.Principal.FindFirst(ClaimTypes.Name).Value;
            string tenantID = context.AuthenticationTicket.Principal.FindFirst(AzureADConstants.TenantIdClaimType).Value;

            // Check if the caller is recorded in the db of users who went through the individual onboardoing
            // or if the caller comes from an admin-consented, recorded issuer.
            // If not, the caller was neither from a trusted issuer or a registered user - throw to block the authentication flow
            if ((db.Tenants.FirstOrDefault(a => ((a.IssValue == issuer) && (a.AdminConsented))) == null)
                && (db.Users.FirstOrDefault(b => ((b.UPN == UPN) && (b.TenantID == tenantID))) == null))
            {
                throw new SecurityTokenValidationException("Did you forget to sign-up?");
            }

            return Task.FromResult(0);
        }

        // Redeem the auth code for a token to the Graph API and cache it for later.
        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            // Retrieve the token cache service
            ITokenCache tokenCache = (ITokenCache)context.HttpContext.ApplicationServices.GetService(typeof(ITokenCache));

            string tenantId = context.AuthenticationTicket.Principal.FindFirst(AzureADConstants.TenantIdClaimType).Value;
            string userObjectID = context.AuthenticationTicket.Principal.FindFirst(AzureADConstants.ObjectIdClaimType).Value;
            AuthenticationContext authContext = new AuthenticationContext(String.Format(Configuration["AzureAd:AuthorityFormat"], tenantId), tokenCache.Init(userObjectID));
            ClientCredential credential = new ClientCredential(Configuration["AzureAd:ClientId"], Configuration["AzureAd:ClientSecret"]);
            AuthenticationResult authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(
                context.Code, new Uri(context.RedirectUri), credential, Configuration["AzureAd:GraphResourceId"]);
        }

        private Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error?message=" + context.Exception.Message);
            return Task.FromResult(0);
        }
    }
}
