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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace TodoListWebApp
{
    public partial class Startup
    {
        // Inject custom logic for validating which users we allow to sign in
        // Here we check that the user (or their tenant admin) has signed up for the application.
        private Task OnTokenValidated(TokenValidatedContext context)
        {
            // Retrieve the db service
            TodoListWebAppContext db = (TodoListWebAppContext)context.HttpContext.RequestServices.GetService(typeof(TodoListWebAppContext));

            // Retrieve caller data from the incoming principal
            string issuer = context.Ticket.Principal.FindFirst(AzureADConstants.Issuer).Value;
            string objectID = context.Ticket.Principal.FindFirst(AzureADConstants.ObjectIdClaimType).Value;
            string tenantID = context.Ticket.Principal.FindFirst(AzureADConstants.TenantIdClaimType).Value;
            string upn = context.Ticket.Principal.FindFirst(ClaimTypes.Upn).Value;

            // Look up existing sign up records from the database
            Tenant tenant = db.Tenants.FirstOrDefault(a => a.IssValue.Equals(issuer));
            AADUserRecord user = db.Users.FirstOrDefault(b => b.ObjectID.Equals(objectID));

            // If the user is signing up, add the user or tenant to the database record of sign ups.
            string adminConsentSignUp = null;
            if (context.Properties.Items.TryGetValue(Constants.AdminConsentKey, out adminConsentSignUp))
            {
                if (adminConsentSignUp == Constants.True)
                {
                    if (tenant == null)
                    {
                        tenant = new Tenant { Created = DateTime.Now, IssValue = issuer, Name = context.Properties.Items[Constants.TenantNameKey], AdminConsented = true };
                        db.Tenants.Add(tenant);
                    }
                    else
                    {
                        tenant.AdminConsented = true;
                    }
                }
                else if (user == null)
                {
                    user = new AADUserRecord { UPN = upn, ObjectID = objectID };
                    db.Users.Add(user);
                }

                db.SaveChanges();
            }

            // Ensure that the caller is recorded in the db of users who went through the individual onboarding
            // or if the caller comes from an admin-consented, recorded issuer.
            if ((tenant == null || !tenant.AdminConsented) && (user == null))
            {
                // If not, the caller was neither from a trusted issuer or a registered user - throw to block the authentication flow
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

        // If the user is trying to sign up for their entire tenant, attach the admin_consent parameter to the request
        private Task OnRedirectToIdentityProvider(RedirectContext context)
        {
            string adminConsentSignUp = null;
            if (context.Request.Path == new PathString("/Account/SignUp") && context.Properties.Items.TryGetValue(Constants.AdminConsentKey, out adminConsentSignUp))
            {
                if (adminConsentSignUp == Constants.True)
                {
                    context.ProtocolMessage.Prompt = AzureADConstants.AdminConsent;
                }
            }

            return Task.FromResult(0);
        }

        private Task OnAuthenticationFailed(FailureContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
            return Task.FromResult(0);
        }
    }
}
