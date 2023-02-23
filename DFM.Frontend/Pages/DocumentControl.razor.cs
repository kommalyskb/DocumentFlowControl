using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Resources;
using HttpClientService;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
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
        //protected override void OnParametersSet()
        //{


        //    base.OnParametersSet();
        //}


        //protected override void OnInitialized()
        //{


        //    base.OnInitialized();
        //}
        protected override async Task OnParametersSetAsync()
        {
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
            if (oldPage != Page)
            {
                if (!string.IsNullOrWhiteSpace(DocId) && !string.IsNullOrWhiteSpace(MessageID) && !string.IsNullOrWhiteSpace(MessageRole))
                {
                    await loadDocumentModel();
                }
                else
                {
                    formMode = FormMode.List;
                }
                oldPage = Page!;
            }
            if (oldLink != Link)
            {
                if (!string.IsNullOrWhiteSpace(DocId) && !string.IsNullOrWhiteSpace(MessageID) && !string.IsNullOrWhiteSpace(MessageRole))
                {
                    await loadDocumentModel();
                }
                else
                {
                    formMode = FormMode.List;
                }
                oldLink = Link!;
            }
            if (DocId == "none")
            {
                formMode = FormMode.List;
            }

            if (oldDocID != DocId)
            {
                Console.WriteLine($"[{DateTime.Now}] - Old: {oldDocID}, Doc: {DocId}");
                oldDocID = DocId!;
                if (!string.IsNullOrWhiteSpace(DocId) && !string.IsNullOrWhiteSpace(MessageID) && !string.IsNullOrWhiteSpace(MessageRole))
                {
                    await loadDocumentModel();
                }
            }

            
        }
        protected override async Task OnInitializedAsync()
        {
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
                            documentModel!.InboxType = InboxType.Inbound;

                        }
                        else
                        {
                            documentModel!.InboxType = InboxType.Outbound;
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
                            documentModel!.InboxType = InboxType.Inbound;

                        }
                        else
                        {
                            documentModel!.InboxType = InboxType.Outbound;
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
                            documentModel!.InboxType = InboxType.Inbound;

                        }
                        else
                        {
                            documentModel!.InboxType = InboxType.Outbound;
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
                            }

                        }
                        if (string.IsNullOrWhiteSpace(rawDocument.SendDate))
                        {
                            rawDocument.SendDate = DateTime.Now.ToString("dd/MM/yyyy");
                        }

                        ModuleType moduleType = ModuleType.DocumentInbound;
                        if (Link == "inbound")
                        {
                            documentModel!.InboxType = InboxType.Inbound;
                            moduleType = ModuleType.DocumentInbound;
                        }
                        else
                        {
                            moduleType = ModuleType.DocumentOutbound;
                            documentModel!.InboxType = InboxType.Outbound;
                        }


                        
                        documentRequest.RawDocument = rawDocument;
                        documentRequest.DocumentModel = documentModel;
                        // Bind Receiver
                        documentRequest.Main = mainReciver;
                        documentRequest.CoProcesses = recipients!.Where(x => x.CoProcess && x.Role.RoleID != mainReciver!.Id).ToList();

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
                            List<string>? notifyRole = new List<string>() { documentRequest.Main.Id };
                            notifyRole.AddRange(documentRequest.CoProcesses.Select(x => x.Role.RoleID)!);
                            // Save notification
                            NotificationModel noticeRequest = new NotificationModel
                            {
                                RefDocument = result.Response.Id,
                                id = Guid.NewGuid().ToString("N"),
                                IsRead = false,
                                ModuleType = moduleType,
                                RoleID = documentRequest.Main.Id,
                                Title = documentRequest.RawDocument.Title,
                                SendFrom = $"{employee.Name.Local} {employee.FamilyName.Local}"
                            };
                            await httpService.Post<NotificationModel, CommonResponse>($"{endpoint.API}/api/v1/Notification/Create", noticeRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                            AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                            // Send EventNotify
                            
                            Console.WriteLine($"[{DateTime.Now}] Send message to Notify Roles: {JsonSerializer.Serialize(notifyRole)}");
                            await EventNotify.InvokeAsync(new SocketSendModel
                            {
                                Topic = new SocketTopic
                                {
                                    SocketType = SocketType.PUSH_NOTIFY,
                                    RoleIDs = notifyRole
                                },
                                Message = "new document come over"
                            });
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

        async Task onUpdateWhenOpenDocument()
        {
            try
            {
                onProcessing = true;
                if (employee == null)
                {
                    employee = await storageHelper.GetEmployeeProfileAsync();
                }
                string url = $"{endpoint.API}/api/v1/Document/SaveDocument/{roleId}";
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }

                // Bind Files
                rawDocument!.Attachments = files.Select(x => x.Info).ToList();
                rawDocument!.RelateFles = relateFiles.Select(x => x.Info).ToList();

                DocumentRequest documentRequest = new DocumentRequest();
                if (Link == "inbound")
                {
                    documentModel!.InboxType = InboxType.Inbound;

                }
                else
                {
                    documentModel!.InboxType = InboxType.Outbound;
                }
                documentRequest.RawDocument = rawDocument;
                documentRequest.DocumentModel = documentModel;
                int index = documentRequest.DocumentModel.Recipients!.IndexOf(myRole!);
                documentRequest.DocumentModel.Recipients[index].IsRead = true;
                documentRequest.DocumentModel.Recipients[index].ReadDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                documentRequest.DocumentModel.Recipients[index].RecipientInfo.Fullname = new ShortEmpInfo
                {
                    EmployeeID = employee.EmployeeID,
                    Name = new MultiLanguage
                    {
                        Eng = $"{employee.Name.Eng} {employee.FamilyName.Eng}",
                        Local = $"{employee.Name.Local} {employee.FamilyName.Local}"
                    }
                };
                // Send request for save document
                var result = await httpService.Post<DocumentRequest, CommonResponse>(url, documentRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
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
                onProcessing = true;
                string url = $"{endpoint.API}/api/v1/Document/GetDocument/{DocId}";
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }

                roleId = MessageRole;
                var doc = await httpService.Get<DocumentModel, CommonResponse>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                if (!doc.Success)
                {
                    nav.NavigateTo($"/pages/doc/{Link}/draft/none", true);
                    AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, ບໍ່ສາມາດ ໂຫຼດເອກະສານໄດ້", Defaults.Classes.Position.BottomRight, Severity.Error);
                    return;
                }

                await bindDocumentModel(doc.Response); // Bind document model
                await onUpdateNotification(); // Update notification
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
                await EventNotify.InvokeAsync(new SocketSendModel
                {
                    Topic = new SocketTopic
                    {
                        SocketType = SocketType.READ_NOTIIFY,
                        UserID = employee.id
                    },
                    Message = $"read document: {DocId}"
                });

            }
            catch (Exception)
            {

                
            }
        }

        private async Task bindDocumentModel(DocumentModel doc)
        {
            try
            {
                // Row click
                documentModel = doc;

                myRole = documentModel!.Recipients!.LastOrDefault(x => x.RecipientInfo.RoleID == roleId);
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
                    noNeedFolder = true;
                }
                else
                {
                    noNeedFolder = false;
                }
            }
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }

           
        }
        bool validateField()
        {

            if (string.IsNullOrWhiteSpace(rawDocument!.Title))
            {
                return false;
            }
            return true;
        }
        async Task onSaveClickAsync()
        {
            try
            {
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
                    documentModel!.InboxType = InboxType.Inbound;

                }
                else
                {
                    documentModel!.InboxType = InboxType.Outbound;
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

        void disposedObj()
        {
            documentModel = new();
            rawDocument = new();
            files = new List<AttachmentDto>();
            relateFiles = new List<AttachmentDto>();
            mainReciver = new();
            rawDocument!.ResponseUnit = publisher!.Id;
            noNeedFolder = true;
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


                }

                // Get Publisher
                string urlGetPublisher = $"{endpoint.API}/api/v1/Organization/GetPublisher/{employee.OrganizationID!}/{roleId}";
                var publisherResult = await httpService.Get<CommonResponseId>(urlGetPublisher, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                if (publisherResult.Success)
                {
                    publisher = publisherResult.Response;
                }


                onProcessing = false;

                await InvokeAsync(StateHasChanged);
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
