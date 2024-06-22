using Discord.Interactions;

using MediatR;

namespace MRogalski.SplitLoot.Features.SplitLoot;

public sealed class SplitLootInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;

    public SplitLootInteraction(IMediator mediator)
    {
        _mediator = mediator;
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
