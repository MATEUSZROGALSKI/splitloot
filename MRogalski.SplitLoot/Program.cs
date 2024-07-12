using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MRogalski.SplitLoot;
using MRogalski.SplitLoot.Services;

using System.Reflection;

var builder = Host.CreateApplicationBuilder();

builder.Configuration.AddConfiguration(
    new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddUserSecrets(Assembly.GetExecutingAssembly())
        .Build());

builder.Logging.ClearProviders()
    .AddConsole()
    .SetMinimumLevel(LogLevel.Trace);

var discordSocketClient = new DiscordSocketClient(new DiscordSocketConfig()
{
    UseInteractionSnowflakeDate = false,
    LogLevel = Discord.LogSeverity.Info
});

builder.Services
    .AddMediatR(config =>
        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
    .AddSingleton(discordSocketClient)
    .AddSingleton(new InteractionService(discordSocketClient, new()
    {
        DefaultRunMode = Discord.Interactions.RunMode.Async,
        UseCompiledLambda = true,
        LogLevel = Discord.LogSeverity.Info,
    }))
    .AddSingleton(new CommandService(new()
    {
        DefaultRunMode = Discord.Commands.RunMode.Async,
        CaseSensitiveCommands = true,
        LogLevel = Discord.LogSeverity.Info
    }))
    .AddSingleton<IDiscordInteractionService, DiscordInteractionService>()
    .AddSingleton<IDiscordMessageService, DiscordMessageService>()
    .AddHostedService<DiscordClientWorker>();

builder.Build().Run();
