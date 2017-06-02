using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace TractorMod.Framework
{
    /// <summary>The tractor garage building.</summary>
    internal class TractorGarage : Stable
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="blueprint">The blueprint from which to construct a building.</param>
        /// <param name="tile">The tile containing the top-left corner of the building.</param>
        /// <param name="daysOfConstructionLeft">The number of days until the building is constructed.</param>
        public TractorGarage(BluePrint blueprint, Vector2 tile, int daysOfConstructionLeft)
            : base(blueprint, tile)
        {
            this.daysOfConstructionLeft = daysOfConstructionLeft;
        }

        /// <summary>Perform logic when the day starts.</summary>
        /// <param name="dayOfMonth">The current day of month.</param>
        public override void dayUpdate(int dayOfMonth)
        {
            // prevent stable from grabbing horse
        }
    }
}
