using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MRogalski.SplitLoot;

using System.Reflection;


var builder = Host.CreateApplicationBuilder();

var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

builder.Configuration.AddConfiguration(configuration);

builder.Logging.ClearProviders()
        .AddConsole()
        .SetMinimumLevel(LogLevel.Trace);

ConfigureDiscordClient(builder.Services);
var host = builder.Build();
host.Run();

void ConfigureDiscordClient(IServiceCollection services)
{
    var config = new DiscordSocketConfig()
    {
        UseInteractionSnowflakeDate = false,
        LogLevel = global::Discord.LogSeverity.Info
    };

    var discordSocketClient = new DiscordSocketClient(config);

    var interactionService = new InteractionService(discordSocketClient, new()
    {
        DefaultRunMode = RunMode.Async,
        UseCompiledLambda = true,
        LogLevel = global::Discord.LogSeverity.Info,
    });

    services
        .AddMediatR(config =>
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
        .AddSingleton(discordSocketClient)
        .AddSingleton(interactionService)
        .AddHostedService<DiscordClientWorker>();
}