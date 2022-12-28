using DFM.Shared.Entities;
using DFM.Shared.Extensions;

namespace DFM.Frontend.Pages.SecurityLevelComponent
{
    public partial class SecurityForm
    {
        private EmployeeModel? employee;
        string? token = "";
        IEnumerable<string>? authorizeOption;
        string? authorizeView = "ກະລຸນາເລືອກ ຢ່າງຫນ້ອຍ 1 ຢ່າງ";
        IDictionary<string, string>? authorizeTemplates;
        IDictionary<string, RoleTypeModel>? authorizeTemplatesValues;
        List<RoleTypeModel>? roleTypes;
        protected override void OnInitialized()
        {
            roleTypes = new List<RoleTypeModel>();
            authorizeTemplates = new Dictionary<string, string>();
            authorizeTemplatesValues = new Dictionary<string, RoleTypeModel>();
            //-------------
            authorizeTemplates.Add(RoleTypeModel.Prime.ToString(), "ປະທານບໍລິສັດ");
            authorizeTemplates.Add(RoleTypeModel.DeputyPrime.ToString(), "ຮອງປະທານບໍລິສັດ");
            authorizeTemplates.Add(RoleTypeModel.PrimeSecretary.ToString(), "ເລຂາປະທານບໍລິສັດ");
            authorizeTemplates.Add(RoleTypeModel.DeputyPrimeSecretary.ToString(), "ເລຂາຮອງປະທານບໍລິສັດ");
            authorizeTemplates.Add(RoleTypeModel.Director.ToString(), "ຜູ້ອຳນວຍການ");
            authorizeTemplates.Add(RoleTypeModel.DeputyDirector.ToString(), "ຮອງຜູ້ອຳນວຍການ");
            authorizeTemplates.Add(RoleTypeModel.InboundPrime.ToString(), "ຂາເຂົ້າບໍລິສັດ");
            authorizeTemplates.Add(RoleTypeModel.OutboundPrime.ToString(), "ຂາອອກບໍລິສັດ");
            authorizeTemplates.Add(RoleTypeModel.OfficePrime.ToString(), "ຫົວຫນ້າຫ້ອງການ");
            authorizeTemplates.Add(RoleTypeModel.DeputyOfficePrime.ToString(), "ຮອງຫົວຫນ້າຫ້ອງການ");
            authorizeTemplates.Add(RoleTypeModel.InboundOfficePrime.ToString(), "ຂາເຂົ້າຫ້ອງການ");
            authorizeTemplates.Add(RoleTypeModel.OutboundOfficePrime.ToString(), "ຂາອອກຫ້ອງ");
            authorizeTemplates.Add(RoleTypeModel.General.ToString(), "ຫົວຫນ້າຝ່າຍ");
            authorizeTemplates.Add(RoleTypeModel.DeputyGeneral.ToString(), "ຮອງຫົວຫນ້າຝ່າຍ");
            authorizeTemplates.Add(RoleTypeModel.InboundGeneral.ToString(), "ຂາເຂົ້າຝ່າຍ");
            authorizeTemplates.Add(RoleTypeModel.OutboundGeneral.ToString(), "ຂາອອກຝ່າຍ");
            authorizeTemplates.Add(RoleTypeModel.OfficeGeneral.ToString(), "ຫົວຫນ້າຫ້ອງການຝ່າຍ");
            authorizeTemplates.Add(RoleTypeModel.DeputyOfficeGeneral.ToString(), "ຮອງຫົວຫນ້າຫ້ອງການຝ່າຍ");
            authorizeTemplates.Add(RoleTypeModel.Division.ToString(), "ຫົວຫນ້າພະແນກ");
            authorizeTemplates.Add(RoleTypeModel.DeputyDivision.ToString(), "ຮອງພະແນກ");
            authorizeTemplates.Add(RoleTypeModel.Department.ToString(), "ຫົວຫນ້າພາກສ່ວນ");
            authorizeTemplates.Add(RoleTypeModel.DeputyDepartment.ToString(), "ຮອງພາກສ່ວນ");
            authorizeTemplates.Add(RoleTypeModel.Employee.ToString(), "ພະນັກງານວິຊາການ");
            authorizeTemplates.Add(RoleTypeModel.Contract.ToString(), "ສັນຍາຈ້າງ");
            authorizeTemplates.Add(RoleTypeModel.Volunteer.ToString(), "ອາສາສະຫມັກ");
            //----------
            authorizeTemplatesValues.Add(RoleTypeModel.Prime.ToString(), RoleTypeModel.Prime);
            authorizeTemplatesValues.Add(RoleTypeModel.DeputyPrime.ToString(), RoleTypeModel.DeputyPrime);
            authorizeTemplatesValues.Add(RoleTypeModel.PrimeSecretary.ToString(), RoleTypeModel.PrimeSecretary);
            authorizeTemplatesValues.Add(RoleTypeModel.DeputyPrimeSecretary.ToString(), RoleTypeModel.DeputyPrimeSecretary);
            authorizeTemplatesValues.Add(RoleTypeModel.Director.ToString(), RoleTypeModel.Director);
            authorizeTemplatesValues.Add(RoleTypeModel.DeputyDirector.ToString(), RoleTypeModel.DeputyDirector);
            authorizeTemplatesValues.Add(RoleTypeModel.InboundPrime.ToString(), RoleTypeModel.InboundPrime);
            authorizeTemplatesValues.Add(RoleTypeModel.OutboundPrime.ToString(), RoleTypeModel.OutboundPrime);
            authorizeTemplatesValues.Add(RoleTypeModel.OfficePrime.ToString(), RoleTypeModel.OfficePrime);
            authorizeTemplatesValues.Add(RoleTypeModel.DeputyOfficePrime.ToString(), RoleTypeModel.DeputyOfficePrime);
            authorizeTemplatesValues.Add(RoleTypeModel.InboundOfficePrime.ToString(), RoleTypeModel.InboundOfficePrime);
            authorizeTemplatesValues.Add(RoleTypeModel.OutboundOfficePrime.ToString(), RoleTypeModel.OutboundOfficePrime);
            authorizeTemplatesValues.Add(RoleTypeModel.General.ToString(), RoleTypeModel.General);
            authorizeTemplatesValues.Add(RoleTypeModel.DeputyGeneral.ToString(), RoleTypeModel.DeputyGeneral);
            authorizeTemplatesValues.Add(RoleTypeModel.InboundGeneral.ToString(), RoleTypeModel.InboundGeneral);
            authorizeTemplatesValues.Add(RoleTypeModel.OutboundGeneral.ToString(), RoleTypeModel.OutboundGeneral);
            authorizeTemplatesValues.Add(RoleTypeModel.OfficeGeneral.ToString(), RoleTypeModel.OfficeGeneral);
            authorizeTemplatesValues.Add(RoleTypeModel.DeputyOfficeGeneral.ToString(), RoleTypeModel.DeputyOfficeGeneral);
            authorizeTemplatesValues.Add(RoleTypeModel.Division.ToString(), RoleTypeModel.Division);
            authorizeTemplatesValues.Add(RoleTypeModel.DeputyDivision.ToString(), RoleTypeModel.DeputyDivision);
            authorizeTemplatesValues.Add(RoleTypeModel.Department.ToString(), RoleTypeModel.Department);
            authorizeTemplatesValues.Add(RoleTypeModel.DeputyDepartment.ToString(), RoleTypeModel.DeputyDepartment);
            authorizeTemplatesValues.Add(RoleTypeModel.Employee.ToString(), RoleTypeModel.Employee);
            authorizeTemplatesValues.Add(RoleTypeModel.Contract.ToString(), RoleTypeModel.Contract);
            authorizeTemplatesValues.Add(RoleTypeModel.Volunteer.ToString(), RoleTypeModel.Volunteer);

           
            // Binding Authorize
            if (!DocumentSecurityModel!.Authorized!.IsNullOrEmpty())
            {
                authorizeOption = new List<string>();
                List<string> existAuthorize = new();
                foreach (var item in DocumentSecurityModel!.Authorized!)
                {
                    existAuthorize.Add(item.ToString());
                }
                authorizeOption = authorizeOption!.Concat(existAuthorize);
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
        private string getSelectionAuthorize(List<string> selectedValues)
        {
            string? selectText = "";
            roleTypes!.Clear();
            foreach (var val in selectedValues)
            {
                selectText += $"{authorizeTemplates![val]}, ";
                roleTypes.Add(authorizeTemplatesValues![val]);
            }
            if (selectedValues.Count > 0)
            {
                DocumentSecurityModel!.Authorized = roleTypes;

            }
            return selectText;

        }
    }
}
