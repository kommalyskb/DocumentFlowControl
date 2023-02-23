using DFM.Shared.Entities;
using MudBlazor;

namespace DFM.Frontend.Shared
{
    public partial class EmployeeSelector
    {
        private string searchString = "";

        //private EmployeeModel? employee;
        //string? token = "";
        private bool FilterFunc(EmployeeModel element)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            if (element.Name.Eng!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.Name.Local!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.FamilyName.Eng!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.FamilyName.Local!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
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
