using DFM.Shared.Common;
using DFM.Shared.Entities;
using MudBlazor;

namespace DFM.Frontend.Pages
{
    public partial class Organization
    {
        string? oldLink = "";
        string? editBreadcrumbText = "";
        protected override void OnInitialized()
        {
            formMode = FormMode.List;
            oldLink = Link!;


            base.OnInitialized();

        }
        void onePreviousButtonClick()
        {
            formMode = FormMode.List;
        }

        async Task onSaveClickAsync()
        {
            formMode = FormMode.List;
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
        
        void onEditButtonClick()
        {
            formMode = FormMode.Edit;
        }
        void onCreateButtonClick()
        {
            formMode = FormMode.Create;
        }

        void onItemChangeEvent(TreeItemData item)
        {
            formMode = FormMode.View;
            editBreadcrumbText = item.Content!.Role.Display.Local;
            roleTreeModel = item.Content;
            orgId = item.OrganizationID;
        }


    }
    
}
