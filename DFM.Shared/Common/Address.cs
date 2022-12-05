using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class Address
    {
        [Indexed]
        public string? Village { get; set; }
        [Indexed]
        public string? District { get; set; }
        [Indexed]
        public string? Province { get; set; }
        [Indexed]
        public string? HouseNo { get; set; }
        [Indexed]
        public string? UnitNo { get; set; }

    }
}
