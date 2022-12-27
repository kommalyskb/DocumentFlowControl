using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class UserRegisterRequestResponse
    {
        public string? id { get; set; }
        public string? userName { get; set; }
        public string? email { get; set; }
        public bool emailConfirmed { get; set; }
        public string? phoneNumber { get; set; }
        public bool phoneNumberConfirmed { get; set; }
        public bool lockoutEnabled { get; set; }
        public bool twoFactorEnabled { get; set; }
        public int accessFailedCount { get; set; }
        public DateTime? lockoutEnd { get; set; }
    }
}
