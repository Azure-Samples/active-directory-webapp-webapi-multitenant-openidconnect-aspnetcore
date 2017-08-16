using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using TodoListWebApp.Models;
using TodoListWebApp.Services;
using TodoListWebApp.Utils;
using Microsoft.AspNetCore.Authentication;

namespace TodoListWebApp.Controllers
{
    public class AccountController : Controller
    {
        // Issue a challege to send the user to AAD for sign in
        public async Task SignIn(string redirectPath)
        {
            if (!User.Identity.IsAuthenticated)
            {
                await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = redirectPath ?? "/" });
            }
        }

        // Clear the cache of tokens for the user, and send a sign out request to AAD
        public async Task SignOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                IAzureAdTokenService tokenCache = (IAzureAdTokenService)HttpContext.RequestServices.GetService(typeof(IAzureAdTokenService));
                tokenCache.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            }
        }

        // Show the sign up form, asking the user to indicate individual vs. tenant-wide sign up
        public ActionResult SignUp()
        {
            return View();
        }

        // Issue a challenge to send the user to AAD to sign in,
        // adding some additional data to the request which will be used in Startup.Auth.cs
        // The Tenant name here serves no functional purpose - it is only used to show how you
        // can collect additional information from the user during sign up.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task SignUp([Bind("ID", "Name", "AdminConsented")] Tenant tenant)
        {
            await HttpContext.ChallengeAsync(
                OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties(new Dictionary<string, string>
                {
                    { Constants.AdminConsentKey, tenant.AdminConsented.ToString() },
                    { Constants.TenantNameKey, tenant.Name }
                }) { RedirectUri = "/Todo" });
        }
        
        public async Task EndSession()
        {
            if (User.Identity.IsAuthenticated)
            {
                IAzureAdTokenService tokenCache = (IAzureAdTokenService)HttpContext.RequestServices.GetService(typeof(IAzureAdTokenService));
                tokenCache.Clear();
            }
            
            // If AAD sends a single sign-out message to the app, end the user's session, but don't redirect to AAD for sign out.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
