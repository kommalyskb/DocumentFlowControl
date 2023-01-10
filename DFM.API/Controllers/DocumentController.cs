using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
using DFM.Shared.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCouch;
using Redis.OM;
using StackExchange.Redis;
using System.Runtime.CompilerServices;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentTransaction documentTransaction;
        private readonly IRoleManager roleManager;
        private readonly IEmployeeManager employeeManager;
        private readonly IFolderManager folderManager;
        private readonly IMinioService minio;
        private readonly IOrganizationChart organization;
        private readonly IRedisConnector redisConnector;
        private readonly IEmailHelper emailHelper;
        private readonly ServiceEndpoint endpoint;
        private readonly SMTPConfig smtp;

        public DocumentController(IDocumentTransaction documentTransaction, IRoleManager roleManager, IEmployeeManager employeeManager,
            IFolderManager folderManager, IMinioService minio, IOrganizationChart organization, IRedisConnector redisConnector,
            IEmailHelper emailHelper, ServiceEndpoint endpoint, SMTPConfig smtp)
        {
            this.documentTransaction = documentTransaction;
            this.roleManager = roleManager;
            this.employeeManager = employeeManager;
            this.folderManager = folderManager;
            this.minio = minio;
            this.organization = organization;
            this.redisConnector = redisConnector;
            this.emailHelper = emailHelper;
            this.endpoint = endpoint;
            this.smtp = smtp;
        }

        /// <summary>
        /// v1.0.0
        ///  ແມ່ນ API ທີ່ໃຊ້ໃນການດຶງເອົາເອກະສານ ຕາມ Role, ກ່ອງເອກະສານ
        /// </summary>
        /// <param name="page"></param>
        /// <param name="link"></param>
        /// <param name="roleId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("GetDocument/{page}/{link}/{roleId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(ResponseQueryDocument), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDocumentV1(string page, string link, string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Verify link
                InboxType inboxType = InboxType.Inbound;
                if (link == "inbound")
                {
                    inboxType = InboxType.Inbound;
                }
                else if (link == "outbound")
                {
                    inboxType = InboxType.Outbound;

                }
                else
                {
                    return BadRequest(new CommonResponse
                    {
                        Code = nameof(ResultCode.INVALID_LINK),
                        Message = ResultCode.INVALID_LINK,
                        Detail = ResultCode.INVALID_LINK,
                        Success = false
                    });

                }

                // Verify page
                TraceStatus traceStatus = (TraceStatus)Enum.Parse(typeof(TraceStatus), page);

                var result = await documentTransaction.GetDocumentByRoleId(roleId, inboxType, traceStatus, cancellationToken);
                if (result.Response.Success)
                {
                    return Ok(new ResponseQueryDocument
                    {
                        Contents = result.Contents,
                        Response = result.Response,
                        RowCount = result.RowCount
                    });
                }
                return BadRequest(new ResponseQueryDocument
                {
                    Contents = result.Contents,
                    Response = result.Response,
                    RowCount = result.RowCount
                });

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// ແມ່ນ API ທີ່ໃຊ້ດຶງຕົວເລກລາຍງານ
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("GetPersonalReport")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(List<PersonalReportSummary>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPersonalReportV1([FromBody] GetPersonalReportRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                
                var result = await documentTransaction.GetPersonalReport(request, cancellationToken);
                return Ok(result);

            }
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary>
        /// ແມ່ນ API ທີ່ໃຊ້ດຶງເອົາລາຍລະອຽດຂອງເອກະສານຕາມ Report
        /// </summary>
        /// <param name="traceStatus"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("DrillDownReport/{traceStatus}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(PersonalReportSummary), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DrillDownReportV1(TraceStatus traceStatus, [FromBody] GetPersonalReportRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {

                var result = await documentTransaction.DrillDownReport(request, traceStatus, cancellationToken);
                if (result == null)
                {
                    return BadRequest(new CommonResponse
                    {
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ResultCode.NOT_FOUND,
                        Message = ResultCode.NOT_FOUND
                    });
                }
                return Ok(result);

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// v1.0.0 ແມ່ນ API ທີ່ໃຊ້ໃນການດຶງເອົາເອກະສານຕາມ Document ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("GetDocument/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(DocumentModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDocumentV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var result = await documentTransaction.GetDocument(id, cancellationToken);
                if (result.Response.Success)
                {
                    return Ok(result.Content);
                }
                return BadRequest(result.Response);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private bool isCrossRoleType(RoleTypeModel source, RoleTypeModel target)
        {
            if ((source == RoleTypeModel.OutboundGeneral || source == RoleTypeModel.OutboundOfficePrime || source == RoleTypeModel.OutboundPrime) &&
                (target == RoleTypeModel.InboundGeneral || target == RoleTypeModel.InboundOfficePrime || target == RoleTypeModel.InboundPrime))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// v1.0.0
        /// ແມ່ນ API ທີ່ໃຊ້ສຳລັບການເຮັດ Transaction ຂອງ ເອກະສານ
        /// </summary>
        /// <param name="request"></param>
        /// <param name="roleId"></param>
        /// <param name="fakeId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("SaveDocument/{roleId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveDocumentV1([FromBody] DocumentRequest request, string roleId, string fakeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                bool isMainDisplay = true;
                string? mainUUID = "";
                // Get Owner
                string userId = fakeId;// GeneratorHelper.NotAvailable;
                if (User.Claims.FirstOrDefault(x => x.Type == "sub") != null)
                {
                    userId = User.Claims.FirstOrDefault(x => x.Type == "sub")!.Value;

                }
                var myProfile = await employeeManager.GetProfile(userId, cancellationToken);
                if (!myProfile.Response.Success)
                {
                    return BadRequest(myProfile.Response);
                }

                // Get My Role
                var myRole = await roleManager.GetRolePosition(roleId, cancellationToken);
                if (!myRole.Response.Success)
                {
                    return BadRequest(myRole.Response);
                }

                // Set organization id
                request.DocumentModel!.OrganizationID = myProfile.Content.OrganizationID;

                if (string.IsNullOrWhiteSpace(request.DocumentModel!.id))
                {
                    // Update NewFile to OldFile flag
                    request.RawDocument!.Attachments.ForEach(x =>
                    {
                        x.IsNewFile = false;
                    });
                    request.RawDocument!.RelateFles.ForEach(x =>
                    {
                        x.IsNewFile = false;
                    });

                    // New documents
                    //request.DocumentModel.id = Guid.NewGuid().ToString("N");
                    request.RawDocument!.DataID = Guid.NewGuid().ToString("N");

                    // Set first reciever
                    TraceStatus ownerStatus = TraceStatus.Draft;
                    string? ownerSendDate = "";
                    BehaviorStatus ownerBehavior = BehaviorStatus.ReadWrite;
                    OperationType sendRoleType = OperationType.NoProcess;

                    if (!string.IsNullOrWhiteSpace(request.Main.Id))
                    {
                        ownerStatus = TraceStatus.Completed;
                        ownerSendDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                        ownerBehavior = BehaviorStatus.ReadOnly;
                        sendRoleType = OperationType.Main;
                    }

                    var ownRoleTrace = new RoleTraceModel
                    {
                        Position = myRole.Content!.Display,
                        RoleID = myRole.Content!.id,
                        RoleType = myRole.Content!.RoleType,
                        Fullname = new ShortEmpInfo
                        {
                            EmployeeID = myProfile.Content.EmployeeID,
                            Name = new MultiLanguage
                            {
                                Eng = $"{myProfile.Content.Name.Eng} {myProfile.Content.FamilyName.Eng}",
                                Local = $"{myProfile.Content.Name.Local} {myProfile.Content.FamilyName.Local}"
                            }
                        }
                    };
                    var rec = new Reciepient
                    {
                        UId = Guid.NewGuid().ToString("N"),
                        DataID = request.RawDocument!.DataID,
                        IsRead = true,
                        ReadDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        ReceiveDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        DocStatus = ownerStatus,
                        BeforeMoveToTrash = ownerStatus,
                        ParentID = "0",
                        SendDate = ownerSendDate,
                        ReceiveRoleType = OperationType.Main,
                        SendRoleType = sendRoleType,
                        RecipientInfo = ownRoleTrace,
                        Behavior = ownerBehavior,
                        Comment = new CommentModel
                        {
                            Comment = request.Main.Comment!.Comment,
                            CommentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            RoleTrace = ownRoleTrace,
                            Attachments = request.Main.Comment.Attachments
                        }
                    };

                    // Set Reciepient
                    request.DocumentModel.Recipients!.Add(rec);
                    // Set Document Data
                    request.DocumentModel.RawDatas!.Add(request.RawDocument!);
                    List<Task> tasks = new List<Task>();
                    // Set Receiver if User select Send
                    // Set main receiver
                    if (!string.IsNullOrWhiteSpace(request.Main.Id))
                    {

                        var receiverRole = await roleManager.GetRolePosition(request.Main.Id, cancellationToken);
                        string? rawDataId = Guid.NewGuid().ToString("N");
                        BehaviorStatus mainBehavior = BehaviorStatus.ProcessOnly;

                        // Check RawData should make dupplicate or not?
                        if (receiverRole.Content.RoleType == RoleTypeModel.InboundPrime)
                        {
                            // If coming from Office Director always in the same raw data
                            // If coming from Outbound always new raw data
                            if (myRole.Content.RoleType == RoleTypeModel.OfficePrime)
                            {
                                // No need dupplicate
                                rawDataId = request.RawDocument!.DataID;
                                mainBehavior = BehaviorStatus.ProcessOnly;
                            }
                            else
                            {
                                // Dupplicate
                                request.DocumentModel.RawDatas!.Add(new RawDocumentData
                                {
                                    DataID = rawDataId,
                                    Attachments = request.RawDocument.Attachments,
                                    CommentDetail = request.RawDocument.CommentDetail,
                                    CommentNo = request.RawDocument.CommentNo,
                                    CommentTitle = request.RawDocument.CommentTitle,
                                    DocDate = request.RawDocument.SendDate,
                                    DocPage = request.RawDocument.DocPage,
                                    ExternalDocID = request.RawDocument.DocNo,
                                    FolderNum = 1,
                                    FormType = request.RawDocument.FormType,
                                    FromUnit = request.RawDocument.ResponseUnit,
                                    IsOriginalFile = request.RawDocument.IsOriginalFile,
                                    RelateFles = request.RawDocument.RelateFles,
                                    Security = request.RawDocument.Security,
                                    Title = request.RawDocument.Title,
                                    TransferType = request.RawDocument.TransferType,
                                    Urgent = request.RawDocument.Urgent,
                                    CopyType = request.RawDocument.CopyType,
                                    DocType = request.RawDocument.DocType
                                });
                                mainBehavior = BehaviorStatus.ReadWrite;
                            }
                        }
                        else
                        {
                            var isSameParent = await organization.IsInSameParent(myRole.Content.OrganizationID, myRole.Content.id, receiverRole.Content.id, cancellationToken);

                            if (isSameParent)
                            {

                                // Need duplicate if sender is Outbound
                                if (myRole.Content!.RoleType == RoleTypeModel.OutboundGeneral ||
                                   myRole.Content!.RoleType == RoleTypeModel.OutboundOfficePrime ||
                                   myRole.Content!.RoleType == RoleTypeModel.OutboundPrime)
                                {
                                    // Set Document Data
                                    request.DocumentModel.RawDatas!.Add(new RawDocumentData
                                    {
                                        DataID = rawDataId,
                                        Attachments = request.RawDocument.Attachments,
                                        CommentDetail = request.RawDocument.CommentDetail,
                                        CommentNo = request.RawDocument.CommentNo,
                                        CommentTitle = request.RawDocument.CommentTitle,
                                        DocDate = request.RawDocument.SendDate,
                                        DocPage = request.RawDocument.DocPage,
                                        ExternalDocID = request.RawDocument.DocNo,
                                        FolderNum = 1,
                                        FormType = request.RawDocument.FormType,
                                        FromUnit = request.RawDocument.ResponseUnit,
                                        IsOriginalFile = request.RawDocument.IsOriginalFile,
                                        RelateFles = request.RawDocument.RelateFles,
                                        Security = request.RawDocument.Security,
                                        Title = request.RawDocument.Title,
                                        TransferType = request.RawDocument.TransferType,
                                        Urgent = request.RawDocument.Urgent,
                                        CopyType = request.RawDocument.CopyType,
                                        DocType = request.RawDocument.DocType
                                    });
                                    mainBehavior = BehaviorStatus.ReadWrite;
                                }
                                else
                                {
                                    // No need to dupplicate
                                    rawDataId = request.RawDocument!.DataID;
                                    if (receiverRole.Content.RoleType == RoleTypeModel.OutboundGeneral ||
                                        receiverRole.Content.RoleType == RoleTypeModel.OutboundOfficePrime ||
                                        receiverRole.Content.RoleType == RoleTypeModel.OutboundPrime)
                                    {
                                        mainBehavior = BehaviorStatus.ReadWrite;

                                    }
                                    else
                                    {
                                        mainBehavior = BehaviorStatus.ProcessOnly;

                                    }
                                }

                            }
                            else
                            {
                                // Need to dupplicate

                                // Set Document Data
                                request.DocumentModel.RawDatas!.Add(new RawDocumentData
                                {
                                    DataID = rawDataId,
                                    Attachments = request.RawDocument.Attachments,
                                    CommentDetail = request.RawDocument.CommentDetail,
                                    CommentNo = request.RawDocument.CommentNo,
                                    CommentTitle = request.RawDocument.CommentTitle,
                                    DocDate = request.RawDocument.SendDate,
                                    DocPage = request.RawDocument.DocPage,
                                    ExternalDocID = request.RawDocument.DocNo,
                                    FolderNum = 1,
                                    FormType = request.RawDocument.FormType,
                                    FromUnit = request.RawDocument.ResponseUnit,
                                    IsOriginalFile = request.RawDocument.IsOriginalFile,
                                    RelateFles = request.RawDocument.RelateFles,
                                    Security = request.RawDocument.Security,
                                    Title = request.RawDocument.Title,
                                    TransferType = request.RawDocument.TransferType,
                                    Urgent = request.RawDocument.Urgent,
                                    CopyType = request.RawDocument.CopyType,
                                    DocType = request.RawDocument.DocType
                                });
                                mainBehavior = BehaviorStatus.ReadWrite;
                            }
                        }

                        // Check condition for main display
                        if (isCrossRoleType(myRole.Content!.RoleType, receiverRole.Content!.RoleType))
                        {
                            isMainDisplay = false;
                        }

                        // Get Email from main recipient
                        //
                        //
                        if (smtp.IsActivate)
                        {
                            var mainRole = await organization.GetEmployee(request.DocumentModel!.OrganizationID, receiverRole.Content!.id);
                            var mainProfile = await employeeManager.GetProfile(mainRole.Content.Employee.UserID);
                            var mailBody = emailHelper.NotificationMailBody($"{endpoint.Frontend}", $"{ownRoleTrace.Fullname.Name.Local}", $"{request.RawDocument.Title}");

                            // Add to email task
                            tasks.Add(emailHelper.Send(new EmailProperty
                            {
                                Body = mailBody,
                                From = smtp.Username,
                                To = new List<string> { mainProfile.Content.Contact.Email },
                                Subject = $"ເອກະສານມາໃຫມ່ - {request.RawDocument.Title}"
                            }));
                        }


                        // For receiver don't have profile yet, it will appear when someone click read
                        var main = new Reciepient
                        {
                            UId = Guid.NewGuid().ToString("N"),
                            DataID = rawDataId,
                            IsRead = false,
                            ReadDate = "",//DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            ReceiveDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            DocStatus = TraceStatus.InProgress,
                            BeforeMoveToTrash = TraceStatus.InProgress,
                            ParentID = rec.UId,
                            SendDate = "",//DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            ReceiveRoleType = OperationType.Main,
                            SendRoleType = OperationType.NoProcess,
                            RecipientInfo = new RoleTraceModel
                            {
                                Position = receiverRole.Content!.Display,
                                RoleID = receiverRole.Content!.id,
                                RoleType = receiverRole.Content!.RoleType,
                                //Fullname = new ShortEmpInfo
                                //{
                                //    EmployeeID = myProfile.Content.EmployeeID,
                                //    Name = new MultiLanguage
                                //    {
                                //        Eng = $"{myProfile.Content.Name.Eng} {myProfile.Content.FamilyName.Eng}",
                                //        Local = $"{myProfile.Content.Name.Local} {myProfile.Content.FamilyName.Local}"
                                //    }
                                //}
                            },
                            Behavior = mainBehavior,
                            IsDisplay = isMainDisplay
                        };
                        // Set Reciepient
                        request.DocumentModel.Recipients!.Add(main);
                        // Set main UUID for create new record if need
                        mainUUID = main.UId;
                    }
                    // Set Co-Process
                    if (request.CoProcesses!.Count > 0)
                    {
                        List<string> emails = new();
                        foreach (var item in request.CoProcesses)
                        {
                            // Get Email from coprocess recipient
                            //
                            //
                            if (smtp.IsActivate)
                            {
                                var itemRole = await organization.GetEmployee(request.DocumentModel!.OrganizationID, item.Role.RoleID);
                                var itemProfile = await employeeManager.GetProfile(itemRole.Content.Employee.UserID);

                                emails.Add(itemProfile.Content.Contact.Email);
                            }


                            // For receiver don't have profile yet, it will appear when someone click read
                            var coUser = new Reciepient
                            {
                                UId = Guid.NewGuid().ToString("N"),
                                DataID = request.RawDocument!.DataID,
                                IsRead = false,
                                ReadDate = "",//DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                                ReceiveDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                                DocStatus = TraceStatus.CoProccess,
                                BeforeMoveToTrash = TraceStatus.CoProccess,
                                ParentID = rec.UId,
                                SendDate = "",//DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                                ReceiveRoleType = OperationType.CoProccess,
                                SendRoleType = OperationType.NoProcess,
                                RecipientInfo = new RoleTraceModel
                                {
                                    Position = item.Role.Display,
                                    RoleID = item.Role.RoleID,
                                    RoleType = item.Role.RoleType,
                                    //Fullname = new ShortEmpInfo
                                    //{
                                    //    EmployeeID = myProfile.Content.EmployeeID,
                                    //    Name = new MultiLanguage
                                    //    {
                                    //        Eng = $"{myProfile.Content.Name.Eng} {myProfile.Content.FamilyName.Eng}",
                                    //        Local = $"{myProfile.Content.Name.Local} {myProfile.Content.FamilyName.Local}"
                                    //    }
                                    //}
                                },
                                Behavior = BehaviorStatus.ProcessOnly
                            };
                            // Set Reciepient
                            request.DocumentModel.Recipients!.Add(coUser);
                        }
                        if (smtp.IsActivate)
                        {
                            var mailBody = emailHelper.NotificationMailBody($"{endpoint.Frontend}", $"{ownRoleTrace.Fullname.Name.Local}", $"{request.RawDocument.Title}");
                            tasks.Add(emailHelper.Send(new EmailProperty
                            {
                                Body = mailBody,
                                From = smtp.Username,
                                To = emails,
                                Subject = $"ເອກະສານມາໃຫມ່ - {request.RawDocument.Title}"
                            }));
                        }

                    }

                    #region ອັບເດດການນຳໃຊ້ ເລກທີ່ໃນ Folder ແລະ ກວດ ເລກທີ່ວ່າມີການໃຊ້ບໍ່ ຖ້າມີໃຫ້ໃຊ້ເລກທີຖັດໄປ
                    if (!string.IsNullOrWhiteSpace(request.RawDocument!.FolderId!))
                    {
                        var folder = await folderManager.GetFolder(request.RawDocument!.FolderId!, cancellationToken);
                        if (folder.Response.Success)
                        {
                            // ກວດວ່າມີການໃຊ້ເລກທີ່ໃນ folder ດັ່ງກ່າວຫຼືບໍ່
                            var isExistFolderNum = folder.Content.DocNoUsed!.Any(x => x == request.RawDocument.FolderNum);
                            if (!isExistFolderNum)
                            {
                                // Set Folder num history
                                folder.Content.DocNoUsed!.Add(request.RawDocument.FolderNum);

                            }
                            else
                            {
                                // Find next folder num for auto increase
                                int newFolderNum = folderManager.FindNextFolderNum(folder.Content);

                                // Update DocNo
                                string formatType = folder.Content.FormatType!;
                                formatType = formatType.Replace("$docno", $"{folder.Content.Prefix!}{newFolderNum}");
                                formatType = formatType.Replace("$sn", $"{folder.Content.ShortName}");
                                formatType = formatType.Replace("$yyyy", $"{DateTime.Now.Year}");

                                request.RawDocument.FolderNum = newFolderNum;
                                request.RawDocument.DocNo = formatType;

                                // Set Folder num history
                                folder.Content.DocNoUsed!.Add(newFolderNum);
                            }


                            // Set next number
                            folder.Content.NextNumber = folderManager.FindNextFolderNum(folder.Content);

                            // Update folder
                            await folderManager.EditFolder(folder.Content, cancellationToken);
                        }
                    }

                    #endregion

                    var result = await documentTransaction.NewDocument(request.DocumentModel, cancellationToken);

                    // Create new record if it's in condition
                    if (!isMainDisplay)
                    {
                        for (int i = 0; i < request.DocumentModel.Recipients.Count; i++)
                        {
                            if (request.DocumentModel.Recipients[i].UId == mainUUID)
                            {
                                request.DocumentModel.Recipients[i].IsDisplay = true;
                            }
                            else
                            {
                                request.DocumentModel.Recipients[i].IsDisplay = false;
                            }
                        }

                        // Set Parent ID for track back
                        request.DocumentModel.ParentID = result.Id;
                        // New Record
                        await documentTransaction.NewDocument(request.DocumentModel, cancellationToken);
                        // Send email
                        if (smtp.IsActivate)
                        {
                            await Task.WhenAll(tasks);
                        }

                    }


                    if (result.Success)
                    {
                        return Ok(result);
                    }
                    return BadRequest(result);
                }
                else
                {
                    // It's mean update document
                    // Removed files
                    request.RawDocument!.Attachments.RemoveAll(x => x.IsRemove);
                    request.RawDocument!.RelateFles.RemoveAll(x => x.IsRemove);

                    // Update NewFile to OldFile flag
                    request.RawDocument!.Attachments.ForEach(x =>
                    {
                        x.IsNewFile = false;
                    });
                    request.RawDocument!.RelateFles.ForEach(x =>
                    {
                        x.IsNewFile = false;
                    });

                    // Validate the existing recipient
                    //var provider = new RedisConnectionProvider(redisConnector.Connection);
                    //var context = provider.RedisCollection<DocumentModel>();
                    //var doc = await context.FindByIdAsync(request.DocumentModel.id);
                    var doc = await documentTransaction.GetDocument(request.DocumentModel.id, cancellationToken);


                    // Update raw data in document model
                    var oldRawData = doc!.Content.RawDatas!.FirstOrDefault(x => x.DataID == request.RawDocument.DataID);

                    if (oldRawData != null)
                    {
                        #region ອັບເດດການນຳໃຊ້ ເລກທີ່ໃນ Folder ແລະ ກວດ ເລກທີ່ວ່າມີການໃຊ້ບໍ່ ຖ້າມີໃຫ້ໃຊ້ເລກທີຖັດໄປ
                        if (string.IsNullOrWhiteSpace(oldRawData!.FolderId))
                        {
                            if (!string.IsNullOrWhiteSpace(request.RawDocument!.FolderId!))
                            {
                                var folder = await folderManager.GetFolder(request.RawDocument!.FolderId!, cancellationToken);
                                if (folder.Response.Success)
                                {
                                    // ກວດວ່າມີການໃຊ້ເລກທີ່ໃນ folder ດັ່ງກ່າວຫຼືບໍ່
                                    var isExistFolderNum = folder.Content.DocNoUsed!.Any(x => x == request.RawDocument.FolderNum);
                                    if (!isExistFolderNum)
                                    {
                                        // Set Folder num history
                                        folder.Content.DocNoUsed!.Add(request.RawDocument.FolderNum);

                                    }
                                    else
                                    {
                                        // Find next folder num for auto increase
                                        int newFolderNum = folderManager.FindNextFolderNum(folder.Content);

                                        // Update DocNo
                                        string formatType = folder.Content.FormatType!;
                                        formatType = formatType.Replace("$docno", $"{folder.Content.Prefix!}{newFolderNum}");
                                        formatType = formatType.Replace("$sn", $"{folder.Content.ShortName}");
                                        formatType = formatType.Replace("$yyyy", $"{DateTime.Now.Year}");

                                        request.RawDocument.FolderNum = newFolderNum;
                                        request.RawDocument.DocNo = formatType;

                                        // Set Folder num history
                                        folder.Content.DocNoUsed!.Add(newFolderNum);
                                    }


                                    // Set next number
                                    folder.Content.NextNumber = folderManager.FindNextFolderNum(folder.Content);

                                    // Update folder
                                    await folderManager.EditFolder(folder.Content, cancellationToken);
                                }
                            }

                        }

                        #endregion

                        int indexData = doc!.Content.RawDatas!.IndexOf(oldRawData);
                        request.DocumentModel.RawDatas![indexData] = request.RawDocument;
                    }

                    // Sender
                    var sender = doc!.Content.Recipients!.FirstOrDefault(x => x.UId == request.Uid);

                    // Update owner recipient 
                    for (int i = 0; i < doc!.Content.Recipients!.Count; i++)
                    {
                        var recipient = doc!.Content.Recipients![i];
                        foreach (var item in request.DocumentModel.Recipients!)
                        {
                            if (recipient.UId == item.UId)
                            {
                                doc!.Content.Recipients![i] = item;
                            }
                        }
                    }

                    List<Task> tasks = new List<Task>();

                    // Set Receiver if User select Send
                    // Set main receiver
                    if (!string.IsNullOrWhiteSpace(request!.Main!.Id))
                    {

                        var receiverRole = await roleManager.GetRolePosition(request.Main.Id, cancellationToken);
                        string? rawDataId = Guid.NewGuid().ToString("N");
                        BehaviorStatus mainBehavior = BehaviorStatus.ProcessOnly;
                        // Check RawData should make dupplicate or not?
                        if (receiverRole.Content.RoleType == RoleTypeModel.InboundPrime)
                        {
                            // If coming from Office Director always in the same raw data
                            // If coming from Outbound always new raw data
                            if (myRole.Content.RoleType == RoleTypeModel.OfficePrime)
                            {
                                // No need dupplicate
                                rawDataId = request.RawDocument!.DataID;
                                mainBehavior = BehaviorStatus.ProcessOnly;
                            }
                            else
                            {
                                // Dupplicate

                                // Set Document Data
                                request.DocumentModel.RawDatas!.Add(new RawDocumentData
                                {
                                    DataID = rawDataId,
                                    Attachments = request.RawDocument.Attachments,
                                    CommentDetail = request.RawDocument.CommentDetail,
                                    CommentNo = request.RawDocument.CommentNo,
                                    CommentTitle = request.RawDocument.CommentTitle,
                                    DocDate = request.RawDocument.SendDate,
                                    DocPage = request.RawDocument.DocPage,
                                    ExternalDocID = request.RawDocument.DocNo,
                                    FolderNum = 1,
                                    FormType = request.RawDocument.FormType,
                                    FromUnit = request.RawDocument.ResponseUnit,
                                    IsOriginalFile = request.RawDocument.IsOriginalFile,
                                    RelateFles = request.RawDocument.RelateFles,
                                    Security = request.RawDocument.Security,
                                    Title = request.RawDocument.Title,
                                    TransferType = request.RawDocument.TransferType,
                                    Urgent = request.RawDocument.Urgent,
                                    CopyType = request.RawDocument.CopyType,
                                    DocType = request.RawDocument.DocType
                                });
                                mainBehavior = BehaviorStatus.ReadWrite;
                            }
                        }
                        else
                        {
                            var isSameParent = await organization.IsInSameParent(myRole.Content.OrganizationID, myRole.Content.id, receiverRole.Content.id, cancellationToken);

                            if (isSameParent)
                            {
                                // Need duplicate if sender is Outbound
                                if (myRole.Content!.RoleType == RoleTypeModel.OutboundGeneral ||
                                   myRole.Content!.RoleType == RoleTypeModel.OutboundOfficePrime ||
                                   myRole.Content!.RoleType == RoleTypeModel.OutboundPrime)
                                {
                                    // Set Document Data
                                    request.DocumentModel.RawDatas!.Add(new RawDocumentData
                                    {
                                        DataID = rawDataId,
                                        Attachments = request.RawDocument.Attachments,
                                        CommentDetail = request.RawDocument.CommentDetail,
                                        CommentNo = request.RawDocument.CommentNo,
                                        CommentTitle = request.RawDocument.CommentTitle,
                                        DocDate = request.RawDocument.SendDate,
                                        DocPage = request.RawDocument.DocPage,
                                        ExternalDocID = request.RawDocument.DocNo,
                                        FolderNum = 1,
                                        FormType = request.RawDocument.FormType,
                                        FromUnit = request.RawDocument.ResponseUnit,
                                        IsOriginalFile = request.RawDocument.IsOriginalFile,
                                        RelateFles = request.RawDocument.RelateFles,
                                        Security = request.RawDocument.Security,
                                        Title = request.RawDocument.Title,
                                        TransferType = request.RawDocument.TransferType,
                                        Urgent = request.RawDocument.Urgent,
                                        CopyType = request.RawDocument.CopyType,
                                        DocType = request.RawDocument.DocType
                                    });
                                    mainBehavior = BehaviorStatus.ReadWrite;
                                }
                                else
                                {
                                    // No need to dupplicate
                                    rawDataId = request.RawDocument!.DataID;
                                    if (receiverRole.Content.RoleType == RoleTypeModel.OutboundGeneral ||
                                        receiverRole.Content.RoleType == RoleTypeModel.OutboundOfficePrime ||
                                        receiverRole.Content.RoleType == RoleTypeModel.OutboundPrime)
                                    {
                                        mainBehavior = BehaviorStatus.ReadWrite;

                                    }
                                    else
                                    {
                                        mainBehavior = BehaviorStatus.ProcessOnly;

                                    }
                                }
                            }
                            else
                            {
                                // Dupplicate

                                // Set Document Data
                                request.DocumentModel.RawDatas!.Add(new RawDocumentData
                                {
                                    DataID = rawDataId,
                                    Attachments = request.RawDocument.Attachments,
                                    CommentDetail = request.RawDocument.CommentDetail,
                                    CommentNo = request.RawDocument.CommentNo,
                                    CommentTitle = request.RawDocument.CommentTitle,
                                    DocDate = request.RawDocument.SendDate,
                                    DocPage = request.RawDocument.DocPage,
                                    ExternalDocID = request.RawDocument.DocNo,
                                    FolderNum = 1,
                                    FormType = request.RawDocument.FormType,
                                    FromUnit = request.RawDocument.ResponseUnit,
                                    IsOriginalFile = request.RawDocument.IsOriginalFile,
                                    RelateFles = request.RawDocument.RelateFles,
                                    Security = request.RawDocument.Security,
                                    Title = request.RawDocument.Title,
                                    TransferType = request.RawDocument.TransferType,
                                    Urgent = request.RawDocument.Urgent,
                                    CopyType = request.RawDocument.CopyType,
                                    DocType = request.RawDocument.DocType
                                });
                                mainBehavior = BehaviorStatus.ReadWrite;
                            }
                        }

                        // Check condition for main display
                        if (isCrossRoleType(myRole.Content!.RoleType, receiverRole.Content!.RoleType))
                        {
                            isMainDisplay = false;
                        }

                        // Get Email from main recipient
                        //
                        //
                        if (smtp.IsActivate)
                        {
                            var mainRole = await organization.GetEmployee(request.DocumentModel!.OrganizationID, receiverRole.Content!.id);
                            var mainProfile = await employeeManager.GetProfile(mainRole.Content.Employee.UserID);
                            var mailBody = emailHelper.NotificationMailBody($"{endpoint.Frontend}", $"{sender.RecipientInfo.Fullname.Name.Local}", $"{request.RawDocument.Title}");

                            // Add to email task
                            tasks.Add(emailHelper.Send(new EmailProperty
                            {
                                Body = mailBody,
                                From = smtp.Username,
                                To = new List<string> { mainProfile.Content.Contact.Email },
                                Subject = $"ເອກະສານມາໃຫມ່ - {request.RawDocument.Title}"
                            }));
                        }

                        // For receiver don't have profile yet, it will appear when someone click read
                        var main = new Reciepient
                        {
                            UId = Guid.NewGuid().ToString("N"),
                            DataID = rawDataId,
                            IsRead = false,
                            ReadDate = "",//DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            ReceiveDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            DocStatus = TraceStatus.InProgress,
                            BeforeMoveToTrash = TraceStatus.InProgress,
                            ParentID = request.Uid,
                            SendDate = "",//DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            ReceiveRoleType = OperationType.Main,
                            SendRoleType = OperationType.NoProcess,
                            RecipientInfo = new RoleTraceModel
                            {
                                Position = receiverRole.Content!.Display,
                                RoleID = receiverRole.Content!.id,
                                RoleType = receiverRole.Content!.RoleType,
                                //Fullname = new ShortEmpInfo
                                //{
                                //    EmployeeID = myProfile.Content.EmployeeID,
                                //    Name = new MultiLanguage
                                //    {
                                //        Eng = $"{myProfile.Content.Name.Eng} {myProfile.Content.FamilyName.Eng}",
                                //        Local = $"{myProfile.Content.Name.Local} {myProfile.Content.FamilyName.Local}"
                                //    }
                                //}
                            },
                            Behavior = mainBehavior,
                            IsDisplay = isMainDisplay
                        };
                        // Set Reciepient
                        doc!.Content.Recipients!.Add(main);
                    }
                    // Set Co-Process
                    if (request.CoProcesses!.Count > 0)
                    {
                        List<string> emails = new();
                        foreach (var item in request.CoProcesses)
                        {
                            // Get Email from coprocess recipient
                            //
                            //
                            if (smtp.IsActivate)
                            {
                                var itemRole = await organization.GetEmployee(request.DocumentModel!.OrganizationID, item.Role.RoleID);
                                var itemProfile = await employeeManager.GetProfile(itemRole.Content.Employee.UserID);
                                emails.Add(itemProfile.Content.Contact.Email);
                            }


                            // For receiver don't have profile yet, it will appear when someone click read
                            var coUser = new Reciepient
                            {
                                UId = Guid.NewGuid().ToString("N"),
                                DataID = request.RawDocument!.DataID,
                                IsRead = false,
                                ReadDate = "",//DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                                ReceiveDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                                DocStatus = TraceStatus.CoProccess,
                                BeforeMoveToTrash = TraceStatus.CoProccess,
                                ParentID = request.Uid,
                                SendDate = "",//DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                                ReceiveRoleType = OperationType.CoProccess,
                                SendRoleType = OperationType.NoProcess,
                                RecipientInfo = new RoleTraceModel
                                {
                                    Position = item.Role.Display,
                                    RoleID = item.Role.RoleID,
                                    RoleType = item.Role.RoleType,
                                    //Fullname = new ShortEmpInfo
                                    //{
                                    //    EmployeeID = myProfile.Content.EmployeeID,
                                    //    Name = new MultiLanguage
                                    //    {
                                    //        Eng = $"{myProfile.Content.Name.Eng} {myProfile.Content.FamilyName.Eng}",
                                    //        Local = $"{myProfile.Content.Name.Local} {myProfile.Content.FamilyName.Local}"
                                    //    }
                                    //}
                                },
                                Behavior = BehaviorStatus.ProcessOnly
                            };
                            // Set Reciepient
                            doc!.Content.Recipients!.Add(coUser);
                        }
                        if (smtp.IsActivate)
                        {
                            var mailBody = emailHelper.NotificationMailBody($"{endpoint.Frontend}", $"{sender.RecipientInfo.Fullname.Name.Local}", $"{request.RawDocument.Title}");
                            tasks.Add(emailHelper.Send(new EmailProperty
                            {
                                Body = mailBody,
                                From = smtp.Username,
                                To = emails,
                                Subject = $"ເອກະສານມາໃຫມ່ - {request.RawDocument.Title}"
                            }));
                        }

                    }



                    // Set recipient
                    request.DocumentModel.Recipients = doc!.Content.Recipients!;

                    var result = await documentTransaction.EditDocument(request.DocumentModel, cancellationToken);
                    // Send email
                    if (smtp.IsActivate)
                    {
                        await Task.WhenAll(tasks);
                    }
                    // Create new record if it's in condition
                    if (!isMainDisplay)
                    {
                        for (int i = 0; i < request.DocumentModel.Recipients.Count; i++)
                        {
                            if (request.DocumentModel.Recipients[i].UId == mainUUID)
                            {
                                request.DocumentModel.Recipients[i].IsDisplay = true;
                            }
                            else
                            {
                                request.DocumentModel.Recipients[i].IsDisplay = false;
                            }
                        }

                        // Set Parent ID for track back
                        request.DocumentModel.ParentID = result.Id;
                        // New Record
                        await documentTransaction.NewDocument(request.DocumentModel, cancellationToken);
                    }

                    if (result.Success)
                    {
                        return Ok(result);
                    }
                    return BadRequest(result);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet("{bucket}/{filename}/{title}/Download")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadV1(string bucket, string filename, string title, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fileObj = await minio.GetObject(bucket, filename);
            if (fileObj.IsSuccess)
            {
                string mimeType = MimeMapping.MimeUtility.GetMimeMapping(filename);
                return File(fileObj.ByteStream, mimeType, title);
            }

            return NotFound();
        }

    }
}
