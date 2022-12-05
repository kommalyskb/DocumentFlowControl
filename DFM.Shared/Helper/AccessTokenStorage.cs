using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public class AccessTokenStorage
    {
        private readonly IJSRuntime _jsRuntime;

        public AccessTokenStorage(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string?> GetTokenAsync()
            => await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "accessToken");

        public async Task SetTokenAsync(string? token)
        {
            //if (token == null)
            //{
            //    await _jsRuntime.InvokeAsync<object>("localStorage.removeItem",
            //                                                    "accessToken");
            //}
            //else
            //{
            await _jsRuntime.InvokeAsync<object>("localStorage.setItem",
                                                   "accessToken", token);
            //}
        }
        public async Task<string?> GetRefreshTokenAsync()
            => await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "refreshToken");

        public async Task SetRefreshTokenAsync(string? refreshToken)
        {
            //if (refreshToken == null)
            //{
            //    await _jsRuntime.InvokeAsync<object>("localStorage.removeItem",
            //                                                    "refreshToken");
            //}
            //else
            //{
            await _jsRuntime.InvokeAsync<object>("localStorage.setItem",
                                                   "refreshToken", refreshToken);
            //}
        }
        public async Task RemoveTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "accessToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
        }

    }
}
