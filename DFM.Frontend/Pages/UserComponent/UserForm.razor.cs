using DFM.Shared.Entities;

namespace DFM.Frontend.Pages.UserComponent
{
    public partial class UserForm
    {
        private EmployeeModel? employee;
        string? token = "";

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
    }
}
