﻿using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public interface ICascadingService
    {
        Task<(bool Success, EmployeeModel Content)> GetEmployeeProfile(string token, string userId, CancellationToken cancellationToken = default);
        Task<(bool Success, EmployeeModel Content)> GetEmployeeProfile(string token, CancellationToken cancellationToken = default);
        Task<(bool Success, IEnumerable<TabItemDto> Contents)> GetRoles(string token, CancellationToken cancellationToken = default);
        Task<(bool Success, IEnumerable<RuleMenu> Contents)> GetRules(string token, CancellationToken cancellationToken = default);
    }
    public class CascadingService : ICascadingService
    {
        private readonly IHttpService httpService;
        private readonly ServiceEndpoint endpoint;

        public CascadingService(IHttpService httpService, ServiceEndpoint endpoint)
        {
            this.httpService = httpService;
            this.endpoint = endpoint;
        }

        public async Task<(bool Success, EmployeeModel Content)> GetEmployeeProfile(string token, string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            string url = $"{endpoint.API}/api/v1/Employee/GetItem";

            var result = await httpService.Get<EmployeeModel>(url, new AuthorizeHeader("bearer", token), cancellationToken);
            return (result.Success, result.Response);
        }
        public async Task<(bool Success, EmployeeModel Content)> GetEmployeeProfile(string token, CancellationToken cancellationToken = default(CancellationToken))
        {
            string url = $"{endpoint.API}/api/v1/Employee/GetItem";

            var result = await httpService.Get<EmployeeModel>(url, new AuthorizeHeader("bearer", token), cancellationToken);
            return (result.Success, result.Response);
        }

        public async Task<(bool Success, IEnumerable<TabItemDto> Contents)> GetRoles(string token, CancellationToken cancellationToken = default(CancellationToken))
        {
            string url = $"{endpoint.API}/api/v1/Organization/GetRole";

            var result = await httpService.Get<IEnumerable<TabItemDto>>(url, new AuthorizeHeader("bearer", token));
            return (result.Success, result.Response);
        }

        public async Task<(bool Success, IEnumerable<RuleMenu> Contents)> GetRules(string token, CancellationToken cancellationToken = default(CancellationToken))
        {
            string url = $"{endpoint.API}/api/v1/RuleMenu/GetItem";

            var result = await httpService.Get<IEnumerable<RuleMenu>>(url, new AuthorizeHeader("bearer", token));
            return (result.Success, result.Response);
        }
    }
}
