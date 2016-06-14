using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using TodoListWebApp.Models;
using TodoListWebApp.Utils;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoListWebApp.Controllers
{

    public class OnboardingController : Controller
    {
        private TodoListWebAppContext _db;
        private AzureADConfig _aadConfig;

        public OnboardingController(TodoListWebAppContext context, IOptions<AzureADConfig> config)
        {
            _db = context;
            _aadConfig = config.Value;
        }

        // GET: /Onboarding/SignUp
        public ActionResult SignUp()
        {
            return View();
        }

        // POST: /Onboarding/SignUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignUp([Bind("ID", "Name", "AdminConsented")] Tenant tenant)
        {
            // generate a random value to identify the request, & store it in the temporary entry for the tenant, 
            // we'll use it later to assess if the request was originated from us.  This is necessary if we want
            // to prevent attackers from provisioning themselves to access our app without having gone through our
            // onboarding process (e.g. payments, etc)
            string stateMarker = Guid.NewGuid().ToString();
            tenant.IssValue = stateMarker;
            tenant.Created = DateTime.Now;
            _db.Tenants.Add(tenant);
            _db.SaveChanges();

            // If the prospect customer wants to provision the app for all users in his/her tenant, use the 'prompt=admin_consent' parameter
            // Else, just add the state from above to the request below.
            string extraRequestParameters = String.Concat(String.Format("state={0}", stateMarker), tenant.AdminConsented ? "&prompt=admin_consent" : string.Empty);

            try
            {

                // Create an OAuth2 request using ADAL.  This will trigger a consent flow that will provision the app in the target tenant
                // Note that you don't really need ADAL to create this request, it's just a helper method that ADAL gives you.
                AuthenticationContext authContext = new AuthenticationContext(String.Format(_aadConfig.AuthorityFormat, AzureADConstants.Common));
                Uri authorizationRequest = await authContext.GetAuthorizationRequestUrlAsync(
                    _aadConfig.GraphResourceId,
                    _aadConfig.ClientId,
                    new Uri(String.Concat(_aadConfig.RedirectUri, "Onboarding/ProcessCode")),
                    UserIdentifier.AnyUser,
                    extraRequestParameters
                );

                // Send the user to consent
                return new RedirectResult(authorizationRequest.AbsoluteUri);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: /Onboarding/ProcessCode
        public async Task<ActionResult> ProcessCode(string code, string error, string error_description, string resource, string state)
        {
            // Check for OAuth errors
            if (!string.IsNullOrEmpty(error))
            {
                return new RedirectResult(String.Concat("/Home/Error?message=", error_description));
            }

            // Is this a response to a request we generated? Let's see if the state is carrying an ID we previously saved
            var tenant = _db.Tenants.FirstOrDefault(a => a.IssValue == state);
            if (tenant == null)
            {
                return new RedirectResult("/Home/Error?message=Request state did not match");
            }

            // Get a token for the Graph, that will provide us with information abut the caller
            ClientCredential credential = new ClientCredential(_aadConfig.ClientId, _aadConfig.ClientSecret);
            AuthenticationContext authContext = new AuthenticationContext(String.Format(_aadConfig.AuthorityFormat, AzureADConstants.Common));
            AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(code, new Uri(String.Concat(_aadConfig.RedirectUri, "Onboarding/ProcessCode")), credential);

            // Store a record of sign-up for the application.
            // If the admin signed up his entire tenant, save a sign-up record for the tenant in the db.
            // If admin/user only signed up for themselves, just save a sign-up record for the user in the db.
            if (tenant.AdminConsented)
            {
                // Read the tenantID out of the authResult and use it to store an issuer string representing the tenant.
                tenant.IssValue = String.Format("https://sts.windows.net/{0}/", result.TenantId);
            }
            else
            {
                _db.Tenants.Remove(tenant);
                if (_db.Users.FirstOrDefault(a => a.ObjectID == result.UserInfo.UniqueId) == null)
                {
                    _db.Users.Add(new AADUserRecord { UPN = result.UserInfo.DisplayableId, ObjectID = result.UserInfo.UniqueId });
                }
            }

            // Remove any leftover state records from incomplete authorization requests
            DateTime tenMinsAgo = DateTime.Now.Subtract(new TimeSpan(0, 10, 0));
            var garbage = _db.Tenants.Where(a => (!a.IssValue.StartsWith("https") && (a.Created < tenMinsAgo)));
            foreach (Tenant t in garbage)
                _db.Tenants.Remove(t);

            _db.SaveChanges();
            return View();
        }
    }
}
