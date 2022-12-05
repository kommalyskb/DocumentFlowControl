using CouchDBService;
using DFM.Shared.Configurations;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFM.Testcases.DocUrgentLevel
{
    public class P01
    {
        IDocumentUrgent documentUrgent;
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
            documentUrgent = new DocumentUrgent(couchContext, dBConfig, redisConnector);
        }
        [Fact(DisplayName = "ເພີ່ມ ລະດັບຄວາມເລັ່ງດ່ວນ ໃຫມ່")]
        public async Task NC1()
        {
            var result = await documentUrgent.NewUrgentLevel(new DocumentUrgentModel
            {
                id = Guid.NewGuid().ToString("N"),
                Level = "ດ່ວນສຸດ",
                OrganizationID = "776dca2568194b77a8fc9c7c7f377e08",
            });

            Assert.True(result.Success);
        }

        [Fact(DisplayName = "ແກ້ໄຂ ຂໍ້ມູນ ລະດັບຄວາມເລັ່ງດ່ວນ")]
        public async Task NC2()
        {
            var result = await documentUrgent.EditUrgentLevel(new DocumentUrgentModel
            {
                id = "b5e9706a47b340c7954484c2dbcd51f1",
                Level = "ດ່ວນສຸດ",
                OrganizationID = "b98c5c46cebd430bb7d9fe596d73c459",
            });

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ລຶບ ລະດັບຄວາມເລັ່ງດ່ວນ")]
        public async Task NC3()
        {
            var result = await documentUrgent.RemoveUrgentLevel("73201bd296474dbe81b78aca49085dba");

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ສະແດງ ລະດັບຄວາມເລັ່ງດ່ວນ ທັງຫມົດ ຕາມ OrgId")]
        public async Task NC4()
        {
            var result = await documentUrgent.GetUrgentLevelByOrgId("312038d8cc284e1fab33aa4df5173c84");

            Assert.NotEqual(0, result.RowCount);
        }
        [Fact(DisplayName = "ສະແດງ ລະດັບຄວາມເລັ່ງດ່ວນ info ຕາມ ລະດັບຄວາມເລັ່ງດ່ວນID")]
        public async Task NC5()
        {
            var result = await documentUrgent.GetUrgentLevel("73201bd296474dbe81b78aca49085dba");

            Assert.True(result.Response.Success);
        }
    }
}
