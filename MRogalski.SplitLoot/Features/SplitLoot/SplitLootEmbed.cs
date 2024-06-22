using Discord;

using System.Text;

namespace MRogalski.SplitLoot.Features.SplitLoot;

internal sealed class SplitLootEmbed
{
    public Embed Build(SplitLootResponse response)
    {
        var builder = new EmbedBuilder();
        builder.WithTimestamp(DateTimeOffset.UtcNow);
        builder.WithTitle("Split loot instructions");


        var stringBuilder = new StringBuilder();
        foreach (var key in response.Transfers!.Keys)
        {
            stringBuilder.AppendLine($"▶ {key}:");
            foreach (var transfer in response.Transfers[key])
            {
                stringBuilder.AppendLine($"\t`transfer {transfer.Amount} to {transfer.SendTo}`");
            }
            stringBuilder.AppendLine($"———————————————");
        }

        builder.Description = stringBuilder.ToString();

        return builder.Build();
    }

}
