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

namespace DFM.Testcases.DocType
{
    public class P01
    {
        IDocumentType documentType;
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

            documentType = new DocumentType(couchContext, dBConfig, redisConnector);
        }
        [Fact(DisplayName = "ເພີ່ມ ປະເພດເອກະສານ ໃຫມ່")]
        public async Task NC1()
        {
            var result = await documentType.NewDocumentType(new DataTypeModel
            {
                id = Guid.NewGuid().ToString("N"),
                OrganizationID = "776dca2568194b77a8fc9c7c7f377e08",
                DocType = "ຫນັງສືແຈ້ງການ"
                
            });

            Assert.True(result.Success);
        }

        [Fact(DisplayName = "ແກ້ໄຂ ຂໍ້ມູນ ປະເພດເອກະສານ")]
        public async Task NC2()
        {
            var result = await documentType.EditDocumentType(new DataTypeModel
            {
                id = "e53c0d4235dc4f7d8da067afbe90dbd6",
                OrganizationID = "b98c5c46cebd430bb7d9fe596d73c459",
                DocType = "ໃບສັ່ງຊື້"
            });

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ລຶບ ປະເພດເອກະສານ")]
        public async Task NC3()
        {
            var result = await documentType.RemoveDocumentType("5dd8a023799c40aab7a6ada4e57b97f9");

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ສະແດງ ປະເພດເອກະສານ ທັງຫມົດ ຕາມ OrgId")]
        public async Task NC4()
        {
            var result = await documentType.GetDocumentTypeByOrgId("312038d8cc284e1fab33aa4df5173c84");

            Assert.NotEqual(0, result.RowCount);
        }
        [Fact(DisplayName = "ສະແດງ ປະເພດເອກະສານ info ຕາມ ປະເພດເອກະສານID")]
        public async Task NC5()
        {
            var result = await documentType.GetDocumentType("5dd8a023799c40aab7a6ada4e57b97f9");

            Assert.True(result.Response.Success);
        }
    }
}
