using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Solutions for hardcoded puzzles.</summary>
    /// <param name="IslandMermaidFluteBlockSequence">The sequence of flute block pitches for the <see cref="IslandSouthEast"/> mermaid music puzzle.</param>
    internal record PuzzleSolutionsData(int[] IslandMermaidFluteBlockSequence);
}
