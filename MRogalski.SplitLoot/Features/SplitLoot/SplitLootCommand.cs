using Discord.Commands;

using MediatR;

using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MRogalski.SplitLoot.Features.SplitLoot;

public sealed class SplitLootCommand
{
    private readonly IMediator _mediator;
    private readonly ILogger<SplitLootCommand> _logger;

    public SplitLootCommand(IMediator mediator, ILogger<SplitLootCommand> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [Command("splitloot")]
    [Alias("sl")]
    [Summary("Calculates per player profit out of party hunt analyzer session data")]
    public async Task SplitLootCommandAsync([Summary("Session data")] string session)
    {
        
    }
}
