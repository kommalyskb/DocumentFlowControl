using CouchDBService;
using DFM.Shared.Common;
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

namespace DFM.Testcases.FolderMgr
{
    public class P01
    {
        IFolderManager folderManager;
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
            folderManager = new FolderManager(couchContext, dBConfig, redisConnector);
        }

        [Fact(DisplayName = "ເພີ່ມ ແຟ້ມເອກະສານ ໃຫມ່")]
        public async Task NC1()
        {
            var result = await folderManager.NewFolder(new FolderModel
            {
                id = Guid.NewGuid().ToString("N"),
                OrganizationID = "b98c5c46cebd430bb7d9fe596d73c459",
                ExpiredDate = "2022-12-31",
                FormatType = "$docno/$sn/$yyyy",
                Seq = 1,
                Prefix = "00",
                ShortName = "CSE",
                Start = 1,
                StartDate = "2022-09-01",
                Title = "ແຟ້ມເອກະສານ ໃບບິນຮັບເງິນ 2022",
                InboxType = InboxType.Outbound,
                NextNumber = 1,
                
            });

            Assert.True(result.Success);
        }

        [Fact(DisplayName = "ແກ້ໄຂ ຂໍ້ມູນ ແຟ້ມເອກະສານ")]
        public async Task NC2()
        {
            var result = await folderManager.EditFolder(new FolderModel
            {
                id = "0defb85badf84cd6908f520a323bc4ea",
                OrganizationID = "b98c5c46cebd430bb7d9fe596d73c459",
                ExpiredDate = "2022-12-31",
                FormatType = "$docno/$sn/$yyyy",
                Seq = 1,
                Prefix = "00",
                ShortName = "CSE",
                Start = 1,
                StartDate = "2022-09-01",
                Title = "ແຟ້ມເອກະສານ ໃບບິນຮັບເງິນ 2022",
                InboxType = InboxType.Outbound,
                NextNumber = 1,
                Supervisors = new List<string> { "b451225800c549a9bc83acc72dcb06d3", "41b1585dd1e643569aa67f9fdc249547", "cc680d2372124c869c77a3712e19da90", "38c2a99355e34e17916c9bce34de878c" },
                SupportDocTypes = new List<string> { "561be39e55c546d8b0a6c54a0dfb69f4", "70caf78d0b8149abb14d58b231c8e3ba", "e53c0d4235dc4f7d8da067afbe90dbd6" }
            });

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ລຶບ ແຟ້ມເອກະສານ")]
        public async Task NC3()
        {
            var result = await folderManager.RemoveFolder("7f3a3c5ecf254f2189a3513ccf3a73aa");

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ສະແດງ ແຟ້ມເອກະສານ ທັງຫມົດ ຕາມ RoleID ທີ່ສາມາດນຳໃຊ້ໄດ້")]
        public async Task NC4()
        {
            var result = await folderManager.GetFolderByRoleID("3ec60be4c74642009da765045a979bd3", InboxType.Inbound);

            Assert.NotEqual(0, result.RowCount);
        }
        [Fact(DisplayName = "ສະແດງ ແຟ້ມເອກະສານ info ຕາມ ແຟ້ມເອກະສານID")]
        public async Task NC5()
        {
            var result = await folderManager.GetFolder("3d67c9baa350458485c6dd704a92115d");

            Assert.True(result.Response.Success);
        }
    }
}
