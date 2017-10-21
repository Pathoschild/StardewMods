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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations stored in the mod's folder.</param>
        public ChestFactory(ITranslationHelper translations)
        {
            this.Translations = translations;
        }

        /// <summary>Get all player chests.</summary>
        public IEnumerable<ManagedChest> GetChests()
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
                            yield return new ManagedChest(new ChestContainer(chest), this.GetLocationName(location), tile, this.Translations.Get("default-name.chest", new { number = ++namelessChests }));
                    }
                }

                // farmhouse containers
                if (location is FarmHouse house && Game1.player.HouseUpgradeLevel > 0)
                {
                    Chest fridge = house.fridge;
                    if (fridge != null)
                        yield return new ManagedChest(new ChestContainer(fridge), location.Name, Vector2.Zero, this.Translations.Get("default-name.fridge"));
                }

                // buildings
                if (location is BuildableGameLocation buildableLocation)
                {
                    int namelessHuts = 0;
                    foreach (Building building in buildableLocation.buildings)
                    {
                        if (building is JunimoHut hut)
                            yield return new ManagedChest(new JunimoHutContainer(hut), this.GetLocationName(location), new Vector2(hut.tileX, hut.tileY), this.Translations.Get("default-name.junimo-hut", new { number = ++namelessHuts }));
                    }
                }

                // shipping bin
                if (location is Farm farm)
                    yield return new ManagedChest(new ShippingBinContainer(farm.shippingBin), farm.Name, Vector2.Zero, this.Translations.Get("default-name.shipping-bin"));
            }
        }

        /// <summary>Get all player chests in the order they should be displayed.</summary>
        /// <param name="selected">The container to show even if it's ignored.</param>
        /// <param name="excludeIgnored">Whether to exclude chests marked as hidden.</param>
        public IEnumerable<ManagedChest> GetChestsForDisplay(IContainer selected = null, bool excludeIgnored = true)
        {
            return this.GetChests()
                .Where(chest => !excludeIgnored || !chest.IsIgnored || chest.Container == selected)
                .OrderBy(p => p.Order ?? int.MaxValue)
                .ThenBy(p => p.Name);
        }

        /// <summary>Get the player chest on the specified tile (if any).</summary>
        /// <param name="tile">The tile to check.</param>
        public ManagedChest GetChestFromTile(Vector2 tile)
        {
            if (!Game1.currentLocation.Objects.TryGetValue(tile, out Object obj) || !(obj is Chest chest))
                return null;

            return this.GetChests().FirstOrDefault(p => p.Container.Inventory == chest.items);
        }

        /// <summary>Get the player chest from the specified menu (if any).</summary>
        /// <param name="menu">The menu to check.</param>
        public ManagedChest GetChestFromMenu(ItemGrabMenu menu)
        {
            // get from opened inventory
            {
                object target = menu.behaviorOnItemGrab?.Target;
                List<Item> inventory = (target as Chest)?.items ?? (target as ChestContainer)?.Inventory;
                if (inventory != null)
                {
                    ManagedChest chest = this.GetChests().FirstOrDefault(p => p.Container.Inventory == inventory);
                    if (chest != null)
                        return chest;
                }
            }

            // fallback to open chest
            return this.GetChests().FirstOrDefault(p => p.Container.IsOpen());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the display name for a location.</summary>
        /// <param name="location">The game location.</param>
        private string GetLocationName(GameLocation location)
        {
            return location.Name;
        }
    }
}
