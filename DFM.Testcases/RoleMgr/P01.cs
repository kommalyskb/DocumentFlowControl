﻿using CouchDBService;
using DFM.Shared.Configurations;
using DFM.Shared.Extensions;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFM.Testcases.RoleMgr
{
    public class P01
    {
        IRuleMenuManager manager;
        public P01()
        {
            ICouchContext couchContext = new CouchContext();
            DBConfig dBConfig = new DBConfig()
            {
                Writer = new DBInfo
                {
                    Scheme = "http",
                    Password = "1qaz2wsx",
                    Port = 5984,
                    SrvAddr = "localhost",
                    Username = "admin"
                },
                Reader = new DBInfo
                {
                    Scheme = "http",
                    Password = "1qaz2wsx",
                    Port = 5984,
                    SrvAddr = "localhost",
                    Username = "admin"
                }

            };
            IRedisConnector redisConnector = new RedisConnector(new RedisConf()
            {
                Server = "localhost",
                Port = 12000,
                Password = "1qaz2wsx"
            });
            manager = new RuleMenuManager(couchContext, dBConfig, redisConnector);
        }


        [Fact]
        public async Task UpdateRoleMenu()
        {
            var result = await manager.UpdateRules(new Shared.Entities.RuleMenu ());
        }

        [Fact]
        public async Task UpdateCache()
        {
            await manager.UpdateCache("");
        }
    }
}
