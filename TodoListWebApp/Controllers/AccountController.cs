using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TodoListWebApp.Models;
using TodoListWebApp.Services;

namespace TodoListWebApp.Controllers
{
    public class AccountController : Controller
    {
        public async Task SignIn(string redirectPath)
        {
            if (!User.Identity.IsAuthenticated)
            {
                await HttpContext.Authentication.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = redirectPath ?? "/" });
            }
        }

        public async Task SignOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                IAzureAdTokenService tokenCache = (IAzureAdTokenService)HttpContext.RequestServices.GetService(typeof(IAzureAdTokenService));
                tokenCache.Clear();
                await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            }
        }
    }
}
