using DFM.Shared.Entities;
using HttpClientService;
using System.Text.Json;
using System.Threading.Channels;

namespace DFM.Frontend.Pages.FolderComponent
{
    public partial class FolderView
    {
        private EmployeeModel? employee;
        IEnumerable<DataTypeModel>? docTypeModels;
        IEnumerable<RoleTreeModel>? supervisors;
        string? token = "";
        string? displayFormat = "";
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

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine($"{DateTime.Now} Load folder");
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            token = await accessToken.GetTokenAsync();

            // Use chanel to control concurrency
            var channel = Channel.CreateUnbounded<(bool success, int component, string response)>();
            // Consumer
            var consumer = bindDataToVariables(channel.Reader);
            // Producer
            var produce = loadDataTasks(channel.Writer);

            // Await for result
            await consumer;
            await produce;

            // Setup format type for human readable
            displayFormat = FolderModel!.FormatType;
            displayFormat = displayFormat!.Replace("$docno", $"ເລກທີ");
            displayFormat = displayFormat!.Replace("$sn", $"ຕົວຫຍໍ້");
            displayFormat = displayFormat!.Replace("$yyyy", $"ປີ");
        }

        private async Task bindDataToVariables(ChannelReader<(bool success, int component, string response)> reader)
        {
            await foreach (var item in reader.ReadAllAsync())
            {
                if (item.success)
                {
                    if (item.component == 1)
                    {
                        //urgentModels = JsonSerializer.Deserialize<IEnumerable<DocumentUrgentModel>>(item.response);
                    }
                    if (item.component == 2)
                    {
                        //securityModels = JsonSerializer.Deserialize<IEnumerable<DocumentSecurityModel>>(item.response);
                    }
                    if (item.component == 3)
                    {
                        docTypeModels = JsonSerializer.Deserialize<IEnumerable<DataTypeModel>>(item.response);
                    }
                    if (item.component == 4)
                    {
                        //folderModels = JsonSerializer.Deserialize<IEnumerable<FolderModel>>(item.response);
                    }
                    if (item.component == 5)
                    {
                        supervisors = JsonSerializer.Deserialize<IEnumerable<RoleTreeModel>>(item.response);
                    }
                }
            }
        }

        private async Task loadDataTasks(ChannelWriter<(bool success, int component, string response)> writer)
        {
            List<Task> tasks = new List<Task>();
            //tasks.Add(fetchAPI($"{endpoint.API}/api/v1/UrgentLevel/GetItems/{employee!.OrganizationID}", 1, writer));
            //tasks.Add(fetchAPI($"{endpoint.API}/api/v1/SecurityLevel/GetItems/{employee!.OrganizationID}", 2, writer));
            tasks.Add(fetchAPI($"{endpoint.API}/api/v1/DocumentType/GetItems/{employee!.OrganizationID}", 3, writer));
            //tasks.Add(fetchAPI($"{endpoint.API}/api/v1/Folder/GetItems/{RoleId}", 4, writer));
            tasks.Add(fetchAPI($"{endpoint.API}/api/v1/Organization/GetSupervisors/{employee!.OrganizationID}", 5, writer));

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
                // Load Folder
                var result = await httpService.Get<IEnumerable<RoleTreeModel>>(url, new AuthorizeHeader("bearer", token));
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
