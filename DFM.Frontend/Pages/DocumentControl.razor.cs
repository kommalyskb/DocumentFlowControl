﻿using Confluent.Kafka;
using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using DFM.Shared.Resources;
using EnsureThat;
using HttpClientService;
using IronPdf.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using Newtonsoft.Json.Linq;
using Serilog;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Text.Json;

namespace DFM.Frontend.Pages
{
    public partial class DocumentControl
    {
        string? oldPage = "inbox";
        string? oldLink = "";
        long maxFileSize = 1024 * 1024 * 25; // 5 MB or whatever, don't just use max int
        private EmployeeModel? employee;
        string? token;
        IEnumerable<RoleTreeModel>? recipients;
        PartialRole? selectedRole;
        CommonResponseId? publisher;
        readonly int delayTime = 500;
        //IEnumerable<TabItemDto>? myRoles;
        string oldDocID = "none";

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                Log.Information("Parameter Set fire");
                Log.Information($"Parameter set: Page: {Page}, Link: {Link}, DocID: {DocId}, MsgID: {MessageID}, Trace: {traceStatus}, Form: {formMode}");

                // Check Page
                if (Page == "inbox")
                {
                    traceStatus = TraceStatus.InProgress;
                }
                else if (Page == "draft")
                {
                    traceStatus = TraceStatus.Draft;
                }
                else if (Page == "completed")
                {
                    traceStatus = TraceStatus.Completed;
                }
                else if (Page == "bin")
                {
                    traceStatus = TraceStatus.Trash;
                }
                else if (Page == "coprocess")
                {
                    traceStatus = TraceStatus.CoProccess;
                }
                // Check navigate from navbar
                if (DocId == "none")
                {
                    oldDocID = "none";
                    formMode = FormMode.List;
                    //

                    // Check Change from old page to other  page
                    if (oldPage != Page)
                    {
                        if (!string.IsNullOrWhiteSpace(DocId) && !string.IsNullOrWhiteSpace(MessageID) && !string.IsNullOrWhiteSpace(MessageRole))
                        {
                            await loadDocumentModel();
                        }
                        else
                        {
                            formMode = FormMode.List;
                            //
                        }
                        oldPage = Page!;
                    }

                    // Check navigate from inbound to outbound or outbound to inbound
                    if (oldLink != Link)
                    {
                        if (!string.IsNullOrWhiteSpace(DocId) && !string.IsNullOrWhiteSpace(MessageID) && !string.IsNullOrWhiteSpace(MessageRole))
                        {
                            await loadDocumentModel();
                        }
                        else
                        {
                            formMode = FormMode.List;
                            //
                        }
                        oldLink = Link!;

                        #region Validate Token
                        var getTokenState = await tokenState.ValidateToken();
                        if (!getTokenState)
                            nav.NavigateTo("/authorize");
                        #endregion


                        // Load recipient
                        string urlGetOrgItem = $"{endpoint.API}/api/v1/Organization/GetItem/{employee.OrganizationID!}/{roleId}/{Link}";
                        var result = await httpService.Get<IEnumerable<RoleTreeModel>>(urlGetOrgItem, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
                        if (result.Success)
                        {
                            var expected = result.Response.ToList();
                            expected.RemoveAll(x => x.Role.RoleID == roleId);
                            recipients = expected;
                            if (recipients.IsNullOrEmpty())
                            {
                                showSendButton = false;
                            }

                        }
                        else
                        {
                            recipients = null;
                            showSendButton = false;
                        }
                    }
                }
                else
                {
                    // Click from notification
                    if (oldDocID != DocId)
                    {

                        Log.Information($"Old: {oldDocID}, Doc: {DocId}");
                        oldDocID = DocId!;
                        if (!string.IsNullOrWhiteSpace(DocId) && !string.IsNullOrWhiteSpace(MessageID) && !string.IsNullOrWhiteSpace(MessageRole))
                        {
                            await loadDocumentModel();
                        }
                    }
                }
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }
            

        }
        protected override async Task OnInitializedAsync()
        {
            try
            {
                var rules = await storageHelper.GetRuleMenuAsync();
                if (!ValidateRule.isInRole(rules, $"/pages/doc/{Link}/{Page}"))
                {
                    nav.NavigateTo("/pages/unauthorized");
                }
                if (!ValidateRule.isInRole(rules, $"/pages/doc/{Link}/{Page}/{DocId}"))
                {
                    nav.NavigateTo("/pages/unauthorized");
                }


                // Check Page
                if (Page == "inbox")
                {
                    traceStatus = TraceStatus.InProgress;
                }
                else if (Page == "draft")
                {
                    traceStatus = TraceStatus.Draft;
                }
                else if (Page == "completed")
                {
                    traceStatus = TraceStatus.Completed;
                }
                else if (Page == "bin")
                {
                    traceStatus = TraceStatus.Trash;
                }
                else if (Page == "coprocess")
                {
                    traceStatus = TraceStatus.CoProccess;
                }
                if (!string.IsNullOrWhiteSpace(DocId) && !string.IsNullOrWhiteSpace(MessageID) && !string.IsNullOrWhiteSpace(MessageRole))
                {
                    await loadDocumentModel();
                }
                else
                {
                    formMode = FormMode.List;

                }

                oldLink = Link!;
                oldPage = Page!;
            }
            catch (Exception)
            {

                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }
            
        }
        


