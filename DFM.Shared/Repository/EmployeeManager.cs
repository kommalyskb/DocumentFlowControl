﻿using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using MyCouch.Requests;
using Redis.OM;
using Redis.OM.Searching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFM.Shared.Repository
{
    public class EmployeeManager : IEmployeeManager
    {
        private readonly ICouchContext couchContext;
        private readonly IRedisConnector redisConnector;
        private readonly IOrganizationChart organizationChart;
        private readonly CouchDBHelper read_couchDbHelper;
        private readonly CouchDBHelper write_couchDbHelper;
        private readonly IRedisCollection<EmployeeModel> context;

        public EmployeeManager(ICouchContext couchContext, DBConfig dbConfig, IRedisConnector redisConnector, IOrganizationChart organizationChart)
        {
            this.couchContext = couchContext;
            this.redisConnector = redisConnector;
            this.organizationChart = organizationChart;
            this.read_couchDbHelper = new CouchDBHelper
           (
               scheme: dbConfig.Reader.Scheme,
               srvAddr: dbConfig.Reader.SrvAddr,
               dbName: "dfm_employee_db",
               username: dbConfig.Reader.Username,
               password: dbConfig.Reader.Password,
               port: dbConfig.Reader.Port
           );
            this.write_couchDbHelper = new CouchDBHelper
            (
                scheme: dbConfig.Writer.Scheme,
                srvAddr: dbConfig.Writer.SrvAddr,
                dbName: "dfm_employee_db",
                username: dbConfig.Writer.Username,
                password: dbConfig.Writer.Password,
                port: dbConfig.Writer.Port
            );
            var provider = new RedisConnectionProvider(redisConnector.Connection);
            context = provider.RedisCollection<EmployeeModel>();
        }

        public async Task<CommonResponse> NewEmployeeProfile(EmployeeModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {

                // Redis first


                var result = await couchContext.InsertAsync<EmployeeModel>
               (
                   couchDBHelper: write_couchDbHelper,
                   model: request,
                   cancellationToken: cancellationToken
               );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Employee}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(request));
                    request.revision = result.Rev;
                    await context.InsertAsync(request, TimeSpan.FromHours(1));

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

        public async Task<CommonResponse> EditEmployeeProfile(EmployeeModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record

                var existing = await GetProfile(request.id!, cancellationToken);

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
                request.revision = existing.Content.revision;


                var result = await couchContext.EditAsync<EmployeeModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: request,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {

                    //----------Update Organization
                    await organizationChart.EditEmployeeOnly(request.OrganizationID!, new PartialEmployeeProfile
                    {
                        UserID = request.UserID,
                        Gender = request.Gender,
                        Name = request.Name,
                        FamilyName = request.FamilyName
                    }, cancellationToken);
                    //-----------------------------
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Employee}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(request));
                    request.revision = result.Rev;
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

        public async Task<CommonResponse> RemoveEmployeeProfile(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record

                var existing = await GetProfile(id, cancellationToken);

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

                

                // Remove
                var result = await couchContext.DeleteAsync
                   (
                       couchDBHelper: write_couchDbHelper,
                       id: id,
                       rev: existing.Content.revision,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //----------Update Organization
                    // No content for update
                    //-----------------------------
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Employee}{result.Id}"; // Set key for cache
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

        public async Task<(decimal RowCount, IEnumerable<EmployeeModel> Contents, CommonResponse Response)> GetProfileByOrgId(string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {

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

                var result = await couchContext.ViewQueryAsync<EmployeeModel>
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
        public async Task<(CommonResponse Response, EmployeeModel Content)> GetProfile(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find from cache
                //string recordKey = $"{RedisPrefix.Employee}{id}"; // Set key for cache
                //var mem = redisConnector.Connection.GetDatabase(1);
                //var cache = await mem.StringGetAsync(recordKey);

                var cache = await context.FindByIdAsync(id);
                if (cache == null)
                {
                    // Find the existing record
                    var existing = await couchContext.GetAsync<EmployeeModel>
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
                    existing.Content.revision = existing.Rev;
                    existing.Content.id = existing.Id;
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(existing.Content));
                    await context.InsertAsync(existing.Content, TimeSpan.FromHours(1));

                    return (new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                        Message = ResultCode.SUCCESS_OPERATION
                    }, existing.Content);
                }

                //var result = JsonSerializer.Deserialize<EmployeeModel>(cache!);
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

    }
}
