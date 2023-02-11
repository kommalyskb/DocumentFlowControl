using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using Redis.OM;

namespace DFM.API.Repositories
{
    public class IndexCreationService : IHostedService
    {
        private readonly IRedisConnector redisConnector;

        public IndexCreationService(IRedisConnector redisConnector)
        {
            this.redisConnector = redisConnector;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
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
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
