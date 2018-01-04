using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace Pathoschild.Stardew.ChestsAnywhere
{
    /// <summary>Encapsulates logic for finding chests.</summary>
    internal class ChestFactory
    {
        /*********
        ** Properties
        *********/
        /// <summary>Provides translations stored in the mod's folder.</summary>
        private readonly ITranslationHelper Translations;

        /// <summary>Simplifies access to private game data.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Whether to support access to the shipping bin.</summary>
        private readonly bool EnableShippingBin;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations stored in the mod's folder.</param>
        /// <param name="reflection">Simplifies access to private game data.</param>
        /// <param name="enableShippingBin">Whether to support access to the shipping bin.</param>
        public ChestFactory(ITranslationHelper translations, IReflectionHelper reflection, bool enableShippingBin)
        {
            this.Translations = translations;
            this.Reflection = reflection;
            this.EnableShippingBin = enableShippingBin;
        }

        /// <summary>Get all player chests.</summary>
        /// <param name="range">Determines whether given locations are in range of the player for remote chest access.</param>
        /// <param name="excludeHidden">Whether to exclude chests marked as hidden.</param>
        /// <param name="alwaysIncludeContainer">A container to include even if it would normally be hidden.</param>
        public IEnumerable<ManagedChest> GetChests(RangeHandler range, bool excludeHidden = false, IContainer alwaysIncludeContainer = null)
        {
            IEnumerable<ManagedChest> Search()
            {
                foreach (GameLocation location in CommonHelper.GetLocations())
                {
                    // chests in location
                    {
                        int namelessChests = 0;
                        foreach (KeyValuePair<Vector2, Object> pair in location.Objects)
                        {
                            Vector2 tile = pair.Key;
                            if (pair.Value is Chest chest && chest.playerChest)
                                yield return new ManagedChest(new ChestContainer(chest), location, tile, this.Translations.Get("default-name.chest", new { number = ++namelessChests }));
                        }
                    }

                    // farmhouse containers
                    if (location is FarmHouse house && Game1.player.HouseUpgradeLevel > 0)
                    {
                        Chest fridge = house.fridge;
                        if (fridge != null)
                            yield return new ManagedChest(new ChestContainer(fridge), location, Vector2.Zero, this.Translations.Get("default-name.fridge"));
                    }

                    // buildings
                    if (location is BuildableGameLocation buildableLocation)
                    {
                        int namelessHuts = 0;
                        foreach (Building building in buildableLocation.buildings)
                        {
                            if (building is JunimoHut hut)
                                yield return new ManagedChest(new JunimoHutContainer(hut), location, new Vector2(hut.tileX, hut.tileY), this.Translations.Get("default-name.junimo-hut", new { number = ++namelessHuts }));
                        }
                    }

                    // shipping bin
                    if (this.EnableShippingBin && location is Farm farm)
                        yield return new ManagedChest(new ShippingBinContainer(farm, this.Reflection), farm, Vector2.Zero, this.Translations.Get("default-name.shipping-bin"));
                }
            }

            return (
                from chest in Search()
                orderby chest.Order ?? int.MaxValue, chest.Name
                where
                    chest.Container.IsSameAs(alwaysIncludeContainer)
                    || (
                        (!excludeHidden || !chest.IsIgnored)
                        && range.IsInRange(chest.Location)
                    )
                select chest
            );
        }

        /// <summary>Get the player chest on the specified tile (if any).</summary>
        /// <param name="tile">The tile to check.</param>
        public ManagedChest GetChestFromTile(Vector2 tile)
        {
            if (!Game1.currentLocation.Objects.TryGetValue(tile, out Object obj) || !(obj is Chest chest))
                return null;

            RangeHandler range = RangeHandler.CurrentLocation();
            return this.GetChests(range).FirstOrDefault(p => p.Container.IsSameAs(chest.items));
        }

        /// <summary>Get the player chest from the specified menu (if any).</summary>
        /// <param name="menu">The menu to check.</param>
        public ManagedChest GetChestFromMenu(ItemGrabMenu menu)
        {
            RangeHandler range = RangeHandler.Unlimited();

            // get from opened inventory
            {
                object target = menu.behaviorOnItemGrab?.Target;
                List<Item> inventory = (target as Chest)?.items ?? (target as IContainer)?.Inventory;
                if (inventory != null)
                {
                    ManagedChest chest = this.GetChests(range).FirstOrDefault(p => p.Container.IsSameAs(inventory));
                    if (chest != null)
                        return chest;
                }
            }

            // fallback to open chest
            return this.GetChests(range).FirstOrDefault(p => p.Container.IsOpen());
        }
    }
}
