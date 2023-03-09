using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Helper;
using HttpClientService;
using System.Linq;

namespace DFM.Frontend.Pages
{
    public partial class Index
    {
        private EmployeeModel? employee;
        private IEnumerable<TabItemDto>? roles;
        IEnumerable<string>? selectValues;
        protected override async Task OnInitializedAsync()
        {
            var rules = await storageHelper.GetRuleMenuAsync();
            if (!ValidateRule.isInRole(rules, $"/pages/home"))
            {
                nav.NavigateTo("/pages/unauthorized");
            }
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }

            if (roles == null)
            {
                roles = await storageHelper.GetRolesAsync();

            }
            findInboundOutboundSameParent();
            if (roles != null)
            {
                roleID = roles!.FirstOrDefault()!.Role!.RoleID!;
                selectValues = new List<string>() { roleID };
            }
            //var split = roleID.Split('|');

            await InvokeAsync(StateHasChanged);

        }

        private bool isMultiRole()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleID))
                {
                    return false;
                }
                else
                {

                    var splitRole = roleID!.Split('|');
                    if (splitRole.Length > 1)
                    {
                        foreach (var item in splitRole)
                        {
                            var itemSplit = item.Split(':');
                            if (itemSplit[1] == nameof(RoleTypeModel.InboundGeneral) || itemSplit[1] == nameof(RoleTypeModel.InboundOfficePrime) || itemSplit[1] == nameof(RoleTypeModel.InboundPrime))
                            {
                                roleInbound = itemSplit[0];
                            }
                            else
                            {
                                roleOutbound = itemSplit[0];
                            }

                        }
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }


        }
        private void findInboundOutboundSameParent()
        {
            try
            {
                var inbound_outbound = roles!.Where(x => x.Role.RoleType == RoleTypeModel.InboundGeneral ||
            x.Role.RoleType == RoleTypeModel.InboundOfficePrime || x.Role.RoleType == RoleTypeModel.InboundPrime ||
            x.Role.RoleType == RoleTypeModel.OutboundGeneral ||
            x.Role.RoleType == RoleTypeModel.OutboundOfficePrime || x.Role.RoleType == RoleTypeModel.OutboundPrime);

                // Find same parent
                string? oldParentId = "";
                foreach (var item in inbound_outbound)
                {
                    if (string.IsNullOrWhiteSpace(oldParentId))
                    {
                        oldParentId = item.ParentID;
                    }
                    else
                    {
                        // Check this item has already add to Ienumerable
                        if (oldParentId == item.ParentID)
                        {
                            continue;
                        }
                        else
                        {
                            oldParentId = item.ParentID;
                        }
                    }
                    

                    var sameParent = inbound_outbound.Where(x => x.ParentID == item.ParentID).ToList();
                    if (sameParent.Count > 1)
                    {
                        // Remove this role from roles
                        var newRoles = roles!.Where(x => x.ParentID != item.ParentID);
                        //supervisorOption = supervisorOption.Concat(new HashSet<string>() { item });
                        TabItemDto? combineInbounOutbound = new TabItemDto();
                        string? id = "";
                        string? name = "";
                        foreach (var s in sameParent)
                        {
                            if (s == sameParent.Last())
                            {
                                id += $"{s.Role.RoleID}:{s.Role.RoleType}";
                                name += $"{s.Role.Display.Local}";
                            }
                            else
                            {
                                id += $"{s.Role.RoleID}:{s.Role.RoleType}|";
                                name += $"{s.Role.Display.Local} - ";
                            }

                        }
                        combineInbounOutbound.Role.RoleID = id;
                        combineInbounOutbound.Role.Display.Local = name;

                        roles = newRoles.Concat(new List<TabItemDto> { combineInbounOutbound });
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
