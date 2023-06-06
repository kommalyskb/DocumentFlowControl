using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using Minio.DataModel;
using MyCouch.Requests;
using MyCouch.Responses;
using Redis.OM;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFM.Shared.Repository
{
    public class DocumentTransaction : IDocumentTransaction
    {
        private readonly ICouchContext couchContext;
        private readonly IRedisConnector redisConnector;
        private readonly IRoleManager roleManager;
        private readonly CouchDBHelper read_couchDbHelper;
        private readonly CouchDBHelper write_couchDbHelper;
        public DocumentTransaction(ICouchContext couchContext, DBConfig dbConfig, IRedisConnector redisConnector, IRoleManager roleManager)
        {
            this.couchContext = couchContext;
            this.redisConnector = redisConnector;
            this.roleManager = roleManager;
            this.read_couchDbHelper = new CouchDBHelper
           (
               scheme: dbConfig.Reader.Scheme,
               srvAddr: dbConfig.Reader.SrvAddr,
               dbName: "dfm_document_db",
               username: dbConfig.Reader.Username,
               password: dbConfig.Reader.Password,
               port: dbConfig.Reader.Port
           );
            this.write_couchDbHelper = new CouchDBHelper
            (
                scheme: dbConfig.Writer.Scheme,
                srvAddr: dbConfig.Writer.SrvAddr,
                dbName: "dfm_document_db",
                username: dbConfig.Writer.Username,
                password: dbConfig.Writer.Password,
                port: dbConfig.Writer.Port
            );
        }

        public async Task<CommonResponseId> NewDocument(DocumentModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DocumentModel>();



                var result = await couchContext.InsertAsync<DocumentModel>
                               (
                                    couchDBHelper: write_couchDbHelper,
                                    model: request,
                                    cancellationToken: cancellationToken
                               );

                if (result.IsSuccess)
                {
                    ////Set cache after create success
                    //string recordKey = $"{RedisPrefix.Document}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(request));

                    // Insert new redis cache
                    request.revision = result.Rev;
                    await context.InsertAsync(request, TimeSpan.FromHours(1));

                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.NEW_DOCUMENT),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.NEW_DOCUMENT
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

        public async Task<CommonResponseId> EditDocument(DocumentModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record

                var existing = await GetDocument(request.id!, cancellationToken);

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

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DocumentModel>();

                request.revision = existing.Content.revision;

                var result = await couchContext.EditAsync<DocumentModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: request,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Document}{result.Id}"; // Set key for cache
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
        public async Task<CommonResponseId> UpdateReadDocumentStatus(Reciepient reciepient, string docID, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record

                var existing = await GetDocument(docID, cancellationToken);

                if (!existing.Response.Success)
                {
                    return new CommonResponseId()
                    {
                        Id = docID,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DocumentModel>();
                // Find index of recipient
                var recipientContent = existing.Content.Recipients!.LastOrDefault(x => x.UId == reciepient.UId);
                var index = existing.Content.Recipients!.IndexOf(recipientContent!);
                // Then replace it
                existing.Content.Recipients[index] = reciepient;

                var result = await couchContext.EditAsync<DocumentModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: existing.Content,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Document}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(request));
                    existing.Content.revision = result.Rev;
                    await context.UpdateAsync(existing.Content);

                    // Check if this document has parent then try to update there parent too
                    if (!string.IsNullOrWhiteSpace(existing.Content.ParentID))
                    {
                        // Find that document
                        var parentDoc = await GetDocument(existing.Content.ParentID, cancellationToken);

                        if (parentDoc.Response.Success)
                        {
                            var recParent = parentDoc.Content.Recipients!.LastOrDefault(x => x.UId == reciepient.UId);
                            var parentIndex = parentDoc.Content.Recipients!.IndexOf(recParent!);

                            parentDoc.Content.Recipients[parentIndex] = reciepient;

                            var parentResult = await couchContext.EditAsync<DocumentModel>
                            (
                                couchDBHelper: write_couchDbHelper,
                                model: parentDoc.Content,
                                cancellationToken: cancellationToken
                            );
                            // Update cache
                            parentDoc.Content.revision = parentResult.Rev;
                            await context.UpdateAsync(parentDoc.Content);
                        }
                    }


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
        public async Task<(CommonResponse Response, DocumentModel Content)> GetDocument(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find from cache
                //string recordKey = $"{RedisPrefix.Document}{id}"; // Set key for cache
                //var mem = redisConnector.Connection.GetDatabase(1);
                //var cache = await mem.StringGetAsync(recordKey);

                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DocumentModel>();
                var cache = await context.FindByIdAsync(id);

                if (cache == null)
                {
                    // Find the existing record
                    var existing = await couchContext.GetAsync<DocumentModel>
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

        // Not used
        public async Task<CommonResponse> SendDocument(string docId, List<Reciepient> reciepients, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record

                var existing = await GetDocument(docId, cancellationToken);

                if (!existing.Response.Success)
                {

                    // Should create new record

                    throw new NotImplementedException();
                    return new CommonResponseId()
                    {
                        Id = docId,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }

                var request = existing.Content;
                request.Recipients!.AddRange(reciepients);

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DocumentModel>();

                var result = await couchContext.EditAsync<DocumentModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: request,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Document}{result.Id}"; // Set key for cache
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
        public async Task<(decimal RowCount, IEnumerable<DocumentModel> Contents, CommonResponse Response)> GetDocumentByRoleId(string roleId, InboxType inboxType, TraceStatus traceStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DocumentModel>();
                List<DocumentModel> Documents = new();

                // Query via redis
                if (traceStatus == TraceStatus.Terminated || traceStatus == TraceStatus.Completed)
                {
                    var request = new List<QueryDocByRole>
                    {
                        new QueryDocByRole
                        {
                            RoleId = roleId,
                            DocStatus = TraceStatus.Completed,
                            InboxType = inboxType
                        },
                        new QueryDocByRole
                        {
                            RoleId = roleId,
                            DocStatus = TraceStatus.Terminated,
                            InboxType = inboxType
                        }
                    };
                    var count = await couchContext.ViewQueryAsync<QueryDocByRole, object>
                    (
                        couchDBHelper: read_couchDbHelper,
                        designName: "query",
                        viewName: "byRoleId",
                        keys: request.ToArray(),
                        limit: -1,
                        page: 0,
                        reduce: true,
                        desc: false,
                        cancellationToken
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

                    var result = await couchContext.ViewQueryAsync<QueryDocByRole, DocumentModel>
                        (
                            couchDBHelper: read_couchDbHelper,
                            designName: "query",
                            viewName: "byRoleId",
                            keys: request.ToArray(),
                            limit: -1,
                            page: 0,
                            reduce: false,
                            desc: false,
                        cancellationToken
                        );

                    foreach (var item in result.Rows)
                    {
                        var doc = new DocumentModel
                        {
                            id = item.Id,
                            //InboxType = item.Value.InboxType,
                            RawDatas = item.Value.RawDatas,
                            OrganizationID = item.Value.OrganizationID
                        };
                        var myTails = item.Value.Recipients!
                            .Where(x => x.DocStatus == TraceStatus.Terminated || x.DocStatus == TraceStatus.Completed);
                        doc.Recipients!.AddRange(myTails);

                        Documents.Add(doc);
                    }

                    return (Convert.ToDecimal(count.Rows[0].Value), Documents, new CommonResponse()
                    {
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                        Message = ResultCode.SUCCESS_OPERATION
                    });
                }
                else
                {
                    var request = new QueryDocByRole
                    {
                        RoleId = roleId,
                        DocStatus = traceStatus,
                        InboxType = inboxType
                    };
                    var count = await couchContext.ViewQueryAsync<QueryDocByRole, object>
                    (
                        couchDBHelper: read_couchDbHelper,
                        designName: "query",
                        viewName: "byRoleId",
                        keys: request,
                        limit: -1,
                        page: 0,
                        reduce: true,
                        desc: false,
                        cancellationToken
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

                    var result = await couchContext.ViewQueryAsync<QueryDocByRole, DocumentModel>
                        (
                            couchDBHelper: read_couchDbHelper,
                            designName: "query",
                            viewName: "byRoleId",
                            keys: request,
                            limit: -1,
                            page: 0,
                            reduce: false,
                            desc: false,
                        cancellationToken
                        );

                    foreach (var item in result.Rows)
                    {
                        var doc = new DocumentModel
                        {
                            id = item.Id,
                            //InboxType = item.Value.InboxType,
                            RawDatas = item.Value.RawDatas,
                            OrganizationID = item.Value.OrganizationID
                        };
                        var myTails = item.Value.Recipients!
                            .Where(x => x.DocStatus != TraceStatus.Terminated && x.DocStatus != TraceStatus.Completed);
                        doc.Recipients!.AddRange(myTails);

                        Documents.Add(doc);
                    }

                    return (Convert.ToDecimal(count.Rows[0].Value), Documents, new CommonResponse()
                    {
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                        Message = ResultCode.SUCCESS_OPERATION
                    });
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<PersonalReportSummary>> GetPersonalReport(GetPersonalReportRequest request, CancellationToken cancellationToken = default)
        {
            try
            {

                List<QueryReportRequest> requests = new List<QueryReportRequest>();
                foreach (string roleID in request.roleIDs!)
                {
                    requests.Add(new QueryReportRequest
                    {
                        inboxType = request.inboxType,
                        roleID = roleID
                    });
                }

                List<QueryReportResponse> countDraft = await queryPersonalReport(requests, "report_draft", cancellationToken);
                List<QueryReportResponse> countFinished = await queryPersonalReport(requests, "report_finished", cancellationToken);
                List<QueryReportResponse> countInprogress = await queryPersonalReport(requests, "report_inprogress", cancellationToken);
                var start = Math.Truncate(request.start);
                var end = Math.Truncate(request.end);
                List<PersonalReportSummary> result = new();
                if (countDraft.Count > 0)
                {
                    IEnumerable<ReportPersonalGroup>? draftResult;
                    if (start == -1 && end == -1)
                    {
                        draftResult = countDraft
                        .GroupBy(x => x.roleId)
                        .Select(c => new ReportPersonalGroup { RoleID = c.Key, Count = c.Count() });
                    }
                    else
                    {
                        draftResult = countDraft
                        .Where(x => x.createDate >= start && x.createDate <= end)
                        .GroupBy(x => x.roleId)
                        .Select(c => new ReportPersonalGroup { RoleID = c.Key, Count = c.Count() });
                    }

                    //draft = countDraft.Where(x => x.createDate >= start && x.createDate <= end).Count();
                    foreach (var item in draftResult)
                    {
                        var isExist = result.FirstOrDefault(x => x.RoleID!.Equals(item.RoleID));

                        if (isExist == null)
                        {
                            var position = countDraft.FirstOrDefault(x => x.roleId == item.RoleID);
                            result.Add(new PersonalReportSummary
                            {
                                RoleID = item.RoleID,
                                Position = position!.roleName,
                                Draft = item.Count,
                                FullName = position!.fullName
                            });
                        }
                        else
                        {
                            result[result.IndexOf(isExist)].Draft = item.Count;
                        }

                    }
                }

                if (countFinished.Count > 0)
                {
                    IEnumerable<ReportPersonalGroup>? finishedResult;
                    if (start == -1 && end == -1)
                    {
                        finishedResult = countFinished
                        .GroupBy(x => x.roleId)
                        .Select(c => new ReportPersonalGroup { RoleID = c.Key, Count = c.Count() });
                    }
                    else
                    {
                        finishedResult = countFinished
                        .Where(x => x.createDate >= start && x.createDate <= end)
                        .GroupBy(x => x.roleId)
                        .Select(c => new ReportPersonalGroup { RoleID = c.Key, Count = c.Count() });
                    }

                    //finished = countFinished.Where(x => x.createDate >= start && x.createDate <= end).Count();
                    foreach (var item in finishedResult)
                    {
                        var isExist = result.FirstOrDefault(x => x.RoleID!.Equals(item.RoleID));
                        if (isExist == null)
                        {
                            var position = countFinished.FirstOrDefault(x => x.roleId == item.RoleID);
                            result.Add(new PersonalReportSummary
                            {
                                RoleID = item.RoleID,
                                Position = position!.roleName,
                                Finished = item.Count,
                                FullName = position!.fullName
                            });
                        }
                        else
                        {
                            result[result.IndexOf(isExist)].Finished = item.Count;
                        }

                    }
                }

                if (countInprogress.Count > 0)
                {
                    IEnumerable<ReportPersonalGroup>? inprogressResult;
                    if (start == -1 && end == -1)
                    {
                        inprogressResult = countInprogress
                        .GroupBy(x => x.roleId)
                        .Select(c => new ReportPersonalGroup { RoleID = c.Key, Count = c.Count() });
                    }
                    else
                    {
                        inprogressResult = countInprogress
                        .Where(x => x.createDate >= start && x.createDate <= end)
                        .GroupBy(x => x.roleId)
                        .Select(c => new ReportPersonalGroup { RoleID = c.Key, Count = c.Count() });
                    }

                    //inprogress = countInprogress.Where(x => x.createDate >= start && x.createDate <= end).Count();
                    foreach (var item in inprogressResult)
                    {
                        var isExist = result.FirstOrDefault(x => x.RoleID!.Equals(item.RoleID));
                        if (isExist == null)
                        {
                            var position = countInprogress.FirstOrDefault(x => x.roleId == item.RoleID);
                            result.Add(new PersonalReportSummary
                            {
                                RoleID = item.RoleID,
                                Position = position!.roleName,
                                InProgress = item.Count,
                                FullName = position!.fullName
                            });
                        }
                        else
                        {
                            result[result.IndexOf(isExist)].InProgress = item.Count;
                        }

                    }
                }


                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<PersonalReportSummary> GetDashboard(GetDashboardRequest request, CancellationToken cancellationToken = default)
        {
            try
            {

                QueryReportRequest req = new QueryReportRequest
                {
                    inboxType = request.inboxType,
                    roleID = request.roleID
                };


                List<QueryReportResponse> countDraft = await queryPersonalReport(req, "report_draft", cancellationToken);
                List<QueryReportResponse> countFinished = await queryPersonalReport(req, "report_finished", cancellationToken);
                List<QueryReportResponse> countInprogress = await queryPersonalReport(req, "report_inprogress", cancellationToken);
                PersonalReportSummary result = new PersonalReportSummary
                {
                    Draft = countDraft.Count,
                    Finished = countFinished.Count,
                    InProgress = countInprogress.Count,
                    RoleID = request.roleID
                };

                if (result.Draft > 0)
                {
                    result.Position = countDraft.FirstOrDefault(x => x.roleId == req.roleID)!.roleName;
                }
                else if (result.Finished > 0)
                {
                    result.Position = countFinished.FirstOrDefault(x => x.roleId == req.roleID)!.roleName;
                }
                else if (result.InProgress > 0)
                {
                    result.Position = countInprogress.FirstOrDefault(x => x.roleId == req.roleID)!.roleName;
                }
                else
                {
                    var roleInfo = await roleManager.GetRolePosition(request.roleID!);
                    if (roleInfo.Response.Success)
                    {
                        result.Position = roleInfo.Content.Display.Local;
                    }
                }

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<List<QueryReportResponse>> queryPersonalReport(List<QueryReportRequest> requests, string viewName, CancellationToken cancellationToken)
        {
            var result = await couchContext.ViewQueryAsync<QueryReportRequest, QueryReportResponse>
                                (
                                    couchDBHelper: read_couchDbHelper,
                                    designName: "query",
                                    viewName: viewName,
                                    keys: requests.ToArray(),
                                    limit: -1,
                                    page: 0,
                                    reduce: false,
                                    desc: false,
                                    cancellationToken
                                );
            return result.Rows.Select(x => x.Value).ToList();
        }
        private async Task<List<QueryReportResponse>> queryPersonalReport(QueryReportRequest request, string viewName, CancellationToken cancellationToken)
        {
            var result = await couchContext.ViewQueryAsync<QueryReportRequest, QueryReportResponse>
                                (
                                    couchDBHelper: read_couchDbHelper,
                                    designName: "query",
                                    viewName: viewName,
                                    keys: request,
                                    limit: -1,
                                    page: 0,
                                    reduce: false,
                                    desc: false,
                                    cancellationToken
                                );
            return result.Rows.Select(x => x.Value).ToList();
        }


        public async Task<IEnumerable<DocumentModel>> DrillDownReport(GetPersonalReportRequest request, TraceStatus docStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                List<QueryReportRequest> requests = new List<QueryReportRequest>();
                foreach (string roleID in request.roleIDs)
                {
                    requests.Add(new QueryReportRequest
                    {
                        inboxType = request.inboxType,
                        roleID = roleID
                    });
                }
                var start = Math.Truncate(request.start);
                var end = Math.Truncate(request.end);
                if (docStatus == TraceStatus.Draft)
                {
                    var countDraft = await couchContext.ViewQueryAsync<QueryReportRequest, QueryReportResponse>
                    (
                        couchDBHelper: read_couchDbHelper,
                        designName: "query",
                        viewName: "report_draft",
                        keys: requests.ToArray(),
                        limit: -1,
                        page: 0,
                        reduce: false,
                        desc: false,
                        cancellationToken
                    );
                    if (countDraft.RowCount > 0)
                    {
                        var result = countDraft.Rows.Where(x => x.Value.createDate >= start && x.Value.createDate <= end);
                        return result.Select(x => x.Value.content)!;
                    }
                }
                else if (docStatus == TraceStatus.InProgress)
                {
                    var countInprogress = await couchContext.ViewQueryAsync<QueryReportRequest, QueryReportResponse>
                    (
                        couchDBHelper: read_couchDbHelper,
                        designName: "query",
                        viewName: "report_inprogress",
                        keys: requests.ToArray(),
                        limit: -1,
                        page: 0,
                        reduce: false,
                        desc: false,
                        cancellationToken
                    );
                    if (countInprogress.RowCount > 0)
                    {
                        var result = countInprogress.Rows.Where(x => x.Value.createDate >= start && x.Value.createDate <= end);
                        return result.Select(x => x.Value.content)!;
                    }
                }
                else if (docStatus == TraceStatus.Completed || docStatus == TraceStatus.Terminated)
                {
                    var countFinished = await couchContext.ViewQueryAsync<QueryReportRequest, QueryReportResponse>
                    (
                        couchDBHelper: read_couchDbHelper,
                        designName: "query",
                        viewName: "report_finished",
                        keys: requests.ToArray(),
                        limit: -1,
                        page: 0,
                        reduce: false,
                        desc: false,
                        cancellationToken
                    );
                    if (countFinished.RowCount > 0)
                    {
                        var result = countFinished.Rows.Where(x => x.Value.createDate >= start && x.Value.createDate <= end);
                        return result.Select(x => x.Value.content)!;
                    }
                }

                // No content return
                return null!;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
