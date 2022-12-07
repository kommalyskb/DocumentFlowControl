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

namespace DFM.Testcases.DynamicFlow
{
    public class P01
    {
        IOrganizationChart organizationChart;
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
        }
        [Fact(DisplayName = "ເພີ່ມ ຫຼື ແກ້ໄຂ Dynamic Flow ")]
        public async Task NC1()
        {
            string? orgID = "b98c5c46cebd430bb7d9fe596d73c459";
            RoleTypeModel source = RoleTypeModel.OutboundOfficePrime;
            List<RoleTypeModel> target = new List<RoleTypeModel> { RoleTypeModel.InboundGeneral, RoleTypeModel.OutboundPrime };
            var result = await organizationChart.SaveDynamicFlow(orgID, source, target, ModuleType.DocumentOutbound);
        }

        [Fact(DisplayName = "ລຶບ Dynamic Flow")]
        public async Task NC3()
        {

        }
        [Fact(DisplayName = "ສະແດງ Dynamic Flow ທັງຫມົດ ຕາມ RoleType")]
        public async Task NC4()
        {

        }
        [Fact(DisplayName = "ສະແດງ Dynamic Flow info ຕາມ Dynamic FlowID (ID)")]
        public async Task NC5()
        {
            string? orgID = "776dca2568194b77a8fc9c7c7f377e08";
            var result = await organizationChart.GetDynamicFlowByID(orgID, ModuleType.DocumentInbound);
        }
    }
}
