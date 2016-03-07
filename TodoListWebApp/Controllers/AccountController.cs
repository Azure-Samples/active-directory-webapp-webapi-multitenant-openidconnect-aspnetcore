using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using TodoListWebApp.Models;

namespace TodoListWebApp.Controllers
{
    public class AccountController : Controller
    {
        public void SignIn(string redirectPath)
        {
            if (!User.Identity.IsAuthenticated)
            {
                HttpContext.Authentication.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = redirectPath ?? "/" });
            }
        }

        public void SignOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            }
        }
    }
}
