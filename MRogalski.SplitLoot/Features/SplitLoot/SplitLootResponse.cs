namespace MRogalski.SplitLoot.Features.SplitLoot;

internal sealed class SplitLootResponse
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
}

internal sealed class SplitLootTranfer
{
    public string SendTo { get; set; } = string.Empty;
    public required long Amount { get; set; }
}
