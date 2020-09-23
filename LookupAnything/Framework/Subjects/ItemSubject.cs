using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.DataParsers;
using Pathoschild.Stardew.Common.Items.ItemData;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.FishPond;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a Stardew Valley item.</summary>
    internal class ItemSubject : BaseSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>The lookup target.</summary>
        private readonly Item Target;

        /// <summary>The menu item to render, which may be different from the item that was looked up (e.g. for fences).</summary>
        private readonly Item DisplayItem;

        /// <summary>The crop which will drop the item (if applicable).</summary>
        private readonly Crop FromCrop;

        /// <summary>The crop grown by this seed item (if applicable).</summary>
        private readonly Crop SeedForCrop;

        /// <summary>The context of the object being looked up.</summary>
        private readonly ObjectContext Context;

        /// <summary>Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</summary>
        private readonly bool KnownQuality;

        /// <summary>Whether to only show content once the player discovers it.</summary>
        private readonly bool ProgressionMode;

        /// <summary>Whether to highlight item gift tastes which haven't been revealed in the NPC profile.</summary>
        private readonly bool HighlightUnrevealedGiftTastes;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="progressionMode">Whether to only show content once the player discovers it.</param>
        /// <param name="highlightUnrevealedGiftTastes">Whether to highlight item gift tastes which haven't been revealed in the NPC profile.</param>
        /// <param name="item">The underlying target.</param>
        /// <param name="context">The context of the object being looked up.</param>
        /// <param name="knownQuality">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        /// <param name="fromCrop">The crop associated with the item (if applicable).</param>
        public ItemSubject(SubjectFactory codex, GameHelper gameHelper, bool progressionMode, bool highlightUnrevealedGiftTastes, Item item, ObjectContext context, bool knownQuality, Crop fromCrop = null)
            : base(codex, gameHelper)
        {
            this.ProgressionMode = progressionMode;
            this.HighlightUnrevealedGiftTastes = highlightUnrevealedGiftTastes;
            this.Target = item;
            this.DisplayItem = this.GetMenuItem(item);
            this.FromCrop = fromCrop;
            if ((item as SObject)?.Type == "Seeds" && fromCrop == null) // fromCrop == null to exclude planted coffee beans
                this.SeedForCrop = new Crop(item.ParentSheetIndex, 0, 0);
            this.Context = context;
            this.KnownQuality = knownQuality;
            this.Initialize(this.DisplayItem.DisplayName, this.GetDescription(this.DisplayItem), this.GetTypeValue(this.DisplayItem));
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<ICustomField> GetData()
        {
            // get data
            Item item = this.Target;
            ItemType itemType = item.GetItemType();
            SObject obj = item as SObject;
            bool isCrop = this.FromCrop != null;
            bool isSeed = this.SeedForCrop != null;
            bool isDeadCrop = this.FromCrop?.dead.Value == true;
            bool canSell = obj?.canBeShipped() == true || this.Metadata.Shops.Any(shop => shop.BuysCategories.Contains(item.Category));

            // get overrides
            bool showInventoryFields = true;
            {
                ObjectData objData = this.Metadata.GetObject(item, this.Context);
                if (objData != null)
                {
                    this.Name = objData.NameKey != null ? I18n.GetByKey(objData.NameKey) : this.Name;
                    this.Description = objData.DescriptionKey != null ? I18n.GetByKey(objData.DescriptionKey) : this.Description;
                    this.Type = objData.TypeKey != null ? I18n.GetByKey(objData.TypeKey) : this.Type;
                    showInventoryFields = objData.ShowInventoryFields ?? true;
                }
            }

            // don't show data for dead crop
            if (isDeadCrop)
            {
                yield return new GenericField(this.GameHelper, I18n.Crop_Summary(), I18n.Crop_Summary_Dead());
                yield break;
            }

            // crop fields
            foreach (ICustomField field in this.GetCropFields(this.FromCrop ?? this.SeedForCrop, isSeed))
                yield return field;

            // indoor pot crop
            if (obj is IndoorPot pot)
            {
                Crop potCrop = pot.hoeDirt.Value.crop;
                if (potCrop != null)
                {
                    Item drop = this.GameHelper.GetObjectBySpriteIndex(potCrop.indexOfHarvest.Value);
                    yield return new LinkField(this.GameHelper, I18n.Item_Contents(), drop.DisplayName, () => this.Codex.GetCrop(potCrop, ObjectContext.World));
                }
            }

            // machine output
            foreach (ICustomField field in this.GetMachineOutputFields(obj))
                yield return field;

            // item
            if (showInventoryFields)
            {
                // needed for
                foreach (ICustomField field in this.GetNeededForFields(obj))
                    yield return field;

                // sale data
                if (canSell && !isCrop)
                {
                    // sale price
                    string saleValueSummary = GenericField.GetSaleValueString(this.GetSaleValue(item, this.KnownQuality), item.Stack);
                    yield return new GenericField(this.GameHelper, I18n.Item_SellsFor(), saleValueSummary);

                    // sell to
                    List<string> buyers = new List<string>();
                    if (obj?.canBeShipped() == true)
                        buyers.Add(I18n.Item_SellsTo_ShippingBox());
                    buyers.AddRange(
                        from shop in this.Metadata.Shops
                        where shop.BuysCategories.Contains(item.Category)
                        let name = I18n.GetByKey(shop.DisplayKey).ToString()
                        orderby name
                        select name
                    );
                    yield return new GenericField(this.GameHelper, I18n.Item_SellsTo(), string.Join(", ", buyers));
                }

                // clothing
                if (item is Clothing clothing)
                    yield return new GenericField(this.GameHelper, I18n.Item_CanBeDyed(), this.Stringify(clothing.dyeable.Value));

                // gift tastes
                IDictionary<GiftTaste, GiftTasteModel[]> giftTastes = this.GetGiftTastes(item);
                yield return new ItemGiftTastesField(this.GameHelper, I18n.Item_LovesThis(), giftTastes, GiftTaste.Love, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes);
                yield return new ItemGiftTastesField(this.GameHelper, I18n.Item_LikesThis(), giftTastes, GiftTaste.Like, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes);
                if (this.ProgressionMode || this.HighlightUnrevealedGiftTastes)
                {
                    yield return new ItemGiftTastesField(this.GameHelper, I18n.Item_NeutralAboutThis(), giftTastes, GiftTaste.Neutral, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes);
                    yield return new ItemGiftTastesField(this.GameHelper, I18n.Item_DislikesThis(), giftTastes, GiftTaste.Dislike, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes);
                    yield return new ItemGiftTastesField(this.GameHelper, I18n.Item_HatesThis(), giftTastes, GiftTaste.Hate, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes);
                }
            }

            // recipes
            switch (itemType)
            {
                // for ingredient
                case ItemType.Object:
                    {
                        RecipeModel[] recipes = this.GameHelper.GetRecipesForIngredient(this.DisplayItem).ToArray();
                        if (recipes.Any())
                            yield return new RecipesForIngredientField(this.GameHelper, I18n.Item_Recipes(), item, recipes);
                    }
                    break;

                // for machine
                case ItemType.BigCraftable:
                    {
                        RecipeModel[] recipes = this.GameHelper.GetRecipesForMachine(this.DisplayItem as SObject).ToArray();
                        if (recipes.Any())
                            yield return new RecipesForMachineField(this.GameHelper, I18n.Item_Recipes(), recipes);
                    }
                    break;
            }

            // fish
            if (item.Category == SObject.FishCategory)
            {
                // spawn rules
                yield return new FishSpawnRulesField(this.GameHelper, I18n.Item_FishSpawnRules(), item.ParentSheetIndex);

                // fish pond data
                foreach (FishPondData fishPondData in Game1.content.Load<List<FishPondData>>("Data\\FishPondData"))
                {
                    if (!fishPondData.RequiredTags.All(item.HasContextTag))
                        continue;

                    int minChanceOfAnyDrop = (int)Math.Round(Utility.Lerp(0.15f, 0.95f, 1 / 10f) * 100);
                    int maxChanceOfAnyDrop = (int)Math.Round(Utility.Lerp(0.15f, 0.95f, FishPond.MAXIMUM_OCCUPANCY / 10f) * 100);
                    string preface = I18n.Building_FishPond_Drops_Preface(chance: I18n.Generic_Range(min: minChanceOfAnyDrop, max: maxChanceOfAnyDrop));
                    yield return new FishPondDropsField(this.GameHelper, I18n.Item_FishPondDrops(), -1, fishPondData, preface);
                    break;
                }
            }

            // fence
            if (item is Fence fence)
            {
                string healthLabel = I18n.Item_FenceHealth();

                // health
                if (Game1.getFarm().isBuildingConstructed(Constant.BuildingNames.GoldClock))
                    yield return new GenericField(this.GameHelper, healthLabel, I18n.Item_FenceHealth_GoldClock());
                else
                {
                    float maxHealth = fence.isGate.Value ? fence.maxHealth.Value * 2 : fence.maxHealth.Value;
                    float health = fence.health.Value / maxHealth;
                    double daysLeft = Math.Round(fence.health.Value * this.Constants.FenceDecayRate / 60 / 24);
                    double percent = Math.Round(health * 100);
                    yield return new PercentageBarField(this.GameHelper, healthLabel, (int)fence.health.Value, (int)maxHealth, Color.Green, Color.Red, I18n.Item_FenceHealth_Summary(percent: (int)percent, count: (int)daysLeft));
                }
            }

            // movie ticket
            if (obj?.ParentSheetIndex == 809 && !obj.bigCraftable.Value)
            {
                MovieData movie = MovieTheater.GetMovieForDate(Game1.Date);
                if (movie == null)
                    yield return new GenericField(this.GameHelper, I18n.Item_MovieTicket_MovieThisWeek(), I18n.Item_MovieTicket_MovieThisWeek_None());
                else
                {
                    // movie this week
                    yield return new GenericField(this.GameHelper, I18n.Item_MovieTicket_MovieThisWeek(), new IFormattedText[]
                    {
                        new FormattedText(movie.Title, bold: true),
                        new FormattedText(Environment.NewLine),
                        new FormattedText(movie.Description)
                    });

                    // movie tastes
                    IDictionary<GiftTaste, string[]> tastes = this.GameHelper.GetMovieTastes()
                        .GroupBy(entry => entry.Value)
                        .ToDictionary(group => group.Key, group => group.Select(p => p.Key.Name).OrderBy(p => p).ToArray());

                    yield return new MovieTastesField(this.GameHelper, I18n.Item_MovieTicket_LovesMovie(), tastes, GiftTaste.Love);
                    yield return new MovieTastesField(this.GameHelper, I18n.Item_MovieTicket_LikesMovie(), tastes, GiftTaste.Like);
                    yield return new MovieTastesField(this.GameHelper, I18n.Item_MovieTicket_DislikesMovie(), tastes, GiftTaste.Dislike);
                }
            }

            // dyes
            yield return new ColorField(this.GameHelper, I18n.Item_ProducesDye(), item);

            // owned and times cooked/crafted
            if (showInventoryFields && !isCrop)
            {
                // owned
                yield return new GenericField(this.GameHelper, I18n.Item_NumberOwned(), I18n.Item_NumberOwned_Summary(count: this.GameHelper.CountOwnedItems(item)));

                // times crafted
                RecipeModel[] recipes = this.GameHelper
                    .GetRecipes()
                    .Where(recipe => recipe.OutputItemIndex == this.Target.ParentSheetIndex && recipe.OutputItemType == this.Target.GetItemType())
                    .ToArray();
                if (recipes.Any())
                {
                    string label = recipes.First().Type == RecipeType.Cooking ? I18n.Item_NumberCooked() : I18n.Item_NumberCrafted();
                    int timesCrafted = recipes.Sum(recipe => recipe.GetTimesCrafted(Game1.player));
                    if (timesCrafted >= 0) // negative value means not available for this recipe type
                        yield return new GenericField(this.GameHelper, label, I18n.Item_NumberCrafted_Summary(count: timesCrafted));
                }
            }

            // see also crop
            bool seeAlsoCrop =
                isSeed
                && item.ParentSheetIndex != this.SeedForCrop.indexOfHarvest.Value // skip seeds which produce themselves (e.g. coffee beans)
                && !(item.ParentSheetIndex >= 495 && item.ParentSheetIndex <= 497) // skip random seasonal seeds
                && item.ParentSheetIndex != 770; // skip mixed seeds
            if (seeAlsoCrop)
            {
                Item drop = this.GameHelper.GetObjectBySpriteIndex(this.SeedForCrop.indexOfHarvest.Value);
                yield return new LinkField(this.GameHelper, I18n.Item_SeeAlso(), drop.DisplayName, () => this.Codex.GetCrop(this.SeedForCrop, ObjectContext.Inventory));
            }
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<IDebugField> GetDebugFields()
        {
            Item target = this.Target;
            SObject obj = target as SObject;
            Crop crop = this.FromCrop ?? this.SeedForCrop;

            // pinned fields
            yield return new GenericDebugField("item ID", target.ParentSheetIndex, pinned: true);
            yield return new GenericDebugField("category", $"{target.Category} ({target.getCategoryName()})", pinned: true);
            if (obj != null)
            {
                yield return new GenericDebugField("edibility", obj.Edibility, pinned: true);
                yield return new GenericDebugField("item type", obj.Type, pinned: true);
            }
            if (crop != null)
            {
                yield return new GenericDebugField("crop fully grown", this.Stringify(crop.fullyGrown.Value), pinned: true);
                yield return new GenericDebugField("crop phase", $"{crop.currentPhase} (day {crop.dayOfCurrentPhase} in phase)", pinned: true);
            }

            // raw fields
            foreach (IDebugField field in this.GetDebugFieldsFrom(target))
                yield return field;
            if (crop != null)
            {
                foreach (IDebugField field in this.GetDebugFieldsFrom(crop))
                    yield return new GenericDebugField($"crop::{field.Label}", field.Value, field.HasValue, field.IsPinned);
            }
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            this.DisplayItem.drawInMenu(spriteBatch, position, 1, 1f, 1f, StackDrawType.Hide, Color.White, false);
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the equivalent menu item for the specified target. (For example, the inventory item matching a fence object.)</summary>
        /// <param name="item">The target item.</param>
        private Item GetMenuItem(Item item)
        {
            // fence
            if (item is Fence fence)
            {
                // get equivalent object's sprite ID
                FenceType fenceType = (FenceType)fence.whichType.Value;
                int? spriteID = null;
                if (fence.isGate.Value)
                    spriteID = 325;
                else if (fenceType == FenceType.Wood)
                    spriteID = 322;
                else if (fenceType == FenceType.Stone)
                    spriteID = 323;
                else if (fenceType == FenceType.Iron)
                    spriteID = 324;
                else if (fenceType == FenceType.Hardwood)
                    spriteID = 298;

                // get object
                if (spriteID.HasValue)
                    return new SObject(spriteID.Value, 1);
            }

            return item;
        }

        /// <summary>Get the item description.</summary>
        /// <param name="item">The item.</param>
        [SuppressMessage("ReSharper", "AssignmentIsFullyDiscarded", Justification = "Discarding the value is deliberate. We need to call the property to trigger the data load, but we don't actually need the result.")]
        private string GetDescription(Item item)
        {
            try
            {
                _ = item.DisplayName; // force display name to load, which is needed to get the description outside the inventory for some reason
                return item.getDescription();
            }
            catch (KeyNotFoundException)
            {
                return null; // e.g. incubator
            }
        }

        /// <summary>Get the item type.</summary>
        /// <param name="item">The item.</param>
        private string GetTypeValue(Item item)
        {
            string categoryName = item.getCategoryName();
            return !string.IsNullOrWhiteSpace(categoryName)
                ? categoryName
                : I18n.Type_Other();
        }

        /// <summary>Get the custom fields for a crop.</summary>
        /// <param name="crop">The crop to represent.</param>
        /// <param name="isSeed">Whether the crop being displayed is for an unplanted seed.</param>
        private IEnumerable<ICustomField> GetCropFields(Crop crop, bool isSeed)
        {
            if (crop == null)
                yield break;

            var data = new CropDataParser(crop, isPlanted: !isSeed);

            // add next-harvest field
            if (!isSeed)
            {
                // get next harvest
                SDate nextHarvest = data.GetNextHarvest();

                // generate field
                string summary;
                if (data.CanHarvestNow)
                    summary = I18n.Generic_Now();
                else if (!Game1.currentLocation.IsGreenhouse && !data.Seasons.Contains(nextHarvest.Season))
                    summary = I18n.Crop_Harvest_TooLate(date: this.Stringify(nextHarvest));
                else
                    summary = $"{this.Stringify(nextHarvest)} ({this.GetRelativeDateStr(nextHarvest)})";

                yield return new GenericField(this.GameHelper, I18n.Crop_Harvest(), summary);
            }

            // crop summary
            {
                List<string> summary = new List<string>();

                // harvest
                summary.Add(data.HasMultipleHarvests
                    ? I18n.Crop_Summary_HarvestOnce(daysToFirstHarvest: data.DaysToFirstHarvest)
                    : I18n.Crop_Summary_HarvestMulti(daysToFirstHarvest: data.DaysToFirstHarvest, daysToNextHarvests: data.DaysToSubsequentHarvest)
                );

                // seasons
                summary.Add(I18n.Crop_Summary_Seasons(seasons: string.Join(", ", I18n.GetSeasonNames(data.Seasons))));

                // drops
                if (crop.minHarvest != crop.maxHarvest && crop.chanceForExtraCrops.Value > 0)
                    summary.Add(I18n.Crop_Summary_DropsXToY(min: crop.minHarvest.Value, max: crop.maxHarvest.Value, percent: (int)Math.Round(crop.chanceForExtraCrops.Value * 100, 2)));
                else if (crop.minHarvest.Value > 1)
                    summary.Add(I18n.Crop_Summary_DropsX(count: crop.minHarvest.Value));

                // crop sale price
                Item drop = data.GetSampleDrop();
                summary.Add(I18n.Crop_Summary_SellsFor(price: GenericField.GetSaleValueString(this.GetSaleValue(drop, false), 1)));

                // generate field
                yield return new GenericField(this.GameHelper, I18n.Crop_Summary(), "-" + string.Join($"{Environment.NewLine}-", summary));
            }
        }

        /// <summary>Get the custom fields for machine output.</summary>
        /// <param name="machine">The machine whose output to represent.</param>
        private IEnumerable<ICustomField> GetMachineOutputFields(SObject machine)
        {
            if (machine == null)
                yield break;

            SObject heldObj = machine.heldObject.Value;
            int minutesLeft = machine.MinutesUntilReady;

            // cask
            if (machine is Cask cask)
            {
                // output item
                if (heldObj != null)
                {
                    ItemQuality curQuality = (ItemQuality)heldObj.Quality;

                    // calculate aging schedule
                    float effectiveAge = this.Constants.CaskAgeSchedule.Values.Max() - cask.daysToMature.Value;
                    var schedule =
                        (
                            from entry in this.Constants.CaskAgeSchedule
                            let quality = entry.Key
                            let baseDays = entry.Value
                            where baseDays > effectiveAge
                            orderby baseDays ascending
                            let daysLeft = (int)Math.Ceiling((baseDays - effectiveAge) / cask.agingRate.Value)
                            select new
                            {
                                Quality = quality,
                                DaysLeft = daysLeft,
                                HarvestDate = SDate.Now().AddDays(daysLeft)
                            }
                        )
                        .ToArray();

                    // display fields
                    yield return new ItemIconField(this.GameHelper, I18n.Item_Contents(), heldObj);
                    if (minutesLeft <= 0 || !schedule.Any())
                        yield return new GenericField(this.GameHelper, I18n.Item_CaskSchedule(), I18n.Item_CaskSchedule_Now(quality: curQuality));
                    else
                    {
                        string scheduleStr = string.Join(Environment.NewLine, (
                            from entry in schedule
                            let str = I18n.GetPlural(entry.DaysLeft, I18n.Item_CaskSchedule_Tomorrow(quality: entry.Quality), I18n.Item_CaskSchedule_InXDays(quality: entry.Quality, count: entry.DaysLeft, date: entry.HarvestDate))
                            select $"-{str}"
                        ));
                        yield return new GenericField(this.GameHelper, I18n.Item_CaskSchedule(), $"{I18n.Item_CaskSchedule_NowPartial(quality: curQuality)}{Environment.NewLine}{scheduleStr}");
                    }
                }
            }

            // crab pot
            else if (machine is CrabPot pot)
            {
                // bait
                if (heldObj == null)
                {
                    if (pot.bait.Value != null)
                        yield return new ItemIconField(this.GameHelper, I18n.Item_CrabpotBait(), pot.bait.Value);
                    else if (Game1.player.professions.Contains(11)) // no bait needed if luremaster
                        yield return new GenericField(this.GameHelper, I18n.Item_CrabpotBait(), I18n.Item_CrabpotBaitNotNeeded());
                    else
                        yield return new GenericField(this.GameHelper, I18n.Item_CrabpotBait(), I18n.Item_CrabpotBaitNeeded());
                }

                // output item
                if (heldObj != null)
                {
                    string summary = I18n.Item_Contents_Ready(name: heldObj.DisplayName);
                    yield return new ItemIconField(this.GameHelper, I18n.Item_Contents(), heldObj, summary);
                }
            }

            // furniture
            else if (machine is Furniture)
            {
                // displayed item
                if (heldObj != null)
                {
                    string summary = I18n.Item_Contents_Placed(name: heldObj.DisplayName);
                    yield return new ItemIconField(this.GameHelper, I18n.Item_Contents(), heldObj, summary);
                }
            }

            // auto-grabber
            else if (machine.ParentSheetIndex == Constant.ObjectIndexes.AutoGrabber)
            {
                string readyText = I18n.Stringify(heldObj is Chest output && output.items.Any());
                yield return new GenericField(this.GameHelper, I18n.Item_Contents(), readyText);
            }

            // generic machine
            else
            {
                // output item
                if (heldObj != null)
                {

                    string summary = minutesLeft <= 0
                    ? I18n.Item_Contents_Ready(name: heldObj.DisplayName)
                    : I18n.Item_Contents_Partial(name: heldObj.DisplayName, time: this.Stringify(TimeSpan.FromMinutes(minutesLeft)));
                    yield return new ItemIconField(this.GameHelper, I18n.Item_Contents(), heldObj, summary);
                }
            }
        }

        /// <summary>Get the custom fields indicating what an item is needed for.</summary>
        /// <param name="obj">The machine whose output to represent.</param>
        private IEnumerable<ICustomField> GetNeededForFields(SObject obj)
        {
            if (obj == null || obj.GetItemType() != ItemType.Object)
                yield break;

            List<string> neededFor = new List<string>();

            // bundles
            {
                string[] missingBundles =
                    (
                        from bundle in this.GetUnfinishedBundles(obj)
                        orderby bundle.Area, bundle.DisplayName
                        let countNeeded = this.GetIngredientCountNeeded(bundle, obj)
                        select countNeeded > 1
                            ? $"{this.GetTranslatedBundleArea(bundle)}: {bundle.DisplayName} x {countNeeded}"
                            : $"{this.GetTranslatedBundleArea(bundle)}: {bundle.DisplayName}"
                    )
                    .ToArray();
                if (missingBundles.Any())
                    neededFor.Add(I18n.Item_NeededFor_CommunityCenter(bundles: string.Join(", ", missingBundles)));
            }

            // polyculture achievement (ship 15 crops)
            if (this.Constants.PolycultureCrops.Contains(obj.ParentSheetIndex))
            {
                int needed = this.Constants.PolycultureCount - this.GameHelper.GetShipped(obj.ParentSheetIndex);
                if (needed > 0)
                    neededFor.Add(I18n.Item_NeededFor_Polyculture(count: needed));
            }

            // full shipment achievement (ship every item)
            if (this.GameHelper.GetFullShipmentAchievementItems().Any(p => p.Key == obj.ParentSheetIndex && !p.Value))
                neededFor.Add(I18n.Item_NeededFor_FullShipment());

            // full collection achievement (donate every artifact)
            if (obj.needsToBeDonated())
                neededFor.Add(I18n.Item_NeededFor_FullCollection());

            // recipe achievements
            {
                var recipes =
                    (
                        from recipe in this.GameHelper.GetRecipesForIngredient(this.DisplayItem)
                        let item = recipe.CreateItem(this.DisplayItem)
                        orderby item.DisplayName
                        select new { recipe.Type, item.DisplayName, TimesCrafted = recipe.GetTimesCrafted(Game1.player) }
                    )
                    .ToArray();

                // gourmet chef achievement (cook every recipe)
                string[] uncookedNames = (from recipe in recipes where recipe.Type == RecipeType.Cooking && recipe.TimesCrafted <= 0 select recipe.DisplayName).ToArray();
                if (uncookedNames.Any())
                    neededFor.Add(I18n.Item_NeededFor_GourmetChef(recipes: string.Join(", ", uncookedNames)));

                // craft master achievement (craft every item)
                string[] uncraftedNames = (from recipe in recipes where recipe.Type == RecipeType.Crafting && recipe.TimesCrafted <= 0 select recipe.DisplayName).ToArray();
                if (uncraftedNames.Any())
                    neededFor.Add(I18n.Item_NeededFor_CraftMaster(recipes: string.Join(", ", uncraftedNames)));
            }

            // yield
            if (neededFor.Any())
                yield return new GenericField(this.GameHelper, I18n.Item_NeededFor(), string.Join(", ", neededFor));
        }

        /// <summary>Get unfinished bundles which require this item.</summary>
        /// <param name="item">The item for which to find bundles.</param>
        private IEnumerable<BundleModel> GetUnfinishedBundles(SObject item)
        {
            // no bundles for Joja members
            if (Game1.player.hasOrWillReceiveMail(Constant.MailLetters.JojaMember))
                yield break;

            // avoid false positives
            if (item.bigCraftable.Value || item is Cask || item is Fence || item is Furniture || item is IndoorPot || item is Sign || item is Torch || item is Wallpaper)
                yield break; // avoid false positives

            // get community center
            CommunityCenter communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            if (communityCenter.areAllAreasComplete() && communityCenter.isBundleComplete(36))
                yield break;

            // get bundles
            foreach (BundleModel bundle in this.GameHelper.GetBundleData())
            {
                // ignore completed bundle
                if (communityCenter.isBundleComplete(bundle.ID))
                    continue;

                bool isMissing = this.GetIngredientsFromBundle(bundle, item).Any(p => this.IsIngredientNeeded(bundle, p));
                if (isMissing)
                    yield return bundle;
            }
        }

        /// <summary>Get the translated name for a bundle's area.</summary>
        /// <param name="bundle">The bundle.</param>
        private string GetTranslatedBundleArea(BundleModel bundle)
        {
            switch (bundle.Area)
            {
                case "Pantry":
                    return I18n.BundleArea_Pantry();
                case "Crafts Room":
                    return I18n.BundleArea_CraftsRoom();
                case "Fish Tank":
                    return I18n.BundleArea_FishTank();
                case "Boiler Room":
                    return I18n.BundleArea_BoilerRoom();
                case "Vault":
                    return I18n.BundleArea_Vault();
                case "Bulletin Board":
                    return I18n.BundleArea_BulletinBoard();
                case "Abandoned Joja Mart":
                    return I18n.BundleArea_AbandonedJojaMart();
                default:
                    return bundle.Area;
            }
        }

        /// <summary>Get the possible sale values for an item.</summary>
        /// <param name="item">The item.</param>
        /// <param name="qualityIsKnown">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        private IDictionary<ItemQuality, int> GetSaleValue(Item item, bool qualityIsKnown)
        {
            // get sale price
            // derived from Utility.getSellToStorePriceOfItem
            int GetPrice(Item i)
            {
                int price = (i as SObject)?.sellToStorePrice() ?? (i.salePrice() / 2);
                return price > 0 ? price : 0;
            }

            // single quality
            if (!this.GameHelper.CanHaveQuality(item) || qualityIsKnown)
            {
                ItemQuality quality = qualityIsKnown && item is SObject obj
                    ? (ItemQuality)obj.Quality
                    : ItemQuality.Normal;

                return new Dictionary<ItemQuality, int> { [quality] = GetPrice(item) };
            }

            // multiple qualities
            int[] iridiumItems = this.Constants.ItemsWithIridiumQuality;
            var prices = new Dictionary<ItemQuality, int>
            {
                [ItemQuality.Normal] = GetPrice(new SObject(item.ParentSheetIndex, 1)),
                [ItemQuality.Silver] = GetPrice(new SObject(item.ParentSheetIndex, 1, quality: (int)ItemQuality.Silver)),
                [ItemQuality.Gold] = GetPrice(new SObject(item.ParentSheetIndex, 1, quality: (int)ItemQuality.Gold))
            };
            if (item.GetItemType() == ItemType.Object && (iridiumItems.Contains(item.Category) || iridiumItems.Contains(item.ParentSheetIndex)))
                prices[ItemQuality.Iridium] = GetPrice(new SObject(item.ParentSheetIndex, 1, quality: (int)ItemQuality.Iridium));
            return prices;
        }

        /// <summary>Get how much each NPC likes receiving an item as a gift.</summary>
        /// <param name="item">The potential gift item.</param>
        private IDictionary<GiftTaste, GiftTasteModel[]> GetGiftTastes(Item item)
        {
            return this.GameHelper.GetGiftTastes(item)
                .GroupBy(p => p.Taste)
                .ToDictionary(p => p.Key, p => p.Distinct().ToArray());
        }

        /// <summary>Get bundle ingredients matching the given item.</summary>
        /// <param name="bundle">The bundle to search.</param>
        /// <param name="item">The item to match.</param>
        private IEnumerable<BundleIngredientModel> GetIngredientsFromBundle(BundleModel bundle, SObject item)
        {
            return bundle.Ingredients
                .Where(p => p.ItemID == item.ParentSheetIndex && p.Quality <= (ItemQuality)item.Quality); // get ingredients
        }

        /// <summary>Get whether an ingredient is still needed for a bundle.</summary>
        /// <param name="bundle">The bundle to check.</param>
        /// <param name="ingredient">The ingredient to check.</param>
        private bool IsIngredientNeeded(BundleModel bundle, BundleIngredientModel ingredient)
        {
            CommunityCenter communityCenter = Game1.locations.OfType<CommunityCenter>().First();

            return !communityCenter.bundles[bundle.ID][ingredient.Index];
        }

        /// <summary>Get the number of an ingredient needed for a bundle.</summary>
        /// <param name="bundle">The bundle to check.</param>
        /// <param name="item">The ingredient to check.</param>
        private int GetIngredientCountNeeded(BundleModel bundle, SObject item)
        {
            return this.GetIngredientsFromBundle(bundle, item)
                .Where(p => this.IsIngredientNeeded(bundle, p))
                .Sum(p => p.Stack);
        }
    }
}
