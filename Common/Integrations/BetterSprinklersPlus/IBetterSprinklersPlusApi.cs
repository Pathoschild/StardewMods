using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Common.Integrations.BetterSprinklersPlus
{
    /// <summary>The API provided by the Better Sprinklers mod.</summary>
    public interface IBetterSprinklersPlusApi
    {
        /// <summary>Get the maximum supported coverage width or height.</summary>
        int GetMaxGridSize();

        /// <summary>Get the relative tile coverage by supported sprinkler ID.</summary>
        IDictionary<int, Vector2[]> GetSprinklerCoverage();
    }
}
