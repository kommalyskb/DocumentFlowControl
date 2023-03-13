using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Text.Json;

namespace DFM.Frontend.Pages
{
    public partial class Setup
    {
        string? token;
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
            try
            {
                #region Validate Token
                var getTokenState = await tokenState.ValidateToken();
                if (!getTokenState)
                    nav.NavigateTo("/authorize");
                #endregion
                onProcessing = true;
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }

                if (string.IsNullOrWhiteSpace(model.Name.Local) || string.IsNullOrWhiteSpace(model.Name.Eng))
                {
                    AlertMessage("ກະລຸນາ ປ້ອນຊື່ອົງກອນຂອງທ່ານ", Defaults.Classes.Position.BottomRight, Severity.Error);
                    onProcessing = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(employee.Name.Local) || string.IsNullOrWhiteSpace(employee.Name.Eng) ||
                    string.IsNullOrWhiteSpace(employee.FamilyName.Local) || string.IsNullOrWhiteSpace(employee.FamilyName.Eng))
                {
                    AlertMessage("ກະລຸນາ ປ້ອນຊື່ ພະນັກງານ ທີ່ຈະເປັນ ຜູ້ດູແລລະບົບ", Defaults.Classes.Position.BottomRight, Severity.Error);
                    onProcessing = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(employee.Username) || string.IsNullOrWhiteSpace(employee.Password) ||
                    string.IsNullOrWhiteSpace(employee.Contact.Email) || string.IsNullOrWhiteSpace(employee.Contact.Phone))
                {
                    AlertMessage("ກະລຸນາ ກວດເບີ່ງວ່າຂໍ້ມູນ Username, Password, Email, Phone ປ້ອນແລ້ວບໍ່", Defaults.Classes.Position.BottomRight, Severity.Error);
                    onProcessing = false;
                    return;
                }
                string url = $"{endpoint.API}/api/v1/Organization/NewItem";
                NewOrganizationRequest req = new NewOrganizationRequest()
                {
                    Name = model.Name
                };
                var orgResult = await httpService.Post<NewOrganizationRequest, CommonResponseId>(url, req, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
                if (orgResult.Success)
                {
                    Console.WriteLine(JsonSerializer.Serialize(orgResult.Response));
                    string isNotify = "no";
                    if (notify)
                    {
                        isNotify = "yes";
                    }
                    string urlEmployee = $"{endpoint.API}/api/v1/Employee/SaveItem?notify={isNotify}";
                    employee.OrganizationID = orgResult.Response.Id;
                    var empResult = await httpService.Post<EmployeeModel, CommonResponse>(urlEmployee, employee, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

                    Log.Information(await empResult.HttpResponseMessage.Content.ReadAsStringAsync());

                    if (empResult.Success)
                    {
                        nav.NavigateTo($"/authorize", true);
                        AlertMessage("ທຸລະກຳຂອງທ່ານສຳເລັດ, ກະລຸນາກວດສອບກ່ອງ ອີເມວຂາເຂົ້າຂອງທ່ານ", Defaults.Classes.Position.BottomRight, Severity.Success);
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
            catch (Exception)
            {

                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }
           
        }

        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
        }
    }
}
