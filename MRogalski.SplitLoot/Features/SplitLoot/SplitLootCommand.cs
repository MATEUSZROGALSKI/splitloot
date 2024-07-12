using Discord.Commands;

using MediatR;

using System.Text.RegularExpressions;

namespace MRogalski.SplitLoot.Features.SplitLoot
{
    internal class SplitLootCommand(IMediator mediator) : ModuleBase<SocketCommandContext>
    {
        private readonly IMediator _mediator = mediator;

        [Command("Session")]
        [Summary("Calculates per player profit out of party hunt analyzer session data.")]
        public async Task HandleAsync([Remainder][Summary("Party hunt session")] string session)
        {
            var request = new SplitLootRequest
            {
                CallerId = Context.User.Id,
                Clipboard = Regex.Replace(session, @"(\r\n|\r|\n)", " ")
            };

            var response = await _mediator.Send(request);
            if (!string.IsNullOrEmpty(response.Error))
            {
                await ReplyAsync(response.Error);
            }
            else
            {
                await ReplyAsync(embed: new SplitLootEmbed().Build(response));
            }
        }
    }
}
