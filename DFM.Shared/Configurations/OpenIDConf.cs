using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Configurations
{
    public class OpenIDConf
    {
        public string? Authority { get; set; }
        public string? ApiName { get; set; }
        public string? ApiSecret { get; set; }
        public string? SwaggerUIClient { get; set; }
        public string? APIScope { get; set; }
        public string? AdminClient { get; set; }
        public string? AdminSecret { get; set; }
        public string? AdminUsername { get; set; }
        public string? AdminPassword { get; set; }
        public string? AdminScope { get; set; }
    }
}
