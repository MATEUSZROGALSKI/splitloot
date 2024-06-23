using Discord;
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

    private async Task SplitLootInternalAsync(string sessionData)
    {
        await DeferAsync();

        var request = new SplitLootRequest
        {
            CallerId = Context.User.Id,
            Clipboard = Regex.Replace(sessionData, @"\R", " ")
        };

        var sw = new Stopwatch();
        sw.Start();
        var response = await _mediator.Send(request);
        sw.Stop();
        _logger.LogInformation("Executing 'SplitLootHandler' took {elapsed} milliseconds generating '{response}' response", sw.ElapsedMilliseconds, response);

        if (!string.IsNullOrEmpty(response.Error))
        {
            await FollowupAsync(response.Error);
        }
        else
        {
            await FollowupAsync(embed: new SplitLootEmbed().Build(response));
        }
    }

    [CommandContextType(Discord.InteractionContextType.Guild | Discord.InteractionContextType.BotDm)]
    [SlashCommand("sl", "Calculates per player profit out of party hunt analyzer", runMode: RunMode.Async)]
    public async Task SLInteractionAsync([Summary("clipboard", "Party hunt analyzer")] string clipboard) => await SplitLootInternalAsync(clipboard);

    [CommandContextType(Discord.InteractionContextType.Guild | Discord.InteractionContextType.BotDm)]
    [SlashCommand("splitloot", "Calculates per player profit out of party hunt analyzer", runMode: RunMode.Async)]
    public async Task SplitLootInteractionAsync([Summary("clipboard", "Party hunt analyzer")] string clipboard) => await SplitLootInternalAsync(clipboard);
}
