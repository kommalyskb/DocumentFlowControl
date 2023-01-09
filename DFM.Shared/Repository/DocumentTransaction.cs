﻿using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using MyCouch.Requests;
using MyCouch.Responses;
using Redis.OM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFM.Shared.Repository
{
    public class DocumentTransaction : IDocumentTransaction
    {
        private readonly ICouchContext couchContext;
        private readonly IRedisConnector redisConnector;
        private readonly CouchDBHelper read_couchDbHelper;
        private readonly CouchDBHelper write_couchDbHelper;
        public DocumentTransaction(ICouchContext couchContext, DBConfig dbConfig, IRedisConnector redisConnector)
        {
            this.couchContext = couchContext;
            this.redisConnector = redisConnector;
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
                    request.rev = result.Rev;
                    await context.InsertAsync(request);

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

                request.rev = existing.Content.rev;

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
                            InboxType = item.Value.InboxType,
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
                            InboxType = item.Value.InboxType,
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

        public async Task<PersonalReportSummary> GetPersonalReport(GetPersonalReportRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                int total = 0;
                int draft = 0;
                int inprogress = 0;
                int finished = 0;

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
                if (countDraft.Count > 0)
                {
                    draft = countDraft.Where(x => x.createDate >= start && x.createDate <= end).Count();
                }

                if (countFinished.Count > 0)
                {
                    finished = countFinished.Where(x => x.createDate >= start && x.createDate <= end).Count();
                }

                if (countInprogress.Count > 0)
                {
                    inprogress = countInprogress.Where(x => x.createDate >= start && x.createDate <= end).Count();
                }

                total = draft + finished + inprogress;

                return new PersonalReportSummary
                {
                    Total = total,
                    InProgress = inprogress,
                    Draft = draft,
                    Finished = finished
                };
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
