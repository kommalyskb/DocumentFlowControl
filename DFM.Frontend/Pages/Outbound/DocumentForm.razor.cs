using DFM.Frontend.Shared;
using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Helper;
using Google.Protobuf.WellKnownTypes;
using HttpClientService;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Text.Json;
using System.Threading.Channels;

namespace DFM.Frontend.Pages.Outbound
{
    public partial class DocumentForm
    {
        readonly int delayTime = 500;
        int docNumber = 1;
        long maxFileSize = 1024 * 1024 * 25; // 5 MB or whatever, don't just use max int

        protected override async Task OnInitializedAsync()
        {
            token = await accessToken.GetTokenAsync();
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            await Task.Delay(500);
            if (employee is not null)
            {
                // Use chanel to control concurrency
                var channel = Channel.CreateUnbounded<(bool success, int component, string response)>();
                // Consumer
                var consumer = bindDataToVariables(channel.Reader);
                // Producer
                var produce = loadDataTasks(channel.Writer);

                // Await for result
                await consumer;
                await produce;

                // Check This load is new draft or from row click
                if (string.IsNullOrWhiteSpace(DocumentModel!.id))
                {
                    // New draft
                }
                else
                {
                    // From row click
                    //var myDoc = DocumentModel!.Reciepients!.LastOrDefault(x => x.ReciepientInfo.RoleID == RoleId);
                    //RawDocument = DocumentModel!.RawDatas!.LastOrDefault(x => x.DataID == myDoc!.DataID);
                    // Set organization if new document
                    //if (string.IsNullOrWhiteSpace(DocumentModel.OrganizationID))
                    //{
                    //    DocumentModel.OrganizationID = employee.OrganizationID;
                    //}

                    // Set MudSelect default values
                    if (!string.IsNullOrEmpty(RawDocument!.FolderId))
                    {
                        selectFolderValues = new List<string>() { RawDocument!.FolderId! };

                    }
                    if (!string.IsNullOrEmpty(RawDocument!.Urgent!.id))
                    {
                        selectUrgentValues = new List<string>() { RawDocument!.Urgent!.id! };

                    }
                    if (!string.IsNullOrEmpty(RawDocument!.Security!.id))
                    {
                        selectSecurityValues = new List<string>() { RawDocument!.Security!.id! };
                    }
                    if (!string.IsNullOrEmpty(RawDocument!.DocType))
                    {
                        selectDoctypeValues = new List<string>() { RawDocument!.DocType! };
                    }

                    // Set DocNumber = Folder Num
                    docNumber = RawDocument!.FolderNum;

                    // Bind file
                    //Attachments = RawDocument!.Attachments.Select(x => new AttachmentDto
                    //{
                    //    Info = x,
                    //}).ToList();
                    //RelateFiles = RawDocument!.RelateFles.Select(x => new AttachmentDto
                    //{
                    //    Info = x,
                    //}).ToList();

                    // Generate Link if this document has files

                }



                await InvokeAsync(StateHasChanged);
                Console.WriteLine($"{DateTime.Now} Load Inbound document form");

            }



        }

        private async Task bindDataToVariables(ChannelReader<(bool success, int component, string response)> reader)
        {
            await foreach (var item in reader.ReadAllAsync())
            {
                if (item.success)
                {
                    if (item.component == 1)
                    {
                        urgentModels = JsonSerializer.Deserialize<IEnumerable<DocumentUrgentModel>>(item.response);
                    }
                    if (item.component == 2)
                    {
                        securityModels = JsonSerializer.Deserialize<IEnumerable<DocumentSecurityModel>>(item.response);
                    }
                    if (item.component == 3)
                    {
                        docTypeModels = JsonSerializer.Deserialize<IEnumerable<DataTypeModel>>(item.response);
                    }
                    if (item.component == 4)
                    {
                        var allFolders = JsonSerializer.Deserialize<IEnumerable<FolderModel>>(item.response);
                        // Remove Expire folder
                        foreach (var folder in allFolders!)
                        {
                            var expiredDate = DateTime.ParseExact(folder.ExpiredDate!, "dd/MM/yyyy", null);
                            if (DateTime.Now <= expiredDate)
                            {
                                folderModels!.Add(folder);
                            }
                        }
                    }
                    if (item.component == 5)
                    {
                        document = JsonSerializer.Deserialize<DocumentModel>(item.response);
                    }
                }
            }
        }

