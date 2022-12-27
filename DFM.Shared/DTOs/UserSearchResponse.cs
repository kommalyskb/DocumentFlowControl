using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class UserSearchResponse
    {
        public int pageSize { get; set; }
        public int totalCount { get; set; }
        public List<UserSearch>? users { get; set; } = new();
    }

    public class UserSearch
    {
        public string? userName { get; set; }
        public string? email { get; set; }
        public bool emailConfirmed { get; set; }
        public string? phoneNumber { get; set; }
        public bool phoneNumberConfirmed { get; set; }
        public bool lockoutEnabled { get; set; }
        public bool twoFactorEnabled { get; set; }
        public int accessFailedCount { get; set; }
        public string? lockoutEnd { get; set; }
        public string? id { get; set; }
    }
}
