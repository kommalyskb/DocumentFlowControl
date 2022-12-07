using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using MyCouch.Requests;
using Redis.OM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFM.Shared.Repository
{
    public class RoleManager : IRoleManager
    {
        private readonly ICouchContext couchContext;
        private readonly IRedisConnector redisConnector;
        private readonly IOrganizationChart organizationChart;
        private readonly CouchDBHelper read_couchDbHelper;
        private readonly CouchDBHelper write_couchDbHelper;

        public RoleManager(ICouchContext couchContext, DBConfig dbConfig, IRedisConnector redisConnector, IOrganizationChart organizationChart)
        {
            this.couchContext = couchContext;
            this.redisConnector = redisConnector;
            this.organizationChart = organizationChart;
            this.read_couchDbHelper = new CouchDBHelper
           (
               scheme: dbConfig.Reader.Scheme,
               srvAddr: dbConfig.Reader.SrvAddr,
               dbName: "dfm_role_db",
               username: dbConfig.Reader.Username,
               password: dbConfig.Reader.Password,
               port: dbConfig.Reader.Port
           );
            this.write_couchDbHelper = new CouchDBHelper
            (
                scheme: dbConfig.Writer.Scheme,
                srvAddr: dbConfig.Writer.SrvAddr,
                dbName: "dfm_role_db",
                username: dbConfig.Writer.Username,
                password: dbConfig.Writer.Password,
                port: dbConfig.Writer.Port
            );
        }

        public async Task<CommonResponse> NewRolePosition(RoleManagementModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<RoleManagementModel>();

                if (request.RoleType == RoleTypeModel.Prime)
                {
                    var isExistingPrime = context.Any(x => x.OrganizationID == request.OrganizationID && x.RoleType == RoleTypeModel.Prime);
                    if (isExistingPrime)
                    {
                        return new CommonResponse
                        {
                            Code = nameof(ResultCode.DUPPLICATE_ROLE),
                            Detail = ResultCode.DUPPLICATE_ROLE,
                            Success = false,
                            Message = ResultCode.DUPPLICATE_ROLE
                        };
                    }
                }

                if (request.RoleType == RoleTypeModel.OfficePrime)
                {
                    var isExistingPrime = context.Any(x => x.OrganizationID == request.OrganizationID && x.RoleType == RoleTypeModel.OfficePrime);
                    if (isExistingPrime)
                    {
                        return new CommonResponse
                        {
                            Code = nameof(ResultCode.DUPPLICATE_ROLE),
                            Detail = ResultCode.DUPPLICATE_ROLE,
                            Success = false,
                            Message = ResultCode.DUPPLICATE_ROLE
                        };
                    }
                }

                var result = await couchContext.InsertAsync<RoleManagementModel>
                               (
                                   couchDBHelper: write_couchDbHelper,
                                   model: request,
                                   cancellationToken: cancellationToken
                               );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Role}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(request));
                    request.rev = result.Rev;
                    await context.InsertAsync(request);


                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.SUCCESS_OPERATION
                    };
                }
                else
                {
                    return new CommonResponseId()
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = nameof(ResultCode.REQUEST_FAIL),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.REQUEST_FAIL
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }
            

        }


        public async Task<CommonResponse> EditRolePosition(RoleManagementModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record

                var existing = await GetRolePosition(request.id!, cancellationToken);

                if (!existing.Response.Success)
                {
                    return new CommonResponseId()
                    {
                        Id = request.id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }
                request.rev = existing.Content.rev;
                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<RoleManagementModel>();

                if (request.RoleType == RoleTypeModel.Prime)
                {
                    var isExistingPrime = context.Any(x => x.OrganizationID == request.OrganizationID && x.RoleType == RoleTypeModel.Prime);
                    if (isExistingPrime)
                    {
                        return new CommonResponse
                        {
                            Code = nameof(ResultCode.DUPPLICATE_ROLE),
                            Detail = ResultCode.DUPPLICATE_ROLE,
                            Success = false,
                            Message = ResultCode.DUPPLICATE_ROLE
                        };
                    }
                }

                if (request.RoleType == RoleTypeModel.OfficePrime)
                {
                    var isExistingPrime = context.Any(x => x.OrganizationID == request.OrganizationID && x.RoleType == RoleTypeModel.OfficePrime);
                    if (isExistingPrime)
                    {
                        return new CommonResponse
                        {
                            Code = nameof(ResultCode.DUPPLICATE_ROLE),
                            Detail = ResultCode.DUPPLICATE_ROLE,
                            Success = false,
                            Message = ResultCode.DUPPLICATE_ROLE
                        };
                    }
                }

                var result = await couchContext.EditAsync<RoleManagementModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: request,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //----------Update Organization
                    await organizationChart.EditRoleOnly(request.OrganizationID!, new PartialRole
                    {
                        RoleID = request.id,
                        Display = request.Display,
                        RoleType = request.RoleType
                    }, cancellationToken);
                    //-----------------------------
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Role}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(request));
                    request.rev = result.Rev;
                    await context.UpdateAsync(request);


                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.SUCCESS_OPERATION
                    };
                }
                else
                {
                    return new CommonResponseId()
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = nameof(ResultCode.REQUEST_FAIL),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.REQUEST_FAIL
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }
            

        }

        public async Task<CommonResponse> RemoveRolePosition(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record
                var existing = await GetRolePosition(id, cancellationToken);

                if (!existing.Response.Success)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<RoleManagementModel>();

                // Remove
                var result = await couchContext.DeleteAsync
                   (
                       couchDBHelper: write_couchDbHelper,
                       id: id,
                       rev: existing.Content.rev,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //----------Update Organization
                    await organizationChart.RemoveRoleAndEmployee(existing.Content.OrganizationID!, id, cancellationToken);
                    //-----------------------------

                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Role}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.KeyDeleteAsync(recordKey);
                    await context.DeleteAsync(existing.Content);


                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.SUCCESS_OPERATION
                    };
                }
                else
                {
                    return new CommonResponseId()
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = nameof(ResultCode.REQUEST_FAIL),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.REQUEST_FAIL
                    };
                }

            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<(decimal RowCount, IEnumerable<RoleManagementModel> Contents, CommonResponse Response)> GetRolePositionByOrgID(string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<RoleManagementModel>();

                // Query via redis

                var count = await couchContext.ViewQueryAsync<object>
                (
                    couchDBHelper: read_couchDbHelper,
                    designName: "query",
                    viewName: "byOrgId",
                    key: orgId,
                    limit: -1,
                    page: 0,
                    reduce: true,
                    desc: false
                );

                if (count.Rows.Count() == 0)
                {
                    return (0, default!, new CommonResponse()
                    {
                        Code = nameof(ResultCode.CONTENT_IS_EMPTY),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.CONTENT_IS_EMPTY),
                        Message = ResultCode.CONTENT_IS_EMPTY
                    });
                }

                var result = await couchContext.ViewQueryAsync<RoleManagementModel>
                    (
                        couchDBHelper: read_couchDbHelper,
                        designName: "query",
                        viewName: "byOrgId",
                        key: orgId,
                        limit: -1,
                        page: 0,
                        reduce: false,
                        desc: false
                    );
                return (Convert.ToDecimal(count.Rows[0].Value), result.Rows.Select(x => x.Value), new CommonResponse()
                {
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Success = true,
                    Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                    Message = ResultCode.SUCCESS_OPERATION
                });
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<(CommonResponse Response, RoleManagementModel Content)> GetRolePosition(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find from cache
                //string recordKey = $"{RedisPrefix.Role}{id}"; // Set key for cache
                //var mem = redisConnector.Connection.GetDatabase(1);
                //var cache = await mem.StringGetAsync(recordKey);
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<RoleManagementModel>();

                var cache = await context.FindByIdAsync(id);

                if (cache == null)
                {
                    // Find the existing record
                    var existing = await couchContext.GetAsync<RoleManagementModel>
                        (
                            couchDBHelper: read_couchDbHelper,
                            id: id,
                            cancellationToken: cancellationToken
                        );

                    if (!existing.IsSuccess)
                    {
                        return (new CommonResponseId()
                        {
                            Id = id,
                            Code = nameof(ResultCode.NOT_FOUND),
                            Success = false,
                            Detail = ValidateString.IsNullOrWhiteSpace(existing.Reason),
                            Message = ResultCode.NOT_FOUND
                        }, default!);
                    }

                    existing.Content.rev = existing.Rev;
                    existing.Content.id = existing.Id;
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(existing.Content));
                    await context.InsertAsync(existing.Content);

                    return (new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                        Message = ResultCode.SUCCESS_OPERATION
                    }, existing.Content);
                }

                //var result = JsonSerializer.Deserialize<RoleManagementModel>(cache!);
                return (new CommonResponseId()
                {
                    Id = id,
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Success = true,
                    Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                    Message = ResultCode.SUCCESS_OPERATION
                }, cache!);
            }
            catch (Exception)
            {

                throw;
            }
            

        }

        public async Task<(CommonResponse Response, List<RoleManagementModel> Contents)> GetRolesPosition(List<string> roles, CancellationToken cancellationToken = default)
        {
            var result = await couchContext.ViewQueryAsync<string, RoleManagementModel>
                    (
                        couchDBHelper: read_couchDbHelper,
                        designName: "query",
                        viewName: "byId",
                        keys: roles.ToArray(),
                        limit: -1,
                        page: 0,
                        reduce: false,
                        desc: false
                    );


            if (result.RowCount > 0)
            {
                return (new CommonResponse()
                {
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Success = true,
                    Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                    Message = ResultCode.SUCCESS_OPERATION
                }, result.Rows.Select(x => x.Value).ToList());
            }

            return (new CommonResponse()
            {
                Code = nameof(ResultCode.NOT_FOUND),
                Success = false,
                Detail = ResultCode.NOT_FOUND,
                Message = ResultCode.NOT_FOUND
            }, default!);
        }
        
    }
}
