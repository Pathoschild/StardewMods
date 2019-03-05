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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The attachment settings.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public MeleeWeaponAttachment(MeleeWeaponConfig config, IReflectionHelper reflection)
            : base(reflection)
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
            return tool is MeleeWeapon;
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
                return this.UseToolOnTile(tool, tile);

            // break mine containers
            if (this.Config.BreakMineContainers && tileObj is BreakableContainer container)
                return container.performToolAction(tool, location);

            // attack monsters
            if (this.Config.AttackMonsters)
            {
                MeleeWeapon weapon = (MeleeWeapon)tool;
                return this.UseWeaponOnTile(weapon, tile, player, location);
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Use a weapon on the given tile.</summary>
        /// <param name="weapon">The weapon to use.</param>
        /// <param name="tile">The tile to attack.</param>
        /// <param name="player">The current player.</param>
        /// <param name="location">The current location.</param>
        /// <remarks>This is a simplified version of <see cref="MeleeWeapon.DoDamage"/>. This doesn't account for player bonuses (since it's hugely overpowered anyway), doesn't cause particle effects, doesn't trigger animation timers, etc.</remarks>
        private bool UseWeaponOnTile(MeleeWeapon weapon, Vector2 tile, Farmer player, GameLocation location)
        {
            bool attacked = location.damageMonster(
                areaOfEffect: this.GetAbsoluteTileArea(tile),
                minDamage: weapon.minDamage.Value,
                maxDamage: weapon.maxDamage.Value,
                isBomb: false,
                knockBackModifier: weapon.knockback.Value,
                addedPrecision: weapon.addedPrecision.Value,
                critChance: weapon.critChance.Value,
                critMultiplier: weapon.critMultiplier.Value,
                triggerMonsterInvincibleTimer: weapon.type.Value != MeleeWeapon.dagger,
                who: player
            );
            if (attacked)
                location.playSound(weapon.type.Value == MeleeWeapon.club ? "clubhit" : "daggerswipe");
            return attacked;
        }
    }
}
