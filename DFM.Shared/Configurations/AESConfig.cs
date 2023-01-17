using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Configurations
{
    public class AESConfig
    {
        public string? Key { get; set; }
        public string? IV { get; set; }
        public BaseConfig Base { get; set; }
    }
    public enum BaseConfig
    {
        HEX,
        BASE64
    }
}
