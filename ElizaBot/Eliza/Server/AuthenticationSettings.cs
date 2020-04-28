using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eliza.Server
{
    public class AuthenticationSettings
    {
        public ApplicationAuthenticationSettings Discord { get; set; }
    }

    public class ApplicationAuthenticationSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
