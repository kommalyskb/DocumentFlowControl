using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using System.Security;
using System.Text.Json;
using System.Threading.Channels;

namespace DFM.Frontend.Pages.FolderComponent
{
    public partial class FolderForm
    {
        private EmployeeModel? employee;
        IEnumerable<DataTypeModel>? docTypeModels;
        string? token = "";
        IEnumerable<string>? formatDefaults;
        IEnumerable<RoleTreeModel>? supervisors;
        string? docType  = "ກະລຸນາເລືອກ ຢ່າງຫນ້ອຍ 1 ຢ່າງ";
        string? supervisorText = "ກະລຸນາເລືອກ ຢ່າງຫນ້ອຍ 1 ຢ່າງ";
        IEnumerable<string>? docTypeOptions;
        IEnumerable<string>? supervisorOption;
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
            employee = await storageHelper.GetEmployeeProfileAsync();
            token = await accessToken.GetTokenAsync();
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

                if (!string.IsNullOrWhiteSpace(FolderModel!.FormatType))
                {
                    string displayFormat = FolderModel!.FormatType;
                    displayFormat = displayFormat!.Replace("$docno", $"ເລກທີ");
                    displayFormat = displayFormat!.Replace("$sn", $"ຕົວຫຍໍ້");
                    displayFormat = displayFormat!.Replace("$yyyy", $"ປີ");
                    formatDefaults = new List<string> { displayFormat };
                }

                
                docTypeOptions = new HashSet<string>();
                foreach (var item in FolderModel!.SupportDocTypes)
                {
                    docTypeOptions = docTypeOptions.Concat(new HashSet<string>() { item });
                }
                supervisorOption = new HashSet<string>();
                foreach (var item in FolderModel!.Supervisors)
                {
                    supervisorOption = supervisorOption.Concat(new HashSet<string>() { item });
                }
                await InvokeAsync(StateHasChanged);
                Console.WriteLine($"{DateTime.Now} Load Folder form ");
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
        private string getSelectionDocType(List<string> selectedValues)
        {
            string? selectText = "";
            if (selectedValues.Count > 0)
            {
                FolderModel!.SupportDocTypes = selectedValues;

            }
            foreach (var val in selectedValues)
            {
                var item = docTypeModels!.FirstOrDefault(x => x.id == val);
                selectText += $"{item!.DocType}, ";
            }
            return selectText;

        }
        private string getSelectionSupervisor(List<string> selectedValues)
        {
            string? selectText = "";
            if (selectedValues.Count > 0)
            {
                FolderModel!.Supervisors = selectedValues;

            }
            foreach (var val in selectedValues)
            {
                var item = supervisors!.FirstOrDefault(x => x.Role.RoleID == val);
                selectText += $"{item!.Role.Display.Local} - {item!.Employee.Name.Local}, ";
            }
            return selectText;

        }
    }
}
