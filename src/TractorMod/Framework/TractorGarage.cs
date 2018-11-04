using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The tractor garage building.</summary>
    internal class TractorGarage : Stable
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tractorID">The unique ID for the associated tractor.</param>
        /// <param name="blueprint">The blueprint from which to construct a building.</param>
        /// <param name="tile">The tile containing the top-left corner of the building.</param>
        /// <param name="daysOfConstructionLeft">The number of days until the building is constructed.</param>
        public TractorGarage(Guid tractorID, BluePrint blueprint, Vector2 tile, int daysOfConstructionLeft)
            : base(tractorID, blueprint, tile)
        {
            this.daysOfConstructionLeft.Value = daysOfConstructionLeft;
        }

        /// <summary>Perform initial load logic.</summary>
        /// <remarks>This overrides the stable logic to avoid spawning a horse.</remarks>
        public override void load() { }

        /// <summary>Perform logic when the day starts.</summary>
        /// <param name="dayOfMonth">The current day of month.</param>
        /// <remarks>This overrides the stable logic to avoid spawning a horse.</remarks>
        public override void dayUpdate(int dayOfMonth)
        {
            // allow instant build with CJB Cheats Menu (though tractor will appear on the next day)
            if (this.daysOfConstructionLeft.Value > 0)
                this.daysOfConstructionLeft.Value--;
        }
    }
}
