using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;
using System.ComponentModel;
using System.Text.Json;

namespace DFM.Frontend.Shared
{
    public partial class TabDataGrid
    {
        private List<string> _events = new();
        string? token = "";
        private List<DocumentDto> Elements = new();
        private ResponseQueryDocument? responseQueryDocument;
        //private int unread = 0;
        private EmployeeModel? employee;
        IEnumerable<DocumentUrgentModel>? urgentModels;
        TraceStatus oldStatus;
        private IEnumerable<DataTypeModel>? docTypeModel;

        // events
        async Task RowClicked(DataGridRowClickEventArgs<DocumentDto> args)
        {
            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {JsonSerializer.Serialize(args.Item)}");
            //Console.WriteLine(JsonSerializer.Serialize(args.Item));

            // Open new page
            if (args.Item is not null)
            {
                if (responseQueryDocument != null)
                {
                    if (responseQueryDocument.Contents!.Count() > 0)
                    {
                        var model = responseQueryDocument!.Contents!.FirstOrDefault(x => x.id == args.Item.Id);
                        if (model != null)
                        {
                            await OnRowClick.InvokeAsync(model);

                        }
                    }
                }
                
            }
            
        }

        void SelectedItemsChanged(HashSet<DocumentDto> items)
        {
            //_events.Insert(0, $"Event = SelectedItemsChanged, Data = {JsonSerializer.Serialize(items)}");
            //Console.WriteLine(JsonSerializer.Serialize(items));
        }
        private Func<DocumentDto, string> _cellStyleFunc => x =>
        {
            string style = $"";

            if (!x.IsRead)
                style = "font-weight:bold; background-color:#cce6f7; color:#002f57";
            

            return style;
        };
        private Func<DocumentDto, string> _cellWithFontStyleFunc => x =>
        {
            string style = $"{x.FontColor};";

            if (!x.IsRead)
                style = "font-weight:bold; background-color:#cce6f7; color:#002f57";


            return style;
        };
        private Func<DocumentDto, string> _headerStyleFunc => x =>
        {
            return "font-weight:bold";
        };
        protected override async Task OnInitializedAsync()
        {
                await loadContent();

        }

        protected override async Task OnParametersSetAsync()
        {
            if (oldStatus != TraceStatus)
            {
                await loadContent();
            }
        }

        private async Task loadContent()
        {
            try
            {
                Elements.Clear();

                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }
                if (employee == null)
                {
                    employee = await storageHelper.GetEmployeeProfileAsync();
                }
                oldStatus = TraceStatus;
                // Load Urgent Level
                string urgentUrl = $"{endpoint.API}/api/v1/UrgentLevel/GetItems/{employee!.OrganizationID}";
                var urgentLevel = await httpService.Get<IEnumerable<DocumentUrgentModel>>(urgentUrl, new AuthorizeHeader("bearer", token));
                if (urgentLevel.Success)
                {
                    urgentModels = urgentLevel.Response;
                }
                // Load Document Type
                string docTypeUrl = $"{endpoint.API}/api/v1/DocumentType/GetItems/{employee!.OrganizationID}";
                var docType = await httpService.Get<IEnumerable<DataTypeModel>>(docTypeUrl, new AuthorizeHeader("bearer", token));
                if (docType.Success)
                {
                    docTypeModel = docType.Response;
                }

                // Load document
                string url = $"{endpoint.API}/api/v1/Document/GetDocument/{TraceStatus}/{Link}/{RoleId}";

                var result = await httpService.Get<ResponseQueryDocument>(url, new AuthorizeHeader("bearer", token));
                if (result.Success)
                {
                    Elements.Clear();
                    responseQueryDocument = result.Response;
                    foreach (var item in result.Response.Contents!)
                    {
                        var myDoc = item.Recipients!.LastOrDefault(x => x.RecipientInfo.RoleID == RoleId);
                        var rawDocumentData = item.RawDatas!.LastOrDefault(x => x.DataID == myDoc!.DataID);
                        var urgentLabel = urgentModels!.FirstOrDefault(x => x.id == rawDocumentData!.Urgent.id)!;
                        var docTypeLabel = docTypeModel!.FirstOrDefault(x => x.id == rawDocumentData!.DocType)!;
                        
                        Elements.Add(new DocumentDto
                        {
                            Id = item.id,
                            DocDate = myDoc!.ReceiveDate,
                            DocNo = rawDocumentData!.DocNo,
                            FormType = docTypeLabel != null ? docTypeLabel!.DocType : "",
                            Title = rawDocumentData!.Title,
                            UrgentLevel = urgentLabel != null ? urgentLabel!.Level! : "",
                            IsRead = myDoc!.IsRead,
                            Uid = myDoc!.UId,
                            CreateDate = myDoc!.CreateDate,
                            CompletedDate = myDoc!.CompletedDate,
                            FontColor = urgentLabel != null ? urgentLabel.FontColor! : "",

                        });
                    }
                    //unread = Elements.Count(x => !x.IsRead);

                    // Order Element
                    if (TraceStatus == TraceStatus.Completed || TraceStatus == TraceStatus.Terminated)
                    {
                        Elements = Elements.OrderByDescending(x => x.CompletedDate).ToList();
                    }
                    else
                    {
                        Elements = Elements.OrderByDescending(x => x.CreateDate).ToList();
                    }
                   
                }
            }
            catch (Exception)
            {

            }
            
        }
    }
}
