using Microsoft.AspNetCore.HttpOverrides;

namespace DFM.Frontend
{
    public static class StartupHelper
    {
        public static void AddForwardHeaders(this IApplicationBuilder app)
        {
            var forwardingOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false
            };

            forwardingOptions.KnownNetworks.Clear();
            forwardingOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardingOptions);
        }
    }
}
