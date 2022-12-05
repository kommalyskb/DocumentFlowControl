using Microsoft.AspNetCore.Components;

namespace DFM.Frontend.Shared
{
    public partial class NavMenu
    {

        [Parameter] public EventCallback OpenDrawer { get; set; }
        [Parameter] public bool IsOpen { get; set; }
        //[Inject] AccessTokenStorage accessToken { get; set; }
        private async Task Open()
        {
            OpenDrawer.InvokeAsync();
        }
        string token = "";
        bool isAdmin;
        protected override async Task OnInitializedAsync()
        {
            //token = await accessToken.GetTokenAsync();
            //var tokenHandler = new JwtSecurityTokenHandler();
            //var jwtSecurityToken = tokenHandler.ReadJwtToken(token);

            //if (jwtSecurityToken.Claims.Where(x => x.Type == "role").FirstOrDefault() != null)
            //{
            //    isAdmin = jwtSecurityToken.Claims.Where(x => x.Type == "role").FirstOrDefault().Value == "Telbiz-Admin";
            //    await InvokeAsync(StateHasChanged);
            //}
        }
    }
}
