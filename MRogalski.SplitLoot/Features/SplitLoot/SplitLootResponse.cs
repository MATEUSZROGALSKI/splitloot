using System.Text;

namespace MRogalski.SplitLoot.Features.SplitLoot;

internal sealed record class SplitLootResponse
{
    public IDictionary<string, IEnumerable<SplitLootTranfer>>? Transfers { get; set; }

    public string? Error { get; set; }

    public static SplitLootResponse FromError(Exception ex)
    {
        return new SplitLootResponse
        {
            Error = $"Error while calculating loot split: {ex.Message}"
        };
    }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Error))
        {
            return Error;
        }

        var sb = new StringBuilder();
        foreach (var key in Transfers!.Keys)
        {
            sb.AppendLine(key);
            foreach (var value in Transfers[key])
            {
                sb.AppendLine($"transfer {value.Amount} to {value.SendTo}");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}

internal sealed class SplitLootTranfer
{
    public string SendTo { get; set; } = string.Empty;
    public required long Amount { get; set; }
}
