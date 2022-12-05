using DFM.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class TokenEndPointResponse : CommonResponse
    {
        public string? AccessToken { get; set; }
        public int? Expire { get; set; }
        public string? RefreshToken { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
