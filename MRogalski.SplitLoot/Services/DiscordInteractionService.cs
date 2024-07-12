using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MRogalski.SplitLoot.Features.SplitLoot;

using System.Reflection;

namespace MRogalski.SplitLoot.Services
{
    internal class DiscordInteractionService : IDiscordInteractionService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly ILogger<DiscordInteractionService> _logger;

        private readonly IServiceProvider _serviceProvider;

        public DiscordInteractionService(DiscordSocketClient client, InteractionService interactionService, ILogger<DiscordInteractionService> logger)
        {
            _client = client;
            _interactionService = interactionService;
            _logger = logger;
            _serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders()
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Debug);
                })
                .AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
                .BuildServiceProvider();
        }

        public async Task InitializeAsync()
        {
            await _interactionService.AddModuleAsync<SplitLootInteraction>(_serviceProvider);
            await _interactionService.RegisterCommandsGloballyAsync(true);

            _client.InteractionCreated += HandleInteractionAsync;
        }

        public async Task SuspendAsync()
        {
            await _interactionService.RemoveModuleAsync<SplitLootInteraction>();
            
            _client.InteractionCreated -= HandleInteractionAsync;
        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            _logger.LogInformation("Interaction received");
            var ctx = new SocketInteractionContext(_client, interaction);
            var result = await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
            if (result.Error.HasValue)
            {
                _logger.LogError("Error while executing interaction: {error}", result.Error.Value);
            }
        }
    }
}
