using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private AzureADConfig _aadConfig;
        private IAzureAdTokenService _tokenCache;

        public UserProfileController(IOptions<AzureADConfig> config, IAzureAdTokenService tokenCache)
        {
            _aadConfig = config.Value;
            _tokenCache = tokenCache;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            try
            {
                string token = await _tokenCache.GetAccessTokenForAadGraph();

                // Call the Graph API and retrieve the user's profile.
                string requestUrl = String.Format("{0}/{1}{2}?api-version={3}", 
                    _aadConfig.GraphBaseEndpoint, 
                    User.FindFirst(AzureADConstants.TenantIdClaimType).Value,
                    "/me", 
                    _aadConfig.GraphApiVersion);

                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
