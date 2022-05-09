namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>A monster entry parsed from the game data.</summary>
    /// <param name="Name">The monster name.</param>
    /// <param name="Health">The monster's health points.</param>
    /// <param name="DamageToFarmer">The damage points the monster afflicts on the player.</param>
    /// <param name="IsGlider">Whether the monster can fly.</param>
    /// <param name="DurationOfRandomMovements">The amount of time between random movement changes (in milliseconds).</param>
    /// <param name="Resilience">The monster's damage resistance.</param>
    /// <param name="Jitteriness">The probability that a monster will randomly change direction when checked.</param>
    /// <param name="MoveTowardsPlayerThreshold">The tile distance within which the monster will begin moving towards the player.</param>
    /// <param name="Speed">The speed at which the monster moves.</param>
    /// <param name="MissChance">The probability that the player will miss when attacking this monster.</param>
    /// <param name="IsMineMonster">Whether the monster appears in the mines.</param>
    /// <param name="Drops">The items dropped by this monster and their probability to drop.</param>
    internal record MonsterData(
        string Name,
        int Health,
        int DamageToFarmer,
        bool IsGlider,
        int DurationOfRandomMovements,
        int Resilience,
        double Jitteriness,
        int MoveTowardsPlayerThreshold,
        int Speed,
        double MissChance,
        bool IsMineMonster,
        ItemDropData[] Drops
    );
}
