using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

await Host.CreateDefaultBuilder(Environment.GetCommandLineArgs())
    .ConfigureServices(services =>
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: true)
            .AddCommandLine(Environment.GetCommandLineArgs())
            .Build();
        
        services.AddSingleton<ProcessService>();
        services.AddSingleton(configuration);
        services.AddScoped<IMessageWriter, MessageWriter>();
        services.AddHostedService<MonitoringService>();
    })
    .Build()
    .RunAsync();
