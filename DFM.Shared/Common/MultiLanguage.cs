using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class MultiLanguage
    {
        [Indexed]
        public string? Eng { get; set; }
        [Indexed]
        public string? Local { get; set; }
    }
}
