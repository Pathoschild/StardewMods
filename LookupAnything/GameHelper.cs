using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux;
using Pathoschild.Stardew.Common.Integrations.MultiFertilizer;
using Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod;
using Pathoschild.Stardew.Common.Items.ItemData;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.ItemScanning;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Crafting;
using StardewValley.GameData.FishPond;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything
{
    /// <summary>Provides utility methods for interacting with the game code.</summary>
    internal class GameHelper
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Custom Farming Redux integration.</summary>
        private readonly CustomFarmingReduxIntegration CustomFarmingRedux;

        /// <summary>The Producer Framework Mod integration.</summary>
        private readonly ProducerFrameworkModIntegration ProducerFrameworkMod;

        /// <summary>Parses the raw game data into usable models.</summary>
        private readonly DataParser DataParser;

        /// <summary>Scans the game world for owned items.</summary>
        private readonly WorldItemScanner WorldItemScanner;

        /// <summary>Provides methods for searching and constructing items.</summary>
        private readonly ItemRepository ItemRepository = new();

        /// <summary>Encapsulates logging to the console.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The cached item data filtered to <see cref="ItemType.Object"/> items.</summary>
        private Lazy<SearchableItem[]> Objects;

        /// <summary>The cached recipes.</summary>
        private Lazy<RecipeModel[]> Recipes;


        /*********
        ** Accessors
        *********/
        /// <summary>Provides metadata that's not available from the game data directly.</summary>
        public Metadata Metadata { get; }

        /// <summary>The MultiFertilizer integration.</summary>
        public MultiFertilizerIntegration MultiFertilizer { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <param name="monitor">Encapsulates logging to the console.</param>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="reflection">Simplifies access to protected code.</param>
        public GameHelper(Metadata metadata, IMonitor monitor, IModRegistry modRegistry, IReflectionHelper reflection)
        {
            this.DataParser = new DataParser(this);
            this.Metadata = metadata;
            this.Monitor = monitor;
            this.WorldItemScanner = new WorldItemScanner(reflection);

            this.CustomFarmingRedux = new CustomFarmingReduxIntegration(modRegistry, this.Monitor);
            this.MultiFertilizer = new MultiFertilizerIntegration(modRegistry, monitor);
            this.ProducerFrameworkMod = new ProducerFrameworkModIntegration(modRegistry, this.Monitor);

            this.ResetCache(reflection, monitor);
        }

        /// <summary>Reset the low-level cache used to store expensive query results, so the data is recalculated on demand.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="monitor">The monitor with which to log errors.</param>
        [MemberNotNull(nameof(GameHelper.Objects), nameof(GameHelper.Recipes))]
        public void ResetCache(IReflectionHelper reflection, IMonitor monitor)
        {
            this.Objects = new(() => this.ItemRepository.GetAll(itemTypes: new[] { ItemType.Object }).ToArray());
            this.Recipes = new(() => this.GetAllRecipes(reflection, monitor).ToArray());
        }

        /****
        ** Date/time helpers
        ****/
        /// <summary>Format a game time in military 24-hour notation.</summary>
        /// <param name="time">The time to format.</param>
        public string FormatMilitaryTime(int time)
        {
            time %= 2400;
            return $"{time / 100:00}:{time % 100:00}";
        }

        /// <summary>Get a translated season name for the current language.</summary>
        /// <param name="season">The English season name.</param>
        public string TranslateSeason(string season)
        {
            int number = Utility.getSeasonNumber(season);
            return number != -1
                ? Utility.getSeasonNameFromNumber(number)
                : season;
        }

        /// <summary>Get a date from its component parts if they're valid.</summary>
        /// <param name="day">The day of month.</param>
        /// <param name="season">The season name.</param>
        /// <param name="date">The resulting date, if valid.</param>
        /// <returns>Returns whether the date is valid.</returns>
        public bool TryGetDate(int day, string season, out SDate date)
        {
            return this.TryGetDate(day, season, Game1.year, out date);
        }

        /// <summary>Get a date from its component parts if they're valid.</summary>
        /// <param name="day">The day of month.</param>
        /// <param name="season">The season name.</param>
        /// <param name="year">The year.</param>
        /// <param name="date">The resulting date, if valid.</param>
        /// <returns>Returns whether the date is valid.</returns>
        public bool TryGetDate(int day, string season, int year, out SDate date)
        {
            try
            {
                date = new SDate(day, season, year);
                return true;
            }
            catch
            {
                date = SDate.Now();
                return false;
            }
        }

        /****
        ** Data helpers
        ****/
        /// <summary>Get the number of times the player has shipped a given item.</summary>
        /// <param name="itemID">The item's parent sprite index.</param>
        public int GetShipped(int itemID)
        {
            return Game1.player.basicShipped.ContainsKey(itemID)
                ? Game1.player.basicShipped[itemID]
                : 0;
        }

        /// <summary>Get all shippable items.</summary>
        /// <remarks>Derived from <see cref="Utility.hasFarmerShippedAllItems"/>.</remarks>
        public IEnumerable<KeyValuePair<int, bool>> GetFullShipmentAchievementItems()
        {
            return (
                from entry in this.Objects.Value
                let obj = (SObject)entry.Item
                where
                    obj.Type != "Arch"
                    && obj.Type != "Fish"
                    && obj.Type != "Mineral"
                    && obj.Type != "Cooking"
                    && SObject.isPotentialBasicShippedCategory(obj.ParentSheetIndex, obj.Category.ToString())

                select new KeyValuePair<int, bool>(obj.ParentSheetIndex, Game1.player.basicShipped.ContainsKey(obj.ParentSheetIndex))
            );
        }

        /// <summary>Get all items owned by the player.</summary>
        /// <remarks>
        /// Derived from <see cref="Utility.iterateAllItems"/> with some differences:
        ///   * removed items held by other players, items floating on the ground, spawned forage, and output in a non-ready machine (except casks which can be emptied anytime);
        ///   * added hay in silos.
        /// </remarks>
        public IEnumerable<FoundItem> GetAllOwnedItems()
        {
            return this.WorldItemScanner.GetAllOwnedItems();
        }

        /// <summary>Get all NPCs currently in the world.</summary>
        public IEnumerable<NPC> GetAllCharacters()
        {
            return Utility
                .getAllCharacters(new List<NPC>())
                .Distinct(); // fix rare issue where the game duplicates an NPC (seems to happen when the player's child is born)
        }

        /// <summary>Count how many of an item the player owns.</summary>
        /// <param name="item">The item to count.</param>
        public int CountOwnedItems(Item item)
        {
            return (
                from found in this.GetAllOwnedItems()
                let foundItem = found.Item
                where this.AreEquivalent(foundItem, item)
                let canStack = foundItem.canStackWith(foundItem)
                select canStack ? found.GetCount() : 1
            ).Sum();
        }

        /// <summary>Get whether the specified NPC has social data like a birthday and gift tastes.</summary>
        /// <param name="npc">The NPC to check.</param>
        public bool IsSocialVillager(NPC npc)
        {
            if (!npc.isVillager())
                return false;

            if (this.Metadata.Constants.ForceSocialVillagers.TryGetValue(npc.Name, out bool social))
                return social;

            return
                Game1.player.friendshipData.ContainsKey(npc.Name)
                || npc.CanSocialize;
        }

        /// <summary>Get how much each NPC likes receiving an item as a gift.</summary>
        /// <param name="item">The item to check.</param>
        public IEnumerable<GiftTasteModel> GetGiftTastes(Item item)
        {
            if (!item.canBeGivenAsGift())
                yield break;

            foreach (NPC npc in this.GetAllCharacters())
            {
                if (!this.IsSocialVillager(npc))
                    continue;

                GiftTaste? taste = this.GetGiftTaste(npc, item);
                if (taste.HasValue)
                    yield return new GiftTasteModel(npc, item, taste.Value);
            }
        }

        /// <summary>Get the items a specified NPC can receive.</summary>
        /// <param name="npc">The NPC to check.</param>
        public IEnumerable<GiftTasteModel> GetGiftTastes(NPC npc)
        {
            if (!this.IsSocialVillager(npc))
                return Array.Empty<GiftTasteModel>();

            return
                (
                    from entry in this.ItemRepository.GetAll(itemTypes: new[] { ItemType.Object }, includeVariants: false)
                    let item = entry.CreateItem()
                    let taste = this.GetGiftTaste(npc, item)
                    where taste.HasValue
                    select new GiftTasteModel(npc, item, taste.Value)
                );
        }

        /// <summary>Get how much each NPC likes watching this week's movie. If an NPC would reject the movie, the gift taste is set to null.</summary>
        public IEnumerable<KeyValuePair<NPC, GiftTaste?>> GetMovieTastes()
        {
            foreach (NPC npc in this.GetAllCharacters())
            {
                if (!this.IsSocialVillager(npc))
                    continue;

                string rawTaste = MovieTheater.GetResponseForMovie(npc);
                switch (rawTaste)
                {
                    case "love" or "like" or "dislike":
                        yield return new KeyValuePair<NPC, GiftTaste?>(npc, Enum.Parse<GiftTaste>(rawTaste, ignoreCase: true));
                        break;

                    case "reject":
                        yield return new KeyValuePair<NPC, GiftTaste?>(npc, null);
                        break;

                        // any other value will be ignored for movie preferences
                }
            }
        }

        /// <summary>Read parsed data about a fish pond's population gates for a specific fish.</summary>
        /// <param name="data">The fish pond data.</param>
        public IEnumerable<FishPondPopulationGateData> GetFishPondPopulationGates(FishPondData data)
        {
            return this.DataParser.GetFishPondPopulationGates(data);
        }

        /// <summary>Read parsed data about a fish pond's item drops for a specific fish.</summary>
        /// <param name="data">The fish pond data.</param>
        public IEnumerable<FishPondDropData> GetFishPondDrops(FishPondData data)
        {
            return this.DataParser.GetFishPondDrops(data);
        }

        /// <summary>Read parsed data about the spawn rules for a specific fish.</summary>
        /// <param name="fishID">The fish ID.</param>
        /// <remarks>Derived from <see cref="GameLocation.getFish"/>.</remarks>
        public FishSpawnData? GetFishSpawnRules(int fishID)
        {
            return this.DataParser.GetFishSpawnRules(fishID, this.Metadata);
        }

        /// <summary>Get parsed data about the friendship between a player and NPC.</summary>
        /// <param name="player">The player.</param>
        /// <param name="npc">The NPC.</param>
        /// <param name="friendship">The current friendship data.</param>
        public FriendshipModel GetFriendshipForVillager(Farmer player, NPC npc, Friendship friendship)
        {
            return this.DataParser.GetFriendshipForVillager(player, npc, friendship, this.Metadata);
        }

        /// <summary>Get parsed data about the friendship between a player and NPC.</summary>
        /// <param name="player">The player.</param>
        /// <param name="pet">The pet.</param>
        public FriendshipModel GetFriendshipForPet(Farmer player, Pet pet)
        {
            return this.DataParser.GetFriendshipForPet(player, pet);
        }

        /// <summary>Get parsed data about the friendship between a player and NPC.</summary>
        /// <param name="player">The player.</param>
        /// <param name="animal">The farm animal.</param>
        public FriendshipModel GetFriendshipForAnimal(Farmer player, FarmAnimal animal)
        {
            return this.DataParser.GetFriendshipForAnimal(player, animal, this.Metadata);
        }

        /// <summary>Parse monster data.</summary>
        public IEnumerable<MonsterData> GetMonsterData()
        {
            return this.DataParser.GetMonsters();
        }

        /// <summary>Read parsed data about the Community Center bundles.</summary>
        public IEnumerable<BundleModel> GetBundleData()
        {
            return this.DataParser.GetBundles(this.Monitor);
        }

        /// <summary>Get the recipes for which an item is needed.</summary>
        public IEnumerable<RecipeModel> GetRecipes()
        {
            return this.Recipes.Value;
        }

        /// <summary>Get the recipes for which an item is needed.</summary>
        /// <param name="item">The item.</param>
        public IEnumerable<RecipeModel> GetRecipesForIngredient(Item item)
        {
            // ignore invalid ingredients
            if (item.GetItemType() != ItemType.Object)
                return Enumerable.Empty<RecipeModel>();

            // from cached recipes
            List<RecipeModel> recipes = this.GetRecipes()
                .Where(recipe =>
                    recipe.Ingredients.Any(p => p.Matches(item))
                    && !recipe.ExceptIngredients.Any(p => p.Matches(item))
                )
                .ToList();

            // resolve conflicts from mods like Producer Framework Mod: if multiple machine recipes
            // take the same item as input, ID takes precedence over category. This only occurs
            // with mod recipes, since there are no such conflicts in the vanilla recipes.
            recipes.RemoveAll(recipe =>
            {
                if (recipe.Type != RecipeType.MachineInput)
                    return false;

                RecipeIngredientModel? ingredient = recipe.Ingredients.FirstOrDefault();
                return
                    ingredient?.PossibleIds.Any(p => p < 0) == true
                    && recipes.Any(other => other.Ingredients.FirstOrDefault()?.PossibleIds.Contains(item.ParentSheetIndex) == true && other.DisplayType == recipe.DisplayType);
            });

            // from construction recipes
            recipes.AddRange(this.GetConstructionRecipes(item));

            return recipes;
        }

        /// <summary>Get the recipes that create an item.</summary>
        /// <param name="item">The item.</param>
        public IEnumerable<RecipeModel> GetRecipesForOutput(Item item)
        {
            return this
                .GetRecipes()
                .Where(recipe => this.AreEquivalent(item, recipe.TryCreateItem(item)));
        }

        /// <summary>Get the recipes for a given machine.</summary>
        /// <param name="machine">The machine.</param>
        public IEnumerable<RecipeModel> GetRecipesForMachine(SObject? machine)
        {
            if (machine == null)
                return Enumerable.Empty<RecipeModel>();

            // from cached recipes
            return this.GetRecipes()
                .Where(recipe => recipe.IsForMachine(machine))
                .ToList();
        }

        /// <summary>Get the current quests which need an item.</summary>
        /// <param name="item">The item to check.</param>
        public IEnumerable<QuestModel> GetQuestsWhichNeedItem(SObject item)
        {
            // get all quests
            var quests =
                Game1.player.questLog.Select(quest => new QuestModel(quest))
                .Concat(Game1.player.team.specialOrders.Select(order => new QuestModel(order)));

            // get matching quests
            foreach (QuestModel quest in quests)
            {
                bool needsItem =
                    !string.IsNullOrWhiteSpace(quest.DisplayText)
                    && quest.NeedsItem(item);

                if (needsItem)
                    yield return quest;
            }
        }

        /// <summary>Get an object by its parent sprite index if it can be parsed.</summary>
        /// <param name="index">The parent sprite index.</param>
        /// <param name="obj">The constructed object.</param>
        /// <param name="stack">The number of items in the stack.</param>
        /// <param name="bigcraftable">Whether to create a bigcraftable item.</param>
        public bool TryGetObjectBySpriteIndex(int index, [NotNullWhen(true)] out SObject? obj, int stack = 1, bool bigcraftable = false)
        {
            try
            {
                obj = this.GetObjectBySpriteIndex(index, stack, bigcraftable);
                return true;
            }
            catch
            {
                obj = null;
                return false;
            }
        }

        /// <summary>Get an object by its parent sprite index.</summary>
        /// <param name="index">The parent sprite index.</param>
        /// <param name="stack">The number of items in the stack.</param>
        /// <param name="bigcraftable">Whether to create a bigcraftable item.</param>
        public SObject GetObjectBySpriteIndex(int index, int stack = 1, bool bigcraftable = false)
        {
            try
            {
                return bigcraftable
                    ? new SObject(Vector2.Zero, index) { stack = { stack } }
                    : new SObject(index, stack);
            }
            catch (Exception ex)
            {
                string error = $"The game can't construct {(bigcraftable ? "bigcraftable" : "object")} #{index}.";

                var data = bigcraftable ? Game1.bigCraftablesInformation : Game1.objectInformation;
                if (data != null && data.TryGetValue(index, out string? dataStr))
                    error += $"\nRaw data: {dataStr}";
                else
                    error += " No raw data found.";

                throw new InvalidOperationException(error, ex);
            }
        }

        /// <summary>Get an object by its parent sprite index.</summary>
        /// <param name="category">The category number.</param>
        public IEnumerable<SObject> GetObjectsByCategory(int category)
        {
            foreach (var entry in this.Objects.Value.Where(obj => obj.Item.Category == category))
                yield return (SObject)entry.CreateItem();
        }

        /// <summary>Get whether an item can have a quality (which increases its sale price).</summary>
        /// <param name="item">The item.</param>
        public bool CanHaveQuality(Item item)
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
        public Vector2 GetScreenCoordinatesFromCursor()
        {
            return new Vector2(Game1.getMouseX(), Game1.getMouseY());
        }

        /// <summary>Get the viewport coordinates represented by a tile position.</summary>
        /// <param name="coordinates">The absolute coordinates.</param>
        public Vector2 GetScreenCoordinatesFromAbsolute(Vector2 coordinates)
        {
            return coordinates - new Vector2(Game1.uiViewport.X, Game1.uiViewport.Y);
        }

        /// <summary>Get the viewport coordinates represented by a tile position.</summary>
        /// <param name="tile">The tile position.</param>
        public Rectangle GetScreenCoordinatesFromTile(Vector2 tile)
        {
            Vector2 position = this.GetScreenCoordinatesFromAbsolute(tile * new Vector2(Game1.tileSize));
            return new Rectangle((int)position.X, (int)position.Y, Game1.tileSize, Game1.tileSize);
        }

        /// <summary>Get whether a sprite on a given tile could occlude a specified tile position.</summary>
        /// <param name="spriteTile">The tile of the possible sprite.</param>
        /// <param name="occludeTile">The tile to check for possible occlusion.</param>
        /// <param name="spriteSize">The largest expected sprite size (measured in tiles), or <c>null</c> to use <see cref="Constant.MaxTargetSpriteSize"/>.</param>
        public bool CouldSpriteOccludeTile(Vector2 spriteTile, Vector2 occludeTile, Vector2? spriteSize = null)
        {
            spriteSize ??= Constant.MaxTargetSpriteSize;
            return
                spriteTile.Y >= occludeTile.Y // sprites never extend downward from their tile
                && Math.Abs(spriteTile.X - occludeTile.X) <= spriteSize.Value.X
                && Math.Abs(spriteTile.Y - occludeTile.Y) <= spriteSize.Value.Y;
        }

        /// <summary>Get the pixel coordinates within a sprite sheet corresponding to a sprite displayed in the world.</summary>
        /// <param name="worldPosition">The pixel position in the world.</param>
        /// <param name="worldRectangle">The sprite rectangle in the world.</param>
        /// <param name="spriteRectangle">The sprite rectangle in the sprite sheet.</param>
        /// <param name="spriteEffects">The transformation to apply on the sprite.</param>
        public Vector2 GetSpriteSheetCoordinates(Vector2 worldPosition, Rectangle worldRectangle, Rectangle spriteRectangle, SpriteEffects spriteEffects = SpriteEffects.None)
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
        public TPixel GetSpriteSheetPixel<TPixel>(Texture2D spriteSheet, Vector2 position) where TPixel : struct
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
        /// <param name="onlyCustom">Only return the sprite info if it's custom.</param>
        /// <returns>Returns a tuple containing the sprite sheet and the sprite's position and dimensions within the sheet.</returns>
        public SpriteInfo? GetSprite(Item? item, bool onlyCustom = false)
        {
            SObject? obj = item as SObject;

            // Custom Farming Redux
            if (obj != null && this.CustomFarmingRedux.IsLoaded)
            {
                SpriteInfo? data = this.CustomFarmingRedux.GetSprite(obj);
                if (data != null)
                    return data;
            }

            if (onlyCustom)
                return null;

            // standard object
            if (obj != null)
            {
                return obj.bigCraftable.Value
                    ? new SpriteInfo(Game1.bigCraftableSpriteSheet, SObject.getSourceRectForBigCraftable(obj.ParentSheetIndex))
                    : new SpriteInfo(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, obj.ParentSheetIndex, SObject.spriteSheetTileSize, SObject.spriteSheetTileSize));
            }

            // boots or ring
            if (item is Boots or Ring)
            {
                int indexInTileSheet = (item as Boots)?.indexInTileSheet ?? ((Ring)item).indexInTileSheet;
                return new SpriteInfo(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, indexInTileSheet, SObject.spriteSheetTileSize, SObject.spriteSheetTileSize));
            }

            // clothing
            if (item is Clothing clothing)
            {
                switch (clothing.clothesType.Value)
                {
                    case (int)Clothing.ClothesType.SHIRT:
                        return new ShirtSpriteInfo(clothing);

                    case (int)Clothing.ClothesType.PANTS:
                        return new SpriteInfo(FarmerRenderer.pantsTexture, new Rectangle(192 * (clothing.indexInTileSheetMale.Value % (FarmerRenderer.pantsTexture.Width / 192)), 688 * (clothing.indexInTileSheetMale.Value / (FarmerRenderer.pantsTexture.Width / 192)) + 672, 16, 16));
                }
            }

            // hat
            if (item is Hat hat)
                return new SpriteInfo(FarmerRenderer.hatsTexture, new Rectangle(hat.which.Value * 20 % FarmerRenderer.hatsTexture.Width, hat.which.Value * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20));

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
        public Vector2 DrawHoverBox(SpriteBatch spriteBatch, string label, Vector2 position, float wrapWidth)
        {
            return CommonHelper.DrawHoverBox(spriteBatch, label, position, wrapWidth);
        }

        /// <summary>Show an error message to the player.</summary>
        /// <param name="message">The message to show.</param>
        public void ShowErrorMessage(string message)
        {
            CommonHelper.ShowErrorMessage(message);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether two items are the same type (ignoring flavor text like 'blueberry wine' vs 'cranberry wine').</summary>
        /// <param name="a">The first item to compare.</param>
        /// <param name="b">The second item to compare.</param>
        private bool AreEquivalent(Item? a, Item? b)
        {
            if (a == null || b == null || a.ParentSheetIndex != b.ParentSheetIndex)
                return false;

            // special case: torches change from SObject to Torch when placed
            if (new[] { a, b }.All(p => p is Torch || (p.ParentSheetIndex == 93 && p.GetItemType() == ItemType.Object)))
                return true;

            // equivalent
            return
                a.Category == b.Category
                && a.GetType() == b.GetType()
                && a.GetItemType() == b.GetItemType()
                && (a as Boots)?.indexInTileSheet == (b as Boots)?.indexInTileSheet
                && (a as BreakableContainer)?.Type == (b as BreakableContainer)?.Type
                && (a as Fence)?.isGate == (b as Fence)?.isGate
                && (a as Fence)?.whichType == (b as Fence)?.whichType
                && (a as Hat)?.which == (b as Hat)?.which
                && (a as MeleeWeapon)?.type == (b as MeleeWeapon)?.type
                && (a as Ring)?.indexInTileSheet == (b as Ring)?.indexInTileSheet
                && (a as Tool)?.InitialParentTileIndex == (b as Tool)?.InitialParentTileIndex;
        }

        /// <summary>Get all machine recipes, including those from mods like Producer Framework Mod.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="monitor">The monitor with which to log errors.</param>
        private RecipeModel[] GetAllRecipes(IReflectionHelper reflection, IMonitor monitor)
        {
            // get vanilla recipes
            List<RecipeModel> recipes = this.DataParser.GetRecipes(this.Metadata, reflection, monitor).ToList();

            // get recipes from Producer Framework Mod
            if (this.ProducerFrameworkMod.IsLoaded)
            {
                List<RecipeModel> customRecipes = new List<RecipeModel>();
                foreach (ProducerFrameworkRecipe recipe in this.ProducerFrameworkMod.GetRecipes())
                {
                    if (recipe.HasContextTags())
                        continue;

                    // remove vanilla recipes overridden by a PFM one
                    // This is always an integer currently, but the API may return context_tag keys in the future.
                    recipes.RemoveAll(r => r.Type == RecipeType.MachineInput && r.MachineParentSheetIndex == recipe.MachineId && recipe.InputId != null && r.Ingredients[0].PossibleIds.Contains(recipe.InputId.Value));

                    // get machine
                    if (!this.TryGetObjectBySpriteIndex(recipe.MachineId, out SObject? machine, bigcraftable: true))
                        continue;

                    // add recipe
                    customRecipes.Add(new RecipeModel(
                        key: null,
                        type: RecipeType.MachineInput,
                        displayType: machine.DisplayName,
                        ingredients: recipe.Ingredients.Select(p => new RecipeIngredientModel(p.InputId!.Value, p.Count)),
                        item: ingredient =>
                        {
                            SObject output = this.GetObjectBySpriteIndex(recipe.OutputId);
                            if (ingredient?.ParentSheetIndex != null)
                            {
                                output.preservedParentSheetIndex.Value = ingredient.ParentSheetIndex;
                                output.preserve.Value = recipe.PreserveType;
                            }
                            return output;
                        },
                        isKnown: () => true,
                        exceptIngredients: recipe.ExceptIngredients.Select(id => new RecipeIngredientModel(id!.Value, 1)),
                        outputItemIndex: recipe.OutputId,
                        minOutput: recipe.MinOutput,
                        maxOutput: recipe.MaxOutput,
                        outputChance: (decimal)recipe.OutputChance,
                        machineParentSheetIndex: recipe.MachineId,
                        isForMachine: p => p is SObject obj && obj.GetItemType() == ItemType.BigCraftable && obj.ParentSheetIndex == recipe.MachineId
                    ));
                }

                recipes.AddRange(customRecipes);
            }

            // get tailoring recipes
            recipes.AddRange(this.GetAllTailorRecipes());

            return recipes.ToArray();
        }

        /// <summary>Get every valid tailoring recipe.</summary>
        private IEnumerable<RecipeModel> GetAllTailorRecipes()
        {
            // build tag => items cache
            var objectList = this.Objects.Value.Select(p => p.Item).ToArray();
            var contextLookupCache =
                (
                    from item in objectList
                    from tag in item.GetContextTags()
                    select new { item, tag }
                )
                .GroupBy(group => group.tag)
                .ToDictionary(group => group.Key, group => group.Select(p => p.item).ToArray());

            // build cache lookup logic
            Item[] GetObjectsWithTags(List<string> contextTags)
            {
                // simple tag lookup
                if (contextTags.Count == 1 && !contextTags[0].StartsWith("!"))
                {
                    return contextLookupCache.TryGetValue(contextTags[0], out Item[]? items)
                        ? items
                        : Array.Empty<Item>();
                }

                // complex lookup
                {
                    string cacheKey = string.Join("|", contextTags.OrderBy(p => p));

                    if (!contextLookupCache.TryGetValue(cacheKey, out Item[]? items))
                    {
                        contextLookupCache[cacheKey] = items = objectList
                            .Where(entry => contextTags.All(entry.HasContextTag))
                            .ToArray();
                    }

                    return items;
                }
            }

            // build recipe list
            var seenPermutation = new HashSet<string>();
            TailoringMenu tailor = new TailoringMenu();
            foreach (TailorItemRecipe recipe in tailor._tailoringRecipes)
            {
                // get input items
                Item[] clothItems = GetObjectsWithTags(recipe.FirstItemTags);
                Item[] spoolItems = GetObjectsWithTags(recipe.SecondItemTags);

                // get output IDs
                int[] outputItemIds = recipe.CraftedItemIDs?.Any() == true
                    ? recipe.CraftedItemIDs.Select(id => int.TryParse(id, out int value) ? value : -1).ToArray()
                    : new[] { recipe.CraftedItemID };

                // build recipe models
                foreach (int outputId in outputItemIds)
                {
                    if (outputId < 0)
                        continue;

                    foreach (Item clothItem in clothItems)
                    {
                        foreach (Item spoolItem in spoolItems)
                        {
                            // skip if this combination was handled by an earlier recipe
                            if (!seenPermutation.Add($"{clothItem.ParentSheetIndex}|{spoolItem.ParentSheetIndex}"))
                                continue;

                            // get recipe output
                            Item output;
                            try
                            {
                                output = this.GetTailoredItem(outputId, tailor, spoolItem);
                            }
                            catch (Exception ex)
                            {
                                this.Monitor.LogOnce($"Failed to get output #{outputId} for tailoring recipe [{string.Join(", ", recipe.FirstItemTags ?? new())}] + [{string.Join(", ", recipe.SecondItemTags ?? new())}]. Technical details:\n{ex}", LogLevel.Warn);
                                continue;
                            }

                            // build recipe
                            yield return new RecipeModel(
                                key: null,
                                type: RecipeType.TailorInput,
                                displayType: I18n.RecipeType_Tailoring(),
                                ingredients: new[]
                                {
                                    new RecipeIngredientModel(clothItem.ParentSheetIndex, 1),
                                    new RecipeIngredientModel(spoolItem.ParentSheetIndex, 1)
                                },
                                item: _ => output.getOne(),
                                isKnown: () => Game1.player.HasTailoredThisItem(output),
                                outputItemIndex: recipe.CraftedItemID,
                                machineParentSheetIndex: null,
                                isForMachine: _ => false
                            );
                        }
                    }
                }
            }
        }

        /// <summary>Get the item produced by a tailoring recipe based on the output ID.</summary>
        /// <param name="craftedItemId">The output item ID.</param>
        /// <param name="tailor">The tailoring menu.</param>
        /// <param name="spoolItem">The item in the tailoring spool slot.</param>
        /// <remarks>Derived from <see cref="TailoringMenu.CraftItem"/>.</remarks>
        private Item GetTailoredItem(int craftedItemId, TailoringMenu tailor, Item spoolItem)
        {
            Item obj = craftedItemId >= 0 ? (craftedItemId is < 2000 or >= 3000 ? new Clothing(craftedItemId) : new Hat(craftedItemId - 2000)) : new SObject(-craftedItemId, 1);
            if (obj is Clothing clothing)
                tailor.DyeItems(clothing, spoolItem, 1);
            return obj;
        }

        /// <summary>Get an NPC's preference for an item.</summary>
        /// <param name="npc">The NPC whose gift taste to get.</param>
        /// <param name="item">The item to check.</param>
        /// <returns>Returns the NPC's gift taste if applicable, else <c>null</c>.</returns>
        private GiftTaste? GetGiftTaste(NPC npc, Item item)
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

        /// <summary>Get construction recipes which use an item as a building material.</summary>
        /// <param name="input">The ingredient to match.</param>
        /// <remarks>Derived from <see cref="CarpenterMenu(bool)"/>.</remarks>
        private IEnumerable<RecipeModel> GetConstructionRecipes(Item? input)
        {
            if (input?.GetItemType() != ItemType.Object)
                yield break;

            var data = Game1.content.Load<Dictionary<string, string>>("Data\\Blueprints");
            foreach ((string key, string value) in data)
            {
                // ignore invalid blueprints
                if (key == "Mine Elevator" || value.StartsWith("animal/"))
                    continue;

                // parse blueprint
                BluePrint blueprint;
                try
                {
                    blueprint = new BluePrint(key);
                }
                catch
                {
                    continue;
                }

                // create recipe
                if (blueprint.itemsRequired.Any())
                {
                    Building building = new Building(blueprint, Vector2.Zero);
                    var recipe = new RecipeModel(blueprint, building);

                    if (recipe.Ingredients.Any(p => p.Matches(input)))
                        yield return recipe;
                }
            }
        }
    }
}
