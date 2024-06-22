using MediatR;

using System.Text.RegularExpressions;

namespace MRogalski.SplitLoot.Features.SplitLoot;

internal sealed class SplitLootHandler : IRequestHandler<SplitLootRequest, SplitLootResponse>
{
    private sealed class Player
    {
        public string Name { get; set; } = string.Empty;
        public int Loot { get; set; }
        public int Supplies { get; set; }
        public int Balance { get; set; }
        public int DistributedLoot { get; set; }
    }

    private sealed class Session
    {
        public List<Player> Players { get; set; } = [];
    }

    private sealed class Transfer
    {
        public string FromPlayer { get; set; } = string.Empty;
        public string ToPlayer { get; set; } = string.Empty;
        public int Amount { get; set; }
    }

    private static int ParseNumericValue(string numericValue)
    {
        return int.Parse(Regex.Replace(numericValue, ",", ""));
    }

    private Session ParseSessionData(string sessionData)
    {
        Session session = new Session();

        const string playerPattern = @"(?<=\d{1}\s{1})(?'player'(?'name'(?:\s{0,1}\w{1,}){1,})(?:\s{1}\(Leader\)){0,1}(?'individual'\s{5}(?'type'Loot|Supplies|Balance|Damage|Healing):\s{1}(?'numericvalue'-{0,}(?:\d{1,3},{0,}){1,})){5})";

        MatchCollection playerMatches = Regex.Matches(sessionData, playerPattern, RegexOptions.Multiline);
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

            session.Players.Add(player);
        }

        session.LootType = "Leader";

        foreach (var player in session.Players)
        {
            player.Balance = player.Loot - player.Supplies;
        }

        int totalProfitLoss = session.Players.Sum(p => p.Balance);

        int equalShare = totalProfitLoss / session.Players.Count;

        foreach (var player in session.Players)
        {
            player.DistributedLoot = equalShare - player.Balance;
        }

        return session;
    }

    private List<Transfer> CalculateTransfers(Session session)
    {
        var transfers = new List<Transfer>();
        var playersToSend = session.Players.Where(p => p.DistributedLoot < 0).OrderBy(p => p.DistributedLoot).ToList();
        var playersToReceive = session.Players.Where(p => p.DistributedLoot > 0).OrderByDescending(p => p.DistributedLoot).ToList();

        int i = 0, j = 0;
        while (i < playersToSend.Count && j < playersToReceive.Count)
        {
            var sender = playersToSend[i];
            var receiver = playersToReceive[j];
            int amount = Math.Min(-sender.DistributedLoot, receiver.DistributedLoot);

            transfers.Add(new Transfer
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

        return transfers;
    }

    public async Task<SplitLootResponse> Handle(SplitLootRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var session = ParseSessionData(request.Clipboard);
            var transfers = CalculateTransfers(session);
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
}
