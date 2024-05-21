using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The base class for tool implementations.</summary>
    internal abstract class BaseAttachment : IAttachment
    {
        /*********
        ** Fields
        *********/
        /// <summary>Fetches metadata about loaded mods.</summary>
        protected IModRegistry ModRegistry { get; }

        /// <summary>The millisecond game times elapsed when requested cooldowns started.</summary>
        private readonly IDictionary<string, long> CooldownStartTimes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Whether the Farm Type Manager mod is installed.</summary>
        private readonly bool HasFarmTypeManager;

        /// <summary>A fake pickaxe to use for clearing dead crops to ensure consistent behavior.</summary>
        private readonly Lazy<Pickaxe> FakePickaxe = new(() => new Pickaxe());


        /*********
        ** Accessors
        *********/
        /// <summary>The minimum number of ticks between each update.</summary>
        public int RateLimit { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public abstract bool IsEnabled(Farmer player, Tool? tool, Item? item, GameLocation location);

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public abstract bool Apply(Vector2 tile, SObject? tileObj, TerrainFeature? tileFeature, Farmer player, Tool? tool, Item? item, GameLocation location);

        /// <summary>Method called when the tractor attachments have been activated for a location.</summary>
        /// <param name="location">The current tractor location.</param>
        public virtual void OnActivated(GameLocation location)
        {
            this.CooldownStartTimes.Clear();
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
        /// <param name="rateLimit">The minimum number of ticks between each update.</param>
        protected BaseAttachment(IModRegistry modRegistry, int rateLimit = 0)
        {
            this.ModRegistry = modRegistry;
            this.RateLimit = rateLimit;

            this.HasFarmTypeManager = modRegistry.IsLoaded("Esca.FarmTypeManager");
        }

        /// <summary>Start a cooldown if it's not already started.</summary>
        /// <param name="key">An arbitrary cooldown ID to check.</param>
        /// <param name="delay">The cooldown time.</param>
        /// <returns>Returns true if the cooldown was successfully started, or false if it was already in progress.</returns>
        protected bool TryStartCooldown(string key, TimeSpan delay)
        {
            long currentTime = (long)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
            if (!this.CooldownStartTimes.TryGetValue(key, out long startTime) || (currentTime - startTime) >= delay.TotalMilliseconds)
            {
                this.CooldownStartTimes[key] = currentTime;
                return true;
            }

            return false;
        }

        /// <summary>Use a tool on a tile.</summary>
        /// <param name="tool">The tool to use.</param>
        /// <param name="tile">The tile to affect.</param>
        /// <param name="player">The current player.</param>
        /// <param name="location">The current location.</param>
        /// <returns>Returns <c>true</c> for convenience when implementing tools.</returns>
        protected bool UseToolOnTile(Tool tool, Vector2 tile, Farmer player, GameLocation location)
        {
            // use tool on center of tile
            this.UpdateToolBeforeUse(tool, tile, player);
            tool.swingTicker++;
            tool.DoFunction(location, (int)player.lastClick.X, (int)player.lastClick.Y, 0, player);
            return true;
        }

        /// <summary>Get the pixel position relative to the top-left corner of the map at which to use a tool.</summary>
        /// <param name="tile">The tile to affect.</param>
        protected Vector2 GetToolPixelPosition(Vector2 tile)
        {
            return (tile * Game1.tileSize) + new Vector2(Game1.tileSize / 2f);
        }

        /// <summary>Use a weapon on the given tile.</summary>
        /// <param name="weapon">The weapon to use.</param>
        /// <param name="tile">The tile to attack.</param>
        /// <param name="player">The current player.</param>
        /// <param name="location">The current location.</param>
        /// <returns>Returns whether a monster was attacked.</returns>
        /// <remarks>This is a simplified version of <see cref="MeleeWeapon.DoDamage"/>. This doesn't account for player bonuses (since it's hugely overpowered anyway), doesn't cause particle effects, doesn't trigger animation timers, etc.</remarks>
        protected bool UseWeaponOnTile(MeleeWeapon weapon, Vector2 tile, Farmer player, GameLocation location)
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
                location.playSound(weapon.type.Value == MeleeWeapon.club ? "clubhit" : "daggerswipe", tile);
            return attacked;
        }

        /// <summary>Trigger the player action on the given tile.</summary>
        /// <param name="location">The location for which to trigger an action.</param>
        /// <param name="tile">The tile for which to trigger an action.</param>
        /// <param name="player">The player for which to trigger an action.</param>
        protected bool CheckTileAction(GameLocation location, Vector2 tile, Farmer player)
        {
            return location.checkAction(new Location((int)tile.X, (int)tile.Y), Game1.viewport, player);
        }

        /// <summary>Remove the specified items from the player inventory.</summary>
        /// <param name="player">The player whose inventory to edit.</param>
        /// <param name="item">The item instance to deduct.</param>
        protected void ConsumeItem(Farmer player, Item item)
        {
            item.Stack -= 1;

            if (item.Stack <= 0)
                player.removeItemFromInventory(item);
        }

        /// <summary>Remove the specified items from the chest inventory.</summary>
        /// <param name="chest">The chest whose inventory to edit.</param>
        /// <param name="item">The item instance to deduct.</param>
        protected void ConsumeItem(Chest chest, Item item)
        {
            item.Stack -= 1;

            if (item.Stack <= 0)
            {
                IInventory inventory = chest.GetItemsForPlayer();

                for (int i = 0; i < inventory.Count; i++)
                {
                    Item slot = inventory[i];
                    if (slot != null && object.ReferenceEquals(item, slot))
                    {
                        inventory[i] = null;
                        break;
                    }
                }
            }
        }

        /// <summary>Get a rectangle representing the tile area in absolute pixels from the map origin.</summary>
        /// <param name="tile">The tile position.</param>
        protected Rectangle GetAbsoluteTileArea(Vector2 tile)
        {
            Vector2 pos = tile * Game1.tileSize;
            return new Rectangle((int)pos.X, (int)pos.Y, Game1.tileSize, Game1.tileSize);
        }

        /// <summary>Get the resource clump which covers a given tile, if any.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        protected bool HasResourceClumpCoveringTile(GameLocation location, Vector2 tile, IReflectionHelper reflection)
        {
            return this.TryGetResourceClumpCoveringTile(location, tile, Game1.player, reflection, out _, out _);
        }

        /// <summary>Get the resource clump which covers a given tile, if any.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="player">The current player.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="clump">The resource clump on the tile, if found.</param>
        /// <param name="applyTool">Applies a tool to the resource clump.</param>
        protected bool TryGetResourceClumpCoveringTile(GameLocation location, Vector2 tile, Farmer player, IReflectionHelper reflection, [NotNullWhen(true)] out ResourceClump? clump, [NotNullWhen(true)] out Func<Tool, bool>? applyTool)
        {
            Rectangle tileArea = this.GetAbsoluteTileArea(tile);

            // normal resource clumps
            foreach (ResourceClump cur in location.resourceClumps)
            {
                if (cur.getBoundingBox().Intersects(tileArea))
                {
                    clump = cur;
                    applyTool = tool => this.UseToolOnTile(tool, tile, player, location);
                    return true;
                }
            }

            // FarmTypeManager resource clumps
            if (this.HasFarmTypeManager)
            {
                foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
                {
                    if (feature.GetType().FullName == "FarmTypeManager.LargeResourceClump" && feature.getBoundingBox().Intersects(tileArea))
                    {
                        clump = reflection.GetField<NetRef<ResourceClump>>(feature, "Clump").GetValue().Value;
                        applyTool = tool =>
                        {
                            this.UpdateToolBeforeUse(tool, tile, player);
                            return feature.performToolAction(tool, 0, tile);
                        };
                        return true;
                    }
                }
            }

            clump = null;
            applyTool = null;
            return false;
        }

        /// <summary>Get the best target farm animal for a tool.</summary>
        /// <param name="tool">The tool to check.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <remarks>Derived from <see cref="Shears.beginUsing"/> and <see cref="Utility.GetBestHarvestableFarmAnimal"/>.</remarks>
        protected FarmAnimal? GetBestHarvestableFarmAnimal(Tool tool, GameLocation location, Vector2 tile)
        {
            // get best harvestable animal
            Vector2 useAt = this.GetToolPixelPosition(tile);
            FarmAnimal? animal = Utility.GetBestHarvestableFarmAnimal(
                animals: location.Animals.Values,
                tool: tool,
                toolRect: new Rectangle((int)useAt.X, (int)useAt.Y, Game1.tileSize, Game1.tileSize)
            );
            if (animal == null || !animal.CanGetProduceWithTool(tool) || !CommonHelper.IsItemId(animal.currentProduce.Value, allowZero: false) || animal.isBaby())
                return null;

            return animal;

        }

        /// <summary>Get the tilled dirt for a tile, if any.</summary>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="dirt">The tilled dirt found, if any.</param>
        /// <param name="isCoveredByObj">Whether there's an object placed over the tilled dirt.</param>
        /// <param name="pot">The indoor pot containing the dirt, if applicable.</param>
        /// <returns>Returns whether tilled dirt was found.</returns>
        protected bool TryGetHoeDirt(TerrainFeature? tileFeature, SObject? tileObj, [NotNullWhen(true)] out HoeDirt? dirt, out bool isCoveredByObj, out IndoorPot? pot)
        {
            // garden pot
            if (tileObj is IndoorPot foundPot)
            {
                pot = foundPot;
                dirt = pot.hoeDirt.Value;
                isCoveredByObj = false;
                return true;
            }

            // regular dirt
            if ((dirt = tileFeature as HoeDirt) != null)
            {
                pot = null;
                isCoveredByObj = tileObj != null;
                return true;
            }

            // none found
            pot = null;
            dirt = null;
            isCoveredByObj = false;
            return false;
        }

        /// <summary>Break open a container using a tool, if applicable.</summary>
        /// <param name="tile">The tile position</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        protected bool TryBreakContainer(Vector2 tile, SObject? tileObj, Farmer player, Tool tool)
        {
            if (tileObj is BreakableContainer)
            {
                this.UpdateToolBeforeUse(tool, tile, player);
                return tileObj.performToolAction(tool);
            }

            if (tileObj is { TypeDefinitionId: ItemRegistry.type_object, Name: "SupplyCrate" } and not Chest && this.UpdateToolBeforeUse(tool, tile, player) && tileObj.performToolAction(tool))
            {
                tileObj.performRemoveAction();
                Game1.currentLocation.Objects.Remove(tile);
                return true;
            }

            return false;
        }

        /// <summary>Try to harvest tall grass.</summary>
        /// <param name="location">The location being harvested.</param>
        /// <param name="tile">The tile being harvested.</param>
        /// <param name="tileFeature">The terrain feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <returns>Returns whether it was harvested.</returns>
        protected bool TryClearDeadCrop(GameLocation location, Vector2 tile, TerrainFeature? tileFeature, Farmer player)
        {
            return
                tileFeature is HoeDirt { crop: not null } dirt
                && dirt.crop.dead.Value
                && this.UseToolOnTile(this.FakePickaxe.Value, tile, player, location);
        }

        /// <summary>Try to harvest tall grass.</summary>
        /// <param name="grass">The grass to harvest.</param>
        /// <param name="location">The location being harvested.</param>
        /// <param name="tile">The tile being harvested.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <returns>Returns whether it was harvested.</returns>
        /// <remarks>Derived from <see cref="Grass.performToolAction"/>.</remarks>
        protected bool TryHarvestGrass(Grass? grass, GameLocation location, Vector2 tile, Farmer player, Tool tool)
        {
            if (grass == null || !location.terrainFeatures.ContainsKey(tile))
                return false;

            grass.numberOfWeeds.Value = 0; // grass won't drop anything if it thinks it's non-cut
            grass.TryDropItemsOnCut(tool); // need to call this before we remove the grass, since it'll check its location
            location.terrainFeatures.Remove(tile);
            return true;
        }

        /// <summary>Cancel the current player animation if it matches one of the given IDs.</summary>
        /// <param name="player">The player to change.</param>
        /// <param name="animationIds">The animation IDs to detect.</param>
        protected void CancelAnimation(Farmer player, params int[] animationIds)
        {
            int animationId = player.FarmerSprite.currentSingleAnimation;
            foreach (int id in animationIds)
            {
                if (id == animationId)
                {
                    player.completelyStopAnimatingOrDoingAction();
                    player.forceCanMove();

                    break;
                }
            }
        }

        /// <summary>Update a tool's fields before it's used. This sets fields like <see cref="Tool.lastUser"/> to avoid errors in some game code.</summary>
        /// <param name="tool">The tool to use.</param>
        /// <param name="tile">The tile to affect.</param>
        /// <param name="player">The current player.</param>
        /// <returns>Returns <c>true</c> to simplify chaining in conditions.</returns>
        protected bool UpdateToolBeforeUse(Tool tool, Vector2 tile, Farmer player)
        {
            player.lastClick = this.GetToolPixelPosition(tile);
            tool.lastUser = player;

            return true;
        }
    }
}
