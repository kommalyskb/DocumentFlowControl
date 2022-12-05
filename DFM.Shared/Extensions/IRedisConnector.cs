using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Extensions
{
    public interface IRedisConnector
    {
        ConnectionMultiplexer Connection { get; }
    }
}
