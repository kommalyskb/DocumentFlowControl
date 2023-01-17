using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Extensions
{
    public static class BytesFromHEX
    {
        public static byte[] FromHEX(this string str)
        {
            return Convert.FromHexString(str);
        }
    }
}
