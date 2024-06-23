using Discord.Interactions;

using MediatR;

using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MRogalski.SplitLoot.Features.SplitLoot;

public sealed class SplitLootInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;
    private readonly ILogger<SplitLootInteraction> _logger;

    public SplitLootInteraction(IMediator mediator, ILogger<SplitLootInteraction> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [CommandContextType(Discord.InteractionContextType.Guild)]
    [SlashCommand("splitloot", "Calculates per player profit out of party hunt analyzer session data", runMode: RunMode.Async)]
    public async Task SplitLootInteractionAsync([Summary("session", "Party hunt session")] string session)
    {
        await DeferAsync();

        var request = new SplitLootRequest
        {
            CallerId = Context.User.Id,
            Clipboard = Regex.Replace(session, @"(\r\n|\r|\n)", " ")
        };

        var response = await _mediator.Send(request);
        if (!string.IsNullOrEmpty(response.Error))
        {
            await FollowupAsync(response.Error);
        }
        else
        {
            await FollowupAsync(embed: new SplitLootEmbed().Build(response));
        }
    }
}
