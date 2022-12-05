using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class Identity
    {
        [Indexed]
        public string? Number { get; set; }
        [Indexed]
        public string? IssuedDate { get; set; }
        [Indexed]
        public string? IssuedBy { get; set; }
        [Indexed]
        public string? IssuedPlace { get; set; }
        [Indexed]
        public string? ExpiredDate { get; set; }

    }
}
