using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class SocketTopic
    {
        public SocketType SocketType { get; set; }
        public string? UserID { get; set; }
        public List<string>? RoleIDs { get; set; }
    }

    public class SocketSendModel
    {
        public SocketTopic? Topic { get; set; } = new();
        public string? Message { get; set; }
    }

    public enum SocketType
    {
        READ_NOTIIFY,
        PUSH_NOTIFY
    }
}
