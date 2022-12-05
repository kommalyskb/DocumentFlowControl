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

namespace DFM.Testcases.DocSecurity
{
    public class P01
    {
        IDocumentSecurity documentSecurity;
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
            documentSecurity = new DocumentSecurity(couchContext, dBConfig, redisConnector);
        }
        [Fact(DisplayName = "ເພີ່ມ ລະດັບຄວາມປອດໄພ ໃຫມ່")]
        public async Task NC1()
        {
            var result = await documentSecurity.NewSecurityLevel(new DocumentSecurityModel
            {
                id = Guid.NewGuid().ToString("N"),
                Level = "ລັບ",
                OrganizationID = "776dca2568194b77a8fc9c7c7f377e08",
            });

            Assert.True(result.Success);
        }

        [Fact(DisplayName = "ແກ້ໄຂ ຂໍ້ມູນ ລະດັບຄວາມປອດໄພ")]
        public async Task NC2()
        {
            var result = await documentSecurity.EditSecurityLevel(new DocumentSecurityModel
            {
                id = "bcb1d31edc304eeabfe6ba1f6a2641a6",
                Level = "ທົ່ວໄປ",
                OrganizationID = "b98c5c46cebd430bb7d9fe596d73c459",
            });

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ລຶບ ລະດັບຄວາມປອດໄພ")]
        public async Task NC3()
        {
            var result = await documentSecurity.RemoveSecurityLevel("22addeb6a18848c5b84408976fda7c72");

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ສະແດງ ລະດັບຄວາມປອດໄພ ທັງຫມົດ ຕາມ OrgId")]
        public async Task NC4()
        {
            var result = await documentSecurity.GetSecurityLevelByOrgId("312038d8cc284e1fab33aa4df5173c84");

            Assert.NotEqual(0, result.RowCount);
        }
        [Fact(DisplayName = "ສະແດງ ລະດັບຄວາມປອດໄພ info ຕາມ ລະດັບຄວາມປອດໄພID")]
        public async Task NC5()
        {
            var result = await documentSecurity.GetSecurityLevel("22addeb6a18848c5b84408976fda7c72");

            Assert.True(result.Response.Success);

        }
    }
}
