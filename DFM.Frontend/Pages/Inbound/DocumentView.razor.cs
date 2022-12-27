using DFM.Frontend.Shared;
using DFM.Shared.Entities;
using HttpClientService;
using Microsoft.JSInterop;
using MudBlazor;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using System.Threading.Channels;

namespace DFM.Frontend.Pages.Inbound
{
    public partial class DocumentView
    {
       
        protected override async Task OnInitializedAsync()
        {
            token = await accessToken.GetTokenAsync();
            employee = await storageHelper.GetEmployeeProfileAsync();
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

                // From row click
                //var myDoc = DocumentModel!.Reciepients!.LastOrDefault(x => x.ReciepientInfo.RoleID == RoleId);
                //RawDocument = DocumentModel!.RawDatas!.LastOrDefault(x => x.DataID == myDoc!.DataID);


                // Set organization if new document
                //if (string.IsNullOrWhiteSpace(DocumentModel.OrganizationID))
                //{
                //    DocumentModel.OrganizationID = employee.OrganizationID;
                //}

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
                        folderModels = JsonSerializer.Deserialize<IEnumerable<FolderModel>>(item.response);
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
            tasks.Add(fetchAPI($"{endpoint.API}/api/v1/Folder/GetItems/{RoleId}/inbound?view=1", 4, writer));
            tasks.Add(fetchAPI($"{endpoint.API}/api/v1/Document/GetDocument/{DocumentModel!.id}", 5, writer));

            await Task.WhenAll(tasks);
            writer.Complete();
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
                    var jsonDoc = JsonSerializer.Serialize(result.Response);
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
    }
}
