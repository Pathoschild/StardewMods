using System.Linq;
using StardewValley;

namespace Pathoschild.LookupAnything
{
    /// <summary>The assumptions made by the mod.</summary>
    internal class Assumptions
    {
        /// <summary>Get whether an item can have a quality (which increases its sale price).</summary>
        /// <param name="item">The item.</param>
        public static bool CanHaveQuality(Object item)
        {
            // check category
            if (new[] { "Artifact", "Trash", "Crafting", "Seed", "Decor", "Resource", "Fertilizer", "Bait", "Fishing Tackle" }.Contains(item.getCategoryName()))
                return false;

            // check type
            if (new[] { "Crafting", "asdf"/*dig spots*/, "Quest" }.Contains(item.Type))
                return false;

            return true;
        }
    }
}
