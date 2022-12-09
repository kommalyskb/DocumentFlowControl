using DFM.Shared.Configurations;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFM.Shared.Extensions
{
    public class RedisConnector : IRedisConnector
    {
        public RedisConnector(RedisConf configuration)
        {

            Console.WriteLine($"Redis: - {JsonSerializer.Serialize(configuration)}");
            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(ConfigurationOptions.Parse($"{configuration.Server}:{configuration.Port}, password={configuration.Password}"));
                //if (configuration.Mode == RunTimeMode.Prod)
                //{
                //    var options = ConfigurationOptions.Parse($"{configuration.Server}:{configuration.Port}");
                //    options.User = configuration.User;
                //    options.Password = configuration.Password;
                //    options.Ssl = true;
                //    return ConnectionMultiplexer.Connect(options);
                //}
                //else
                //{
                //    return ConnectionMultiplexer.Connect(ConfigurationOptions.Parse($"{configuration.Server}:{configuration.Port}"));
                //}

            });
        }

        private Lazy<ConnectionMultiplexer> lazyConnection;

        public ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }


    }
}
