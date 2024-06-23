using MediatR;

using System.Text.RegularExpressions;

namespace MRogalski.SplitLoot.Features.SplitLoot;

internal sealed class SplitLootHandler : IRequestHandler<SplitLootRequest, SplitLootResponse>
{
    private sealed class Player
    {
        public string Name { get; set; } = string.Empty;
        public long Loot { get; set; }
        public long Supplies { get; set; }
        public long Balance { get; set; }
        public long DistributedLoot { get; set; }
    }

    private sealed class Session
    {
        public long Balance { get; set; }
        public long IndividualBalance { get; set; }
        public List<Player> Players { get; set; } = [];
    }

    private sealed class Transfer
    {
        public string FromPlayer { get; set; } = string.Empty;
        public string ToPlayer { get; set; } = string.Empty;
        public long Amount { get; set; }
    }

    private static long ParseNumericValue(string numericValue)
    {
        return long.Parse(Regex.Replace(numericValue, ",", ""));
    }

    private Session ParseSessionData(string sessionData)
    {
        const string playerPattern = @"(?<=\d{1}\s{1})(?'player'(?'name'(?:\s{0,1}\w{1,}){1,})(?:\s{1}\(Leader\)){0,1}(?'individual'\s{5}(?'type'Loot|Supplies|Balance|Damage|Healing):\s{1}(?'numericvalue'-{0,}(?:\d{1,3},{0,}){1,})){5})";

        var session = new Session();
        var playerMatches = Regex.Matches(sessionData, playerPattern, RegexOptions.Multiline);
        foreach (Match match in playerMatches)
        {
            var player = new Player
            {
                Name = match.Groups["name"].Value.Trim()
            };

            foreach (Capture capture in match.Groups["individual"].Captures)
            {
                var kv = capture.Value.Trim().Split(" ");
                switch (kv[0].ToLower())
                {
                    case "loot:": player.Loot = ParseNumericValue(kv[1]); break;
                    case "supplies:": player.Supplies = ParseNumericValue(kv[1]); break;
                    case "balance:": player.Balance = ParseNumericValue(kv[1]); break;
                };
            }
            session.Balance += player.Balance;

            session.Players.Add(player);
        }

        session.IndividualBalance = session.Balance / session.Players.Count;

        foreach (var player in session.Players)
        {
            player.DistributedLoot = session.IndividualBalance - player.Balance;
        }

        return session;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<SplitLootResponse> Handle(SplitLootRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var transfers = new List<Transfer>();
            var session = ParseSessionData(request.Clipboard);

            var playersToSend = session.Players.Where(p => p.DistributedLoot < 0).OrderBy(p => p.DistributedLoot).ToList();
            var playersToReceive = session.Players.Where(p => p.DistributedLoot > 0).OrderByDescending(p => p.DistributedLoot).ToList();

            int i = 0, j = 0;
            while (i < playersToSend.Count && j < playersToReceive.Count)
            {
                var sender = playersToSend[i];
                var receiver = playersToReceive[j];
                var amount = Math.Min(-sender.DistributedLoot, receiver.DistributedLoot);

                transfers.Add(new()
                {
                    FromPlayer = sender.Name,
                    ToPlayer = receiver.Name,
                    Amount = amount
                });

                sender.DistributedLoot += amount;
                receiver.DistributedLoot -= amount;

                if (sender.DistributedLoot == 0) i++;
                if (receiver.DistributedLoot == 0) j++;
            }

            return new SplitLootResponse
            {
                Transfers = transfers
                    .GroupBy(p => p.FromPlayer)
                    .ToDictionary(
                        g => g.Key, 
                        g => g.Select(t =>
                            new SplitLootTranfer
                            {
                                Amount = t.Amount,
                                SendTo = t.ToPlayer
                            }))
            };
        }
        catch (Exception ex)
        {
            return SplitLootResponse.FromError(ex);
        }
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
