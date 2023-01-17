using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Configurations
{
    public class SMTPConf
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool IsActivate { get; set; }
    }
}
