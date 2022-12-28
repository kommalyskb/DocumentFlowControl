using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Configurations
{
    public class SMTPConfig
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public string? Username { get; internal set; }
        public string? Password { get; internal set; }
        public bool IsActivate { get; set; }
    }
}
