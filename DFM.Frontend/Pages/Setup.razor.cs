using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages
{
    public partial class Setup
    {

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

        private IEnumerable<string> UserCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 12 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 12 ອັກສອນ";
        }
        private IEnumerable<string> PasswordCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 6 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 12 ອັກສອນ";
        }

        async Task startSetup()
        {
            onProcessing = true;
            string token = await accessToken.GetTokenAsync();
            string url = $"{endpoint.API}/api/v1/Organization/NewItem";
            NewOrganizationRequest req = new NewOrganizationRequest()
            {
                Name = model.Name
            };
            var orgResult = await httpService.Post<NewOrganizationRequest, CommonResponseId>(url, req, new AuthorizeHeader("bearer", token));
            if (orgResult.Success)
            {
                string urlEmployee = $"{endpoint.API}/api/v1/Employee/SaveItem?notify=yes";
                var empResult = await httpService.Post<EmployeeModel, CommonResponse>(urlEmployee, employee, new AuthorizeHeader("bearer", token));
                if (empResult.Success)
                {
                    AlertMessage("ທຸລະກຳຂອງທ່ານສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                }
                else
                {
                    AlertMessage("ບໍ່ສາມາດສ້າງພະນັກງານໃຫມ່ໄດ້", Defaults.Classes.Position.BottomRight, Severity.Error);
                }
            }
            else
            {
                AlertMessage("ບໍ່ສາມາດສ້າງບໍລິສັດໃຫມ່ໄດ້", Defaults.Classes.Position.BottomRight, Severity.Error);
            }
            onProcessing = false;
        }

        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
        }
    }
}
