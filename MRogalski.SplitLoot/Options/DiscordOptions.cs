﻿namespace MRogalski.SplitLoot.Options;

public sealed record class DiscordOptions
{
    public string AuthToken => Environment.GetEnvironmentVariable("DISCORD_AUTH_TOKEN");
}

