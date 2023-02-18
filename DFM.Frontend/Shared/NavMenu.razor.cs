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
            await OpenDrawer.InvokeAsync();
        }
        string token = "";
        bool isAdmin;
        
    }
}
