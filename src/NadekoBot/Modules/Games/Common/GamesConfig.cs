﻿using Cloneable;
using NadekoBot.Common;
using NadekoBot.Common.Yml;

namespace NadekoBot.Modules.Games.Common;

[Cloneable]
public sealed partial class GamesConfig : ICloneable<GamesConfig>
{
    [Comment("DO NOT CHANGE")]
    public int Version { get; set; }

    [Comment("Hangman related settings (.hangman command)")]
    public HangmanConfig Hangman { get; set; } = new HangmanConfig()
    {
        CurrencyReward = 0
    };
        
    [Comment("Trivia related settings (.t command)")]
    public TriviaConfig Trivia { get; set; } = new TriviaConfig()
    {
        CurrencyReward = 0,
        MinimumWinReq = 1,
    };

    [Comment("List of responses for the .8ball command. A random one will be selected every time")]
    public List<string> EightBallResponses { get; set; } = new List<string>()
    {
        "Most definitely yes.",
        "For sure.",
        "Totally!",
        "Of course!",
        "As I see it, yes.",
        "My sources say yes.",
        "Yes.",
        "Most likely.",
        "Perhaps...",
        "Maybe...",
        "Hm, not sure.",
        "It is uncertain.",
        "Ask me again later.",
        "Don't count on it.",
        "Probably not.",
        "Very doubtful.",
        "Most likely no.",
        "Nope.",
        "No.",
        "My sources say no.",
        "Don't even think about it.",
        "Definitely no.",
        "NO - It may cause disease contraction!"
    };

    [Comment("List of animals which will be used for the animal race game (.race)")]
    public List<RaceAnimal> RaceAnimals { get; set; } = new List<RaceAnimal>()
    {
        new RaceAnimal { Icon = "🐼", Name = "Panda" },
        new RaceAnimal { Icon = "🐻", Name = "Bear" },
        new RaceAnimal { Icon = "🐧", Name = "Pengu" },
        new RaceAnimal { Icon = "🐨", Name = "Koala" },
        new RaceAnimal { Icon = "🐬", Name = "Dolphin" },
        new RaceAnimal { Icon = "🐞", Name = "Ladybird" },
        new RaceAnimal { Icon = "🦀", Name = "Crab" },
        new RaceAnimal { Icon = "🦄", Name = "Unicorn" }
    };
}

[Cloneable]
public sealed partial class HangmanConfig
{
    [Comment("The amount of currency awarded to the winner of a hangman game")]
    public long CurrencyReward { get; set; }
}

[Cloneable]
public sealed partial class TriviaConfig
{
    [Comment("The amount of currency awarded to the winner of the trivia game.")]
    public long CurrencyReward { get; set; } = 0;

    [Comment(@"Users won't be able to start trivia games which have 
a smaller win requirement than the one specified by this setting.")]
    public int MinimumWinReq { get; set; } = 1;
}

[Cloneable]
public sealed partial class RaceAnimal
{
    public string Icon { get; set; }
    public string Name { get; set; }
}