        void onCreateButtonClick()
        {
            disposedObj();
            formMode = FormMode.Create;

        }
        void onePreviousButtonClick()
        {
            formMode = FormMode.List;

        }

        async Task onDeleteButtonClick()
        {
            try
            {
                bool? isDelete = await delBox!.Show();
                if (isDelete.HasValue)
                {
                    if (isDelete.Value)
                    {
                        #region Validate Token
                        var getTokenState = await tokenState.ValidateToken();
                        if (!getTokenState)
                            nav.NavigateTo("/authorize");
                        #endregion
                        // Delete button had fire
                        onProcessing = true;
                        if (employee == null)
                        {
                            employee = await storageHelper.GetEmployeeProfileAsync();
                        }
                        await InvokeAsync(StateHasChanged);
                        string url = $"{endpoint.API}/api/v1/Document/SaveDocument/{roleId}";
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            token = await accessToken.GetTokenAsync();
                        }

                        // Set current status to delete
                        var index = documentModel!.Recipients!.IndexOf(myRole!);
                        documentModel!.Recipients![index].DocStatus = TraceStatus.Trash;
                        //documentModel!.Reciepients![index].Behavior = BehaviorStatus.ReadOnly;

                        DocumentRequest documentRequest = new DocumentRequest();
                        if (Link == "inbound")
                        {
                            //documentModel!.InboxType = InboxType.Inbound;
                            documentModel!.Recipients![index].InboxType = InboxType.Inbound;

                        }
                        else
                        {
                            //documentModel!.InboxType = InboxType.Outbound;
                            documentModel!.Recipients![index].InboxType = InboxType.Outbound;
                        }
                        if (myRole != null)
                        {
                            documentRequest.Uid = myRole!.UId;

                        }
                        documentRequest.RawDocument = rawDocument;
                        documentRequest.DocumentModel = documentModel;

                        // Send request for save document
                        var result = await httpService.Post<DocumentRequest, CommonResponse>(url, documentRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                        onProcessing = false;

                        if (result.Success)
                        {
                            nav.NavigateTo($"/pages/doc/{Link}/{Page}/none", true);
                            AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                        }
                        else
                        {
                            AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
                        }
                        await Task.Delay(delayTime);

                        disposedObj();
                        formMode = FormMode.List;

                    }
                }
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }


        }
        async Task onTerminateButtonClick()
        {
            try
            {
                bool? isTerminated = await terminateBox!.Show();
                if (isTerminated.HasValue)
                {
                    if (isTerminated.Value)
                    {
                        #region Validate Token
                        var getTokenState = await tokenState.ValidateToken();
                        if (!getTokenState)
                            nav.NavigateTo("/authorize");
                        #endregion
                        if (employee == null)
                        {
                            employee = await storageHelper.GetEmployeeProfileAsync();
                        }
                        // Delete button had fire
                        onProcessing = true;
                        await InvokeAsync(StateHasChanged);
                        string url = $"{endpoint.API}/api/v1/Document/SaveDocument/{roleId}";
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            token = await accessToken.GetTokenAsync();
                        }

                        // Set current status to delete
                        var index = documentModel!.Recipients!.IndexOf(myRole!);
                        documentModel!.Recipients![index].DocStatus = TraceStatus.Terminated;
                        documentModel!.Recipients![index].BeforeMoveToTrash = TraceStatus.Terminated;
                        documentModel!.Recipients![index].Behavior = BehaviorStatus.ReadOnly;
                        documentModel!.Recipients![index].Comment = new CommentModel
                        {
                            Comment = terminateComment,
                            CommentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            RoleTrace = new RoleTraceModel
                            {
                                Fullname = new ShortEmpInfo
                                {
                                    EmployeeID = employee.EmployeeID,
                                    Name = new MultiLanguage
                                    {
                                        Eng = $"{employee.Name.Eng} {employee.FamilyName.Local}",
                                        Local = $"{employee.Name.Local} {employee.FamilyName.Local}"
                                    }
                                },
                                RoleID = myRole!.RecipientInfo.RoleID,
                                RoleType = myRole.RecipientInfo.RoleType,
                                Position = myRole.RecipientInfo.Position

                            }
                        };

                        DocumentRequest documentRequest = new DocumentRequest();
                        if (Link == "inbound")
                        {
                            documentModel!.Recipients![index].InboxType = InboxType.Inbound;
                            //documentModel!.InboxType = InboxType.Inbound;

                        }
                        else
                        {
                            documentModel!.Recipients![index].InboxType = InboxType.Outbound;
                            //documentModel!.InboxType = InboxType.Outbound;
                        }
                        documentRequest.RawDocument = rawDocument;
                        documentRequest.DocumentModel = documentModel;

                        // Send request for save document
                        var result = await httpService.Post<DocumentRequest, CommonResponse>(url, documentRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                        onProcessing = false;

                        if (result.Success)
                        {
                            nav.NavigateTo($"/pages/doc/{Link}/{Page}/none", true);
                            AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                        }
                        else
                        {
                            AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
                        }
                        await Task.Delay(delayTime);

                        disposedObj();
                        formMode = FormMode.List;

                    }
                }
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }


        }
        void onEditButtonClick()
        {
            if (string.IsNullOrWhiteSpace(rawDocument!.ResponseUnit))
            {
                rawDocument!.ResponseUnit = publisher!.Id;
            }
            formMode = FormMode.Edit;

        }
        async Task onRestoreButtonClick()
        {
            try
            {
                bool? isRestore = await restoreBox!.Show();
                if (isRestore.HasValue)
                {
                    if (isRestore.Value)
                    {
                        #region Validate Token
                        var getTokenState = await tokenState.ValidateToken();
                        if (!getTokenState)
                            nav.NavigateTo("/authorize");
                        #endregion
                        // Delete button had fire
                        onProcessing = true;
                        await InvokeAsync(StateHasChanged);
                        if (employee == null)
                        {
                            employee = await storageHelper.GetEmployeeProfileAsync();
                        }
                        string url = $"{endpoint.API}/api/v1/Document/SaveDocument/{roleId}";
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            token = await accessToken.GetTokenAsync();
                        }

                        // Set current status to delete
                        var index = documentModel!.Recipients!.IndexOf(myRole!);
                        documentModel!.Recipients![index].DocStatus = myRole!.BeforeMoveToTrash;

                        DocumentRequest documentRequest = new DocumentRequest();
                        if (Link == "inbound")
                        {
                            documentModel!.Recipients![index].InboxType = InboxType.Inbound;
                            //documentModel!.InboxType = InboxType.Inbound;

                        }
                        else
                        {
                            documentModel!.Recipients![index].InboxType = InboxType.Outbound;
                            //documentModel!.InboxType = InboxType.Outbound;
                        }
                        if (myRole != null)
                        {
                            documentRequest.Uid = myRole!.UId;

                        }
                        documentRequest.RawDocument = rawDocument;
                        documentRequest.DocumentModel = documentModel;

                        // Send request for save document
                        var result = await httpService.Post<DocumentRequest, CommonResponse>(url, documentRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                        onProcessing = false;

                        if (result.Success)
                        {
                            nav.NavigateTo($"/pages/doc/{Link}/{Page}/none", true);
                            AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                        }
                        else
                        {
                            AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
                        }
                        await Task.Delay(delayTime);

                        disposedObj();
                        formMode = FormMode.List;

                    }
                }
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }


        }
        async void onRowClick(DocumentModel item)
        {

            onProcessing = true;
            await bindDocumentModel(item);
            onProcessing = false;
            await InvokeAsync(StateHasChanged);

        }
        async Task onSendButtonClick()
        {
            try
            {
                bool? sendBox = await mbox!.Show();
                if (sendBox.HasValue)
                {
                    if (sendBox.Value)
                    {
                        #region Validate Token
                        var getTokenState = await tokenState.ValidateToken();
                        if (!getTokenState)
                            nav.NavigateTo("/authorize");
                        #endregion
                        onProcessing = true;
                        await InvokeAsync(StateHasChanged);
                        // Add receiver to model before save
                        if (employee == null)
                        {
                            employee = await storageHelper.GetEmployeeProfileAsync();
                        }
                        // Validate field
                        if (!validateField())
                        {
                            AlertMessage("ກະລຸນາປ້ອນຂໍ້ມູນໃຫ້ຄົບຖ້ວນ", Defaults.Classes.Position.BottomRight, Severity.Warning);
                            onProcessing = false;
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(mainReciver!.Id))
                        {
                            AlertMessage("ກະລຸນາເລືອກຜູ້ຮັບ", Defaults.Classes.Position.BottomRight, Severity.Warning);
                            onProcessing = false;
                            return;
                        }

                        string url = $"{endpoint.API}/api/v1/Document/SaveDocument/{roleId}";
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            token = await accessToken.GetTokenAsync();
                        }

                        // Upload file via Minio SDK
                        await manageFileToServer();

                        // Bind Files
                        rawDocument!.Attachments = files.Select(x => x.Info).ToList();
                        rawDocument!.RelateFles = relateFiles.Select(x => x.Info).ToList();

                        DocumentRequest documentRequest = new DocumentRequest();
                        if (Link == "inbound")
                        {
                            documentRequest.InboxType = InboxType.Inbound;

                        }
                        else
                        {
                            documentRequest.InboxType = InboxType.Outbound;
                        }
                        if (myRole != null)
                        {
                            documentRequest.Uid = myRole!.UId;
                            var index = documentModel!.Recipients!.IndexOf(myRole!);
                            if (index >= 0)
                            {
                                documentModel!.Recipients![index].DocStatus = TraceStatus.Completed;
                                documentModel!.Recipients![index].BeforeMoveToTrash = TraceStatus.Completed;
                                documentModel!.Recipients![index].Behavior = BehaviorStatus.ReadOnly;
                                documentModel!.Recipients![index].SendRoleType = myRole!.ReceiveRoleType;
                                documentModel!.Recipients![index].SendDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                                documentModel!.Recipients![index].CompletedDate = Convert.ToDecimal(DateTime.Now.ToString("yyyyMMddHHmmss"));
                                documentModel!.Recipients![index].Comment = new CommentModel
                                {
                                    Comment = mainReciver!.Comment!.Comment,
                                    CommentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                                    RoleTrace = new RoleTraceModel
                                    {
                                        Fullname = new ShortEmpInfo
                                        {
                                            EmployeeID = employee!.EmployeeID,
                                            Name = new MultiLanguage
                                            {
                                                Eng = $"{employee.Name.Eng} {employee.FamilyName.Local}",
                                                Local = $"{employee.Name.Local} {employee.FamilyName.Local}"
                                            }
                                        },
                                        RoleID = myRole!.RecipientInfo.RoleID,
                                        RoleType = myRole.RecipientInfo.RoleType,
                                        Position = myRole.RecipientInfo.Position

                                    }
                                };

                                if (Link == "inbound")
                                {
                                    documentModel!.Recipients![index].InboxType = InboxType.Inbound;
                                    //documentModel!.InboxType = InboxType.Inbound;
                                }
                                else
                                {
                                    documentModel!.Recipients![index].InboxType = InboxType.Outbound;
                                    //documentModel!.InboxType = InboxType.Outbound;
                                }
                            }

                        }
                        if (string.IsNullOrWhiteSpace(rawDocument.SendDate))
                        {
                            rawDocument.SendDate = DateTime.Now.ToString("dd/MM/yyyy");
                        }

                        
                        documentRequest.RawDocument = rawDocument;
                        documentRequest.DocumentModel = documentModel;
                        // Bind Receiver
                        documentRequest.Main = generateMain(mainReciver, recipients!);
                        documentRequest.CoProcesses = generateCoprocess(recipients!.Where(x => x.CoProcess && x.Role.RoleID != mainReciver!.Id));
                        
                        var contentStr = JsonSerializer.Serialize(documentRequest);

                        // Send request for save document
                        var result = await httpService.Post<DocumentRequest, CommonResponseId>(url, documentRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                        onProcessing = false;

                        if (result.Success)
                        {
                            if (result.Response.Code == nameof(ResultCode.NEW_DOCUMENT))
                            {
                                nav.NavigateTo($"/pages/doc/{Link}/draft/none", true);
                            }
                            else
                            {
                                nav.NavigateTo($"/pages/doc/{Link}/{Page}/none", true);

                            }
                            List<string>? notifyRole = new List<string>() { documentRequest.Main.Id! };
                            notifyRole.AddRange(documentRequest.CoProcesses.Select(x => x.Model!.Role.RoleID)!);
                            // Save notification
                            List<Task> noticeTasks = new();
                            NotificationModel noticeRequest = new NotificationModel
                            {
                                RefDocument = result.Response.Id,
                                //id = Guid.NewGuid().ToString("N"),
                                IsRead = false,
                                ModuleType = documentRequest.Main.InboxType == InboxType.Inbound ? ModuleType.DocumentInbound : ModuleType.DocumentOutbound,
                                RoleID = documentRequest.Main.Id,
                                Title = documentRequest.RawDocument.Title,
                                SendFrom = $"{employee.Name.Local} {employee.FamilyName.Local}",
                                ChangeNote = "ເອກະສານສົ່ງໃຫ້ທ່ານເປັນຜູ້ແກ້ໄຂຫຼັກ",
                                SendFromRole = myRole!.RecipientInfo.Position.Local
                            };
                            noticeTasks.Add(httpService.Post<NotificationModel, CommonResponse>($"{endpoint.API}/api/v1/Notification/Create", noticeRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token));

                            // Save Notification for CC User
                            if (!documentRequest.CoProcesses.IsNullOrEmpty())
                            {
                                
                                foreach (var r in documentRequest.CoProcesses)
                                {
                                    NotificationModel ccNoticeRequest = new NotificationModel
                                    {
                                        RefDocument = result.Response.Id,
                                        //id = Guid.NewGuid().ToString("N"),
                                        IsRead = false,
                                        ModuleType = r.InboxType == InboxType.Inbound ? ModuleType.DocumentInbound : ModuleType.DocumentOutbound,
                                        RoleID = r.Model!.Role.RoleID,
                                        Title = documentRequest.RawDocument.Title,
                                        SendFrom = $"{employee.Name.Local} {employee.FamilyName.Local}",
                                        ChangeNote = "ເອກະສານສົ່ງໃຫ້ທ່ານແບບຕິດຕາມ",
                                        SendFromRole = myRole.RecipientInfo.Position.Local
                                    };
                                    noticeTasks.Add(httpService.Post<NotificationModel, CommonResponse>($"{endpoint.API}/api/v1/Notification/Create", ccNoticeRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token));
                                }
                            }

                            await Task.WhenAll(noticeTasks);

                            AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                            
                            Console.WriteLine($"[{DateTime.Now}] Send message to Notify Roles: {JsonSerializer.Serialize(notifyRole)}");
                            var message = new SocketSendModel
                            {
                                Topic = new SocketTopic
                                {
                                    SocketType = SocketType.PUSH_NOTIFY,
                                    RoleIDs = notifyRole
                                },
                                Message = "new document come over"
                            };
                            await socketHelper.SendThroughSocket(message);
                            //await EventNotify.InvokeAsync(message);
                        }
                        else
                        {
                            AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
                        }
                        await Task.Delay(delayTime);

                        disposedObj();
                        formMode = FormMode.List;

                    }
                }
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }

        }

