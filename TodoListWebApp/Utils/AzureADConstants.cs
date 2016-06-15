using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListWebApp.Utils
{
    public class AzureADConstants
    {
        public static string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        public static string ObjectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public static string Common = "common";
        public static string AdminConsent = "admin_consent";
        public static string Issuer = "iss";
    }
}
