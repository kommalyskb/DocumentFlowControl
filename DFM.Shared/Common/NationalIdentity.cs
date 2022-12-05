using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class NationalIdentity
    {
        [Indexed]
        public string? Ethnicity { get; set; }
        [Indexed]
        public string? Nationality { get; set; }
        [Indexed]
        public string? Race { get; set; }
        [Indexed]
        public string? Tribe { get; set; }
        [Indexed]
        public string? Religion { get; set; }
    }
}
