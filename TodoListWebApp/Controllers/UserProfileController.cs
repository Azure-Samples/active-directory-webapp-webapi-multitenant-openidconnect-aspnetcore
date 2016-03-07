using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using TodoListWebApp.Models;
using TodoListWebApp.Services;
using TodoListWebApp.Utils;

namespace TodoListWebApp.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private TodoListWebAppContext _db;
        private AzureADConfig _aadConfig;
        private ITokenCache _tokenCache;

        public UserProfileController(TodoListWebAppContext context, IOptions<AzureADConfig> config, ITokenCache tokenCache)
        {
            _db = context;
            _aadConfig = config.Value;
            _tokenCache = tokenCache;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            try
            {
                // Retrieve the user's objectID, tenantID, and access token since they are parameters used to query the Graph API.
                string tenantId = User.FindFirst(AzureADConstants.TenantIdClaimType).Value;
                string userObjectID = User.FindFirst(AzureADConstants.ObjectIdClaimType).Value;
                AuthenticationContext authContext = new AuthenticationContext(String.Format(_aadConfig.AuthorityFormat, tenantId), _tokenCache.Init(userObjectID));
                ClientCredential credential = new ClientCredential(_aadConfig.ClientId, _aadConfig.ClientSecret);
                AuthenticationResult result = await authContext.AcquireTokenSilentAsync(_aadConfig.GraphResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                // Call the Graph API and retrieve the user's profile.
                string requestUrl = String.Format("{0}/{1}{2}?api-version={3}", _aadConfig.GraphBaseEndpoint, tenantId, "/me", _aadConfig.GraphApiVersion);
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                HttpResponseMessage response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(response.ReasonPhrase);
                }

                string responseString = await response.Content.ReadAsStringAsync();
                return View(JsonConvert.DeserializeObject<AADUserProfile>(responseString));
            }
            catch (AdalException ex)
            {
                return new RedirectResult("/Home/Error?message=Unable to get tokens; you may need to sign in again.");
            }
            catch (Exception ex)
            {
                return new RedirectResult(String.Concat("/Home/Error?message=", ex.Message));
            }
        }
    }
}
