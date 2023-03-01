using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Extensions;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFM.Testcases.Report
{
    public class P01
    {
        IDocumentTransaction documentService;
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
                Port = 6379
            });
            IOrganizationChart organizationChart = new OrganizationChart(couchContext, dBConfig, redisConnector, new EnvConf());
            IRoleManager roleManager = new RoleManager(couchContext, dBConfig, redisConnector, organizationChart);
            documentService = new DocumentTransaction(couchContext, dBConfig, redisConnector, roleManager);
        }

        [Fact]
        public async Task PersonalReport()
        {
            List<string> roleIDs = new List<string> { "b0bdfe59e30c434a8d990610c1e93aa0", "fba6e7d73a044f84a371d615b2ffbf68" };

            var result = await documentService.GetPersonalReport(new GetPersonalReportRequest
            {
                // InboxType.Inbound, roleIDs, 20221201000000, 20231231000000
                inboxType = InboxType.Inbound,
                roleIDs = roleIDs,
                start = 20221201000000,
                end = 20231231000000
            });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task DrillDownReport()
        {
            List<string> roleIDs = new List<string> { "b0bdfe59e30c434a8d990610c1e93aa0", "fba6e7d73a044f84a371d615b2ffbf68" };
            var result = await documentService.DrillDownReport(new GetPersonalReportRequest
            {
                // InboxType.Inbound, roleIDs, 20221201000000, 20231231000000
                inboxType = InboxType.Inbound,
                roleIDs = roleIDs,
                start = 20221201000000,
                end = 20231231000000
            }, TraceStatus.Draft);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Dashboard()
        {

            var result = await documentService.GetDashboard(new GetDashboardRequest
            {
                // InboxType.Inbound, roleIDs, 20221201000000, 20231231000000
                inboxType = InboxType.Inbound,
                roleID = "b0bdfe59e30c434a8d990610c1e93aa0"
            });

            Assert.NotNull(result);
        }
    }
}
