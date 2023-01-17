using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Configurations
{

    public class FrontendIdentity
    {
        public string? ClientID { get; set; }
        public string? Secret { get; set; }
        public string? Scope { get; set; }
    }

}