        private MainReceiver generateMain(MainReceiver mainReciver, IEnumerable<RoleTreeModel> roleTrees)
        {
            try
            {
                var item = roleTrees.FirstOrDefault(x => x.Role.RoleID == mainReciver.Id);
                if (item == null)
                {
                    return null;
                }
                if (item.RoleType == RoleTypeModel.InboundGeneral || item.RoleType == RoleTypeModel.InboundOfficePrime || item.RoleType == RoleTypeModel.InboundPrime)
                {
                    mainReciver.InboxType = InboxType.Inbound;

                }
                else if (item.RoleType == RoleTypeModel.OutboundGeneral || item.RoleType == RoleTypeModel.OutboundOfficePrime || item.RoleType == RoleTypeModel.OutboundPrime)
                {
                    mainReciver.InboxType = InboxType.Outbound;
                }
                else
                {
                    if (Link == "inbound")
                    {
                        mainReciver.InboxType = InboxType.Inbound;
                    }
                    else
                    {
                        mainReciver.InboxType = InboxType.Outbound;
                    }
                }
                return mainReciver;
            }
            catch (Exception)
            {

                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
                return null;
            }
            
        }

        async Task onUpdateWhenOpenDocument()
        {
            try
            {
                #region Validate Token
                var getTokenState = await tokenState.ValidateToken();
                if (!getTokenState)
                    nav.NavigateTo("/authorize");
                #endregion
                onProcessing = true;
                if (employee == null)
                {
                    employee = await storageHelper.GetEmployeeProfileAsync();
                }
                string url = $"{endpoint.API}/api/v1/Document/ReadDocument/{documentModel!.id}";
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }

                int index = documentModel.Recipients!.IndexOf(myRole!);
                documentModel.Recipients[index].IsRead = true;
                documentModel.Recipients[index].ReadDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                documentModel.Recipients[index].RecipientInfo.Fullname = new ShortEmpInfo
                {
                    EmployeeID = employee.EmployeeID,
                    Name = new MultiLanguage
                    {
                        Eng = $"{employee.Name.Eng} {employee.FamilyName.Eng}",
                        Local = $"{employee.Name.Local} {employee.FamilyName.Local}"
                    }
                };

                var updateRequest = documentModel.Recipients[index];
                // Send request for save document
                var result = await httpService.Post<Reciepient, CommonResponse>(url, updateRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
                await Task.Delay(delayTime);
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }


        }

