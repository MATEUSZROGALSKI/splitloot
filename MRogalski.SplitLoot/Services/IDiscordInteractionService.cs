using Discord.WebSocket;

namespace MRogalski.SplitLoot.Services
{
    internal interface IDiscordInteractionService
    {
        Task InitializeAsync();
        Task SuspendAsync();
    }
}
