using CouchDBService;
using DFM.Shared.Configurations;
using DFM.Shared.Extensions;
using DFM.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Repository
{
    public class NotificationManager : INotificationManager
    {

        private readonly ICouchContext couchContext;
        private readonly IRedisConnector redisConnector;
        private readonly CouchDBHelper read_couchDbHelper;
        private readonly CouchDBHelper write_couchDbHelper;
        public NotificationManager(ICouchContext couchContext, DBConfig dbConfig, IRedisConnector redisConnector)
        {
            this.couchContext = couchContext;
            this.redisConnector = redisConnector;
            this.read_couchDbHelper = new CouchDBHelper
           (
               scheme: dbConfig.Reader.Scheme,
               srvAddr: dbConfig.Reader.SrvAddr,
               dbName: "dfm_notice_db",
               username: dbConfig.Reader.Username,
               password: dbConfig.Reader.Password,
               port: dbConfig.Reader.Port
           );
            this.write_couchDbHelper = new CouchDBHelper
            (
                scheme: dbConfig.Writer.Scheme,
                srvAddr: dbConfig.Writer.SrvAddr,
                dbName: "dfm_notice_db",
                username: dbConfig.Writer.Username,
                password: dbConfig.Writer.Password,
                port: dbConfig.Writer.Port
            );

        }
    }
}
