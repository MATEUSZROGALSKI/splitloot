using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MRogalski.SplitLoot.Extensions;
using MRogalski.SplitLoot.Options;
using MRogalski.SplitLoot.Services;

namespace MRogalski.SplitLoot;

internal class DiscordClientWorker : IHostedService
{
    private readonly ILogger<DiscordClientWorker> _logger;
    private readonly DiscordOptions _options;
    private readonly DiscordSocketClient _client;
    private readonly IDiscordInteractionService _interactionService;
    private readonly IDiscordMessageService _messageService;

    public DiscordClientWorker(ILogger<DiscordClientWorker> logger, DiscordSocketClient client, IDiscordInteractionService interactionService, IDiscordMessageService messageService)
    {
        _logger = logger;
        _options = new DiscordOptions();
        _client = client;
        _interactionService = interactionService;
        _messageService = messageService;
    }

    private async Task InitializeAsync()
    {
        await _client.CurrentUser.ModifyAsync(props =>
            props.Username = "SplitLoot");

        await _client.SetCustomStatusAsync("Hey! Psst.. wanna calculate your loot?");

        await _interactionService.InitializeAsync();
        await _messageService.InitializeAsync();
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async Task SuspendAsync()
    {
        _client.Ready -= InitializeAsync;
        _client.Log -= _logger.LogDiscordEventAsync;

        await _interactionService.SuspendAsync();
        await _messageService.SuspendAsync();
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Log += _logger.LogDiscordEventAsync;
        _client.Ready += InitializeAsync;

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
