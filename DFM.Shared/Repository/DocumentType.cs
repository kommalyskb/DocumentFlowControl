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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFM.Shared.Repository
{
    public class DocumentType : IDocumentType
    {
        private readonly ICouchContext couchContext;
        private readonly IRedisConnector redisConnector;
        private readonly CouchDBHelper read_couchDbHelper;
        private readonly CouchDBHelper write_couchDbHelper;
        public DocumentType(ICouchContext couchContext, DBConfig dbConfig, IRedisConnector redisConnector)
        {
            this.couchContext = couchContext;
            this.redisConnector = redisConnector;
            this.read_couchDbHelper = new CouchDBHelper
           (
               scheme: dbConfig.Reader.Scheme,
               srvAddr: dbConfig.Reader.SrvAddr,
               dbName: "dfm_doctype_db",
               username: dbConfig.Reader.Username,
               password: dbConfig.Reader.Password,
               port: dbConfig.Reader.Port
           );
            this.write_couchDbHelper = new CouchDBHelper
            (
                scheme: dbConfig.Writer.Scheme,
                srvAddr: dbConfig.Writer.SrvAddr,
                dbName: "dfm_doctype_db",
                username: dbConfig.Writer.Username,
                password: dbConfig.Writer.Password,
                port: dbConfig.Writer.Port
            );
        }

        public async Task<CommonResponse> NewDocumentType(DataTypeModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DataTypeModel>();

                var result = await couchContext.InsertAsync<DataTypeModel>
                               (
                                   couchDBHelper: write_couchDbHelper,
                                   model: request,
                                   cancellationToken: cancellationToken
                               );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.DocType}{result.Id}"; // Set key for cache
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

        public async Task<CommonResponse> EditDocumentType(DataTypeModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record

                var existing = await GetDocumentType(request.id!, cancellationToken);

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
                var context = provider.RedisCollection<DataTypeModel>();

                var result = await couchContext.EditAsync<DataTypeModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: request,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.DocType}{result.Id}"; // Set key for cache
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

        public async Task<CommonResponse> RemoveDocumentType(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record

                var existing = await GetDocumentType(id, cancellationToken);

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
                var context = provider.RedisCollection<DataTypeModel>();

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
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.DocType}{result.Id}"; // Set key for cache
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

        public async Task<(decimal RowCount, IEnumerable<DataTypeModel> Contents, CommonResponse Response)> GetDocumentTypeByOrgId(string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DataTypeModel>();

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

                var result = await couchContext.ViewQueryAsync<DataTypeModel>
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
                return (Convert.ToDecimal(count.Rows[0].Value), result.Rows.Select(x => x.Value).OrderBy(x => x.SortOrder), new CommonResponse()
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
        public async Task<(CommonResponse Response, DataTypeModel Content)> GetDocumentType(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find from cache
                //string recordKey = $"{RedisPrefix.DocType}{id}"; // Set key for cache
                //var mem = redisConnector.Connection.GetDatabase(1);
                //var cache = await mem.StringGetAsync(recordKey);
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DataTypeModel>();

                var cache = await context.FindByIdAsync(id);

                if (cache == null)
                {
                    // Find the existing record
                    var existing = await couchContext.GetAsync<DataTypeModel>
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

                //var result = JsonSerializer.Deserialize<DataTypeModel>(cache!);
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
