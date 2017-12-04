using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything
{
    /// <summary>Provides utility methods for interacting with the game code.</summary>
    internal static class GameHelper
    {
        /*********
        ** Properties
        *********/
        /// <summary>The cached object data.</summary>
        private static Lazy<ObjectModel[]> Objects;

        /// <summary>The cached villagers' gift tastes.</summary>
        private static Lazy<GiftTasteModel[]> GiftTastes;

        /// <summary>The cached recipes.</summary>
        private static Lazy<RecipeModel[]> Recipes;


        /*********
        ** Public methods
        *********/
        /****
        ** State
        ****/
        /// <summary>Reset the low-level cache used to store expensive query results, so the data is recalculated on demand.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        /// <param name="monitor">The monitor with which to log errors.</param>
        public static void ResetCache(Metadata metadata, IReflectionHelper reflectionHelper, ITranslationHelper translations, IMonitor monitor)
        {
            GameHelper.Objects = new Lazy<ObjectModel[]>(() => DataParser.GetObjects(monitor).ToArray());
            GameHelper.GiftTastes = new Lazy<GiftTasteModel[]>(() => DataParser.GetGiftTastes(GameHelper.Objects.Value).ToArray());
            GameHelper.Recipes = new Lazy<RecipeModel[]>(() => DataParser.GetRecipes(metadata, reflectionHelper, translations).ToArray());
        }

        /****
        ** Data helpers
        ****/
        /// <summary>Get the number of times the player has shipped a given item.</summary>
        /// <param name="itemID">The item's parent sprite index.</param>
        public static int GetShipped(int itemID)
        {
            return Game1.player.basicShipped.ContainsKey(itemID)
                ? Game1.player.basicShipped[itemID]
                : 0;
        }

        /// <summary>Get all shippable items.</summary>
        /// <remarks>Derived from <see cref="Utility.hasFarmerShippedAllItems"/>.</remarks>
        public static IEnumerable<KeyValuePair<int, bool>> GetFullShipmentAchievementItems()
        {
            return (
                from obj in GameHelper.Objects.Value
                where obj.Type != "Arch" && obj.Type != "Fish" && obj.Type != "Mineral" && obj.Type != "Cooking" && SObject.isPotentialBasicShippedCategory(obj.ParentSpriteIndex, obj.Category.ToString())
                select new KeyValuePair<int, bool>(obj.ParentSpriteIndex, Game1.player.basicShipped.ContainsKey(obj.ParentSpriteIndex))
            );
        }

        /// <summary>Get all items owned by the player.</summary>
        /// <remarks>Derived from <see cref="Utility.doesItemWithThisIndexExistAnywhere"/>.</remarks>
        public static IEnumerable<Item> GetAllOwnedItems()
        {
            List<Item> items = new List<Item>();

            // inventory
            items.AddRange(Game1.player.Items);

            // in locations
            foreach (GameLocation location in CommonHelper.GetLocations())
            {
                // map objects
                foreach (SObject item in location.objects.Values)
                {
                    if (item is Chest chest)
                    {
                        if (chest.playerChest)
                        {
                            items.Add(chest);
                            items.AddRange(chest.items);
                        }
                    }
                    else if (item is Cask)
                    {
                        items.Add(item);
                        items.Add(item.heldObject); // cask contents can be retrieved anytime
                    }
                    else if (item.bigCraftable)
                    {
                        items.Add(item);
                        if (item.minutesUntilReady == 0)
                            items.Add(item.heldObject);
                    }
                    else if (!item.IsSpawnedObject)
                    {
                        items.Add(item);
                        items.Add(item.heldObject);
                    }
                }

                // furniture
                if (location is DecoratableLocation decorableLocation)
                {
                    foreach (Furniture furniture in decorableLocation.furniture)
                    {
                        items.Add(furniture);
                        items.Add(furniture.heldObject);
                    }
                }

                // building output
                if (location is Farm farm)
                {
                    foreach (var building in farm.buildings)
                    {
                        if (building is Mill mill)
                            items.AddRange(mill.output.items);
                        else if (building is JunimoHut hut)
                            items.AddRange(hut.output.items);
                    }
                }

                // farmhouse fridge
                if (location is FarmHouse house)
                    items.AddRange(house.fridge.items);
            }

            return items.Where(p => p != null);
        }

        /// <summary>Get all NPCs currently in the world.</summary>
        public static IEnumerable<NPC> GetAllCharacters()
        {
            return Utility
                .getAllCharacters()
                .Distinct(); // fix rare issue where the game duplicates an NPC (seems to happen when the player's child is born)
        }

        /// <summary>Count how many of an item the player owns.</summary>
        /// <param name="item">The item to count.</param>
        public static int CountOwnedItems(Item item)
        {
            return (
                from worldItem in GameHelper.GetAllOwnedItems()
                where GameHelper.AreEquivalent(worldItem, item)
                let canStack = worldItem.canStackWith(worldItem)
                select canStack ? Math.Max(1, worldItem.Stack) : 1
            ).Sum();
        }

        /// <summary>Get whether two items are the same type (ignoring flavour text like 'blueberry wine' vs 'cranberry wine').</summary>
        /// <param name="a">The first item to compare.</param>
        /// <param name="b">The second item to compare.</param>
        private static bool AreEquivalent(Item a, Item b)
        {
            return
                // same generic item type
                a.GetType() == b.GetType()
                && a.category == b.category
                && a.parentSheetIndex == b.parentSheetIndex

                // same discriminators
                && a.GetSpriteType() == b.GetSpriteType()
                && (a as Boots)?.indexInTileSheet == (b as Boots)?.indexInTileSheet
                && (a as BreakableContainer)?.type == (b as BreakableContainer)?.type
                && (a as Fence)?.isGate == (b as Fence)?.isGate
                && (a as Fence)?.whichType == (b as Fence)?.whichType
                && (a as Hat)?.which == (b as Hat)?.which
                && (a as MeleeWeapon)?.type == (b as MeleeWeapon)?.type
                && (a as Ring)?.indexInTileSheet == (b as Ring)?.indexInTileSheet
                && (a as Tool)?.initialParentTileIndex == (b as Tool)?.initialParentTileIndex
                && (a as Tool)?.CurrentParentTileIndex == (b as Tool)?.CurrentParentTileIndex;
        }

        /// <summary>Get whether the specified NPC has social data like a birthday and gift tastes.</summary>
        /// <param name="npc">The NPC to check.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public static bool IsSocialVillager(NPC npc, Metadata metadata)
        {
            return npc.isVillager() && !metadata.Constants.AsocialVillagers.Contains(npc.name);
        }

        /// <summary>Get how much each NPC likes receiving an item as a gift.</summary>
        /// <param name="item">The item to check.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public static IEnumerable<KeyValuePair<NPC, GiftTaste>> GetGiftTastes(Item item, Metadata metadata)
        {
            if (!item.canBeGivenAsGift())
                yield break;

            foreach (NPC npc in GameHelper.GetAllCharacters())
            {
                if (!GameHelper.IsSocialVillager(npc, metadata))
                    continue;

                GiftTaste? taste = GameHelper.GetGiftTaste(npc, item);
                if (taste.HasValue)
                    yield return new KeyValuePair<NPC, GiftTaste>(npc, taste.Value);
            }
        }

        /// <summary>Get the items a specified NPC can receive.</summary>
        /// <param name="npc">The NPC to check.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public static IDictionary<SObject, GiftTaste> GetGiftTastes(NPC npc, Metadata metadata)
        {
            if (!GameHelper.IsSocialVillager(npc, metadata))
                return new Dictionary<SObject, GiftTaste>();

            // get giftable items
            HashSet<int> giftableItemIDs = new HashSet<int>(
                from int refID in GameHelper.GiftTastes.Value.Select(p => p.RefID)
                from ObjectModel obj in GameHelper.Objects.Value
                where obj.ParentSpriteIndex == refID || obj.Category == refID
                select obj.ParentSpriteIndex
            );

            // get gift tastes
            return
                (
                    from int itemID in giftableItemIDs
                    let item = GameHelper.GetObjectBySpriteIndex(itemID)
                    let taste = GameHelper.GetGiftTaste(npc, item)
                    where taste.HasValue
                    select new { Item = item, Taste = taste.Value }
                )
                .ToDictionary(p => p.Item, p => p.Taste);
        }

        /// <summary>Get the recipes for which an item is needed.</summary>
        public static IEnumerable<RecipeModel> GetRecipes()
        {
            return GameHelper.Recipes.Value;
        }

        /// <summary>Get the recipes for which an item is needed.</summary>
        /// <param name="item">The item.</param>
        public static IEnumerable<RecipeModel> GetRecipesForIngredient(Item item)
        {
            return (
                from recipe in GameHelper.GetRecipes()
                where
                    (recipe.Ingredients.ContainsKey(item.parentSheetIndex) || recipe.Ingredients.ContainsKey(item.category))
                    && recipe.ExceptIngredients?.Contains(item.parentSheetIndex) != true
                select recipe
            );
        }

        /// <summary>Get an object by its parent sprite index.</summary>
        /// <param name="index">The parent sprite index.</param>
        /// <param name="stack">The number of items in the stack.</param>
        public static SObject GetObjectBySpriteIndex(int index, int stack = 1)
        {
            return new SObject(index, stack);
        }

        /// <summary>Get the sprite sheet to which the item's <see cref="Item.parentSheetIndex"/> refers.</summary>
        /// <param name="item">The item to check.</param>
        public static ItemSpriteType GetSpriteType(this Item item)
        {
            if (item is SObject obj)
            {
                if (obj is Furniture)
                    return ItemSpriteType.Furniture;
                if (obj is Wallpaper)
                    return ItemSpriteType.Wallpaper;
                return obj.bigCraftable
                    ? ItemSpriteType.BigCraftable
                    : ItemSpriteType.Object;
            }
            if (item is Boots)
                return ItemSpriteType.Boots;
            if (item is Hat)
                return ItemSpriteType.Hat;
            if (item is Tool)
                return ItemSpriteType.Tool;

            return ItemSpriteType.Unknown;
        }

        /// <summary>Get all objects matching the reference ID.</summary>
        /// <param name="refID">The reference ID. This can be a category (negative value) or parent sprite index (positive value).</param>
        public static IEnumerable<SObject> GetObjectsByReferenceID(int refID)
        {
            // category
            if (refID < 0)
            {
                return (
                    from pair in Game1.objectInformation
                    where Regex.IsMatch(pair.Value, $"\b{refID}\b")
                    select GameHelper.GetObjectBySpriteIndex(pair.Key)
                );
            }

            // parent sprite index
            return new[] { GameHelper.GetObjectBySpriteIndex(refID) };
        }

        /// <summary>Get whether an item can have a quality (which increases its sale price).</summary>
        /// <param name="item">The item.</param>
        public static bool CanHaveQuality(Item item)
        {
            // check category
            if (new[] { "Artifact", "Trash", "Crafting", "Seed", "Decor", "Resource", "Fertilizer", "Bait", "Fishing Tackle" }.Contains(item.getCategoryName()))
                return false;

            // check type
            if (new[] { "Crafting", "asdf" /*dig spots*/, "Quest" }.Contains((item as SObject)?.Type))
                return false;

            return true;
        }

        /****
        ** Coordinates
        ****/
        /// <summary>Get the viewport coordinates from the current cursor position.</summary>
        public static Vector2 GetScreenCoordinatesFromCursor()
        {
            return new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY());
        }

        /// <summary>Get the viewport coordinates represented by a tile position.</summary>
        /// <param name="coordinates">The absolute coordinates.</param>
        public static Vector2 GetScreenCoordinatesFromAbsolute(Vector2 coordinates)
        {
            return coordinates - new Vector2(Game1.viewport.X, Game1.viewport.Y);
        }

        /// <summary>Get the viewport coordinates represented by a tile position.</summary>
        /// <param name="tile">The tile position.</param>
        public static Rectangle GetScreenCoordinatesFromTile(Vector2 tile)
        {
            Vector2 position = GameHelper.GetScreenCoordinatesFromAbsolute(tile * new Vector2(Game1.tileSize));
            return new Rectangle((int)position.X, (int)position.Y, Game1.tileSize, Game1.tileSize);
        }

        /// <summary>Get whether a sprite on a given tile could occlude a specified tile position.</summary>
        /// <param name="spriteTile">The tile of the possible sprite.</param>
        /// <param name="occludeTile">The tile to check for possible occlusion.</param>
        public static bool CouldSpriteOccludeTile(Vector2 spriteTile, Vector2 occludeTile)
        {
            Vector2 spriteSize = Constant.MaxTargetSpriteSize;
            return
                spriteTile.Y >= occludeTile.Y // sprites never extend downard from their tile
                && Math.Abs(spriteTile.X - occludeTile.X) <= spriteSize.X
                && Math.Abs(spriteTile.Y - occludeTile.Y) <= spriteSize.Y;
        }

        /// <summary>Get the pixel coordinates within a sprite sheet corresponding to a sprite displayed in the world.</summary>
        /// <param name="worldPosition">The pixel position in the world.</param>
        /// <param name="worldRectangle">The sprite rectangle in the world.</param>
        /// <param name="spriteRectangle">The sprite rectangle in the sprite sheet.</param>
        /// <param name="spriteEffects">The transformation to apply on the sprite.</param>
        public static Vector2 GetSpriteSheetCoordinates(Vector2 worldPosition, Rectangle worldRectangle, Rectangle spriteRectangle, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            // get position within sprite rectangle
            float x = (worldPosition.X - worldRectangle.X) / Game1.pixelZoom;
            float y = (worldPosition.Y - worldRectangle.Y) / Game1.pixelZoom;

            // flip values
            if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally))
                x = spriteRectangle.Width - x;
            if (spriteEffects.HasFlag(SpriteEffects.FlipVertically))
                y = spriteRectangle.Height - y;

            // get position within sprite sheet
            x += spriteRectangle.X;
            y += spriteRectangle.Y;

            // return coordinates
            return new Vector2(x, y);
        }

        /// <summary>Get a pixel from a sprite sheet.</summary>
        /// <typeparam name="TPixel">The pixel value type.</typeparam>
        /// <param name="spriteSheet">The sprite sheet.</param>
        /// <param name="position">The position of the pixel within the sprite sheet.</param>
        public static TPixel GetSpriteSheetPixel<TPixel>(Texture2D spriteSheet, Vector2 position) where TPixel : struct
        {
            // get pixel index
            int x = (int)position.X;
            int y = (int)position.Y;
            int spriteIndex = y * spriteSheet.Width + x; // (pixels in preceding rows) + (preceding pixels in current row)

            // get pixel
            TPixel[] pixels = new TPixel[spriteSheet.Width * spriteSheet.Height];
            spriteSheet.GetData(pixels);
            return pixels[spriteIndex];
        }

        /// <summary>Get the sprite for an item.</summary>
        /// <param name="item">The item.</param>
        /// <returns>Returns a tuple containing the sprite sheet and the sprite's position and dimensions within the sheet.</returns>
        public static Tuple<Texture2D, Rectangle> GetSprite(Item item)
        {
            // standard object
            if (item is SObject obj)
            {
                return obj.bigCraftable
                    ? Tuple.Create(Game1.bigCraftableSpriteSheet, SObject.getSourceRectForBigCraftable(obj.ParentSheetIndex))
                    : Tuple.Create(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, obj.ParentSheetIndex, SObject.spriteSheetTileSize, SObject.spriteSheetTileSize));
            }

            // boots or ring
            if (item is Boots || item is Ring)
            {
                int indexInTileSheet = (item as Boots)?.indexInTileSheet ?? ((Ring)item).indexInTileSheet;
                return Tuple.Create(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, indexInTileSheet, SObject.spriteSheetTileSize, SObject.spriteSheetTileSize));
            }

            // unknown item
            return null;
        }


        /****
        ** UI
        ****/
        /// <summary>Draw a pretty hover box for the given text.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="label">The text to display.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="wrapWidth">The maximum width to display.</param>
        public static Vector2 DrawHoverBox(SpriteBatch spriteBatch, string label, Vector2 position, float wrapWidth)
        {
            return CommonHelper.DrawHoverBox(spriteBatch, label, position, wrapWidth);
        }

        /// <summary>Show an informational message to the player.</summary>
        /// <param name="message">The message to show.</param>
        public static void ShowInfoMessage(string message)
        {
            CommonHelper.ShowInfoMessage(message);
        }

        /// <summary>Show an error message to the player.</summary>
        /// <param name="message">The message to show.</param>
        public static void ShowErrorMessage(string message)
        {
            CommonHelper.ShowErrorMessage(message);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get an NPC's preference for an item.</summary>
        /// <param name="npc">The NPC whose gift taste to get.</param>
        /// <param name="item">The item to check.</param>
        /// <returns>Returns the NPC's gift taste if applicable, else <c>null</c>.</returns>
        private static GiftTaste? GetGiftTaste(NPC npc, Item item)
        {
            try
            {
                return (GiftTaste)npc.getGiftTasteForThisItem(item);
            }
            catch
            {
                // fails for non-social NPCs
                return null;
            }
        }
    }
}