        private async Task loadDataTasks(ChannelWriter<(bool success, int component, string response)> writer)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(fetchAPI($"{endpoint.API}/api/v1/UrgentLevel/GetItems/{employee!.OrganizationID}", 1, writer));
            tasks.Add(fetchAPI($"{endpoint.API}/api/v1/SecurityLevel/GetItems/{employee!.OrganizationID}", 2, writer));
            tasks.Add(fetchAPI($"{endpoint.API}/api/v1/DocumentType/GetItems/{employee!.OrganizationID}", 3, writer));
            tasks.Add(fetchAPI($"{endpoint.API}/api/v1/Folder/GetItems/{RoleId}/outbound?view=0", 4, writer));
            tasks.Add(fetchAPI($"{endpoint.API}/api/v1/Document/GetDocument/{DocumentModel!.id}", 5, writer));

            await Task.WhenAll(tasks);
            writer.Complete();
        }

        private async Task fetchAPI(string url, int component, ChannelWriter<(bool success, int component, string response)> writer)
        {
            if (component == 1)
            {
                // Load Urgent Level
                var result = await httpService.Get<IEnumerable<DocumentUrgentModel>>(url, new AuthorizeHeader("bearer", token));
                if (result.Success)
                {
                    var jsonDoc = JsonSerializer.Serialize(result.Response);
                    await writer.WriteAsync((result.Success, component, jsonDoc));
                }
                else
                {
                    await writer.WriteAsync((result.Success, component, ""));
                }

            }
            if (component == 2)
            {
                // Load Security Level
                var result = await httpService.Get<IEnumerable<DocumentSecurityModel>>(url, new AuthorizeHeader("bearer", token));
                if (result.Success)
                {
                    var jsonDoc = JsonSerializer.Serialize(result.Response);
                    await writer.WriteAsync((result.Success, component, jsonDoc));
                }
                else
                {
                    await writer.WriteAsync((result.Success, component, ""));
                }

            }
            if (component == 3)
            {
                // Load Document Type
                var result = await httpService.Get<IEnumerable<DataTypeModel>>(url, new AuthorizeHeader("bearer", token));
                if (result.Success)
                {
                    var jsonDoc = JsonSerializer.Serialize(result.Response);
                    await writer.WriteAsync((result.Success, component, jsonDoc));
                }
                else
                {
                    await writer.WriteAsync((result.Success, component, ""));
                }

            }
            if (component == 4)
            {
                // Load Folder
                var result = await httpService.Get<IEnumerable<FolderModel>>(url, new AuthorizeHeader("bearer", token));
                if (result.Success)
                {
                    var jsonDoc = JsonSerializer.Serialize(result.Response.OrderBy(x => x.Seq));
                    await writer.WriteAsync((result.Success, component, jsonDoc));
                }
                else
                {
                    await writer.WriteAsync((result.Success, component, ""));
                }

            }
            if (component == 5)
            {
                // Load Document
                var result = await httpService.Get<DocumentModel>(url, new AuthorizeHeader("bearer", token));
                if (result.Success)
                {
                    var jsonDoc = JsonSerializer.Serialize(result.Response);
                    await writer.WriteAsync((result.Success, component, jsonDoc));
                }
                else
                {
                    await writer.WriteAsync((result.Success, component, ""));
                }

            }
        }


