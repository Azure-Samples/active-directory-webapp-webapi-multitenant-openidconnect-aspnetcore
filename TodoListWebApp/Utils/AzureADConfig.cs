using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListWebApp.Utils
{
    public class AzureADConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorityFormat { get; set; }
        public string RedirectUri { get; set; }
        public string GraphResourceId { get; set; }
        public string GraphBaseEndpoint { get; set; }
        public string GraphApiVersion { get; set; }
    }
}
