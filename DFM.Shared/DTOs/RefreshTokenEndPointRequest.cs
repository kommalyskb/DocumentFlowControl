using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class RefreshTokenEndPointRequest
    {
        public string? ClientID { get; set; }
        public string? Secret { get; set; }
        public string? RefreshToken { get; set; }
    }
}
