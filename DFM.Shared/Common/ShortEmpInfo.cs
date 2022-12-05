using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class ShortEmpInfo
    {
        [Indexed]
        public string? EmployeeID { get; set; }
        [Indexed]
        public MultiLanguage Name { get; set; } = new();
    }
}
