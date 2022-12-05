﻿using DFM.Shared.Common;
using DFM.Shared.Entities;

namespace DFM.Frontend.Pages
{
    public partial class FolderControl
    {
        string? oldLink = "";
        protected override void OnInitialized()
        {
            formMode = FormMode.List;
            oldLink = Link!;

            base.OnInitialized();

        }
        void onCreateButtonClick()
        {
            formMode = FormMode.Create;
        }
        void onePreviousButtonClick()
        {
            formMode = FormMode.List;
        }
        
        async Task onDeleteButtonClick()
        {
            bool? result = await delBox!.Show();
        }
        
        void onEditButtonClick()
        {
            formMode = FormMode.Edit;
        }
        void onRowClick(FolderModel item)
        {
            // Row click
            formMode = FormMode.View;

        }
        
        async Task onSaveClickAsync()
        {
            formMode = FormMode.List;
        }
        async Task onDeleteClickAsync()
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
        void onTabChangeEvent(string roleId)
        {
            this.roleId = roleId;
        }
        protected override void OnParametersSet()
        {
            if (oldLink != Link)
            {
                formMode = FormMode.List;
                oldLink = Link!;
            }
            base.OnParametersSet();
        }
    }
}
