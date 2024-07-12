using Discord.WebSocket;

namespace MRogalski.SplitLoot.Services
{
    internal interface IDiscordMessageService
    {
        Task InitializeAsync();
        Task SuspendAsync();
    }
}
