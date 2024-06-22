using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MRogalski.SplitLoot.Extensions;
using MRogalski.SplitLoot.Options;

using System.Reflection;

namespace MRogalski.SplitLoot;

internal class DiscordClientWorker : IHostedService
{
    private readonly ILogger<DiscordClientWorker> _logger;
    private readonly DiscordOptions _options;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    public DiscordClientWorker(IServiceProvider serviceProvider, ILogger<DiscordClientWorker> logger, DiscordSocketClient client, InteractionService interactionService)
    {
        _logger = logger;
        _options = new DiscordOptions();
        _client = client;
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
    }

    private async Task InitializeAsync()
    {
        await _client.CurrentUser.ModifyAsync(props =>
            props.Username = "SplitLoot");

        await _client.SetCustomStatusAsync("Hey! Psst.. wanna calculate your loot?");

        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);
        await _interactionService.RegisterCommandsGloballyAsync(true);
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        _logger.LogInformation("Interaction received");
        var ctx = new SocketInteractionContext(_client, interaction);
        var result = await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
        if (result.Error.HasValue)
        {
            _logger.LogError("Error while executing interaction: {error}",result.Error.Value);
        }
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async Task SuspendAsync()
    {
        _client.Ready -= InitializeAsync;
        _client.Log -= _logger.LogDiscordEventAsync;
        _interactionService.Log -= _logger.LogDiscordEventAsync;
        _client.InteractionCreated -= HandleInteractionAsync;
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Log += _logger.LogDiscordEventAsync;
        _interactionService.Log += _logger.LogDiscordEventAsync;
        _client.Ready += InitializeAsync;
        _client.InteractionCreated += HandleInteractionAsync;

        await _client.LoginAsync(TokenType.Bot, _options.AuthToken);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await SuspendAsync();

        await _client.LogoutAsync();
        await _client.StopAsync();
    }
}
