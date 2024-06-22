using Discord;

using Microsoft.Extensions.Logging;

namespace MRogalski.SplitLoot.Extensions;

internal static class ILoggerExtension
{
    public static async Task LogDiscordEventAsync<T>(this ILogger<T> logger, LogMessage msg)
    {
        switch (msg.Severity)
        {
            case LogSeverity.Verbose:
                logger.LogInformation(msg.ToString());
                break;

            case LogSeverity.Info:
                logger.LogInformation(msg.ToString());
                break;

            case LogSeverity.Warning:
                logger.LogWarning(msg.ToString());
                break;

            case LogSeverity.Error:
                logger.LogError(msg.ToString());
                break;

            case LogSeverity.Critical:
                logger.LogCritical(msg.ToString());
                break;
        }
        await Task.CompletedTask;
    }
}