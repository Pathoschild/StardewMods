using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Common;
using Pathoschild.LookupAnything.Framework;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything
{
    /// <summary>Provides utility methods for interacting with the game code.</summary>
    internal static class GameHelper
    {
        /*********
        ** Properties
        *********/
        /// <summary>The cached object data.</summary>
        private static readonly Lazy<ObjectModel[]> Objects = new Lazy<ObjectModel[]>(() => DataParser.GetObjects().ToArray());

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
        public static void ResetCache(Metadata metadata)
        {
            GameHelper.GiftTastes = new Lazy<GiftTasteModel[]>(() => DataParser.GetGiftTastes(GameHelper.Objects.Value).ToArray());
            GameHelper.Recipes = new Lazy<RecipeModel[]>(() => DataParser.GetRecipes(metadata).ToArray());
        }

        /****
        ** Data helpers
        ****/
        /// <summary>Add a day offset to the current date.</summary>
        /// <param name="offset">The offset to add in days.</param>
        /// <param name="daysInSeason">The number of days in a season.</param>
        /// <returns>Returns the resulting season and day.</returns>
        public static Tuple<string, int> GetDayOffset(int offset, int daysInSeason)
        {
            // simple case
            string season = Game1.currentSeason;
            int day = Game1.dayOfMonth + offset;

            // handle season transition
            if (day > daysInSeason)
            {
                string[] seasons = { Constant.SeasonNames.Spring, Constant.SeasonNames.Summer, Constant.SeasonNames.Fall, Constant.SeasonNames.Winter };
                int curSeasonIndex = Array.IndexOf(seasons, Game1.currentSeason);
                if (curSeasonIndex == -1)
                    throw new InvalidOperationException($"The current season '{Game1.currentSeason}' wasn't recognised.");
                season = seasons[(curSeasonIndex + day / daysInSeason) % seasons.Length];
                day %= daysInSeason;
            }

            return Tuple.Create(season, day);
        }

        /// <summary>Get how much each NPC likes receiving an item as a gift.</summary>
        /// <param name="item">The item to check.</param>
        public static IDictionary<string, GiftTaste> GetGiftTastes(Item item)
        {
            if (!item.canBeGivenAsGift())
                return new Dictionary<string, GiftTaste>();

            return GameHelper.GiftTastes.Value
                .Where(p => p.ItemID == item.parentSheetIndex)
                .ToDictionary(p => p.Villager, p => p.Taste);
        }

        /// <summary>Get the items a specified NPC can receive.</summary>
        /// <param name="npc">The NPC to check.</param>
        public static IDictionary<Object, GiftTaste> GetGiftTastes(NPC npc)
        {
            if (!npc.isVillager())
                return new Dictionary<Object, GiftTaste>();

            string name = npc.getName();
            return GameHelper.GiftTastes.Value
                .Where(p => p.Villager == name)
                .ToDictionary(p => GameHelper.GetObjectBySpriteIndex(p.ItemID), p => p.Taste);
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
        public static Object GetObjectBySpriteIndex(int index, int stack = 1)
        {
            return new Object(index, stack);
        }

        /// <summary>Get all objects matching the reference ID.</summary>
        /// <param name="refID">The reference ID. This can be a category (negative value) or parent sprite index (positive value).</param>
        public static IEnumerable<Object> GetObjectsByReferenceID(int refID)
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
            if (new[] { "Crafting", "asdf" /*dig spots*/, "Quest" }.Contains((item as Object)?.Type))
                return false;

            return true;
        }

        /****
        ** Reflection
        ****/
        /// <summary>Get a private field value.</summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="parent">The parent object.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        public static T GetPrivateField<T>(object parent, string name, bool required = true)
        {
            if (parent == null)
                return default(T);

            // get field from hierarchy
            FieldInfo field = null;
            for (Type type = parent.GetType(); type != null && field == null; type = type.BaseType)
                field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);

            // validate
            if (field == null)
            {
                if (required)
                    throw new InvalidOperationException($"The {parent.GetType().Name} object doesn't have a private '{name}' field.");
                return default(T);
            }

            // get value
            return (T)field.GetValue(parent);
        }

        /// <summary>Get a private method.</summary>
        /// <param name="parent">The parent object.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        public static MethodInfo GetPrivateMethod(object parent, string name, bool required = true)
        {
            if (parent == null)
                throw new InvalidOperationException($"Can't get private method '{name}' on null instance.");

            // get method from hierarchy
            MethodInfo method = null;
            for (Type type = parent.GetType(); type != null && method == null; type = type.BaseType)
                method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);

            // validate
            if (method == null)
            {
                if (required)
                    throw new InvalidOperationException($"The {parent.GetType().Name} object doesn't have a private '{name}' method.");
                return null;
            }

            // get value
            return method;
        }

        /// <summary>Validate that the game versions match the minimum requirements, and return an appropriate error message if not.</summary>
        public static string ValidateGameVersion()
        {
            string gameVersion = Regex.Replace(Game1.version, "^([0-9.]+).*", "$1");
            string apiVersion = Constants.Version.VersionString;

            if (string.Compare(gameVersion, Constant.MinimumGameVersion, StringComparison.InvariantCultureIgnoreCase) == -1)
                return $"The LookupAnything mod requires the latest version of the game. Please update Stardew Valley from {gameVersion} to {Constant.MinimumGameVersion}.";
            if (string.Compare(apiVersion, Constant.MinimumApiVersion, StringComparison.InvariantCultureIgnoreCase) == -1)
                return $"The LookupAnything mod requires the latest version of SMAPI. Please update SMAPI from {apiVersion} to {Constant.MinimumApiVersion}.";

            return null;
        }

        /****
        ** Error handling
        ****/
        /// <summary>Intercept errors thrown by the action.</summary>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up"). This is displayed on the screen, so it should be simple and avoid characters that might not be available in the sprite font.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="onError">A callback invoked if an error is intercepted.</param>
        public static void InterceptErrors(string verb, Action action, Action<Exception> onError = null)
        {
            GameHelper.InterceptErrors(verb, null, action, onError);
        }

        /// <summary>Intercept errors thrown by the action.</summary>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up"). This is displayed on the screen, so it should be simple and avoid characters that might not be available in the sprite font.</param>
        /// <param name="detailedVerb">A more detailed form of <see cref="verb"/> if applicable. This is displayed in the log, so it can be more technical and isn't constrained by the sprite font.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="onError">A callback invoked if an error is intercepted.</param>
        public static void InterceptErrors(string verb, string detailedVerb, Action action, Action<Exception> onError = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                GameHelper.HandleError(ex, verb, detailedVerb);
                onError?.Invoke(ex);
            }
        }

        /****
        ** Formatting
        ****/
        /// <summary>Select the correct plural form for a word.</summary>
        /// <param name="count">The number.</param>
        /// <param name="single">The singular form.</param>
        /// <param name="plural">The plural form.</param>
        public static string Pluralise(int count, string single, string plural = null)
        {
            return count == 1 ? single : (plural ?? single + "s");
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
            // stardard object
            if (item is Object)
            {
                Object obj = (Object)item;
                return obj.bigCraftable
                    ? Tuple.Create(Game1.bigCraftableSpriteSheet, Object.getSourceRectForBigCraftable(obj.ParentSheetIndex))
                    : Tuple.Create(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, obj.ParentSheetIndex, Object.spriteSheetTileSize, Object.spriteSheetTileSize));
            }

            // boots or ring
            if (item is Boots || item is Ring)
            {
                int indexInTileSheet = (item as Boots)?.indexInTileSheet ?? ((Ring)item).indexInTileSheet;
                return Tuple.Create(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, indexInTileSheet, Object.spriteSheetTileSize, Object.spriteSheetTileSize));
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
        /// <summary>Log an error and warn the user.</summary>
        /// <param name="ex">The exception to handle.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up"). This is displayed on the screen, so it should be simple and avoid characters that might not be available in the sprite font.</param>
        /// <param name="detailedVerb">A more detailed form of <see cref="verb"/> if applicable. This is displayed in the log, so it can be more technical and isn't constrained by the sprite font.</param>
        private static void HandleError(Exception ex, string verb, string detailedVerb = null)
        {
            detailedVerb = detailedVerb ?? verb;
            Log.Error($"[Lookup Anything] Something went wrong {detailedVerb}:{Environment.NewLine}{ex}");
            GameHelper.ShowErrorMessage($"Huh. Something went wrong {verb}. The game error log has the technical details.");
        }
    }
}
