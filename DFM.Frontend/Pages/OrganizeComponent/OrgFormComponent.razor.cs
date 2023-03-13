using DFM.Shared.Entities;
using DFM.Shared.Common;
using StackExchange.Redis;
using HttpClientService;
using System.Linq;
using System.Data;
using MudBlazor;
using DFM.Frontend.Shared;
using DFM.Shared.DTOs;

namespace DFM.Frontend.Pages.OrganizeComponent
{
    public partial class OrgFormComponent
    {
        private EmployeeModel? employee;
        string? token;
        private IEnumerable<RoleTreeModel>? charts;
        private IEnumerable<RoleTreeModel>? groupList;
        private IEnumerable<RoleTreeModel>? leaderList;
        private IEnumerable<string>? groupSelectValues;
        private IEnumerable<string>? parentSelectValues;
        PartialEmployeeProfile? selectedUser = new();
        IEnumerable<EmployeeModel>? employees;
        IEnumerable<TabItemDto>? tabItems;
        bool disabled = false;
        string? fullname;
        string? group;
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

        protected override async Task OnInitializedAsync()
        {
            if (FormMode == FormMode.Create)
            {
                disabled = false;
            }
            else if (FormMode == FormMode.Edit)
            {
                disabled = true;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                token = await accessToken.GetTokenAsync();
            }
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            // Get Organization From ID
            var url = $"{endpoint.API}/api/v1/Organization/GetItem/{employee.OrganizationID}";

            var result = await httpService.Get<IEnumerable<RoleTreeModel>>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
            if (result.Success)
            {
                charts = result.Response;
            }

            // Get Employee
            var employeeUrl = $"{endpoint.API}/api/v1/Employee/GetItems/{employee.OrganizationID}";
            var employeeResult = await httpService.Get<IEnumerable<EmployeeModel>>(employeeUrl, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
            if (employeeResult.Success)
            {
                employees = employeeResult.Response;
            }

            // Set fullname
            if (FormMode == FormMode.Edit)
            {
                fullname = $"{RoleTreeModel!.Employee.Name.Local} {RoleTreeModel!.Employee.FamilyName.Local}";
                url = $"{endpoint.API}/api/v1/Organization/GetRole/{RoleTreeModel!.Employee.UserID}";
                var roleResult = await httpService.Get<IEnumerable<TabItemDto>>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
                tabItems = roleResult.Response;
            }
            
        }

        private async Task openEmployeeSelector()
        {
            //DialogOptions options = new DialogOptions() { CloseButton = true };
            //DialogParameters parameters = new DialogParameters();
            //parameters.Add("Employees", employees);
            //parameters.Add("SelectUser", selectedUser);

            //DialogService.Show<EmployeeSelector>("ລາຍຊື່ພະນັກງານທັງຫມົດ", parameters, options);
            bool? selectBox = await mbox!.Show();
            if (selectBox.HasValue)
            {
                if (selectBox.Value)
                {
                    var selectItem = employees!.FirstOrDefault(x => x.id == selectedUser!.UserID);
                    fullname = $"{selectItem!.Name.Local} {selectItem!.FamilyName.Local}";
                    RoleTreeModel!.Employee = selectedUser!;
                    // Load tab
                    string url = $"{endpoint.API}/api/v1/Organization/GetRole/{selectedUser!.UserID}";


                    if (string.IsNullOrWhiteSpace(token))
                    {
                        token = await accessToken.GetTokenAsync();
                    }

                    var roleResult = await httpService.Get<IEnumerable<TabItemDto>>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
                    tabItems = roleResult.Response;
                    await InvokeAsync(StateHasChanged);
                }
            }
        }
        private async Task onSelectedGroupChanged(IEnumerable<string> values)
        {
            // Will get only General or Office Prime
            var item = charts!.FirstOrDefault(x => x.Role.RoleID == values.FirstOrDefault());
            leaderList = new HashSet<RoleTreeModel>();
            switch (RoleTreeModel!.Role.RoleType)
            {
                case RoleTypeModel.Prime:
                    break;
                case RoleTypeModel.DeputyPrime:
                    break;
                case RoleTypeModel.PrimeSecretary:
                    break;
                case RoleTypeModel.DeputyPrimeSecretary:
                    break;
                case RoleTypeModel.Director:
                    break;
                case RoleTypeModel.DeputyDirector:
                    break;
                case RoleTypeModel.OfficePrime:
                    break;
                case RoleTypeModel.DeputyOfficePrime:
                    break;
                case RoleTypeModel.General:
                    break;
                case RoleTypeModel.DeputyGeneral:
                    break;
                case RoleTypeModel.OfficeGeneral:
                    break;
                case RoleTypeModel.DeputyOfficeGeneral:
                    break;
                case RoleTypeModel.Division: // Action will start from this type
                    {
                        // Get Deputy General of Office Prime
                        var deputyGeneral = charts!.Where(x => x.ParentID == item!.Role.RoleID);
                        foreach (var role in deputyGeneral)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                    }

                    break;
                case RoleTypeModel.DeputyDivision:
                    {
                        // Get Deputy General of Office Prime
                        var deputyGeneral = charts!.Where(x => x.ParentID == item!.Role.RoleID);
                        foreach (var subItem in deputyGeneral)
                        {
                            // Get divisions
                            var divisions = charts!.Where(x => x.ParentID == subItem.Role.RoleID);

                            foreach (var role in divisions)
                            {
                                leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                            }
                        }
                    }
                    break;
                case RoleTypeModel.Department:
                    if (!envConf.ByPassDepartment) // If this company use department level in organization
                    {
                        // Get Deputy General of Office Prime
                        var deputyGeneral = charts!.Where(x => x.ParentID == item!.Role.RoleID);
                        foreach (var subItem in deputyGeneral)
                        {
                            // Get divisions
                            var divisions = charts!.Where(x => x.ParentID == subItem.Role.RoleID);

                            foreach (var div in divisions)
                            {
                                // Get Deputy division
                                var deputyDivs = charts!.Where(x => x.ParentID == div.Role.RoleID);

                                foreach (var role in deputyDivs)
                                {
                                    leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });

                                }
                            }
                        }
                    }
                    break;
                case RoleTypeModel.DeputyDepartment:
                    if (!envConf.ByPassDepartment) // If this company use department level in organization
                    {
                        // Get Deputy General of Office Prime
                        var deputyGeneral = charts!.Where(x => x.ParentID == item!.Role.RoleID);
                        foreach (var subItem in deputyGeneral)
                        {
                            // Get divisions
                            var divisions = charts!.Where(x => x.ParentID == subItem.Role.RoleID);

                            foreach (var div in divisions)
                            {
                                // Get Deputy division
                                var deputyDivs = charts!.Where(x => x.ParentID == div.Role.RoleID);

                                foreach (var depDiv in deputyDivs)
                                {
                                    // Get Department
                                    var departments = charts!.Where(x => x.ParentID == depDiv.Role.RoleID);
                                    foreach (var role in departments)
                                    {
                                        leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });

                                    }
                                }
                            }
                        }
                    }
                    break;
                case RoleTypeModel.Employee:
                    if (!envConf.ByPassDepartment) // If this company use department level in organization
                    {
                        // Get Deputy General of Office Prime
                        var deputyGeneral = charts!.Where(x => x.ParentID == item!.Role.RoleID);
                        foreach (var subItem in deputyGeneral)
                        {
                            // Get divisions
                            var divisions = charts!.Where(x => x.ParentID == subItem.Role.RoleID);

                            foreach (var div in divisions)
                            {
                                // Get Deputy division
                                var deputyDivs = charts!.Where(x => x.ParentID == div.Role.RoleID);

                                foreach (var depDiv in deputyDivs)
                                {
                                    // Get Department
                                    var departments = charts!.Where(x => x.ParentID == depDiv.Role.RoleID);
                                    foreach (var dpt in departments)
                                    {
                                        // Get Deputy department
                                        var depDepartment = charts!.Where(x => x.ParentID == dpt.Role.RoleID);
                                        foreach (var role in depDepartment)
                                        {
                                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });

                                        }

                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Get Deputy General of Office Prime
                        var deputyGeneral = charts!.Where(x => x.ParentID == item!.Role.RoleID);
                        foreach (var subItem in deputyGeneral)
                        {
                            // Get divisions
                            var divisions = charts!.Where(x => x.ParentID == subItem.Role.RoleID);

                            foreach (var role in divisions)
                            {
                                leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                            }
                        }
                    }
                    break;
                case RoleTypeModel.Contract:
                    if (!envConf.ByPassDepartment) // If this company use department level in organization
                    {
                        // Get Deputy General of Office Prime
                        var deputyGeneral = charts!.Where(x => x.ParentID == item!.Role.RoleID);
                        foreach (var subItem in deputyGeneral)
                        {
                            // Get divisions
                            var divisions = charts!.Where(x => x.ParentID == subItem.Role.RoleID);

                            foreach (var div in divisions)
                            {
                                // Get Deputy division
                                var deputyDivs = charts!.Where(x => x.ParentID == div.Role.RoleID);

                                foreach (var depDiv in deputyDivs)
                                {
                                    // Get Department
                                    var departments = charts!.Where(x => x.ParentID == depDiv.Role.RoleID);
                                    foreach (var dpt in departments)
                                    {
                                        // Get Deputy department
                                        var depDepartment = charts!.Where(x => x.ParentID == dpt.Role.RoleID);
                                        foreach (var role in depDepartment)
                                        {
                                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });

                                        }

                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Get Deputy General of Office Prime
                        var deputyGeneral = charts!.Where(x => x.ParentID == item!.Role.RoleID);
                        foreach (var subItem in deputyGeneral)
                        {
                            // Get divisions
                            var divisions = charts!.Where(x => x.ParentID == subItem.Role.RoleID);

                            foreach (var role in divisions)
                            {
                                leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                            }
                        }
                    }
                    break;
                case RoleTypeModel.Volunteer: // Action will stop here
                    if (!envConf.ByPassDepartment) // If this company use department level in organization
                    {
                        // Get Deputy General of Office Prime
                        var deputyGeneral = charts!.Where(x => x.ParentID == item!.Role.RoleID);
                        foreach (var subItem in deputyGeneral)
                        {
                            // Get divisions
                            var divisions = charts!.Where(x => x.ParentID == subItem.Role.RoleID);

                            foreach (var div in divisions)
                            {
                                // Get Deputy division
                                var deputyDivs = charts!.Where(x => x.ParentID == div.Role.RoleID);

                                foreach (var depDiv in deputyDivs)
                                {
                                    // Get Department
                                    var departments = charts!.Where(x => x.ParentID == depDiv.Role.RoleID);
                                    foreach (var dpt in departments)
                                    {
                                        // Get Deputy department
                                        var depDepartment = charts!.Where(x => x.ParentID == dpt.Role.RoleID);
                                        foreach (var role in depDepartment)
                                        {
                                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });

                                        }

                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Get Deputy General of Office Prime
                        var deputyGeneral = charts!.Where(x => x.ParentID == item!.Role.RoleID);
                        foreach (var subItem in deputyGeneral)
                        {
                            // Get divisions
                            var divisions = charts!.Where(x => x.ParentID == subItem.Role.RoleID);

                            foreach (var role in divisions)
                            {
                                leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                            }
                        }
                    }
                    break;
                case RoleTypeModel.InboundPrime:
                    break;
                case RoleTypeModel.InboundOfficePrime:
                    break;
                case RoleTypeModel.InboundGeneral:
                    break;
                case RoleTypeModel.OutboundPrime:
                    break;
                case RoleTypeModel.OutboundOfficePrime:
                    break;
                case RoleTypeModel.OutboundGeneral:
                    break;
                default:
                    break;
            }

            groupSelectValues = values;

            if (leaderList == null || leaderList.Count() == 0)
            {
                parentSelectValues = null;
            }
            await InvokeAsync(StateHasChanged);
        }
        private async Task onSelectedParentChanged(IEnumerable<string> values)
        {

            parentSelectValues = values;
            await InvokeAsync(StateHasChanged);
        }

        private async Task onSelectedRoleChanged(IEnumerable<RoleTypeModel> values)
        {
            try
            {
                // Get Folder ID where index = 0
                var prime = charts!.FirstOrDefault(x => x.RoleType == RoleTypeModel.Prime);
                var deputyPrimes = charts!.Where(x => x.RoleType == RoleTypeModel.DeputyPrime);
                var directors = charts!.Where(x => x.RoleType == RoleTypeModel.Director);
                var office = charts!.FirstOrDefault(x => x.RoleType == RoleTypeModel.OfficePrime);
                var general = charts!.Where(x => x.RoleType == RoleTypeModel.General);
                var item = values.FirstOrDefault();
                leaderList = new HashSet<RoleTreeModel>();
                groupList = new HashSet<RoleTreeModel>();
                switch (item)
                {
                    case RoleTypeModel.Prime:
                        // Nothing here
                        break;
                    case RoleTypeModel.DeputyPrime:
                        // Bind only leader (Parent)
                        leaderList = new HashSet<RoleTreeModel> { prime! };
                        break;
                    case RoleTypeModel.PrimeSecretary:
                        leaderList = new HashSet<RoleTreeModel> { prime! };
                        break;
                    case RoleTypeModel.DeputyPrimeSecretary:
                        foreach (var role in deputyPrimes)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        break;
                    case RoleTypeModel.Director:
                        foreach (var role in deputyPrimes)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        break;
                    case RoleTypeModel.DeputyDirector:
                        foreach (var role in directors)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        break;
                    case RoleTypeModel.OfficePrime:
                        foreach (var role in deputyPrimes)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        break;
                    case RoleTypeModel.DeputyOfficePrime:
                        leaderList = new HashSet<RoleTreeModel> { office! };
                        break;
                    case RoleTypeModel.General:
                        foreach (var role in directors)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        break;
                    case RoleTypeModel.DeputyGeneral:
                        foreach (var role in general)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        break;
                    case RoleTypeModel.OfficeGeneral:
                        foreach (var role in general)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        break;
                    case RoleTypeModel.DeputyOfficeGeneral:
                        foreach (var role in general)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        break;
                    case RoleTypeModel.Division:
                        foreach (var role in general)
                        {
                            groupList = groupList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        groupList = groupList.Concat(new HashSet<RoleTreeModel>() { office! });
                        break;
                    case RoleTypeModel.DeputyDivision:
                        foreach (var role in general)
                        {
                            groupList = groupList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        groupList = groupList.Concat(new HashSet<RoleTreeModel>() { office! });
                        break;
                    case RoleTypeModel.Department:
                        foreach (var role in general)
                        {
                            groupList = groupList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        groupList = groupList.Concat(new HashSet<RoleTreeModel>() { office! });
                        break;
                    case RoleTypeModel.DeputyDepartment:
                        foreach (var role in general)
                        {
                            groupList = groupList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        groupList = groupList.Concat(new HashSet<RoleTreeModel>() { office! });
                        break;
                    case RoleTypeModel.Employee:
                        foreach (var role in general)
                        {
                            groupList = groupList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        groupList = groupList.Concat(new HashSet<RoleTreeModel>() { office! });
                        break;
                    case RoleTypeModel.Contract:
                        foreach (var role in general)
                        {
                            groupList = groupList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        groupList = groupList.Concat(new HashSet<RoleTreeModel>() { office! });
                        break;
                    case RoleTypeModel.Volunteer:
                        foreach (var role in general)
                        {
                            groupList = groupList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        groupList = groupList.Concat(new HashSet<RoleTreeModel>() { office! });
                        break;
                    case RoleTypeModel.InboundPrime:
                        leaderList = new HashSet<RoleTreeModel> { prime! };
                        break;
                    case RoleTypeModel.InboundOfficePrime:
                        leaderList = new HashSet<RoleTreeModel> { office! };
                        break;
                    case RoleTypeModel.InboundGeneral:
                        foreach (var role in general)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        break;
                    case RoleTypeModel.OutboundPrime:
                        leaderList = new HashSet<RoleTreeModel> { prime! };
                        break;
                    case RoleTypeModel.OutboundOfficePrime:
                        leaderList = new HashSet<RoleTreeModel> { office! };
                        break;
                    case RoleTypeModel.OutboundGeneral:
                        foreach (var role in general)
                        {
                            leaderList = leaderList.Concat(new HashSet<RoleTreeModel>() { role });
                        }
                        break;
                    default:
                        break;
                }
                if (leaderList == null || leaderList.Count() == 0)
                {
                    parentSelectValues = null;
                }
                if (groupList == null || groupList.Count() == 0)
                {
                    groupSelectValues = null;
                }
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception)
            {
                await Notice.InvokeAsync("ຕ້ອງເພີ່ມຂໍ້ມູນຕຳແນ່ງໃຫ້ຄົບກ່ອນ");
            }
            
        }
    }
}
