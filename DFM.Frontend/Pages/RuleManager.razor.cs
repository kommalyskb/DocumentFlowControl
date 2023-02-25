using DFM.Shared.Common;
using DFM.Shared.Entities;
using DFM.Shared.Helper;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages
{
    public partial class RuleManager
    {
        readonly int delayTime = 500;
        private EmployeeModel? employee;
        string? token;
        protected override async Task OnInitializedAsync()
        {
            var rules = await storageHelper.GetRuleMenuAsync();
            if (!ValidateRule.isInRole(rules, "/pages/rulemanager"))
            {
                nav.NavigateTo("/pages/unauthorized");
            }

            formMode = FormMode.List;
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
            bool? isDelete = await delBox!.Show();
            if (isDelete.HasValue)
            {
                if (isDelete.Value)
                {
                    onProcessing = true;
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        token = await accessToken.GetTokenAsync();
                    }

                    string url = $"{endpoint.API}/api/v1/RuleMenu/RemoveItem/{ruleMenu!.id}";
                    var result = await httpService.Get<CommonResponseId>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                    if (result.Success)
                    {
                        AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                    }
                    else
                    {
                        AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
                    }


                    onProcessing = false;

                    await Task.Delay(delayTime);

                    disposedObj();
                    formMode = FormMode.List;
                }
            }
        }

        void onEditButtonClick()
        {
            formMode = FormMode.Edit;
        }
        void onRowClick(RuleMenu item)
        {
            // Row click
            formMode = FormMode.View;
            ruleMenu = item;

        }

        async Task onSaveClickAsync()
        {
            onProcessing = true;
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await accessToken.GetTokenAsync();
            }
            
            await InvokeAsync(StateHasChanged);

            httpService.MediaType = MediaType.JSON;
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            ruleMenu.OrgID = employee.OrganizationID;
            string url = $"{endpoint.API}/api/v1/RuleMenu/UpdateRule";
            var result = await httpService.Post<RuleMenu, CommonResponseId>(url, ruleMenu, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

            if (result.Success)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
            }
            else
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
            }

            onProcessing = false;

            await Task.Delay(delayTime);

            disposedObj();
            formMode = FormMode.List;
        }
        void disposedObj()
        {
            ruleMenu = new();
        }

        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
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
