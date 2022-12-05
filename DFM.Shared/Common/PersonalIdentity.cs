using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class PersonalIdentity
    {
        [Indexed(CascadeDepth = 1)]
        public Identity IdCard { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public Identity FamilyBook { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public Identity Passport { get; set; } = new();
    }
}
