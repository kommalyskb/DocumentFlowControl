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
                var hexEncodedBytes = value.FromHEX();
                return JsonSerializer.Deserialize<EmployeeModel>(hexEncodedBytes)!;
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
                value.Username = "";
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
    }
}
