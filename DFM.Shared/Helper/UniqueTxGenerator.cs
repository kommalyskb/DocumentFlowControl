using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public static class UniqueTxGenerator
    {

        public static string NewTX()
        {

            Random random = new Random();
            int num = random.Next(1000, 9999);
            string token = $"{getTime() - 0 * 60 * 1000}{num}";
            return token;
        }

        public static decimal NewTXDecimal()
        {
            string token = $"{getTime() - 0 * 60 * 1000}";
            return Convert.ToDecimal(token);
        }

        private static double getTime()
        {
            var dt = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var d = dt.Ticks / 10000d;
            return Math.Truncate(d);
        }
        public static string NewTX(int digit)
        {
            string base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var uid = Regex.Replace(base64Guid, "[/+=]", "");

            return uid.Length > digit ? uid.Substring(0, digit) : uid;
        }
    }
}
