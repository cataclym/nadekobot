﻿namespace Nadeko.Econ.Gambling;

public sealed class WofGame
{
    public static IReadOnlyList<decimal> DEFAULT_MULTIPLIERS = new[] { 1.7M, 1.5M, 0.2M, 0.1M, 0.3M, 0.5M, 1.2M, 2.4M };
    
    private readonly IReadOnlyList<decimal> _multipliers;
    private readonly NadekoRandom _rng;

    public WofGame(IReadOnlyList<decimal> multipliers)
    {
        _multipliers = multipliers;
        _rng = new();
    }

    public WofGame() : this(DEFAULT_MULTIPLIERS)
    {
    }

    public WofResult Spin(long bet)
    {
        var result = _rng.Next(0, _multipliers.Count);

        var multi = _multipliers[result];
        var amount = bet * multi;

        return new()
        {
            Index = result,
            Multiplier = multi,
            Won = amount
        };
    }
}