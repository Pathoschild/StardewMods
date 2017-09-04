using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
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
                    int namelessCount = 0;
                    foreach (KeyValuePair<Vector2, Object> pair in location.Objects)
                    {
                        Vector2 tile = pair.Key;
                        Chest chest = pair.Value as Chest;
                        if (chest != null && chest.playerChest)
                            yield return new ManagedChest(chest, this.GetLocationName(location), tile, this.Translations.Get("default-name.chest", new { number = ++namelessCount }));
                    }
                }

                // farmhouse containers
                if (location is FarmHouse house && Game1.player.HouseUpgradeLevel > 0)
                {
                    Chest fridge = house.fridge;
                    if (fridge != null)
                        yield return new ManagedChest(fridge, location.Name, Vector2.Zero, this.Translations.Get("default-name.fridge"));
                }
            }
        }

        /// <summary>Get all player chests in the order they should be displayed.</summary>
        /// <param name="selectedChest">The chest to show even if it's ignored.</param>
        /// <param name="excludeIgnored">Whether to exclude chests marked as hidden.</param>
        public IEnumerable<ManagedChest> GetChestsForDisplay(Chest selectedChest = null, bool excludeIgnored = true)
        {
            return this.GetChests()
                .Where(chest => !excludeIgnored || !chest.IsIgnored || chest.Chest == selectedChest)
                .OrderBy(p => p.Order ?? int.MaxValue)
                .ThenBy(p => p.Name);
        }

        /// <summary>Get the player chest on the specified tile (if any).</summary>
        /// <param name="tile">The tile to check.</param>
        public ManagedChest GetChestFromTile(Vector2 tile)
        {
            // get chest
            Chest chest;
            {
                Object obj;
                Game1.currentLocation.Objects.TryGetValue(tile, out obj);
                chest = obj as Chest;
            }

            // return if valid
            if (chest != null && chest.playerChest)
                return new ManagedChest(chest, this.GetLocationName(Game1.currentLocation), tile);
            return null;
        }

        /// <summary>Get the player chest from the specified menu (if any).</summary>
        /// <param name="menu">The menu to check.</param>
        public ManagedChest GetChestFromMenu(ItemGrabMenu menu)
        {
            // from menu target
            Chest target = menu.behaviorOnItemGrab?.Target as Chest;
            ManagedChest chest = target != null
                ? this.GetChests().FirstOrDefault(p => p.Chest == target)
                : null;
            if (chest != null)
                return chest;

            // fallback to open chest
            return this.GetChests().FirstOrDefault(p => p.Chest.currentLidFrame == 135);
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

        /// <summary>Get the display name for a location.</summary>
        /// <param name="location">The game location.</param>
        private string GetLocationName(Building location)
        {
            return location.nameOfIndoors.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        }
    }
}
