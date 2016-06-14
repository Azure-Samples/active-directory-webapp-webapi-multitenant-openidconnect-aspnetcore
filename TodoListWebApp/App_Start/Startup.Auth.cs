using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Tokens;
using TodoListWebApp.Models;
using TodoListWebApp.Services;
using TodoListWebApp.Utils;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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
                ResponseType = OpenIdConnectResponseTypes.CodeIdToken,
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
                    OnTokenValidated = OnTokenValidated,
                }
            });
        }

        // Inject custom logic for validating which users we allow to sign in
        // Here we check that the user (or their tenant admin) has signed up for the application.
        private Task OnTokenValidated(TokenValidatedContext context)
        {
            // Retrieve the db service
            TodoListWebAppContext db = (TodoListWebAppContext)context.HttpContext.RequestServices.GetService(typeof(TodoListWebAppContext));

            // retriever caller data from the incoming principal
            string issuer = context.Ticket.Principal.FindFirst(AzureADConstants.Issuer).Value;
            string objectID = context.Ticket.Principal.FindFirst(AzureADConstants.ObjectIdClaimType).Value;
            string tenantID = context.Ticket.Principal.FindFirst(AzureADConstants.TenantIdClaimType).Value;

            // Check if the caller is recorded in the db of users who went through the individual onboardoing
            // or if the caller comes from an admin-consented, recorded issuer.
            // If not, the caller was neither from a trusted issuer or a registered user - throw to block the authentication flow
            if ((db.Tenants.FirstOrDefault(a => ((a.IssValue == issuer) && (a.AdminConsented))) == null)
                && (db.Users.FirstOrDefault(b => (b.ObjectID == objectID)) == null))
            {
                throw new SecurityTokenValidationException("Did you forget to sign-up?");
            }

            return Task.FromResult(0);
        }

        // Redeem the auth code for a token to the Graph API and cache it for later.
        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            // Redeem auth code for access token and cache it for later use
            context.HttpContext.User = context.Ticket.Principal;
            IAzureAdTokenService tokenService = (IAzureAdTokenService)context.HttpContext.RequestServices.GetService(typeof(IAzureAdTokenService));
            await tokenService.RedeemAuthCodeForAadGraph(context.ProtocolMessage.Code, context.Properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey]);

            // Notify the OIDC middleware that we already took care of code redemption.
            context.HandleCodeRedemption();
        }

        private Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error?message=" + context.Exception.Message);
            return Task.FromResult(0);
        }
    }
}
