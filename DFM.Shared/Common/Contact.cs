using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class Contact
    {
        [Indexed]
        public string? Email { get; set; }
        [Indexed]
        public string? Phone { get; set; }
    }
}
