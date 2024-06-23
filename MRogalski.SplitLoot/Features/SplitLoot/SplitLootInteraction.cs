using Discord.Interactions;

using MediatR;

using Microsoft.Extensions.Logging;

using System.Diagnostics;

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
    [SlashCommand("splitloot", "Calculates per player profit out of party hunt analyzer")]
    public async Task SplitLootInteractionAsync([Summary("clipboard", "Party hunt analyzer")] string clipboard)
    {
        await DeferAsync();

        var request = new SplitLootRequest
        {
            CallerId = Context.User.Id,
            Clipboard = clipboard
        };

        var sw = new Stopwatch();
        sw.Start();
        var response = await _mediator.Send(request);
        sw.Stop();
        _logger.LogInformation($"Executing 'SplitLootHandler' took {sw.ElapsedMilliseconds} milliseconds generating '{response}' response");

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
