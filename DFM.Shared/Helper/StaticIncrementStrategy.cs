using Redis.OM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public class StaticIncrementStrategy : IIdGenerationStrategy
    {
        public static int Current = 0;
        public string GenerateId()
        {
            return (Current++).ToString();
        }
    }
}
