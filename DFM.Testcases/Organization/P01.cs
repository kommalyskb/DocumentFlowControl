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

namespace DFM.Testcases.Organization
{
    public class P01
    {
        IOrganizationChart organizationChart;
        IRoleManager roleManager;
        IEmployeeManager employeeManager;
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
            organizationChart = new OrganizationChart(couchContext, dBConfig, redisConnector);
            employeeManager = new EmployeeManager(couchContext, dBConfig, redisConnector, organizationChart);
            roleManager = new RoleManager(couchContext, dBConfig, redisConnector, organizationChart);
        }
        [Fact(DisplayName = "ເພີ່ມ Role, Employee ເຂົ້າ ໄປໃນ Organization")]
        public async Task NC1()
        {
            var myRole = await roleManager.GetRolePosition("fba6e7d73a044f84a371d615b2ffbf68");
            var profile = await employeeManager.GetProfile("96759b98206c4e83a51795dd0be84206");
            var result = await organizationChart.AddRoleAndEmployee("b98c5c46cebd430bb7d9fe596d73c459", new RoleTreeModel
            {
                RoleType = myRole.Content.RoleType,
                ParentID = "6d6ba29bba5e4bc0a432cff87518c7dc",
                Employee = new PartialEmployeeProfile
                {
                    Name = profile.Content.Name,
                    FamilyName = profile.Content.FamilyName,
                    Gender = profile.Content.Gender,
                    UserID = profile.Content.id
                },
                Role = new PartialRole
                {
                    RoleID = myRole.Content.id,
                    RoleType = myRole.Content.RoleType,
                    Display = myRole.Content.Display
                },
                Publisher = "ບໍລິສັດ ຈະເລີນເຊກອງພະລັງງານ ຈຳກັດ"
            });

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ລຶບ Role, Employee ອອກຈາກ Organization")]
        public async Task NC2()
        {
            var result = await organizationChart.RemoveRoleAndEmployee("776dca2568194b77a8fc9c7c7f377e08", "bcd52dd6296d4a44ba2b93733a699b48");

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ແກ້ໄຂຂໍ້ມູນ Role ໃນ Organization")]
        public async Task NC3()
        {
            var result = await organizationChart.EditRoleOnly("8cb7efe0be82402f8007800db1a9c3f5", new PartialRole
            {
                RoleID = "d9500fc7a9c249c68390551561f1354a",
                RoleType = RoleTypeModel.Prime,
                Display = new MultiLanguage
                {
                    Eng = "CEO",
                    Local = "ຜູ້ອຳນວຍການ"
                }
            });

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ດຶງຂໍ້ມູນ Organization ຕາມ ເງື່ອນ ທີ່ໄດ້ຮັບ: 1. ດຶງ Organization ນັບຕັ້ງແຕ່ Level X ລົງມາ")]
        public async Task NC4()
        {
            var result = await organizationChart.GetChartFrom("8cb7efe0be82402f8007800db1a9c3f5", "d9500fc7a9c249c68390551561f1354a", ModuleType.DocumentInbound);
            Assert.NotEqual(0, result.RowCount);
        }
        
        [Fact(DisplayName = "ແກ້ໄຂຂໍ້ມູນ Employee ໃນ Organization")]
        public async Task NC13()
        {
            var result = await organizationChart.EditEmployeeOnly("8cb7efe0be82402f8007800db1a9c3f5", new PartialEmployeeProfile
            {
                Name = new MultiLanguage
                {
                    Eng = "Kommaly",
                    Local = "ກົມມະລິ"
                },
                FamilyName = new MultiLanguage
                {
                    Eng = "Saykhambay",
                    Local = "ສາຍຄຳໃບ"
                },
                Gender = Gender.Male,
                UserID = "31af54c8579244a79f3882e0bed9efc0"
            });
        }
        [Fact(DisplayName = "ສ້າງ Organization ໃຫມ່")]
        public async Task NC14()
        {
            var result = await organizationChart.NewOrganization(new MultiLanguage { Eng = "CS ENERGY CO.,LTD", Local = "ບໍລິສັດ ຈະເລີນເຊກອງ ພະລັງງານ ຈຳກັດ" }, new AttachmentModel { Bucket = "", FileName = "", Version = 1 });
            Assert.True(result.Success);    
        }

        [Fact(DisplayName = "ດຶງເອົາ Role ທີ່ມີ Employee ຕາມ Filter")]
        public async Task NC5()
        {
            var result = await organizationChart.GetRoles("orgid", "sub");
            Assert.True(result.Response.Success);
        }
        [Fact(DisplayName = "ດຶງຂໍ້ມູນ Organization ID")]
        public async Task NC6()
        {
            var result = await organizationChart.GetChartByID("8cb7efe0be82402f8007800db1a9c3f5");
            Assert.NotEqual(0, result.RowCount);
        }
    }
}
