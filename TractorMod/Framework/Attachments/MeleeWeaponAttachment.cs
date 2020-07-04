using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for melee weapons.</summary>
    internal class MeleeWeaponAttachment : BaseAttachment
    {
        /*********
        ** Fields
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly MeleeWeaponConfig Config;

        /// <summary>A fake pickaxe to use for clearing dead crops to ensure consistent behavior.</summary>
        private readonly Pickaxe FakePickaxe = new Pickaxe();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The attachment settings.</param>
        /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public MeleeWeaponAttachment(MeleeWeaponConfig config, IModRegistry modRegistry, IReflectionHelper reflection)
            : base(modRegistry, reflection)
        {
            this.Config = config;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(Farmer player, Tool tool, Item item, GameLocation location)
        {
            return tool is MeleeWeapon weapon && !weapon.isScythe();
        }

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, Farmer player, Tool tool, Item item, GameLocation location)
        {
            // clear dead crops
            if (this.Config.ClearDeadCrops && tileFeature is HoeDirt dirt && dirt.crop != null && dirt.crop.dead.Value)
                return this.UseToolOnTile(this.FakePickaxe, tile, player, location);

            // break mine containers
            if (this.Config.BreakMineContainers && tileObj is BreakableContainer container)
                return container.performToolAction(tool, location);

            // attack monsters
            if (this.Config.AttackMonsters)
                return this.UseWeaponOnTile((MeleeWeapon)tool, tile, player, location);

            return false;
        }
    }
}
