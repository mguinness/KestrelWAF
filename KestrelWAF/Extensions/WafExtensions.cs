using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KestrelWAF.Extensions;

public static class WafExtensions
{
    public static IServiceCollection AddWebFirewall(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Rule>(opt => configuration.GetSection("Configuration:Ruleset").Bind(opt));

        services.AddSingleton(new MaxMindDb(configuration.GetValue<string>("Configuration:GeoLiteFile")));

        services.AddReverseProxy().LoadFromConfig(configuration.GetSection("ReverseProxy"));

        services.AddMemoryCache();

        return services;
    }

    public static IApplicationBuilder UseWebFirewall(this IApplicationBuilder app)
    {
        app.UseMiddleware<BlockIpMiddleware>();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapReverseProxy();
        });

        return app;
    }
}