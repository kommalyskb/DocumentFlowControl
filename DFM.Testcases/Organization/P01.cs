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
                    Port = 25984,
                    SrvAddr = "202.137.130.53",
                    Username = "admin"
                },
                Reader = new DBInfo
                {
                    Scheme = "http",
                    Password = "1qaz2wsx",
                    Port = 25984,
                    SrvAddr = "202.137.130.53",
                    Username = "admin"
                }

            };
            IRedisConnector redisConnector = new RedisConnector(new RedisConf()
            {
                Server = "localhost",
                Port = 12000,
                Password = "1qaz2wsx"
            });
            organizationChart = new OrganizationChart(couchContext, dBConfig, redisConnector, new EnvConf());
            employeeManager = new EmployeeManager(couchContext, dBConfig, redisConnector, organizationChart);
            roleManager = new RoleManager(couchContext, dBConfig, redisConnector, organizationChart);
        }

        [Fact(DisplayName = "ສ້າງ Organization ໃຫມ່")]
        public async Task NC15()
        {
            var result = await organizationChart.NewOrganization(new MultiLanguage
            {

            }, new AttachmentModel
            {

            });
        }
        [Fact(DisplayName = "ເພີ່ມ Role, Employee ເຂົ້າ ໄປໃນ Organization")]
        public async Task NC1()
        {
            var myRole = await roleManager.GetRolePosition("f60ed05ebd9f4256b5b6279c994ef3fb");
            var profile = await employeeManager.GetProfile("87f978f7e0a14d44996f61b326e5d7a1");
            var result = await organizationChart.AddRoleAndEmployee("b98c5c46cebd430bb7d9fe596d73c459", new RoleTreeModel
            {
                RoleType = myRole.Content.RoleType,
                ParentID = "3f9c134674d44842a9764abe623ebc90",
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
                Publisher = "ຫົວຫນ້າຝ່າຍ ການເງິນ"
            });

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ລຶບ Role, Employee ອອກຈາກ Organization")]
        public async Task NC2()
        {
            var result = await organizationChart.RemoveRoleAndEmployee("b98c5c46cebd430bb7d9fe596d73c459", "198081ca5a774097b432d38c8931825c");

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
            var result = await organizationChart.GetChartFrom("04b2644ebf1a434f87afe53ea30f149b", "cbc5c0f679e747a1b2eb63ea3e89eded", ModuleType.DocumentInbound);
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
            var result = await organizationChart.GetRoles("b98c5c46cebd430bb7d9fe596d73c459", "0e27d2881de2420fba50eef81ed32e22");
            Assert.True(result.Response.Success);
        }
        [Fact(DisplayName = "ດຶງຂໍ້ມູນ Organization ID")]
        public async Task NC6()
        {
            var result = await organizationChart.GetChartByID("8cb7efe0be82402f8007800db1a9c3f5");
            Assert.NotEqual(0, result.RowCount);
        }

        [Fact(DisplayName = "ສະແດງ Role info ທີ່ເປັນ ຂາອອກ ແລະ ຂາເຂົ້າ ທັງຫມົດຂອງ ບໍລິສັດ ຕາມ ໂຄງຮ່າງການຈັດຕັ້ງ")]
        public async Task NC7()
        {

            var result = await organizationChart.GetSupervisorRolesPosition("b98c5c46cebd430bb7d9fe596d73c459");

            Assert.True(result.Response.Success);
        }

        [Fact(DisplayName = "ດຶງເອົາ Employee ທີ່ມີ Role ຕາມ Filter")]
        public async Task NC8()
        {
            var result = await organizationChart.GetEmployee("b98c5c46cebd430bb7d9fe596d73c459", "d35a9b8866fc4f59bcd36cd343eb471e");
            Assert.True(result.Response.Success);
        }
    }
}
