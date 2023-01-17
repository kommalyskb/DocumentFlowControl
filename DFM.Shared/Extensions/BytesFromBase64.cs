using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Extensions
{
    public static class BytesFromBase64
    {
        public static byte[] FromBase64(this string str)
        {
            return Convert.FromBase64String(str);
        }
    }
}
