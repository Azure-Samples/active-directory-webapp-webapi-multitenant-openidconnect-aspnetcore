using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using TodoListWebApp.Models;
using TodoListWebApp.Services;
using TodoListWebApp.Utils;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TodoListWebApp
{
    public partial class Startup
    {
        public void ConfigureServicesAuth(IServiceCollection services)
        {
            // Add Authentication services.
            services.AddAuthentication(sharedOptions => sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme)
                // Configure the OWIN pipeline to use cookie auth.
                .AddCookie(option=> new CookieAuthenticationOptions())

                // Configure the OWIN pipeline to use OpenID Connect auth.
                .AddOpenIdConnect(option=> new OpenIdConnectOptions
                {
                    ResponseType = OpenIdConnectResponseType.CodeIdToken,
                    ClientId = Configuration["AzureAD:ClientId"],
                    Authority = String.Format(Configuration["AzureAd:AuthorityFormat"], AzureADConstants.Common),
                    SignedOutRedirectUri = Configuration["AzureAd:RedirectUri"],
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        // instead of using the default validation (validating against a single issuer value, as we do in line of business apps), 
                        // we inject our own multitenant validation logic
                        ValidateIssuer = false
                    },
                    Events = new OpenIdConnectEvents
                    {
                        OnRemoteFailure = OnAuthenticationFailed,
                        OnAuthorizationCodeReceived = OnAuthorizationCodeReceived,
                        OnTokenValidated = OnTokenValidated,
                        OnRedirectToIdentityProvider = OnRedirectToIdentityProvider
                    }
                });

        }

        // Inject custom logic for validating which users we allow to sign in
        // Here we check that the user (or their tenant admin) has signed up for the application.
        private Task OnTokenValidated(TokenValidatedContext context)
        {
            // Retrieve the db service
            TodoListWebAppContext db = (TodoListWebAppContext)context.HttpContext.RequestServices.GetService(typeof(TodoListWebAppContext));

            // Retrieve caller data from the incoming principal
            string issuer = context.Principal.FindFirst(AzureADConstants.Issuer).Value;
            string objectID = context.Principal.FindFirst(AzureADConstants.ObjectIdClaimType).Value;
            string tenantID = context.Principal.FindFirst(AzureADConstants.TenantIdClaimType).Value;
            string upn = context.Principal.FindFirst(ClaimTypes.Upn).Value;

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
            context.HttpContext.User = context.Principal;
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

        private Task OnAuthenticationFailed(RemoteFailureContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
            return Task.FromResult(0);
        }
    }
}
