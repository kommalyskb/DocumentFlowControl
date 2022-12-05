using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Configurations
{
    public class DBConfig
    {
        public DBConfig()
        {
            Reader = new DBInfo();
            Writer = new DBInfo();
        }
        public DBInfo Reader { get; set; }
        public DBInfo Writer { get; set; }
    }
    public class DBInfo
    {
        public string Scheme { get; set; } = "http";
        public string SrvAddr { get; set; } = "localhost";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public int? Port { get; set; } = 5984;
    }
}
