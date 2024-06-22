using MediatR;

namespace MRogalski.SplitLoot.Features.SplitLoot;

internal sealed class SplitLootRequest : IRequest<SplitLootResponse>
{
    public required ulong CallerId { get; set; }
    public required string Clipboard { get; set; } = string.Empty;
}
