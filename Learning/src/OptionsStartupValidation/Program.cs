using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        //this will bind the configuration section from appSettings.json to an IOptions  and do validation on startup
        services.AddOptions<WeatherProviderConfiguration>()
            .BindConfiguration("ProviderSettings")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        //this will enable us to inject our strongly typed object directly in our services
        //doing it like this will decouple our business services from the IOptions interface
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<WeatherProviderConfiguration>>().Value);
        services.AddHostedService<WeatherService>();
        
    }).Build();

host.StartAsync();


public class WeatherProviderConfiguration
{
    [Required] //we are using DataAnnotations from .net. In this case we are not depending on other libraries such as FluentValidation
    public short PollingIntervalMins { get; set; }
    
    public bool IsEnabled { get; set; }
    
    [Required, Url]
    public string ProviderUrl { get; set; }
    
    [Required]
    public string ApiKey { get; set; }
}

public class WeatherService : IHostedService
{
    private readonly WeatherProviderConfiguration _configuration;

    public WeatherService(WeatherProviderConfiguration configuration) //a strongly typed object is directly injected here without IOptions
    {
        _configuration = configuration;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        LoadConfiguration(_configuration);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    private static void LoadConfiguration(WeatherProviderConfiguration configuration)
    {
        Console.WriteLine(JsonSerializer.Serialize(configuration));
    }
}

//Reference: https://andrewlock.net/adding-validation-to-strongly-typed-configuration-objects-in-dotnet-6/