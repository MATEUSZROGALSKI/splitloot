using Discord.Interactions;

using MediatR;

using System.Text.RegularExpressions;

namespace MRogalski.SplitLoot.Features.SplitLoot;

public sealed class SplitLootInteraction(IMediator mediator) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator = mediator;

    [CommandContextType(Discord.InteractionContextType.Guild)]
    [SlashCommand("splitloot", "Calculates per player profit out of party hunt analyzer session data", runMode: RunMode.Async)]
    public async Task HandleAsync([Summary("session", "Party hunt session")] string session)
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
