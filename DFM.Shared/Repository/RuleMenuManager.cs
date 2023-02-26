using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using MyCouch.Requests;
using Redis.OM;
using Redis.OM.Searching;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Repository
{
    public class RuleMenuManager : IRuleMenuManager
    {
        private readonly ICouchContext couchContext;
        private readonly IRedisConnector redisConnector;
        private readonly CouchDBHelper read_couchDbHelper;
        private readonly CouchDBHelper write_couchDbHelper;
        private readonly IRedisCollection<RuleMenu> context;

        public RuleMenuManager(ICouchContext couchContext, DBConfig dbConfig, IRedisConnector redisConnector)
        {
            this.couchContext = couchContext;
            this.redisConnector = redisConnector;
            this.read_couchDbHelper = new CouchDBHelper
           (
               scheme: dbConfig.Reader.Scheme,
               srvAddr: dbConfig.Reader.SrvAddr,
               dbName: "dfm_rulemenu_db",
               username: dbConfig.Reader.Username,
               password: dbConfig.Reader.Password,
               port: dbConfig.Reader.Port
           );
            this.write_couchDbHelper = new CouchDBHelper
            (
                scheme: dbConfig.Writer.Scheme,
                srvAddr: dbConfig.Writer.SrvAddr,
                dbName: "dfm_rulemenu_db",
                username: dbConfig.Writer.Username,
                password: dbConfig.Writer.Password,
                port: dbConfig.Writer.Port
            );
            var provider = new RedisConnectionProvider(redisConnector.Connection);
            context = provider.RedisCollection<RuleMenu>();
        }

        public async Task<IEnumerable<RuleMenu>> GetRuleMenus(string userId, string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                List<RuleMenu> ruleResult = new();
                var cache = context.Where(x => x.OrgID == orgId).ToList();
                foreach (var c in cache)
                {
                    if (!c.UserIDs!.IsNullOrEmpty())
                    {
                        if (c.UserIDs!.Any(x => x == userId))
                        {
                            ruleResult.Add(c);

                        }
                    }
                }
                if (ruleResult.Count() == 0)
                {
                    List<Task> tasks = new List<Task>();
                    // Get from database
                    var fromDb = await couchContext.ViewQueryAsync<RuleMenu>(read_couchDbHelper, "query", "byOrgID", orgId, -1, 0, false, false, cancellationToken);
                    foreach (var item in fromDb.Rows)
                    {
                        tasks.Add(context.InsertAsync(item.Value));
                    }
                    await Task.WhenAll(tasks);

                    // Query from Redis again
                    cache = context.Where(x => x.OrgID == orgId).ToList();
                    foreach (var c in cache)
                    {
                        if (!c.UserIDs!.IsNullOrEmpty())
                        {
                            if (c.UserIDs!.Any(x => x == userId))
                            {
                                ruleResult.Add(c);

                            }
                        }
                    }
                }

                return ruleResult; 
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<RuleMenu>> GetRuleMenus(IEnumerable<RoleTypeModel> roles, string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                List<RuleMenu> ruleResult = new();
                var cache = context.Where(x => x.OrgID == orgId).ToList();
                foreach (RoleTypeModel role in roles)
                {
                    foreach (var c in cache)
                    {
                        if (!c.RoleTypes!.IsNullOrEmpty())
                        {
                            if (c.RoleTypes!.Any(x => x == role))
                            {
                                ruleResult.Add(c);
                            }
                        }
                        
                    }
                    
                }

                if (ruleResult.Count == 0)
                {
                    List<Task> tasks = new List<Task>();
                    // Get from database
                    var fromDb = await couchContext.ViewQueryAsync<RuleMenu>(read_couchDbHelper, "query", "byOrgID", orgId, -1, 0, false, false, cancellationToken);
                    foreach (var item in fromDb.Rows)
                    {
                        tasks.Add(context.InsertAsync(item.Value));
                    }
                    await Task.WhenAll(tasks);

                    // Query from Redis again
                    cache = context.Where(x => x.OrgID == orgId).ToList();
                    foreach (RoleTypeModel role in roles)
                    {
                        foreach (var c in cache)
                        {
                            if (!c.RoleTypes!.IsNullOrEmpty())
                            {
                                if (c.RoleTypes!.Any(x => x == role))
                                {
                                    ruleResult.Add(c);
                                }
                            }
                        }

                    }
                }

                return ruleResult;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<RuleMenu>> GetRuleMenus(string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var result = context.Where(x => x.OrgID == orgId).ToList();
                if (result.Count() == 0)
                {
                    List<Task> tasks = new List<Task>();
                    // Get from database
                    var fromDb = await couchContext.ViewQueryAsync<RuleMenu>(read_couchDbHelper, "query", "byOrgID", orgId, -1, 0, false, false, cancellationToken);
                    foreach (var item in fromDb.Rows)
                    {
                        tasks.Add(context.InsertAsync(item.Value));
                    }
                    await Task.WhenAll(tasks);

                    // Query from Redis again
                    result = context.Where(x => x.OrgID == orgId).ToList();
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<CommonResponseId> RemoveRule(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var exist = await GetRuleMenu(id, cancellationToken);

                var result = await couchContext.DeleteAsync(write_couchDbHelper, exist.id, exist.revision, cancellationToken);
                if (!result.IsSuccess)
                {
                    return new CommonResponseId
                    {
                        Success = false,
                        Code = nameof(ResultCode.REMOVED_FAILED),
                        Message = ResultCode.REMOVED_FAILED,
                        Detail = result.Reason,
                        Id = id
                    };
                }

                await context.DeleteAsync(exist);

                return new CommonResponseId
                {
                    Success = false,
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Message = ResultCode.SUCCESS_OPERATION,
                    Detail = ResultCode.SUCCESS_OPERATION,
                    Id = id
                };
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<CommonResponseId> UpdateRules(RuleMenu request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.id))
                {
                    // Generate ID
                    request.id = $"{request.OrgID}-{request.Menu}";

                    // Find ID
                    var exist = await GetRuleMenu(request.id, cancellationToken);
                    if (exist == null)
                    {
                        // Create new record
                        var result = await couchContext.InsertAsync<RuleMenu>(write_couchDbHelper, request);
                        if (!result.IsSuccess)
                        {
                            return new CommonResponseId
                            {
                                Code = nameof(ResultCode.COULD_NOT_CREATE_RECORD),
                                Detail = result.Error,
                                Message = ResultCode.COULD_NOT_CREATE_RECORD,
                                Success = false
                            };
                        }

                        request.revision = result.Rev;
                        await context.InsertAsync(request);
                        return new CommonResponseId
                        {
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION,
                            Success = true
                        };
                    }

                    // Update existing
                    request.revision = exist.revision;
                    var updateResult = await couchContext.EditAsync(write_couchDbHelper, exist, cancellationToken);
                    if (!updateResult.IsSuccess)
                    {
                        return new CommonResponseId
                        {
                            Code = nameof(ResultCode.COULD_NOT_UPDATE_RECORD),
                            Detail = updateResult.Error,
                            Message = ResultCode.COULD_NOT_UPDATE_RECORD,
                            Success = false
                        };
                    }

                    // Set new revivision
                    request.revision = updateResult.Rev;
                    await context.UpdateAsync(request);
                    return new CommonResponseId
                    {
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Detail = ResultCode.SUCCESS_OPERATION,
                        Message = ResultCode.SUCCESS_OPERATION,
                        Success = true
                    };
                }
                else
                {
                    // Find ID
                    var exist = await GetRuleMenu(request.id, cancellationToken);
                    if (exist == null)
                    {
                        return new CommonResponseId
                        {
                            Code = nameof(ResultCode.NOT_FOUND),
                            Detail = ResultCode.NOT_FOUND,
                            Message = ResultCode.NOT_FOUND,
                            Success = false
                        };
                    }
                    
                    request.revision = exist.revision;
                    var updateResult = await couchContext.EditAsync(write_couchDbHelper, exist, cancellationToken);
                    if (!updateResult.IsSuccess)
                    {
                        return new CommonResponseId
                        {
                            Code = nameof(ResultCode.COULD_NOT_UPDATE_RECORD),
                            Detail = updateResult.Error,
                            Message = ResultCode.COULD_NOT_UPDATE_RECORD,
                            Success = false
                        };
                    }

                    // Set new revivision
                    request.revision = updateResult.Rev;
                    await context.UpdateAsync(request);
                    return new CommonResponseId
                    {
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Detail = ResultCode.SUCCESS_OPERATION,
                        Message = ResultCode.SUCCESS_OPERATION,
                        Success = true
                    };
                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<RuleMenu> GetRuleMenu(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                

                var rule = await context.FindByIdAsync(id);
                if (rule == null)
                {
                    var result = await couchContext.GetAsync<RuleMenu>(read_couchDbHelper, id, cancellationToken: cancellationToken);
                    if (result.IsSuccess)
                    {
                        result.Content.id = result.Id;
                        result.Content.revision = result.Rev;
                        await context.InsertAsync(result.Content);

                        return result.Content;
                    }
                    return default!;
                }

                return rule;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