        private async Task loadDocumentModel()
        {
            try
            {
                #region Validate Token
                var getTokenState = await tokenState.ValidateToken();
                if (!getTokenState)
                    nav.NavigateTo("/authorize");
                #endregion
                onProcessing = true;
                string url = $"{endpoint.API}/api/v1/Document/GetDocument/{DocId}";
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }

                //roleId = MessageRole;
                var doc = await httpService.Get<DocumentModel, CommonResponse>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                if (!doc.Success)
                {
                    nav.NavigateTo($"/pages/doc/{Link}/draft/none", true);
                    AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, ບໍ່ສາມາດ ໂຫຼດເອກະສານໄດ້", Defaults.Classes.Position.BottomRight, Severity.Error);
                    return;
                }

                await bindDocumentModel(doc.Response); // Bind document model
                if (IsReadMessage!.ToLower() != "true")
                {
                    await onUpdateNotification(); // Update notification

                }
                onProcessing = false;
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception)
            {
                nav.NavigateTo($"/pages/doc/{Link}/draft/none", true);
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, ບໍ່ສາມາດ ໂຫຼດເອກະສານໄດ້", Defaults.Classes.Position.BottomRight, Severity.Error);
            }
        }

        private async Task onUpdateNotification()
        {
            try
            {
                #region Validate Token
                var getTokenState = await tokenState.ValidateToken();
                if (!getTokenState)
                    nav.NavigateTo("/authorize");
                #endregion
                onProcessing = true;
                if (employee == null)
                {
                    employee = await storageHelper.GetEmployeeProfileAsync();
                }
                string url = $"{endpoint.API}/api/v1/Notification/Read/{MessageID}";
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }

                var result = await httpService.Get<CommonResponse>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                // Send notification when read 
                Console.WriteLine($"[{DateTime.Now}] Send message to Notify Owner when read from notification: {employee.id}");
               
                var message = new SocketSendModel
                {
                    Topic = new SocketTopic
                    {
                        SocketType = SocketType.READ_NOTIIFY,
                        UserID = employee.id
                    },
                    Message = $"read document: {DocId}"
                };
                await socketHelper.SendThroughSocket(message);
                //await EventNotify.InvokeAsync(message);
            }
            catch (Exception)
            {

                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }
        }

        private async Task bindDocumentModel(DocumentModel doc)
        {
            try
            {
                // Row click
                documentModel = doc;
                if (string.IsNullOrWhiteSpace(MessageRole))
                {
                    myRole = documentModel!.Recipients!.LastOrDefault(x => x.RecipientInfo.RoleID == roleId);
                }
                else
                {
                    myRole = documentModel!.Recipients!.LastOrDefault(x => x.RecipientInfo.RoleID == MessageRole);
                }
                
                if (myRole != null)
                {
                    rawDocument = documentModel!.RawDatas!.LastOrDefault(x => x.DataID == myRole!.DataID);
                    files = rawDocument!.Attachments.Select(x => new AttachmentDto
                    {
                        Info = x,
                    }).ToList();
                    relateFiles = rawDocument.RelateFles.Select(x => new AttachmentDto
                    {
                        Info = x,
                    }).ToList();

                    if (Link == "inbound" && string.IsNullOrWhiteSpace(rawDocument.DocNo)) // ຖ້າເປັນຂາເຂົ້າ ແລ້ວເອກະສານບໍ່ມີເລກທີຈະສົ່ງຕໍ່ບໍ່ໄດ້
                    {
                        showSendButton = false;
                    }
                    else
                    {
                        showSendButton = true;
                    }

                    // Update when document first open
                    if (!myRole!.IsRead)
                    {
                        await onUpdateWhenOpenDocument(); // Update document

                    }
                    //await onUpdateWhenOpenDocument(); // Update document

                    // Check this is from Trash or View
                    if (myRole!.DocStatus == TraceStatus.Trash)
                    {
                        formMode = FormMode.Trash;


                    }
                    else if (myRole.DocStatus == TraceStatus.Terminated)
                    {
                        formMode = FormMode.Terminated;

                    }
                    else
                    {
                        formMode = FormMode.View;


                    }
                    if (string.IsNullOrWhiteSpace(rawDocument.FolderId))
                    {
                        needFolder = false;
                    }
                    else
                    {
                        needFolder = true;
                    }
                }
                else
                {
                    disposedObj();
                    formMode = FormMode.List;
                    AlertMessage("ເອກະສານດັ່ງກ່າວອາດຖືກຍ້າຍໄປກ່ອງ ສຳເລັດແລ້ວ", Defaults.Classes.Position.BottomRight, Severity.Warning);
                }
                
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }


        }

        private List<CoProcessRequest> generateCoprocess(IEnumerable<RoleTreeModel> roleTrees) 
        {
            try
            {
                List<CoProcessRequest>? result = new();
                foreach (var item in roleTrees)
                {
                    if (item.RoleType == RoleTypeModel.InboundGeneral || item.RoleType == RoleTypeModel.InboundOfficePrime || item.RoleType == RoleTypeModel.InboundPrime)
                    {
                        result.Add(new CoProcessRequest(item, InboxType.Inbound));

                    }
                    else if (item.RoleType == RoleTypeModel.OutboundGeneral || item.RoleType == RoleTypeModel.OutboundOfficePrime || item.RoleType == RoleTypeModel.OutboundPrime)
                    {
                        result.Add(new CoProcessRequest(item, InboxType.Outbound));
                    }
                    else
                    {
                        if (Link == "inbound")
                        {
                            result.Add(new CoProcessRequest(item, InboxType.Inbound));
                        }
                        else
                        {
                            result.Add(new CoProcessRequest(item, InboxType.Outbound));
                        }
                    }
                }

                return result;
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
                return null!;
            }
            
        }
        bool validateField()
        {
            if (Link == "outbound" && string.IsNullOrWhiteSpace(rawDocument!.Title) && string.IsNullOrWhiteSpace(rawDocument!.Signer))
            {
                return false;
            }
            else if (Link == "inbound" && string.IsNullOrWhiteSpace(rawDocument!.Title))
            {
                return false;
            }
            return true;
        }
        async Task onSaveClickAsync()
        {
            try
            {
                #region Validate Token
                var getTokenState = await tokenState.ValidateToken();
                if (!getTokenState)
                    nav.NavigateTo("/authorize");
                #endregion
                onProcessing = true;
                if (employee == null)
                {
                    employee = await storageHelper.GetEmployeeProfileAsync();
                }

                // Validate field
                if (!validateField())
                {
                    AlertMessage("ກະລຸນາປ້ອນຂໍ້ມູນໃຫ້ຄົບຖ້ວນ", Defaults.Classes.Position.BottomRight, Severity.Warning);
                    onProcessing = false;
                    return;
                }


                string url = $"{endpoint.API}/api/v1/Document/SaveDocument/{roleId}";
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }

                // Upload file via Minio SDK
                await manageFileToServer();

                // Bind Files
                rawDocument!.Attachments = files.Select(x => x.Info).ToList();
                rawDocument!.RelateFles = relateFiles.Select(x => x.Info).ToList();

                DocumentRequest documentRequest = new DocumentRequest();

                if (Link == "inbound")
                {
                    documentRequest.InboxType = InboxType.Inbound;

                }
                else
                {
                    documentRequest.InboxType = InboxType.Outbound;
                }

                if (myRole != null)
                {
                    documentRequest.Uid = myRole!.UId;

                }
                documentRequest.RawDocument = rawDocument;
                documentRequest.DocumentModel = documentModel;
                // Send request for save document
                var result = await httpService.Post<DocumentRequest, CommonResponse>(url, documentRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                // Open dialog success message or make small progress bar on top-corner

                onProcessing = false;

                Console.WriteLine($"-------------------------------");
                Console.WriteLine($"Result: {await result.HttpResponseMessage.Content.ReadAsStringAsync()}");
                Console.WriteLine($"-------------------------------");

                if (result.Success)
                {
                    if (result.Response.Code == nameof(ResultCode.NEW_DOCUMENT))
                    {
                        nav.NavigateTo($"/pages/doc/{Link}/draft/none", true);
                    }
                    else
                    {
                        nav.NavigateTo($"/pages/doc/{Link}/{Page}/none", true);

                    }
                    AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                }
                else
                {
                    AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
                }

                await Task.Delay(delayTime);
                disposedObj();
                formMode = FormMode.List;

            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }


        }

        async void disposedObj()
        {
            documentModel = new();
            rawDocument = new();
            files = new List<AttachmentDto>();
            relateFiles = new List<AttachmentDto>();
            mainReciver = new();
            if (publisher == null)
            {
                await getPublisher();
            }
            rawDocument!.ResponseUnit = publisher!.Id;
            needFolder = false;
        }
        void onMessageAlert(string message)
        {
            AlertMessage(message, Defaults.Classes.Position.BottomRight, Severity.Info);
        }
        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
        }
        private async Task manageFileToServer()
        {
            try
            {
                List<Task> tasks = new();
                // Upload new file
                var newAttachFiles = files.Where(x => !x.Info.IsRemove && x.Info.IsNewFile).ToList();
                // Attachment file
                foreach (var item in newAttachFiles)
                {
                    if (item.File != null)
                    {
                        using Stream readStream = item.File!.OpenReadStream(maxFileSize);

                        var buf = new byte[readStream.Length];

                        using MemoryStream ms = new MemoryStream(buf);

                        await readStream.CopyToAsync(ms);

                        var buffer = ms.ToArray();

                        tasks.Add(minio.PutObject(item.Info.Bucket!, item.Info.FileName!, buffer));
                    }

                }

                // Relate file
                var newRelFiles = relateFiles.Where(x => !x.Info.IsRemove && x.Info.IsNewFile).ToList();
                foreach (var item in newRelFiles)
                {
                    if (item.File != null)
                    {
                        using Stream readStream = item.File!.OpenReadStream(maxFileSize);

                        var buf = new byte[readStream.Length];

                        using MemoryStream ms = new MemoryStream(buf);

                        await readStream.CopyToAsync(ms);

                        var buffer = ms.ToArray();

                        tasks.Add(minio.PutObject(item.Info.Bucket!, item.Info.FileName!, buffer));
                    }

                }

                // Remove files
                var removeAttachFiles = files.Where(x => x.Info.IsRemove && !x.Info.IsNewFile).ToList();
                var removeRelFiles = relateFiles.Where(x => x.Info.IsRemove && !x.Info.IsNewFile).ToList();
                foreach (var item in removeAttachFiles)
                {
                    tasks.Add(minio.RemoveObject(item.Info.Bucket!, item.Info.FileName!));
                }
                foreach (var item in removeRelFiles)
                {
                    tasks.Add(minio.RemoveObject(item.Info.Bucket!, item.Info.FileName!));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }


        }

        async Task onTabChangeEventAsync(PartialRole roleItem)
        {
            try
            {
                #region Validate Token
                var getTokenState = await tokenState.ValidateToken();
                if (!getTokenState)
                    nav.NavigateTo("/authorize");
                #endregion
                onProcessing = true;
                roleId = roleItem.RoleID;
                selectedRole = roleItem;
                if (employee == null)
                {
                    employee = await storageHelper.GetEmployeeProfileAsync();
                }
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }
                if (Link == "inbound")
                {
                    if (selectedRole.RoleType == RoleTypeModel.InboundGeneral ||
                        selectedRole.RoleType == RoleTypeModel.InboundOfficePrime ||
                        selectedRole.RoleType == RoleTypeModel.InboundPrime)
                    {
                        showCreateButton = true;
                    }
                    else
                    {
                        showCreateButton = false;
                    }
                }
                else
                {
                    showCreateButton = true;
                }

                string urlGetOrgItem = $"{endpoint.API}/api/v1/Organization/GetItem/{employee.OrganizationID!}/{roleId}/{Link}";
                var result = await httpService.Get<IEnumerable<RoleTreeModel>>(urlGetOrgItem, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
                if (result.Success)
                {
                    var expected = result.Response.ToList();
                    expected.RemoveAll(x => x.Role.RoleID == roleId);
                    recipients = expected;
                    if (recipients.IsNullOrEmpty())
                    {
                        showSendButton = false;
                    }


                }
                else
                {
                    recipients = null;
                    showSendButton = false;
                }

                // Get Publisher
                await getPublisher();


                onProcessing = false;

                await InvokeAsync(StateHasChanged);
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }


        }
        private async Task getPublisher()
        {
            try
            {
                #region Validate Token
                var getTokenState = await tokenState.ValidateToken();
                if (!getTokenState)
                    nav.NavigateTo("/authorize");
                #endregion
                string urlGetPublisher = $"{endpoint.API}/api/v1/Organization/GetPublisher/{employee!.OrganizationID!}/{roleId}";
                var publisherResult = await httpService.Get<CommonResponseId>(urlGetPublisher, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                if (publisherResult.Success)
                {
                    publisher = publisherResult.Response;
                }
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }
            
        }
        private IEnumerable<string> MaxCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 1000 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 1000 ອັກສອນ";
        }
        private IEnumerable<string> MediumCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 500 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 500 ອັກສອນ";
        }
    }
}
