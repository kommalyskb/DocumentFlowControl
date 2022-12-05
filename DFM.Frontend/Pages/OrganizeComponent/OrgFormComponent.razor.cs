using DFM.Shared.Entities;
using DFM.Shared.Common;

namespace DFM.Frontend.Pages.OrganizeComponent
{
    public partial class OrgFormComponent
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

        protected override Task OnInitializedAsync()
        {
            if (FormMode == FormMode.Create)
            {
                disabled = false;
            }
            else if (FormMode == FormMode.Edit)
            {
                disabled = true;
            }
            return base.OnInitializedAsync();
        }
    }
}
