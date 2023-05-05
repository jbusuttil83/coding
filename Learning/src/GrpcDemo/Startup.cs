using GrpcDemo.Service;

namespace GrpcDemo;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGrpc();
    }

    public void Configure(IApplicationBuilder appBuilder)
    {
        appBuilder.UseRouting();
        
        appBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Home Page"); });

            endpoints.MapGrpcService<BetService>();
        });
    }
}