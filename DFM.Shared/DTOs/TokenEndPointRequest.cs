using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class TokenEndPointRequest
    {
        public string? ClientID { get; set; }
        public string? Secret { get; set; }
        public string? GrantType { get; set; }
        public string? Scope { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? RedirectUri { get; set; }

    }
}
