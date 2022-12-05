using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class ShortInfo
    {
        [Indexed]
        public string? Name { get; set; }
        [Indexed]
        public string? FamilyName { get; set; }
        [Indexed]
        public string? Dob { get; set; }
        [Indexed(CascadeDepth = 1)]
        public Address Address { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public Contact Contact { get; set; } = new();
    }
}
