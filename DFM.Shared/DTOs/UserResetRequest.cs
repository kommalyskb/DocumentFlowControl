using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class UserResetRequest
    {
        public string? userId { get; set; }
        public string? password { get; set; }
        public string? confirmPassword { get; set; }
    }
}
