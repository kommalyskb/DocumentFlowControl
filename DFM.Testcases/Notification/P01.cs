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

namespace DFM.Testcases.Notification
{
    public class P01
    {
        INotificationManager notificationManager;
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
                    //SrvAddr = "localhost",
                    SrvAddr = "20.10.100.91",
                    Username = "admin"
                },
                Reader = new DBInfo
                {
                    Scheme = "http",
                    Password = "1qaz2wsx",
                    Port = 5984,
                    //SrvAddr = "localhost",
                    SrvAddr = "20.10.100.91",
                    Username = "admin"
                }

            };
            IRedisConnector redisConnector = new RedisConnector(new RedisConf()
            {
                //Server = "localhost",
                Server = "20.10.100.91",
                Port = 12000,
                Password = "1qaz2wsx"
            });
            notificationManager = new NotificationManager(couchContext, dBConfig, redisConnector);
        }

        [Fact(DisplayName = "ບັນທຶກຂໍ້ມູນໄປລົງໃນ Notification")]
        public async Task SaveNotify()
        {
            NotificationModel request = new NotificationModel
            {

            };
            var result = await notificationManager.CreateNotice(request);
            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ອັບເດດການອ່ານແຈ້ງເຕືອນ")]
        public async Task ReadNotify()
        {
            var result = await notificationManager.ReadNotice("", "");
            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ດຶງລາຍການແຈ້ງເຕືອນຕາມ RoleID")]
        public async Task ListNotifies()
        {
            var result = await notificationManager.ListNotices(default!);
            Assert.True(result.Response.Success);
        }

        [Fact(DisplayName = "ບັນທຶກລາຍການແຈ້ງເຕືອນເຂົ້າ memory")]
        public async Task PumpNotification()
        {
            
        }
    }
}
