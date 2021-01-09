using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Buildings
{
    /// <summary>Provides lookup data for in-game buildings.</summary>
    internal class BuildingLookupProvider : BaseLookupProvider
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Provides subject entries.</summary>
        private readonly ISubjectRegistry Codex;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="codex">Provides subject entries.</param>
        public BuildingLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, ISubjectRegistry codex)
            : base(reflection, gameHelper)
        {
            this.Codex = codex;
        }

        /// <inheritdoc />
        public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
        {
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    Vector2 origin = new Vector2(building.tileX.Value, building.tileY.Value + building.tilesHigh.Value);
                    if (this.GameHelper.CouldSpriteOccludeTile(origin, lookupTile, Constant.MaxBuildingTargetSpriteSize))
                        yield return new BuildingTarget(this.GameHelper, building, () => this.BuildSubject(building));
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Build a subject.</summary>
        /// <param name="building">The entity to look up.</param>
        private ISubject BuildSubject(Building building)
        {
            return new BuildingSubject(this.Codex, this.GameHelper, building, building.getSourceRectForMenu());
        }
    }
}
