using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using Redis.OM;
using Redis.OM.Searching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private IRedisCollection<NotificationModel> context;

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
            var provider = new RedisConnectionProvider(redisConnector.Connection);
            context = provider.RedisCollection<NotificationModel>();
        }

        public async Task<CommonResponseId> CreateNotice(NotificationModel request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await couchContext.InsertAsync(write_couchDbHelper, request, cancellationToken);


                if (result.IsSuccess)
                {
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
        public async Task<CommonResponseId> ReadNotice(string? id, string? userIDRead, CancellationToken cancellationToken = default)
        {
            try
            {
                // Redis first
                var exist = await GetNotice(id, cancellationToken);

                if (!exist.Response.Success)
                {
                    // Break;
                    return new CommonResponseId
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = exist.Response.Code,
                        Success = exist.Response.Success,
                        Detail = exist.Response.Detail,
                        Message = exist.Response.Message
                    };
                }

                var request = exist.Content;
                request.UserIDRead = userIDRead;
                request.ReadDate = $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm")}";
                request.IsRead = true;

                var result = await couchContext.EditAsync(write_couchDbHelper, request, cancellationToken);


                if (result.IsSuccess)
                {
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

        public async Task<(CommonResponse Response, NotificationModel Content)> GetNotice(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
               
                var cache = await context.FindByIdAsync(id);

                if (cache == null)
                {
                    // Find the existing record
                    var existing = await couchContext.GetAsync<NotificationModel>
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

                //var result = JsonSerializer.Deserialize<FolderModel>(cache!);
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


        public async Task<(CommonResponse Response, IEnumerable<NotificationModel> Contents)> ListNotices(IEnumerable<string> roles, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {

                var contents = context.Where(x => roles.Contains(x.RoleID));

                if (contents.Count() == 0)
                {
                    return (new CommonResponse()
                    {
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ResultCode.NOT_FOUND,
                        Message = ResultCode.NOT_FOUND
                    }, default!);
                }

                return (new CommonResponse()
                {
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Success = true,
                    Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                    Message = ResultCode.SUCCESS_OPERATION
                }, contents);

            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
