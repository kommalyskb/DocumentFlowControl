using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;

namespace DFM.Frontend.Pages.Menu
{
    public partial class MenuForm
    {
        IEnumerable<string>? authorizeOption;
        string? authorizeView = "ກະລຸນາເລືອກ ຢ່າງຫນ້ອຍ 1 ຢ່າງ";
        IDictionary<string, string>? authorizeTemplates;
        IDictionary<string, RoleTypeModel>? authorizeTemplatesValues;
        List<RoleTypeModel>? roleTypes;
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
        protected override void OnInitialized()
        {
            roleTypes = new List<RoleTypeModel>();
            authorizeTemplates = new Dictionary<string, string>();
            authorizeTemplatesValues = new Dictionary<string, RoleTypeModel>();
            //-------------
            authorizeTemplates.Add(RoleTypeModel.Prime.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.Prime));
            authorizeTemplates.Add(RoleTypeModel.DeputyPrime.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.DeputyPrime));
            authorizeTemplates.Add(RoleTypeModel.PrimeSecretary.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.PrimeSecretary));
            authorizeTemplates.Add(RoleTypeModel.DeputyPrimeSecretary.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.DeputyPrimeSecretary));
            authorizeTemplates.Add(RoleTypeModel.Director.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.Director));
            authorizeTemplates.Add(RoleTypeModel.DeputyDirector.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.DeputyDirector));
            authorizeTemplates.Add(RoleTypeModel.InboundPrime.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.InboundPrime));
            authorizeTemplates.Add(RoleTypeModel.OutboundPrime.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.OutboundPrime));
            authorizeTemplates.Add(RoleTypeModel.OfficePrime.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.OfficePrime));
            authorizeTemplates.Add(RoleTypeModel.DeputyOfficePrime.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.DeputyOfficePrime));
            authorizeTemplates.Add(RoleTypeModel.InboundOfficePrime.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.InboundOfficePrime));
            authorizeTemplates.Add(RoleTypeModel.OutboundOfficePrime.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.OutboundOfficePrime));
            authorizeTemplates.Add(RoleTypeModel.General.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.General));
            authorizeTemplates.Add(RoleTypeModel.DeputyGeneral.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.DeputyGeneral));
            authorizeTemplates.Add(RoleTypeModel.InboundGeneral.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.InboundGeneral));
            authorizeTemplates.Add(RoleTypeModel.OutboundGeneral.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.OutboundGeneral));
            authorizeTemplates.Add(RoleTypeModel.OfficeGeneral.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.OfficeGeneral));
            authorizeTemplates.Add(RoleTypeModel.DeputyOfficeGeneral.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.DeputyOfficeGeneral));
            authorizeTemplates.Add(RoleTypeModel.Division.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.Division));
            authorizeTemplates.Add(RoleTypeModel.DeputyDivision.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.DeputyDivision));
            authorizeTemplates.Add(RoleTypeModel.Department.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.Department));
            authorizeTemplates.Add(RoleTypeModel.DeputyDepartment.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.DeputyDepartment));
            authorizeTemplates.Add(RoleTypeModel.Employee.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.Employee));
            authorizeTemplates.Add(RoleTypeModel.Contract.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.Contract));
            authorizeTemplates.Add(RoleTypeModel.Volunteer.ToString(), DecodeEnumRoleTypeModel.decodeRoleType(RoleTypeModel.Volunteer));
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
            if (!RuleMenu!.RoleTypes!.IsNullOrEmpty())
            {
                authorizeOption = new List<string>();
                List<string> existAuthorize = new();
                foreach (var item in RuleMenu!.RoleTypes!)
                {
                    existAuthorize.Add(item.ToString());
                }
                authorizeOption = authorizeOption!.Concat(existAuthorize);
            }

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
                RuleMenu!.RoleTypes = roleTypes;

            }
            return selectText;

        }
    }
}
