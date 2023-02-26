using Confluent.Kafka;
using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using Minio.DataModel;
using MyCouch.Requests;
using Redis.OM;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DFM.Shared.Repository
{
    public class OrganizationChart : IOrganizationChart
    {
        private readonly ICouchContext couchContext;
        private readonly IRedisConnector redisConnector;
        private readonly CouchDBHelper read_couchDbHelper;
        private readonly CouchDBHelper write_couchDbHelper;
        private readonly CouchDBHelper read_dynamic_couchDbHelper;
        private readonly CouchDBHelper write_dynamic_couchDbHelper;

        public OrganizationChart(ICouchContext couchContext, DBConfig dbConfig, IRedisConnector redisConnector)
        {
            this.couchContext = couchContext;
            this.redisConnector = redisConnector;
            this.read_couchDbHelper = new CouchDBHelper
           (
               scheme: dbConfig.Reader.Scheme,
               srvAddr: dbConfig.Reader.SrvAddr,
               dbName: "dfm_organization_db",
               username: dbConfig.Reader.Username,
               password: dbConfig.Reader.Password,
               port: dbConfig.Reader.Port
           );
            this.write_couchDbHelper = new CouchDBHelper
            (
                scheme: dbConfig.Writer.Scheme,
                srvAddr: dbConfig.Writer.SrvAddr,
                dbName: "dfm_organization_db",
                username: dbConfig.Writer.Username,
                password: dbConfig.Writer.Password,
                port: dbConfig.Writer.Port
            );

            this.read_dynamic_couchDbHelper = new CouchDBHelper
           (
               scheme: dbConfig.Reader.Scheme,
               srvAddr: dbConfig.Reader.SrvAddr,
               dbName: "dfm_dynamic_db",
               username: dbConfig.Reader.Username,
               password: dbConfig.Reader.Password,
               port: dbConfig.Reader.Port
           );
            this.write_dynamic_couchDbHelper = new CouchDBHelper
            (
                scheme: dbConfig.Writer.Scheme,
                srvAddr: dbConfig.Writer.SrvAddr,
                dbName: "dfm_dynamic_db",
                username: dbConfig.Writer.Username,
                password: dbConfig.Writer.Password,
                port: dbConfig.Writer.Port
            );
        }

        public async Task<CommonResponse> AddRoleAndEmployee(string id, RoleTreeModel roleTreeModel, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find the existing record
                var existing = await GetOrganization(id, cancellationToken);

                if (!existing.Response.Success)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }
                // Remove the existing role in chart
                existing.Content.Chart.RemoveAll(x => x.Role.RoleID == roleTreeModel.Role.RoleID);
                // Add role and employee to organization
                existing.Content.Chart.Add(roleTreeModel);

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<OrganizationModel>();

                var result = await couchContext.EditAsync<OrganizationModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: existing.Content,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Organization}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(existing.Content));
                    existing.Content.rev = result.Rev;
                    existing.Content.id = result.Id;
                    await context.UpdateAsync(existing.Content);


                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.SUCCESS_OPERATION
                    };
                }
                else
                {
                    return new CommonResponseId()
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = nameof(ResultCode.REQUEST_FAIL),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.REQUEST_FAIL
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<CommonResponse> AddRoleAndEmployee(string id, List<RoleTreeModel> roleTreeModel, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find the existing record
                var existing = await GetOrganization(id, cancellationToken);

                if (!existing.Response.Success)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }
                // Remove the existing role in chart
                foreach (var item in roleTreeModel)
                {
                    existing.Content.Chart.RemoveAll(x => x.Role.RoleID == item.Role.RoleID);
                }

                // Add role and employee to organization
                existing.Content.Chart.AddRange(roleTreeModel);

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<OrganizationModel>();

                var result = await couchContext.EditAsync<OrganizationModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: existing.Content,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Organization}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(existing.Content));
                    existing.Content.rev = result.Rev;
                    existing.Content.id = result.Id;
                    await context.UpdateAsync(existing.Content);


                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.SUCCESS_OPERATION
                    };
                }
                else
                {
                    return new CommonResponseId()
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = nameof(ResultCode.REQUEST_FAIL),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.REQUEST_FAIL
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        public async Task<CommonResponse> RemoveRoleAndEmployee(string id, string roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find the existing record
                var existing = await GetOrganization(id, cancellationToken);

                if (!existing.Response.Success)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }
                // Remove the existing role in chart
                existing.Content.Chart.RemoveAll(x => x.Role.RoleID == roleId);

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<OrganizationModel>();

                var result = await couchContext.EditAsync<OrganizationModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: existing.Content,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Organization}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(existing.Content));
                    existing.Content.id = result.Id;
                    existing.Content.rev = result.Rev;
                    await context.UpdateAsync(existing.Content);


                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.SUCCESS_OPERATION
                    };
                }
                else
                {
                    return new CommonResponseId()
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = nameof(ResultCode.REQUEST_FAIL),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.REQUEST_FAIL
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        public async Task<CommonResponse> RemoveRoleAndEmployee(string id, List<string> roles, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find the existing record
                var existing = await GetOrganization(id, cancellationToken);

                if (!existing.Response.Success)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }
                // Remove the existing role in chart
                foreach (var item in roles)
                {
                    existing.Content.Chart.RemoveAll(x => x.Role.RoleID == item);
                }

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<OrganizationModel>();


                var result = await couchContext.EditAsync<OrganizationModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: existing.Content,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Organization}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(existing.Content));
                    existing.Content.rev = result.Rev;
                    existing.Content.id = result.Id;

                    await context.UpdateAsync(existing.Content);

                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.SUCCESS_OPERATION
                    };
                }
                else
                {
                    return new CommonResponseId()
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = nameof(ResultCode.REQUEST_FAIL),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.REQUEST_FAIL
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<CommonResponse> EditRoleOnly(string id, PartialRole roleTreeModel, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find the existing record
                var existing = await GetOrganization(id, cancellationToken);

                if (!existing.Response.Success)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }
                // Remove the existing role in chart
                var items = existing.Content.Chart.Where(x => x.Role.RoleID == roleTreeModel.RoleID).ToList();
                if (items.Count() == 0)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = "Role position not found",
                        Message = ResultCode.NOT_FOUND
                    };
                }

                foreach (var item in items)
                {
                    // Find index of item
                    int index = existing.Content.Chart.IndexOf(item);
                    // Update role only to organization
                    item.RoleType = roleTreeModel.RoleType;
                    item.Role = roleTreeModel;

                    // Set it back to chart
                    existing.Content.Chart[index] = item;
                }


                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<OrganizationModel>();

                var result = await couchContext.EditAsync<OrganizationModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: existing.Content,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Organization}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(existing.Content));
                    existing.Content.rev = result.Rev;
                    existing.Content.id = result.Id;
                    await context.UpdateAsync(existing.Content);


                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.SUCCESS_OPERATION
                    };
                }
                else
                {
                    return new CommonResponseId()
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = nameof(ResultCode.REQUEST_FAIL),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.REQUEST_FAIL
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        public async Task<CommonResponse> EditEmployeeOnly(string id, PartialEmployeeProfile employeeProfile, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find the existing record
                var existing = await GetOrganization(id, cancellationToken);

                if (!existing.Response.Success)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }
                // Remove the existing role in chart
                var items = existing.Content.Chart.Where(x => x.Employee.UserID == employeeProfile.UserID).ToList();
                if (items.Count() == 0)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = "Role position not found",
                        Message = ResultCode.NOT_FOUND
                    };
                }

                foreach (var item in items)
                {
                    // Find index of item
                    int index = existing.Content.Chart.IndexOf(item);
                    // Update employee only to organization
                    item.Employee = employeeProfile;

                    // Set it back to chart
                    existing.Content.Chart[index] = item;
                }

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<OrganizationModel>();

                var result = await couchContext.EditAsync<OrganizationModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: existing.Content,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Organization}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(existing.Content));
                    existing.Content.rev = result.Rev;
                    existing.Content.id = result.Id;
                    await context.UpdateAsync(existing.Content);


                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.SUCCESS_OPERATION
                    };
                }
                else
                {
                    return new CommonResponseId()
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = nameof(ResultCode.REQUEST_FAIL),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.REQUEST_FAIL
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        public async Task<(CommonResponse Response, OrganizationModel Content)> GetOrganization(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find from cache
                //string recordKey = $"{RedisPrefix.Organization}{id}"; // Set key for cache
                //var mem = redisConnector.Connection.GetDatabase(1);
                //var cache = await mem.StringGetAsync(recordKey);

                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<OrganizationModel>();

                var cache = await context.FindByIdAsync(id);
                if (cache == null)
                {
                    // Find the existing record
                    var existing = await couchContext.GetAsync<OrganizationModel>
                        (
                            couchDBHelper: read_couchDbHelper,
                            id: id,
                            cancellationToken: cancellationToken
                        );

                    if (!existing.IsSuccess)
                    {
                        return (new CommonResponseId()
                        {
                            Id = id,
                            Code = nameof(ResultCode.NOT_FOUND),
                            Success = false,
                            Detail = ValidateString.IsNullOrWhiteSpace(existing.Reason),
                            Message = ResultCode.NOT_FOUND
                        }, default!);
                    }

                    existing.Content.rev = existing.Rev;
                    existing.Content.id = existing.Id;
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(existing.Content));
                    await context.InsertAsync(existing.Content);

                    return (new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                        Message = ResultCode.SUCCESS_OPERATION
                    }, existing.Content);
                }

                //var result = JsonSerializer.Deserialize<OrganizationModel>(cache!);
                return (new CommonResponseId()
                {
                    Id = id,
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Success = true,
                    Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                    Message = ResultCode.SUCCESS_OPERATION
                }, cache!);
            }
            catch (Exception)
            {

                throw;
            }


        }

        public async Task<(CommonResponse Response, IEnumerable<TabItemDto> Content)> GetRoles(string orgId, string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var org = await GetOrganization(orgId, cancellationToken);
            if (!org.Response.Success)
            {
                return (org.Response, default!);
            }

            var rolesDto = org.Content.Chart.Where(x => x.Employee.UserID == userId);

            if (rolesDto.Count() == 0)
            {
                return (new CommonResponse
                {
                    Code = nameof(ResultCode.NOT_FOUND),
                    Success = false,
                    Detail = "Employee not found",
                    Message = ResultCode.NOT_FOUND
                }, default!);
            }

            var result = rolesDto.Select(x => new TabItemDto
            {
                OrgID = orgId,
                Employee = x.Employee,
                Role = x.Role
            });

            return (org.Response, result);

        }
        public async Task<(CommonResponse Response, RoleTreeModel Content)> GetEmployee(string orgId, string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var org = await GetOrganization(orgId, cancellationToken);
            if (!org.Response.Success)
            {
                return (org.Response, default!);
            }

            var rolesDto = org.Content.Chart.FirstOrDefault(x => x.Role.RoleID == roleId);

            if (rolesDto == null)
            {
                return (new CommonResponse
                {
                    Code = nameof(ResultCode.NOT_FOUND),
                    Success = false,
                    Detail = "Role not found",
                    Message = ResultCode.NOT_FOUND
                }, default!);
            }

           
            return (org.Response, rolesDto);

        }
        public async Task<CommonResponseId> NewOrganization(MultiLanguage multiLanguage, AttachmentModel attachmentModel, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new OrganizationModel
                {
                    id = Guid.NewGuid().ToString("N"),
                    Name = multiLanguage,
                    Logo = attachmentModel
                };

                // Redis first
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<OrganizationModel>();

                var result = await couchContext.InsertAsync<OrganizationModel>
                   (
                       couchDBHelper: write_couchDbHelper,
                       model: request,
                       cancellationToken: cancellationToken
                   );

                if (result.IsSuccess)
                {
                    //Set cache after create success
                    //string recordKey = $"{RedisPrefix.Organization}{result.Id}"; // Set key for cache
                    //var mem = redisConnector.Connection.GetDatabase(1);
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(request));
                    request.rev = result.Rev;
                    await context.InsertAsync(request);


                    return new CommonResponseId()
                    {
                        Id = result.Id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.SUCCESS_OPERATION
                    };
                }
                else
                {
                    return new CommonResponseId()
                    {
                        Id = GeneratorHelper.NotAvailable,
                        Code = nameof(ResultCode.REQUEST_FAIL),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                        Message = ResultCode.REQUEST_FAIL
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<(decimal RowCount, IEnumerable<RoleTreeModel> Contents, CommonResponse Response)> GetChartFrom(string id, string roleId, ModuleType moduleType, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record
                var existing = await GetOrganization(id, cancellationToken);

                if (!existing.Response.Success)
                {
                    return (0, default!, new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    });
                }

                // Find  Role Type from chart
                var roleItem = existing.Content.Chart.FirstOrDefault(x => x.Role.RoleID == roleId);
                if (roleItem == null)
                {
                    return (0, default!, new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = "Role not found in organization chart",
                        Message = ResultCode.NOT_FOUND
                    });
                }

                // Get Dynamic Flow
                var dynamicFlow = await GetDynamicFlowByID(id, moduleType, cancellationToken);

                if (!dynamicFlow.Response.Success)
                {
                    return (0, default!, new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = "Flow not setup, please setup the flow first",
                        Message = ResultCode.NOT_FOUND
                    });
                }

                var flowItem = dynamicFlow.Roles.FirstOrDefault(x => x.RoleSource == roleItem.RoleType); // ດຶງເອົາຂໍ້ມູນຂອງ Role ໂຕເອງ
                if (flowItem == null)
                {
                    return (0, default!, new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = "Flow not setup, please setup the flow first",
                        Message = ResultCode.NOT_FOUND
                    });
                }
                var charts = existing.Content.Chart;
                var prime = charts.FirstOrDefault(x => x.RoleType == RoleTypeModel.Prime); // ດຶງເອົາຂໍ້ມູນຂອງປະທານ ເພາະມີແຕ່ຄົນດຽວ
                var deputyPrimes = charts.Where(x => x.RoleType == RoleTypeModel.DeputyPrime).ToList();// ດຶງເອົາຂໍ້ມູນຂອງຮອງປະທານ ມີຫຼາຍຄົນ ແຕ່ເປັນຕຳແໜ່ງທີ່ບົ່ງບອກໄດ້ທັນທີ
                var officePrime = charts.FirstOrDefault(x => x.RoleType == RoleTypeModel.OfficePrime);// ດຶງເອົາຂໍ້ມູນຂອງ ຫົວຫນ້າຫ້ອງການ ບໍລິສັດ ເພາະມີແຕ່ຄົນດຽວ
                var depOfficePrimes = charts.Where(x => x.RoleType == RoleTypeModel.DeputyOfficePrime).ToList();// ດຶງເອົາຂໍ້ມູນຂອງ ຮອງຫ້ອງການ ບໍລິສັດ ມີຫຼາຍຄົນ ແຕ່ເປັນຕຳແໜ່ງທີ່ບົ່ງບອກໄດ້ທັນທີ
                var secretPrimes = charts.Where(x => x.RoleType == RoleTypeModel.PrimeSecretary).ToList();// ດຶງເອົາຂໍ້ມູນເລຂາ ປະທານ
                var secretDeputyPrimes = charts.Where(x => x.RoleType == RoleTypeModel.DeputyPrimeSecretary).ToList();// ດຶງເອົາຂໍ້ມູນເລຂາ ຮອງປະທານ
                var inboundPrime = charts.Where(x => x.RoleType == RoleTypeModel.InboundPrime).ToList(); // ດຶງເອົາຂາເຂົ້າ ບໍລິສັດ
                var outboundPrime = charts.Where(x => x.RoleType == RoleTypeModel.OutboundPrime).ToList(); // ດຶງເອົາຂາອອກ ບໍລິສັດ
                var inboundOffice = charts.Where(x => x.RoleType == RoleTypeModel.InboundOfficePrime).ToList(); // ດຶງເອົາຂາເຂົ້າ ບໍລິສັດ
                var outboundOffice = charts.Where(x => x.RoleType == RoleTypeModel.OutboundOfficePrime).ToList(); // ດຶງເອົາຂາອອກ ບໍລິສັດ
                var allInboundGeneral = charts.Where(x => x.RoleType == RoleTypeModel.InboundGeneral).ToList(); // ດຶງເອົາຂາເຂົ້າທຸກຝ່າຍໃນ ບໍລິສັດ
                var allOutboundGeneral = charts.Where(x => x.RoleType == RoleTypeModel.OutboundGeneral).ToList(); // ດຶງເອົາຂາອກທຸກຝ່າຍໃນ ບໍລິສັດ
                List<RoleTreeModel> childCharts = new();
                foreach (var item in flowItem!.RoleTargets!)
                {
                    switch (item)
                    {
                        case RoleTypeModel.Prime:
                            childCharts.Add(prime!); // ຖ້າ ເປົ້າຫມາຍເປັນ ປະທາານແມ່ນໃຫ້ເອົາຈາກ ຕົວປ່ຽນທາງເທິງ
                            break;
                        case RoleTypeModel.DeputyPrime:
                            if (!deputyPrimes!.IsNullOrEmpty())
                            {
                                childCharts.AddRange(deputyPrimes!);// ຖ້າ ເປົ້າຫມາຍເປັນ ຮອງປະທາານແມ່ນໃຫ້ເອົາຈາກ ຕົວປ່ຽນທາງເທິງ

                            }
                            break;
                        case RoleTypeModel.PrimeSecretary:
                            if (!secretPrimes!.IsNullOrEmpty())
                            {
                                childCharts.AddRange(secretPrimes!);// ຖ້າ ເປົ້າຫມາຍເປັນ ເລຂາປະທາານແມ່ນໃຫ້ເອົາຈາກ ຕົວປ່ຽນທາງເທິງ

                            }
                            break;
                        case RoleTypeModel.DeputyPrimeSecretary:
                            if (!secretDeputyPrimes!.IsNullOrEmpty())
                            {
                                childCharts.AddRange(secretDeputyPrimes!);// ຖ້າ ເປົ້າຫມາຍເປັນ ເລຂາຮອງປະທາານແມ່ນໃຫ້ເອົາຈາກ ຕົວປ່ຽນທາງເທິງ

                            }
                            break;
                        case RoleTypeModel.Director:
                            {
                                var targetModel = getParent(charts, roleItem, item); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                                if (targetModel != null)
                                {
                                    childCharts.Add(targetModel!);

                                }
                            }
                            break;
                        case RoleTypeModel.DeputyDirector:
                            if (flowItem.CrossInternal)
                            {
                                // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ,ໃນກໍລະນີທີ່ ຕ້ອງການ ເອົາ Deputy ຂອງຕຳແຫນ່ງຕ່າງໆ ແມ່ນຈະຕ້ອງໄດ້ຊອກຫາ Parent ຂອງ Deputy ດັ່ງກ່າວກ່ອນ
                                var parentModel = getParent(charts, roleItem, RoleTypeModel.Director);
                                var childModels = charts.Where(x => x.ParentID == parentModel!.Role.RoleID);
                                if (!childModels!.IsNullOrEmpty())
                                {
                                    childCharts.AddRange(childModels!);// ຖ້າ ເປົ້າຫມາຍເປັນ ຮອງຫ້ອງແມ່ນໃຫ້ເອົາຈາກ ຕົວປ່ຽນທາງເທິງ

                                }
                            }
                            else
                            {
                                var targetModel = getParent(charts, roleItem, item);// ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                                if (targetModel != null)
                                {
                                    childCharts.Add(targetModel!);

                                }
                            }
                            break;
                        case RoleTypeModel.OfficePrime:
                            childCharts.Add(officePrime!);// ຖ້າ ເປົ້າຫມາຍເປັນ ຫ້ອງການແມ່ນໃຫ້ເອົາຈາກ ຕົວປ່ຽນທາງເທິງ
                            break;
                        case RoleTypeModel.DeputyOfficePrime:
                            if (!depOfficePrimes!.IsNullOrEmpty())
                            {
                                childCharts.AddRange(depOfficePrimes!);// ຖ້າ ເປົ້າຫມາຍເປັນ ຮອງຫ້ອງແມ່ນໃຫ້ເອົາຈາກ ຕົວປ່ຽນທາງເທິງ

                            }
                            break;
                        case RoleTypeModel.General:
                            {
                                var targetModel = getParent(charts, roleItem, item);// ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                                if (targetModel != null)
                                {
                                    childCharts.Add(targetModel!);

                                }
                            }
                            break;
                        case RoleTypeModel.DeputyGeneral:
                            if (flowItem.CrossInternal)
                            {
                                // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ,ໃນກໍລະນີທີ່ ຕ້ອງການ ເອົາ Deputy ຂອງຕຳແຫນ່ງຕ່າງໆ ແມ່ນຈະຕ້ອງໄດ້ຊອກຫາ Parent ຂອງ Deputy ດັ່ງກ່າວກ່ອນ
                                var parentModel = getParent(charts, roleItem, RoleTypeModel.General);
                                var childModels = charts.Where(x => x.ParentID == parentModel!.Role.RoleID);
                                if (!childModels!.IsNullOrEmpty())
                                {
                                    childCharts.AddRange(childModels!);

                                }

                            }
                            else
                            {
                                var targetModel = getParent(charts, roleItem, item);// ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                                if (targetModel != null)
                                {
                                    childCharts.Add(targetModel!);

                                }
                            }
                            break;
                        case RoleTypeModel.OfficeGeneral:
                            {
                                var targetModel = getParent(charts, roleItem, item);// ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                                childCharts.Add(targetModel!);
                                if (targetModel != null)
                                {
                                    childCharts.Add(targetModel!);

                                }
                            }
                            break;
                        case RoleTypeModel.DeputyOfficeGeneral:
                            if (flowItem.CrossInternal)
                            {
                                // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ,ໃນກໍລະນີທີ່ ຕ້ອງການ ເອົາ Deputy ຂອງຕຳແຫນ່ງຕ່າງໆ ແມ່ນຈະຕ້ອງໄດ້ຊອກຫາ Parent ຂອງ Deputy ດັ່ງກ່າວກ່ອນ
                                var parentModel = getParent(charts, roleItem, RoleTypeModel.OfficeGeneral);
                                var childModels = charts.Where(x => x.ParentID == parentModel!.Role.RoleID);
                                if (!childModels!.IsNullOrEmpty())
                                {
                                    childCharts.AddRange(childModels!);

                                }
                            }
                            else
                            {
                                var targetModel = getParent(charts, roleItem, item);// ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                                if (targetModel != null)
                                {
                                    childCharts.Add(targetModel!);

                                }
                            }
                            break;
                        case RoleTypeModel.Division:
                            {
                                var targetModel = getParent(charts, roleItem, item);
                                if (targetModel != null)
                                {
                                    childCharts.Add(targetModel!);

                                }
                            }
                            break;
                        case RoleTypeModel.DeputyDivision:
                            if (flowItem.CrossInternal)
                            {
                                // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ,ໃນກໍລະນີທີ່ ຕ້ອງການ ເອົາ Deputy ຂອງຕຳແຫນ່ງຕ່າງໆ ແມ່ນຈະຕ້ອງໄດ້ຊອກຫາ Parent ຂອງ Deputy ດັ່ງກ່າວກ່ອນ
                                var parentModel = getParent(charts, roleItem, RoleTypeModel.OfficeGeneral);
                                var childModels = charts.Where(x => x.ParentID == parentModel!.Role.RoleID);
                                
                                if (!childModels!.IsNullOrEmpty())
                                {
                                    childCharts.AddRange(childModels!);

                                }
                            }
                            else
                            {
                                var targetModel = getParent(charts, roleItem, item);// ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                                if (targetModel != null)
                                {
                                    childCharts.Add(targetModel!);

                                }
                            }
                            break;
                        case RoleTypeModel.Department:
                            {
                                var targetModel = getParent(charts, roleItem, item);// ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                                if (targetModel != null)
                                {
                                    childCharts.Add(targetModel!);

                                }
                            }
                            break;
                        case RoleTypeModel.DeputyDepartment:
                            if (flowItem.CrossInternal)
                            {
                                // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ,ໃນກໍລະນີທີ່ ຕ້ອງການ ເອົາ Deputy ຂອງຕຳແຫນ່ງຕ່າງໆ ແມ່ນຈະຕ້ອງໄດ້ຊອກຫາ Parent ຂອງ Deputy ດັ່ງກ່າວກ່ອນ
                                var parentModel = getParent(charts, roleItem, RoleTypeModel.OfficeGeneral);
                                var childModels = charts.Where(x => x.ParentID == parentModel!.Role.RoleID);
                                
                                if (!childModels!.IsNullOrEmpty())
                                {
                                    childCharts.AddRange(childModels!);

                                }
                            }
                            else
                            {
                                var targetModel = getParent(charts, roleItem, item);// ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ

                                
                                if (targetModel != null)
                                {
                                    childCharts.Add(targetModel!);

                                }
                            }
                            break;
                        case RoleTypeModel.Employee:
                            break;
                        case RoleTypeModel.Contract:
                            break;
                        case RoleTypeModel.Volunteer:
                            break;
                        case RoleTypeModel.InboundPrime:
                            if (!inboundPrime!.IsNullOrEmpty())
                            {
                                childCharts.AddRange(inboundPrime!);

                            }
                            break;
                        case RoleTypeModel.InboundOfficePrime:
                            if (!inboundOffice!.IsNullOrEmpty())
                            {
                                childCharts.AddRange(inboundOffice!);

                            }
                            break;
                        case RoleTypeModel.InboundGeneral:
                            // If roleItem is Outbound (Any outbound type) should see all Inbound general
                            // Else If roleItem is Inbound (Any Inbound type) should see all inbound general also
                            // Else should see only inbound that same general
                            if (isInboundOrOutbound(roleItem.RoleType))
                            {
                                if (!allInboundGeneral!.IsNullOrEmpty())
                                {
                                    childCharts.AddRange(allInboundGeneral!);

                                }
                            }
                            else
                            {
                                var parentGeneral = getParent(charts, roleItem, RoleTypeModel.General);

                                var myInbound = allInboundGeneral.FirstOrDefault(x => x.ParentID == parentGeneral.Role.RoleID);
                                if (myInbound != null)
                                {
                                    childCharts.Add(myInbound);
                                }
                            }
                            break;
                        case RoleTypeModel.OutboundPrime:
                            childCharts.AddRange(outboundPrime!);
                            break;
                        case RoleTypeModel.OutboundOfficePrime:
                            childCharts.AddRange(outboundOffice!);
                            break;
                        case RoleTypeModel.OutboundGeneral:
                            // If roleItem is Outbound (Any outbound type) should see all Inbound general
                            // Else If roleItem is Inbound (Any Inbound type) should see all inbound general also
                            // Else should see only inbound that same general
                            if (isInboundOrOutbound(roleItem.RoleType))
                            {
                                if (!allInboundGeneral!.IsNullOrEmpty())
                                {
                                    childCharts.AddRange(allInboundGeneral!);

                                }
                            }
                            else
                            {
                                var parentGeneral = getParent(charts, roleItem, RoleTypeModel.General);

                                var myInbound = allInboundGeneral.FirstOrDefault(x => x.ParentID == parentGeneral!.Role.RoleID);
                                if (myInbound != null)
                                {
                                    childCharts.Add(myInbound);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                if (childCharts.Count == 0)
                {
                    return (0, default!, new CommonResponse
                    {
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = "Could not get chart tree",
                        Message = ResultCode.NOT_FOUND
                    });
                }
                else
                {
                    return (childCharts.Count, childCharts!, new CommonResponse
                    {
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ResultCode.SUCCESS_OPERATION,
                        Message = ResultCode.SUCCESS_OPERATION
                    });
                }
            }
            catch (Exception)
            {

                throw;
            }


        }

        private bool isInboundOrOutbound(RoleTypeModel roleType)
        {
            switch (roleType)
            {
                case RoleTypeModel.Prime:
                    return false;
                case RoleTypeModel.DeputyPrime:
                    return false;
                case RoleTypeModel.PrimeSecretary:
                    return false;
                case RoleTypeModel.DeputyPrimeSecretary:
                    return false;
                case RoleTypeModel.Director:
                    return false;
                case RoleTypeModel.DeputyDirector:
                    return false;
                case RoleTypeModel.OfficePrime:
                    return false;
                case RoleTypeModel.DeputyOfficePrime:
                    return false;
                case RoleTypeModel.General:
                    return false;
                case RoleTypeModel.DeputyGeneral:
                    return false;
                case RoleTypeModel.OfficeGeneral:
                    return false;
                case RoleTypeModel.DeputyOfficeGeneral:
                    return false;
                case RoleTypeModel.Division:
                    return false;
                case RoleTypeModel.DeputyDivision:
                    return false;
                case RoleTypeModel.Department:
                    return false;
                case RoleTypeModel.DeputyDepartment:
                    return false;
                case RoleTypeModel.Employee:
                    return false;
                case RoleTypeModel.Contract:
                    return false;
                case RoleTypeModel.Volunteer:
                    return false;
                case RoleTypeModel.InboundPrime:
                    return true;
                case RoleTypeModel.InboundOfficePrime:
                    return true;
                case RoleTypeModel.InboundGeneral:
                    return true;
                case RoleTypeModel.OutboundPrime:
                    return true;
                case RoleTypeModel.OutboundOfficePrime:
                    return true;
                case RoleTypeModel.OutboundGeneral:
                    return true;
                default:
                    return false;
            }
        }

        public async Task<CommonResponseId> GetPublisher(string id, string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record
                var existing = await GetOrganization(id, cancellationToken);

                if (!existing.Response.Success)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    };
                }

                // Find  Role Type from chart
                var roleItem = existing.Content.Chart.FirstOrDefault(x => x.Role.RoleID == roleId);
                if (roleItem == null)
                {
                    return new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = "Role not found in organization chart",
                        Message = ResultCode.NOT_FOUND
                    };
                }

                
                var charts = existing.Content.Chart;
                var prime = charts.FirstOrDefault(x => x.RoleType == RoleTypeModel.Prime); // ດຶງເອົາຂໍ້ມູນຂອງປະທານ ເພາະມີແຕ່ຄົນດຽວ
                var officePrime = charts.FirstOrDefault(x => x.RoleType == RoleTypeModel.OfficePrime);// ດຶງເອົາຂໍ້ມູນຂອງ ຫົວຫນ້າຫ້ອງການ ບໍລິສັດ ເພາະມີແຕ່ຄົນດຽວ
                switch (roleItem.RoleType)
                {
                    case RoleTypeModel.Prime:
                        return new CommonResponseId()
                        {
                            Id = prime.Publisher,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    case RoleTypeModel.DeputyPrime:
                        return new CommonResponseId()
                        {
                            Id = prime.Publisher,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    case RoleTypeModel.PrimeSecretary:
                        return new CommonResponseId()
                        {
                            Id = prime.Publisher,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    case RoleTypeModel.DeputyPrimeSecretary:
                        return new CommonResponseId()
                        {
                            Id = prime.Publisher,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    case RoleTypeModel.Director:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.Director); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.DeputyDirector:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.Director); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.OfficePrime:
                        return new CommonResponseId()
                        {
                            Id = officePrime.Publisher,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    case RoleTypeModel.DeputyOfficePrime:
                        return new CommonResponseId()
                        {
                            Id = officePrime.Publisher,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    case RoleTypeModel.General:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.DeputyGeneral:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.OfficeGeneral:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.DeputyOfficeGeneral:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.Division:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            if (targetModel == null)
                            {
                                return new CommonResponseId()
                                {
                                    Id = officePrime.Publisher,
                                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                                    Success = true,
                                    Detail = ResultCode.SUCCESS_OPERATION,
                                    Message = ResultCode.SUCCESS_OPERATION
                                };
                            }
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.DeputyDivision:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            if (targetModel == null)
                            {
                                return new CommonResponseId()
                                {
                                    Id = officePrime.Publisher,
                                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                                    Success = true,
                                    Detail = ResultCode.SUCCESS_OPERATION,
                                    Message = ResultCode.SUCCESS_OPERATION
                                };
                            }
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.Department:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            if (targetModel == null)
                            {
                                return new CommonResponseId()
                                {
                                    Id = officePrime.Publisher,
                                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                                    Success = true,
                                    Detail = ResultCode.SUCCESS_OPERATION,
                                    Message = ResultCode.SUCCESS_OPERATION
                                };
                            }
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.DeputyDepartment:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            if (targetModel == null)
                            {
                                return new CommonResponseId()
                                {
                                    Id = officePrime.Publisher,
                                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                                    Success = true,
                                    Detail = ResultCode.SUCCESS_OPERATION,
                                    Message = ResultCode.SUCCESS_OPERATION
                                };
                            }
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.Employee:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            if (targetModel == null)
                            {
                                return new CommonResponseId()
                                {
                                    Id = officePrime.Publisher,
                                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                                    Success = true,
                                    Detail = ResultCode.SUCCESS_OPERATION,
                                    Message = ResultCode.SUCCESS_OPERATION
                                };
                            }
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.Contract:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            if (targetModel == null)
                            {
                                return new CommonResponseId()
                                {
                                    Id = officePrime.Publisher,
                                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                                    Success = true,
                                    Detail = ResultCode.SUCCESS_OPERATION,
                                    Message = ResultCode.SUCCESS_OPERATION
                                };
                            }
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.Volunteer:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            if (targetModel == null)
                            {
                                return new CommonResponseId()
                                {
                                    Id = officePrime.Publisher,
                                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                                    Success = true,
                                    Detail = ResultCode.SUCCESS_OPERATION,
                                    Message = ResultCode.SUCCESS_OPERATION
                                };
                            }
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.InboundPrime:
                        return new CommonResponseId()
                        {
                            Id = prime.Publisher,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    case RoleTypeModel.InboundOfficePrime:
                        return new CommonResponseId()
                        {
                            Id = officePrime.Publisher,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    case RoleTypeModel.InboundGeneral:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            if (targetModel == null)
                            {
                                return new CommonResponseId()
                                {
                                    Id = officePrime.Publisher,
                                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                                    Success = true,
                                    Detail = ResultCode.SUCCESS_OPERATION,
                                    Message = ResultCode.SUCCESS_OPERATION
                                };
                            }
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    case RoleTypeModel.OutboundPrime:
                        return new CommonResponseId()
                        {
                            Id = prime.Publisher,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    case RoleTypeModel.OutboundOfficePrime:
                        return new CommonResponseId()
                        {
                            Id = officePrime.Publisher,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ResultCode.SUCCESS_OPERATION,
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    case RoleTypeModel.OutboundGeneral:
                        {
                            var targetModel = getParent(charts, roleItem, RoleTypeModel.General); // ໃນກໍລະນີນີ້ແມ່ນຈະຕ້ອງໄດ້ Loop ຊອກໄປເລື້ອຍໆ ຈົນກວ່າຈະພົບ RoleType ທີ່ຕ້ອງການ
                            if (targetModel == null)
                            {
                                return new CommonResponseId()
                                {
                                    Id = officePrime.Publisher,
                                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                                    Success = true,
                                    Detail = ResultCode.SUCCESS_OPERATION,
                                    Message = ResultCode.SUCCESS_OPERATION
                                };
                            }
                            return new CommonResponseId()
                            {
                                Id = targetModel.Publisher,
                                Code = nameof(ResultCode.SUCCESS_OPERATION),
                                Success = true,
                                Detail = ResultCode.SUCCESS_OPERATION,
                                Message = ResultCode.SUCCESS_OPERATION
                            };
                        }
                    default:
                        return new CommonResponseId()
                        {
                            Id = id,
                            Code = nameof(ResultCode.NOT_FOUND),
                            Success = false,
                            Detail = "Role not found in organization chart",
                            Message = ResultCode.NOT_FOUND
                        };
                }
            }
            catch (Exception)
            {

                throw;
            }


        }
        public async Task<(decimal RowCount, IEnumerable<RoleTreeModel> Contents, CommonResponse Response)> GetChartByID(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Find the existing record
                var existing = await GetOrganization(id, cancellationToken);

                if (!existing.Response.Success)
                {
                    return (0, default!, new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Detail = ValidateString.IsNullOrWhiteSpace(existing.Response.Detail!),
                        Message = ResultCode.NOT_FOUND
                    });
                }

                return (existing.Content.Chart.Count, existing.Content.Chart!, new CommonResponse
                {
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Success = true,
                    Detail = ResultCode.SUCCESS_OPERATION,
                    Message = ResultCode.SUCCESS_OPERATION
                });
            }
            catch (Exception)
            {

                throw;
            }


        }

        public async Task<bool> IsInSameParent(string? orgId, string? child1, string? child2, CancellationToken cancellationToken = default)
        {
            // Find the existing record
            var existing = await GetOrganization(orgId, cancellationToken);
            if (!existing.Response.Success)
            {
                return false;
            }

            // Find  Role Type from chart
            var child1Item = existing.Content.Chart.FirstOrDefault(x => x.Role.RoleID == child1);
            var child2Item = existing.Content.Chart.FirstOrDefault(x => x.Role.RoleID == child2);

            if (child1Item!.ParentID == child2Item!.ParentID)
            {
                return true;
            }
            else
            {
                var result = isInSameParent(existing.Content.Chart, child1Item!, child2Item!);

                return result;
            }

            //RoleTreeModel param1 = new();
            //RoleTreeModel param2 = new();

            //if (child1Item.ParentID == "0")
            //{
            //    param1 = child1Item;
            //}
            //else
            //{
            //    param1 = existing.Content.Chart.FirstOrDefault(x => x.Role.RoleID == child1Item.ParentID);
            //}
            //if (child2Item.ParentID == "0")
            //{
            //    param2 = child2Item;
            //}
            //else
            //{
            //    param2 = existing.Content.Chart.FirstOrDefault(x => x.Role.RoleID == child2Item.ParentID);
            //}

            //if (param1 == null || param2 == null)
            //{
            //    // No role exist
            //    return false;
            //}

            
        }
        private bool isInSameParent(IEnumerable<RoleTreeModel> charts, RoleTreeModel parent1, RoleTreeModel parent2)
        {
            if (parent1.RoleType == RoleTypeModel.General || parent1.RoleType == RoleTypeModel.OfficePrime || parent1.RoleType == RoleTypeModel.Prime || parent1.ParentID == "0")
            {
                // Child 1 stop at top of root
                if (parent2.RoleType == RoleTypeModel.General || parent2.RoleType == RoleTypeModel.OfficePrime || parent2.RoleType == RoleTypeModel.Prime || parent2.ParentID == "0")
                {
                    // Child 2 also come to top of root

                    // Check both is the same parent
                    if (parent1.Role.RoleID == parent2.Role.RoleID)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // Continue find top root of child 2
                    var topRootChild = getParent(charts, parent2);
                    if (topRootChild == null)
                    {
                        // If could not find top root then return
                        return false;
                    }
                    else
                    {
                        if (parent1.Role.RoleID == topRootChild.Role.RoleID)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            if (parent2.RoleType == RoleTypeModel.General || parent2.RoleType == RoleTypeModel.OfficePrime || parent2.RoleType == RoleTypeModel.Prime || parent2.ParentID == "0")
            {
                // Child 2 stop at top of root
                if (parent1.RoleType == RoleTypeModel.General || parent1.RoleType == RoleTypeModel.OfficePrime || parent1.RoleType == RoleTypeModel.Prime || parent1.ParentID == "0")
                {
                    // Child 1 also come to top of root

                    // Check both is the same parent
                    if (parent1.Role.RoleID == parent2.Role.RoleID)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // Continue find top root of child 1
                    var topRootChild = getParent(charts, parent1);
                    if (topRootChild == null)
                    {
                        // If could not find top root then return
                        return false;
                    }
                    else
                    {
                        if (parent2.Role.RoleID == topRootChild.Role.RoleID)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                var topRoot1Child = getParent(charts, parent1);
                var topRoot2Child = getParent(charts, parent2);
                if (topRoot1Child == null)
                {
                    return false;
                }
                if (topRoot2Child == null)
                {
                    return false;
                }
                return isInSameParent(charts, topRoot1Child!, topRoot2Child!);
            }
        }
        private RoleTreeModel? getParent(IEnumerable<RoleTreeModel> charts, RoleTreeModel roleItem, RoleTypeModel roleTarget)
        {
            var parentItem = charts.FirstOrDefault(x => x.Role.RoleID == roleItem.ParentID);

            if (parentItem == null)
            {
                // No role exist
                return null;
            }

            if (parentItem.RoleType == roleTarget)
            {
                return parentItem!;
            }
            //else if (parentItem.ParentID == "0")
            //{
            //    return parentItem!;
            //}
            else
            {
                return getParent(charts, parentItem, roleTarget);
            }


        }
        private RoleTreeModel? getParent(IEnumerable<RoleTreeModel> charts, RoleTreeModel roleItem)
        {
            var parentItem = charts.FirstOrDefault(x => x.Role.RoleID == roleItem.ParentID);

            if (parentItem == null)
            {
                // No role exist
                return null;
            }

            if (parentItem.RoleType == RoleTypeModel.General || parentItem.RoleType == RoleTypeModel.OfficePrime || parentItem.RoleType == RoleTypeModel.Prime)
            {
                return parentItem!;
            }
            //else if (parentItem.ParentID == "0")
            //{
            //    return parentItem!;
            //}
            else
            {
                return getParent(charts, parentItem);
            }


        }

        public async Task<CommonResponse> SaveDynamicFlow(string orgID, RoleTypeModel source, List<RoleTypeModel> targets, ModuleType moduleType, bool isCross = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DynamicFlowModel>();

                var existing = await GetDynamicFlowByID(orgID, moduleType, cancellationToken);
                if (!existing.Response.Success)
                {
                    // Insert new record
                    var request = new DynamicFlowModel
                    {
                        id = orgID,
                        RoleTypeItems = new List<DynamicItem>
                        {
                            new DynamicItem
                            {
                                RoleSource = source,
                                RoleTargets = targets,
                                ModuleType = moduleType,
                                CrossInternal = isCross
                            }
                        }
                    };

                    var result = await couchContext.InsertAsync<DynamicFlowModel>
                       (
                           couchDBHelper: write_dynamic_couchDbHelper,
                           model: request,
                           cancellationToken: cancellationToken
                       );

                    if (result.IsSuccess)
                    {
                        //Set cache after create success
                        //string recordKey = $"{RedisPrefix.Organization}{result.Id}"; // Set key for cache
                        //var mem = redisConnector.Connection.GetDatabase(1);
                        //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(request));
                        request.rev = result.Rev;
                        await context.InsertAsync(request);


                        return new CommonResponseId()
                        {
                            Id = result.Id,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    }
                    else
                    {
                        return new CommonResponseId()
                        {
                            Id = GeneratorHelper.NotAvailable,
                            Code = nameof(ResultCode.REQUEST_FAIL),
                            Success = false,
                            Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                            Message = ResultCode.REQUEST_FAIL
                        };
                    }
                }
                else
                {
                    // Update existing record
                    var isRoleTypeExist = existing!.FlowModel!.RoleTypeItems!.FirstOrDefault(x => x.RoleSource == source);
                    if (isRoleTypeExist == null)
                    {
                        // Add new
                        existing!.FlowModel!.RoleTypeItems!.Add(new DynamicItem
                        {
                            RoleSource = source,
                            RoleTargets = targets,
                            ModuleType = moduleType,
                            CrossInternal = isCross
                        });
                    }
                    else
                    {
                        var index = existing!.FlowModel!.RoleTypeItems!.IndexOf(isRoleTypeExist);
                        existing!.FlowModel!.RoleTypeItems[index].RoleTargets = targets;
                        existing!.FlowModel!.RoleTypeItems[index].ModuleType = moduleType;
                        existing!.FlowModel!.RoleTypeItems[index].RoleSource = source;
                        existing!.FlowModel!.RoleTypeItems[index].CrossInternal = isCross;
                    }

                    var request = existing!.FlowModel!;
                    var result = await couchContext.EditAsync<DynamicFlowModel>
                   (
                       couchDBHelper: write_dynamic_couchDbHelper,
                       model: request,
                       cancellationToken: cancellationToken
                   );

                    if (result.IsSuccess)
                    {
                        //Set cache after create success
                        //string recordKey = $"{RedisPrefix.Folder}{result.Id}"; // Set key for cache
                        //var mem = redisConnector.Connection.GetDatabase(1);
                        //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(request));
                        request.rev = result.Rev;
                        await context.UpdateAsync(request);


                        return new CommonResponseId()
                        {
                            Id = result.Id,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Success = true,
                            Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                            Message = ResultCode.SUCCESS_OPERATION
                        };
                    }
                    else
                    {
                        return new CommonResponseId()
                        {
                            Id = GeneratorHelper.NotAvailable,
                            Code = nameof(ResultCode.REQUEST_FAIL),
                            Success = false,
                            Detail = ValidateString.IsNullOrWhiteSpace(result.Error),
                            Message = ResultCode.REQUEST_FAIL
                        };
                    }
                }



            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<(CommonResponse Response, DynamicFlowModel FlowModel, IEnumerable<DynamicItem> Roles)> GetDynamicFlowByID(string id, ModuleType moduleType, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var provider = new RedisConnectionProvider(redisConnector.Connection);
                var context = provider.RedisCollection<DynamicFlowModel>();

                var cache = await context.FindByIdAsync(id);
                if (cache == null)
                {
                    // Find the existing record
                    var existing = await couchContext.GetAsync<DynamicFlowModel>
                        (
                            couchDBHelper: read_dynamic_couchDbHelper,
                            id: id,
                            cancellationToken: cancellationToken
                        );

                    if (!existing.IsSuccess)
                    {
                        return (new CommonResponseId()
                        {
                            Id = id,
                            Code = nameof(ResultCode.NOT_FOUND),
                            Success = false,
                            Detail = ValidateString.IsNullOrWhiteSpace(existing.Reason),
                            Message = ResultCode.NOT_FOUND
                        }, default!, default!);
                    }

                    existing.Content.rev = existing.Rev;
                    existing.Content.id = existing.Id;
                    //await mem.StringSetAsync(recordKey, JsonSerializer.Serialize(existing.Content));
                    await context.InsertAsync(existing.Content);

                    var response = existing.Content.RoleTypeItems.Where(x => x.ModuleType == moduleType);

                    return (new CommonResponseId()
                    {
                        Id = id,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Success = true,
                        Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                        Message = ResultCode.SUCCESS_OPERATION
                    }, existing.Content!, response);
                }

                //var result = JsonSerializer.Deserialize<OrganizationModel>(cache!);
                var result = cache!.RoleTypeItems!.Where(x => x.ModuleType == moduleType);
                return (new CommonResponseId()
                {
                    Id = id,
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Success = true,
                    Detail = ValidateString.IsNullOrWhiteSpace(ResultCode.SUCCESS_OPERATION),
                    Message = ResultCode.SUCCESS_OPERATION
                }, cache!, result!);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<(decimal RowCount, IEnumerable<RoleTreeModel> Contents, CommonResponse Response)> GetSupervisorRolesPosition(string id, CancellationToken cancellationToken = default)
        {
            var organization = await GetOrganization(id, cancellationToken);

            var result = organization.Content.Chart.Where(x => x.RoleType == RoleTypeModel.InboundGeneral ||
            x.RoleType == RoleTypeModel.InboundOfficePrime || x.RoleType == RoleTypeModel.InboundPrime ||
            x.RoleType == RoleTypeModel.OutboundGeneral || x.RoleType == RoleTypeModel.OutboundOfficePrime ||
            x.RoleType == RoleTypeModel.OutboundPrime);

            return (result.Count(), result, result.Count() > 0 ? new CommonResponse
            {
                Code = nameof(ResultCode.SUCCESS_OPERATION),
                Success = true,
                Detail = ResultCode.SUCCESS_OPERATION,
                Message = ResultCode.SUCCESS_OPERATION
            } : new CommonResponse
            {
                Code = nameof(ResultCode.NOT_FOUND),
                Success = false,
                Detail = ResultCode.NOT_FOUND,
                Message = ResultCode.NOT_FOUND
            });
        }
    }
}
