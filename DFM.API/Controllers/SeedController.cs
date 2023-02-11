using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Redis.OM;
using System.Diagnostics;
using System.Threading.Channels;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Authorize]
    public class SeedController : ControllerBase
    {
        private readonly ICouchContext couchContext;
        private readonly DBConfig dbConfig;
        private readonly IRedisConnector redisConnector;

        public SeedController(ICouchContext couchContext, DBConfig dbConfig, IRedisConnector redisConnector)
        {
            this.couchContext = couchContext;
            this.dbConfig = dbConfig;
            this.redisConnector = redisConnector;
        }

        /// <summary>
        /// v1.0.0 
        /// ແມ່ນການ Seed Database ແລະ re-index
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(int? reindexOnly, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                // Re-Index Redis
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                // re-create index when new deploy version
                await Task.WhenAll(provider.Connection.DropIndexAsync(typeof(RoleManagementModel)),
                provider.Connection.DropIndexAsync(typeof(OrganizationModel)),
                provider.Connection.DropIndexAsync(typeof(FolderModel)),
                provider.Connection.DropIndexAsync(typeof(EmployeeModel)),
                provider.Connection.DropIndexAsync(typeof(DynamicFlowModel)),
                provider.Connection.DropIndexAsync(typeof(DocumentUrgentModel)),
                provider.Connection.DropIndexAsync(typeof(DocumentSecurityModel)),
                provider.Connection.DropIndexAsync(typeof(DocumentModel)),
                provider.Connection.DropIndexAsync(typeof(DataTypeModel)),
                provider.Connection.DropIndexAsync(typeof(MinioLinkCache)),
                provider.Connection.DropIndexAsync(typeof(TokenEndPointResponse)),
                provider.Connection.DropIndexAsync(typeof(NotificationModel))
                );


                await Task.WhenAll(provider.Connection.CreateIndexAsync(typeof(RoleManagementModel)),
                    provider.Connection.CreateIndexAsync(typeof(OrganizationModel)),
                    provider.Connection.CreateIndexAsync(typeof(FolderModel)),
                    provider.Connection.CreateIndexAsync(typeof(EmployeeModel)),
                    provider.Connection.CreateIndexAsync(typeof(DynamicFlowModel)),
                    provider.Connection.CreateIndexAsync(typeof(DocumentUrgentModel)),
                    provider.Connection.CreateIndexAsync(typeof(DocumentSecurityModel)),
                    provider.Connection.CreateIndexAsync(typeof(DocumentModel)),
                    provider.Connection.CreateIndexAsync(typeof(DataTypeModel)),
                    provider.Connection.CreateIndexAsync(typeof(MinioLinkCache)),
                    provider.Connection.CreateIndexAsync(typeof(TokenEndPointResponse)),
                    provider.Connection.CreateIndexAsync(typeof(NotificationModel))
                    );

                if (reindexOnly == 1)
                {

                    stopwatch.Stop();

                    return Ok(new CommonResponse
                    {
                        Code = "200",
                        Success = true,
                        Message = "Succes",
                        Detail = $"Complete Re-Index only {stopwatch.Elapsed.TotalSeconds}"
                    });
                }

                List<Task> allCreateTableTasks = new();
                allCreateTableTasks.Add(tryCreateDatabase("dfm_doctype_db"));
                allCreateTableTasks.Add(tryCreateDatabase("dfm_document_db"));
                allCreateTableTasks.Add(tryCreateDatabase("dfm_dynamic_db"));
                allCreateTableTasks.Add(tryCreateDatabase("dfm_employee_db"));
                allCreateTableTasks.Add(tryCreateDatabase("dfm_folder_db"));
                allCreateTableTasks.Add(tryCreateDatabase("dfm_organization_db"));
                allCreateTableTasks.Add(tryCreateDatabase("dfm_role_db"));
                allCreateTableTasks.Add(tryCreateDatabase("dfm_security_db"));
                allCreateTableTasks.Add(tryCreateDatabase("dfm_urgent_db"));
                allCreateTableTasks.Add(tryCreateDatabase("dfm_notice_db"));

                await Task.WhenAll(allCreateTableTasks);

                List<Task> allCreateViewTasks = new();
                allCreateViewTasks.Add(tryCreateView("dfm_doctype_db", "doctype.json"));
                allCreateViewTasks.Add(tryCreateView("dfm_document_db", "documentdb.json"));
                allCreateViewTasks.Add(tryCreateView("dfm_employee_db", "employee.json"));
                allCreateViewTasks.Add(tryCreateView("dfm_folder_db", "folder.json"));
                allCreateViewTasks.Add(tryCreateView("dfm_role_db", "role.json"));
                allCreateViewTasks.Add(tryCreateView("dfm_security_db", "security.json"));
                allCreateViewTasks.Add(tryCreateView("dfm_urgent_db", "urgent.json"));

                await Task.WhenAll(allCreateViewTasks);

                stopwatch.Stop();

                return Ok(new CommonResponse
                {
                    Code = "200",
                    Success = true,
                    Message = "Succes",
                    Detail = $"Complete Create Database with view and Re-Index in {stopwatch.Elapsed.TotalSeconds}"
                });
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task tryCreateDatabase(string dbName)
        {
            var dbHelper = new CouchDBHelper
            (
                scheme: dbConfig.Writer.Scheme,
                srvAddr: dbConfig.Writer.SrvAddr,
                dbName: dbName,
                username: dbConfig.Writer.Username,
                password: dbConfig.Writer.Password,
                port: dbConfig.Writer.Port
            );
            var result = await couchContext.CheckThenCreateDatabase(dbHelper);

        }
        private async Task tryCreateView(string dbName, string viewFile)
        {
            var dbHelper = new CouchDBHelper
            (
                scheme: dbConfig.Writer.Scheme,
                srvAddr: dbConfig.Writer.SrvAddr,
                dbName: dbName,
                username: dbConfig.Writer.Username,
                password: dbConfig.Writer.Password,
                port: dbConfig.Writer.Port
            );
            var jsonStr = System.IO.File.ReadAllText($"Seed/{viewFile}");
            var result = await couchContext.CreateDatabaseView(dbHelper, jsonStr);

        }
    }
}