        private async Task displayFolderDialogAsync()
        {
            bool? result = await mbox!.Show();
            if (result.HasValue)
            {
                if (result.Value)
                {
                    onFolderProcessing = true;
                    string token = await accessToken.GetTokenAsync();
                    await InvokeAsync(StateHasChanged);

                    folderModel!.InboxType = InboxType.Outbound; // Inbound folder

                    httpService.MediaType = MediaType.JSON;
                    if (string.IsNullOrWhiteSpace(folderModel.id))
                    {
                        if (employee == null)
                        {
                            employee = await storageHelper.GetEmployeeProfileAsync();
                        };
                        folderModel.OrganizationID = employee.OrganizationID;
                        // New folder
                        string url = $"{endpoint.API}/api/v1/Folder/NewItem";
                        var resultFolder = await httpService.Post<FolderModel, CommonResponseId>(url, folderModel, new AuthorizeHeader("bearer", token));

                        if (resultFolder.Success)
                        {
                            await Notice.InvokeAsync("ທຸລະກຳຂອງທ່ານ(ສ້າງແຟ້ມ) ສຳເລັດ");
                        }
                        else
                        {
                            await Notice.InvokeAsync("ທຸລະກຳຂອງທ່ານ(ສ້າງແຟ້ມ) ຜິດພາດ");
                        }
                    }
                    else
                    {
                        // Update folder
                        string url = $"{endpoint.API}/api/v1/Folder/UpdateItem";
                        var resultFolder = await httpService.Post<FolderModel, CommonResponseId>(url, folderModel, new AuthorizeHeader("bearer", token));

                        if (resultFolder.Success)
                        {
                            await Notice.InvokeAsync("ທຸລະກຳຂອງທ່ານ(ສ້າງແຟ້ມ) ສຳເລັດ");
                        }
                        else
                        {
                            await Notice.InvokeAsync("ທຸລະກຳຂອງທ່ານ(ສ້າງແຟ້ມ) ຜິດພາດ");
                        }
                    }

                    onFolderProcessing = false;
                    
                    await Task.Delay(delayTime);
                    // Dispose folder
                    folderModel = new();
                }
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

        private async Task uploadFiles(InputFileChangeEventArgs e)
        {
            if (e.FileCount > 10)
            {
                await Notice.InvokeAsync("ບໍ່ສາມາດເລືອກ ເອກະສານໄດ້ພ້ອມກັນ ເກີນ 10  Files");
                return;
            }
            string prefix = DateTime.Now.ToString("yyyyMM");
            foreach (var file in e.GetMultipleFiles())
            {
                var ext = Path.GetExtension(file.Name);
                var name = file.Name.Replace(ext, "");
                AttachmentDto attach = new();
                attach.File = file;
                decimal fileSize = attach.File.Size / 1048576M;
                string? fileFormat = Icons.Custom.FileFormats.FileDocument;
                if (file.ContentType.Contains("word"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileWord;
                }
                if (file.ContentType.Contains("pdf"))
                {
                    fileFormat = Icons.Custom.FileFormats.FilePdf;
                }
                if (file.ContentType.StartsWith("image"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileImage;
                }
                if (file.ContentType.Contains("excel"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileWord;
                }
                if (file.ContentType.Contains("audio"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileMusic;
                }
                attach.Info = new()
                {
                    Bucket = prefix,
                    Display = file.Name,
                    FileName = $"{Path.GetRandomFileName()}({name}){ext}",
                    Version = 1,
                    FileSize = fileSize,
                    IsNewFile = true,
                    FileFormat = fileFormat,
                    MimeType = file.ContentType
                };
                Attachments!.Add(attach);
            }
            //TODO upload the files to the server

            await InvokeAsync(StateHasChanged);
        }
        private async Task previewPdf(string bucket, string fileName, string title)
        {
            previewButton = true;
            string? pdfUrl;
            // Generate link
            var link = await minio.GenerateLink(bucket, fileName);
            if (link.IsSuccess)
            {
                pdfUrl = link.Response;
            }
            else
            {
                pdfUrl = "";
            }
            previewButton = false;

            DialogOptions options = new DialogOptions() { FullScreen = true, CloseButton = true };
            DialogParameters parameters = new DialogParameters();
            parameters.Add("PdfUrl", $"{pdfUrl}#zoom=85%");
            parameters.Add("IsUrl", true);
            DialogService.Show<PdfViewDialog>(title, parameters, options);
        }
        private async Task previewPdf(IBrowserFile file)
        {
            previewButton = true;
            var readStream = file!.OpenReadStream(maxFileSize);

            var buf = new byte[readStream.Length];

            var ms = new MemoryStream(buf);

            await readStream.CopyToAsync(ms);

            var buffer = ms.ToArray();
            DialogOptions options = new DialogOptions() { FullScreen = true, CloseButton = true };
            DialogParameters parameters = new DialogParameters();
            parameters.Add("PdfStream", buffer);
            parameters.Add("IsUrl", false);
            previewButton = false;
            DialogService.Show<PdfViewDialog>(file.Name, parameters, options);
        }

        private async Task uploadReladteFiles(InputFileChangeEventArgs e)
        {
            if (e.FileCount > 10)
            {
                await Notice.InvokeAsync("ບໍ່ສາມາດເລືອກ ເອກະສານໄດ້ພ້ອມກັນ ເກີນ 10  Files");
                return;
            }
            string prefix = DateTime.Now.ToString("yyyyMM");
            foreach (var file in e.GetMultipleFiles())
            {
                var ext = Path.GetExtension(file.Name);
                var name = file.Name.Replace(ext, "");
                AttachmentDto attach = new();
                attach.File = file;
                decimal fileSize = attach.File.Size / 1048576M;
                string? fileFormat = Icons.Custom.FileFormats.FileDocument;
                if (file.ContentType.Contains("word"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileWord;
                }
                if (file.ContentType.Contains("pdf"))
                {
                    fileFormat = Icons.Custom.FileFormats.FilePdf;
                }
                if (file.ContentType.StartsWith("image"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileImage;
                }
                if (file.ContentType.Contains("excel"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileWord;
                }
                if (file.ContentType.Contains("audio"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileMusic;
                }
                attach.Info = new()
                {
                    Bucket = prefix,
                    Display = file.Name,
                    FileName = $"{Path.GetRandomFileName()}({name}){ext}",
                    Version = 1,
                    FileSize = fileSize,
                    IsNewFile = true,
                    FileFormat = fileFormat,
                    MimeType = file.ContentType
                };
                RelateFiles!.Add(attach);
            }
            //TODO upload the files to the server
            await InvokeAsync(StateHasChanged);
        }

        private async Task onSelectedFolderChanged(IEnumerable<string> values)
        {
            // Get Folder ID where index = 0
            string id = values.FirstOrDefault()!;
            selectFolder = folderModels!.FirstOrDefault(x => x.id == id);
            docNumber = selectFolder!.NextNumber;
            string formatType = selectFolder!.FormatType!;
            formatType = formatType!.Replace("$docno", $"{selectFolder.Prefix!}{docNumber}");
            formatType = formatType!.Replace("$sn", $"{selectFolder.ShortName}");
            formatType = formatType!.Replace("$yyyy", $"{DateTime.Now.Year}");


            RawDocument!.DocNo = formatType;
            RawDocument!.FolderNum = docNumber;
            await InvokeAsync(StateHasChanged);
        }
        private async Task onDocNoClick()
        {
            int oldDocNo = docNumber;
            bool? result = await docnoBox!.Show();
            bool isConfirm = result == null ? false : true;
            if (!isConfirm)
            {
                docNumber = oldDocNo;
            }
            else
            {
                string formatType = selectFolder!.FormatType!;
                formatType = formatType.Replace("$docno", $"{selectFolder.Prefix!}{docNumber}");
                formatType = formatType.Replace("$sn", $"{selectFolder.ShortName}");
                formatType = formatType.Replace("$yyyy", $"{DateTime.Now.Year}");


                RawDocument!.DocNo = formatType;
                RawDocument!.FolderNum = docNumber;
            }
            await InvokeAsync(StateHasChanged);
        }

        private async Task refreshFolder()
        {
            string url = $"{endpoint.API}/api/v1/Folder/GetItems/{RoleId}/outbound?view=0";
            var result = await httpService.Get<IEnumerable<FolderModel>>(url, new AuthorizeHeader("bearer", token));
            if (result.Success)
            {
                folderModels.Clear();
                // Remove Expire folder
                foreach (var folder in result.Response!)
                {
                    var expiredDate = DateTime.ParseExact(folder.ExpiredDate!, "dd/MM/yyyy", null);
                    if (DateTime.Now <= expiredDate)
                    {
                        folderModels!.Add(folder);
                    }
                }
                await Notice.InvokeAsync("ດຶງຂໍ້ມູນແຟ້ມເອກະສານມາໃຫມ່ສຳເລັດ");
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
