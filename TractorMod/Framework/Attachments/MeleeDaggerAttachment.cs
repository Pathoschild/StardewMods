using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for melee dagger weapons.</summary>
    internal class MeleeDaggerAttachment : BaseAttachment
    {
        /*********
        ** Fields
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly MeleeDaggerConfig Config;




        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The attachment settings.</param>
        /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public MeleeDaggerAttachment(MeleeDaggerConfig config, IModRegistry modRegistry, IReflectionHelper reflection)
            : base(modRegistry, reflection)
        {
            this.Config = config;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(Farmer player, Tool? tool, Item? item, GameLocation location)
        {
            return tool is MeleeWeapon { type.Value: MeleeWeapon.dagger };
        }

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool Apply(Vector2 tile, SObject? tileObj, TerrainFeature? tileFeature, Farmer player, Tool? tool, Item? item, GameLocation location)
        {
            tool = tool.AssertNotNull();

            // clear dead crops
            if (this.Config.ClearDeadCrops && this.TryClearDeadCrop(location, tile, tileFeature, player))
                return true;

            // break mine containers
            if (this.Config.BreakMineContainers && this.TryBreakContainer(tile, tileObj, tool, location))
                return true;

            // harvest grass
            if (this.Config.HarvestGrass && this.TryHarvestGrass(tileFeature as Grass, location, tile))
                return true;

            // attack monsters
            if (this.Config.AttackMonsters && this.UseWeaponOnTile((MeleeWeapon)tool, tile, player, location))
                return false;

            return false;
        }
    }
}
