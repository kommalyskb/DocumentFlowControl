using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public static class ValidateString
    {
        public static string IsNullOrWhiteSpace(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? "N/A" : str;
        }
    }
}
