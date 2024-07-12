using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MRogalski.SplitLoot.Features.SplitLoot;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MRogalski.SplitLoot.Services
{
    internal class DiscordMessageService : IDiscordMessageService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly ILogger<DiscordInteractionService> _logger;

        private readonly IServiceProvider _serviceProvider;

        public DiscordMessageService(DiscordSocketClient client, CommandService commands, ILogger<DiscordInteractionService> logger)
        {
            _client = client;
            _commands = commands;
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
            await _commands.AddModuleAsync<SplitLootCommand>(_serviceProvider);
            
            _client.MessageReceived += HandleMessageAsync;
        }

        public async Task SuspendAsync()
        {
            await _commands.RemoveModuleAsync<SplitLootCommand>();

            _client.MessageReceived -= HandleMessageAsync;
        }

        private async Task HandleMessageAsync(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message || message.Author.IsBot) return;

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context: context, argPos: 0, services: null);

            var request = new SplitLootRequest
            {
                CallerId = message.Author.Id,
                Clipboard = Regex.Replace(message.Content, @"(\r\n|\r|\n)", " ")
            };
        }
    }
}
