using DFM.Shared.Common;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages.OrganizeComponent
{
    public partial class ListViewComponent
    {
        private TreeItemData? activatedValue { get { return _activatedValue; } set { _activatedValue = value; OnItemChangeEvent.InvokeAsync(value); } }

        private HashSet<TreeItemData>? selectedValues { get; set; }

        private HashSet<TreeItemData>? treeItems { get; set; } = new HashSet<TreeItemData>();
        TreeItemData? _activatedValue;
        string? token = "";
        private EmployeeModel? employee;
        protected override async Task OnInitializedAsync()
        {
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await accessToken.GetTokenAsync();
            }

            string url = $"{endpoint.API}/api/v1/Organization/GetItem/{employee.OrganizationID!}";
            var result = await httpService.Get<IEnumerable<RoleTreeModel>>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
            if (result.Success)
            {
                // Construct tree view
                var childs = result.Response.Where(x => x.ParentID == "0" || x.ParentID == null);
                constructTreeItem(result.Response, childs, treeItems!);
                
            }
        }
        private HashSet<TreeItemData> constructTreeItem(IEnumerable<RoleTreeModel> organizations, IEnumerable<RoleTreeModel> childModels, HashSet<TreeItemData> parentSets)
        {
            if (organizations.Count() > 0)
            {
                if (childModels.Count() > 0)
                {
                    foreach (var childItem in childModels)
                    {
                        var existingItem = new TreeItemData(childItem, true, employee!.OrganizationID);

                        var childs = organizations.Where(x => x.ParentID == childItem.Role.RoleID);

                        var parentItem = new HashSet<TreeItemData>();

                        var result = constructTreeItem(organizations, childs, parentItem);

                        foreach (var treeContent in result)
                        {

                            existingItem.TreeItems!.Add(treeContent);
                        }

                        parentSets.Add(existingItem);
                    }
                    
                }

            }
            return parentSets;
        }
    }
}
