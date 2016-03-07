using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace TodoListWebApp.Services
{
    // Interface MVC controllers will use to populate the ADAL cache with tokens for the signed-in user.
    public interface ITokenCache
    {
        TokenCache Init(string user);
    }
}
