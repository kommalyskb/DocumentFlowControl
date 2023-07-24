using CouchDBService;
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
using DFM.Shared.Entities;
using DFM.Shared.Common;

namespace DFM.Testcases.Role
{
    public class P01
    {
        IRoleManager roleManager;
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
                User = "default",
                Password = "1qaz2wsx"
            });
            IOrganizationChart organizationChart = new OrganizationChart(couchContext, dBConfig, redisConnector, new EnvConf(), default!);
            roleManager = new RoleManager(couchContext, dBConfig, redisConnector, organizationChart);
        }

        [Fact(DisplayName = "ເພີ່ມ Role ໃຫມ່")]
        public async Task NC1()
        {
            var result = await roleManager.NewRolePosition(new RoleManagementModel
            {
                id = Guid.NewGuid().ToString("N"),
                Display = new MultiLanguage
                {
                    Eng = "Outbound Finance",
                    Local = "ຂາອອກ ຝ່າຍການເງິນ"
                },
                OrganizationID = "b98c5c46cebd430bb7d9fe596d73c459",
                RoleType = RoleTypeModel.OutboundGeneral,
                
            });

            Assert.True(result.Success);
        }

        [Fact(DisplayName = "ແກ້ໄຂ ຂໍ້ມູນ Role")]
        public async Task NC2()
        {
            var result = await roleManager.EditRolePosition(new RoleManagementModel
            {
                id = "9aede16f250c49feb7e4cc581a224874",
                Display = new MultiLanguage
                {
                    Eng = "Vice Office Administrators I",
                    Local = "ຮອງຫ້ອງການ ບໍລິສັດ I"
                },
                OrganizationID = "b98c5c46cebd430bb7d9fe596d73c459",
                RoleType = RoleTypeModel.DeputyOfficePrime,
            });

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ລຶບ Role")]
        public async Task NC3()
        {
            var result = await roleManager.RemoveRolePosition("bd911b1f01974ef39d61164bef53cdcc");

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ສະແດງ Role ທັງຫມົດ ຕາມ OrganizationID")]
        public async Task NC4()
        {
            var result = await roleManager.GetRolePositionByOrgID("312038d8cc284e1fab33aa4df5173c84");

            Assert.NotEqual(0, result.RowCount);
        }
        [Fact(DisplayName = "ສະແດງ Role info ຕາມ RoleID")]
        public async Task NC5()
        {
            var result = await roleManager.GetRolePosition("bd911b1f01974ef39d61164bef53cdcc");

            Assert.True(result.Response.Success);
        }

        [Fact(DisplayName = "ສະແດງ Role info ຕາມ RoleIDs")]
        public async Task NC6()
        {
            List<string> roles = new List<string>
            {
                "41b1585dd1e643569aa67f9fdc249547", "d10481da4d6c469db2ba67b1ff603177", "198081ca5a774097b432d38c8931825c"
            };
            var result = await roleManager.GetRolesPosition(roles);

            Assert.True(result.Response.Success);
        }

       
    }
}
