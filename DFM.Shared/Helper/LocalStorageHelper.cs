using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public class LocalStorageHelper
    {
        private readonly IJSRuntime jsRuntime;

        public LocalStorageHelper(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public async Task<EmployeeModel> GetEmployeeProfileAsync()
        {
            try
            {
                var value = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "employeeProfile");
                if (value != null)
                {
                    var hexEncodedBytes = value.FromHEX();
                    return JsonSerializer.Deserialize<EmployeeModel>(hexEncodedBytes)!;
                }
                return null!;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<IEnumerable<TabItemDto>> GetRolesAsync()
        {
            try
            {
                var value = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "roles");
                if (value != null)
                {
                    var hexEncodedBytes = value.FromHEX();
                    return JsonSerializer.Deserialize<IEnumerable<TabItemDto>>(hexEncodedBytes)!;
                }
                return null!;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public async Task<IEnumerable<RuleMenu>> GetRuleMenuAsync()
        {
            try
            {
                var value = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "rules");
                if (value != null)
                {
                    var hexEncodedBytes = value.FromHEX();
                    return JsonSerializer.Deserialize<IEnumerable<RuleMenu>>(hexEncodedBytes)!;
                }
                return null!;
            }
            catch (Exception)
            {

                throw;
            }

        }


        public async Task SetEmployeeProfileAsync(EmployeeModel value)
        {
            try
            {
                value.Password = "";
                var jsonDoc = JsonSerializer.Serialize(value);
                Console.WriteLine(jsonDoc);
                var plainTextBytes = Encoding.UTF8.GetBytes(jsonDoc).ToHEX();
                Console.WriteLine(plainTextBytes);
                await jsRuntime.InvokeAsync<object>("localStorage.setItem",
                                                       "employeeProfile", plainTextBytes);
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task SetRolesAsync(IEnumerable<TabItemDto> value)
        {
            try
            {
                var jsonDoc = JsonSerializer.Serialize(value);
                Console.WriteLine(jsonDoc);
                var plainTextBytes = Encoding.UTF8.GetBytes(jsonDoc).ToHEX();
                Console.WriteLine(plainTextBytes);
                await jsRuntime.InvokeAsync<object>("localStorage.setItem",
                                                       "roles", plainTextBytes);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task SetRuleMenuAsync(IEnumerable<RuleMenu> value)
        {
            try
            {
                var jsonDoc = JsonSerializer.Serialize(value);
                Console.WriteLine(jsonDoc);
                var plainTextBytes = Encoding.UTF8.GetBytes(jsonDoc).ToHEX();
                Console.WriteLine(plainTextBytes);
                await jsRuntime.InvokeAsync<object>("localStorage.setItem",
                                                       "rules", plainTextBytes);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task RemoveCacheAsync()
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", "rules");
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", "roles");
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", "employeeProfile");
        }
    }
}
