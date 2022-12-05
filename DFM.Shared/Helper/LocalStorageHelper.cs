using DFM.Shared.Entities;
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
                var base64EncodedBytes = Convert.FromBase64String(value);
                return JsonSerializer.Deserialize<EmployeeModel>(base64EncodedBytes)!;
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
                var jsonDoc = JsonSerializer.Serialize(value);
                var plainTextBytes = Encoding.UTF8.GetBytes(jsonDoc);
                await jsRuntime.InvokeAsync<object>("localStorage.setItem",
                                                       "employeeProfile", Convert.ToBase64String(plainTextBytes));
            }
            catch (Exception)
            {

                throw;
            }
            
        }
    }
}
