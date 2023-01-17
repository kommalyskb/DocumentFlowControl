using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Extensions
{
    public static class BytesToHEX
    {
        public static string ToHEX(this byte[] bytes)
        {
            return Convert.ToHexString(bytes);
        }
    }
}
