using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class FamilyInfo
    {
        [Indexed]
        public ShortInfo FatherInfo { get; set; } = new();
        [Indexed]
        public ShortInfo MotherInfo { get; set; } = new();
        [Indexed]
        public ShortInfo CoupleInfo { get; set; } = new();

    }
}
