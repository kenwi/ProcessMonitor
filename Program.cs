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
        
        var streams = new List<Process>();
        services.AddSingleton(streams);

        services.AddSingleton<ProcessService>();
        services.AddSingleton(configuration);
        services.AddScoped<IMessageWriter, MessageWriter>();
        services.AddHostedService<MonitoringService>();
    })
    .Build()
    .RunAsync();

//messageWriter.Write("Monitoring");
//Enumerable.Range(1, 5)
//    .ToList()
//    .ForEach(async page => {
//        var nodes = await GetOnlineUserNodes("female", page);
//        var usernames = nodes.Select(n => n.InnerText);
//        messageWriter.Write($"Online users {page}: {nodes.Length}");
//    });