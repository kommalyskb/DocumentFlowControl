using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;
using Newtonsoft.Json.Linq;

namespace DFM.Frontend.Pages
{
    public partial class Monitor
    {
        private EmployeeModel? employee;
        string? token;
        string? current;
        string? previousBtn = "";
        string? oldLink = "";
        ReportDrillDownEnum isDrillDown = ReportDrillDownEnum.Search;
        protected override async Task OnInitializedAsync()
        {
            oldLink = Link!;
            if (Link == "inbound")
            {
                current = "ເອກະສານຂາເຂົ້າ";
            }
            else
            {
                current = "ເອກະສານຂາອອກ";
            }
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            await InvokeAsync(StateHasChanged);

        }

        protected override void OnParametersSet()
        {
            if (oldLink != Link)
            {
                isDrillDown = ReportDrillDownEnum.Search;
                oldLink = Link!;
                //reportSummary = new();
                //searchRequest = new();
                //documentModel = new();
                //rawDocument = new();
                if (Link == "inbound")
                {
                    current = "ລາຍງານເອກະສານຂາເຂົ້າ";
                }
                else
                {
                    current = "ລາຍງານເອກະສານຂາອອກ";
                }
            }
            base.OnParametersSet();
        }
    }
}
