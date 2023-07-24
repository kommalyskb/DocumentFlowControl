using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
using RandomNameGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFM.Testcases.Employee
{
    public class P01
    {
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
                    SrvAddr = "20.10.100.91",
                    Username = "admin"
                },
                Reader = new DBInfo
                {
                    Scheme = "http",
                    Password = "1qaz2wsx",
                    Port = 5984,
                    SrvAddr = "20.10.100.91",
                    Username = "admin"
                }

            };
            IRedisConnector redisConnector = new RedisConnector(new RedisConf()
            {
                Server = "20.10.100.91",
                Port = 12000,
                Password = "1qaz2wsx"
            });
            IOrganizationChart organizationChart = new OrganizationChart(couchContext, dBConfig, redisConnector, new EnvConf(), default!);
            employeeManager = new EmployeeManager(couchContext, dBConfig, redisConnector, organizationChart);
        }
        [Fact]
        public void TestName()
        {
            Random rand = new Random(DateTime.Now.Second); // we need a random variable to select names randomly

            RandomName nameGen = new RandomName(rand); // create a new instance of the RandomName class

            string name = nameGen.GenerateOnlyName(Sex.Male); // generate a male name, with one middal name.
            string surname = nameGen.GenerateOnlyName(Sex.Male);
        }
        [Fact(DisplayName = "ເພີ່ມ ພະນັກງານ ໃຫມ່")]
        public async Task NC1()
        {
            //for (int i = 0; i < 19; i++)
            //{
            //    Random rand = new Random(DateTime.Now.Second); // we need a random variable to select names randomly

            //    RandomName nameGen = new RandomName(rand); // create a new instance of the RandomName class

            //    string name = nameGen.GenerateOnlyName(Sex.Male); // generate a male name, with one middal name.
            //    string surname = nameGen.GenerateOnlyName(Sex.Male);
            //    var result = await employeeManager.NewEmployeeProfile(new EmployeeModel
            //    {
            //        id = Guid.NewGuid().ToString("N"),
            //        OrganizationID = "b98c5c46cebd430bb7d9fe596d73c459",
            //        Name = new MultiLanguage { Eng = name, Local = name },
            //        FamilyName = new MultiLanguage { Eng = surname, Local = surname }
            //    });
            //}

            Random rand = new Random(DateTime.Now.Second); // we need a random variable to select names randomly

            RandomName nameGen = new RandomName(rand); // create a new instance of the RandomName class

            string name = nameGen.GenerateOnlyName(Sex.Female); // generate a male name, with one middal name.
            string surname = nameGen.GenerateOnlyName(Sex.Female);
            var result = await employeeManager.NewEmployeeProfile(new EmployeeModel
            {
                id = Guid.NewGuid().ToString("N"),
                OrganizationID = "b98c5c46cebd430bb7d9fe596d73c459",
                Name = new MultiLanguage { Eng = name, Local = name },
                FamilyName = new MultiLanguage { Eng = surname, Local = surname }
            });

            Assert.True(true);
        }

        [Fact(DisplayName = "ແກ້ໄຂ ຂໍ້ມູນ ພະນັກງານ")]
        public async Task NC2()
        {
            var result = await employeeManager.EditEmployeeProfile(new EmployeeModel
            {
                id = "31af54c8579244a79f3882e0bed9efc0",
                OrganizationID = "312038d8cc284e1fab33aa4df5173c84",
                Name = new MultiLanguage { Eng = "Kommaly", Local = "ກົມມະລິ"}
            });

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ລຶບ ພະນັກງານ")]
        public async Task NC3()
        {
            var result = await employeeManager.RemoveEmployeeProfile("31af54c8579244a79f3882e0bed9efc0");

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ສະແດງ ພະນັກງານ ທັງຫມົດ ຕາມ OrgID")]
        public async Task NC4()
        {
            var result = await employeeManager.GetProfileByOrgId("312038d8cc284e1fab33aa4df5173c84");

            Assert.NotEqual(0, result.RowCount);
        }
        [Fact(DisplayName = "ສະແດງ ພະນັກງານ info ຕາມ ID ພະນັກງານ")]
        public async Task NC5()
        {
            var result = await employeeManager.GetProfile("962f5b83-9212-4872-9bcd-da5852f7794b");

            Assert.True(result.Response.Success);
        }
        
    }
}
