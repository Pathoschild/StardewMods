using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Data;
using Pathoschild.LookupAnything.Framework.Fields;
using Pathoschild.LookupAnything.Framework.Models;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a Stardew Valley item.</summary>
    internal class ItemSubject : BaseSubject
    {
        /*********
        ** Properties
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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="item">The underlying target.</param>
        /// <param name="context">The context of the object being looked up.</param>
        /// <param name="knownQuality">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        /// <param name="fromCrop">The crop associated with the item (if applicable).</param>
        public ItemSubject(Item item, ObjectContext context, bool knownQuality, Crop fromCrop = null)
        {
            this.Target = item;
            this.DisplayItem = this.GetMenuItem(item);
            this.FromCrop = fromCrop;
            if ((item as Object)?.Type == "Seeds")
                this.SeedForCrop = new Crop(item.parentSheetIndex, 0, 0);
            this.Context = context;
            this.KnownQuality = knownQuality;
            this.Initialise(this.DisplayItem.Name, this.GetDescription(this.DisplayItem), this.GetTypeValue(this.DisplayItem));
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            // get data
            Item item = this.Target;
            Object obj = item as Object;
            bool isCrop = this.FromCrop != null;
            bool isSeed = this.SeedForCrop != null;
            bool isDeadCrop = this.FromCrop?.dead == true;

            // get overrides
            bool showInventoryFields = true;
            {
                ObjectData objData = metadata.GetObject(item, this.Context);
                if (objData != null)
                {
                    this.Name = objData.Name ?? this.Name;
                    this.Description = objData.Description ?? this.Description;
                    this.Type = objData.Type ?? this.Type;
                    showInventoryFields = objData.ShowInventoryFields ?? true;
                }
            }

            // don't show data for dead crop
            if (isDeadCrop)
            {
                yield return new GenericField("Crop", "This crop is dead.");
                yield break;
            }

            // crop fields
            if (isCrop || isSeed)
            {
                // get crop
                Crop crop = this.FromCrop ?? this.SeedForCrop;

                // get harvest schedule
                int harvestablePhase = crop.phaseDays.Count - 1;
                bool canHarvestNow = (crop.currentPhase >= harvestablePhase) && (!crop.fullyGrown || crop.dayOfCurrentPhase <= 0);
                int daysToFirstHarvest = crop.phaseDays.Take(crop.phaseDays.Count - 1).Sum(); // ignore harvestable phase

                // add next-harvest field
                if (isCrop)
                {
                    // calculate next harvest
                    int daysToNextHarvest = 0;
                    Tuple<string, int> dayOfNextHarvest = null;
                    if (!canHarvestNow)
                    {
                        // calculate days until next harvest
                        int daysUntilLastPhase = daysToFirstHarvest - crop.dayOfCurrentPhase - crop.phaseDays.Take(crop.currentPhase).Sum();
                        {
                            // growing: days until next harvest
                            if (!crop.fullyGrown)
                                daysToNextHarvest = daysUntilLastPhase;

                            // regrowable crop harvested today
                            else if (crop.dayOfCurrentPhase >= crop.regrowAfterHarvest)
                                daysToNextHarvest = crop.regrowAfterHarvest;

                            // regrowable crop
                            else
                                daysToNextHarvest = crop.dayOfCurrentPhase; // dayOfCurrentPhase decreases to 0 when fully grown, where <=0 is harvestable
                        }
                        dayOfNextHarvest = GameHelper.GetDayOffset(daysToNextHarvest, metadata.Constants.DaysInSeason);
                    }

                    // generate field
                    string summary;
                    if (canHarvestNow)
                        summary = "now";
                    else if (Game1.currentLocation.Name != Constant.LocationNames.Greenhouse && !crop.seasonsToGrowIn.Contains(dayOfNextHarvest.Item1))
                        summary = $"too late in the season for the next harvest (would be on {dayOfNextHarvest.Item1} {dayOfNextHarvest.Item2})";
                    else
                        summary = $"{dayOfNextHarvest.Item1} {dayOfNextHarvest.Item2} ({GameHelper.Pluralise(daysToNextHarvest, "tomorrow", $"in {daysToNextHarvest} days")})";

                    yield return new GenericField("Harvest", summary);
                }

                // crop summary
                {
                    List<string> summary = new List<string>();

                    // harvest
                    summary.Add($"-harvest after {daysToFirstHarvest} {GameHelper.Pluralise(daysToFirstHarvest, "day")}" + (crop.regrowAfterHarvest != -1 ? $", then every {GameHelper.Pluralise(crop.regrowAfterHarvest, "day", $"{crop.regrowAfterHarvest} days")}" : ""));

                    // seasons
                    summary.Add($"-grows in {string.Join(", ", crop.seasonsToGrowIn)}");

                    // drops
                    if (crop.minHarvest != crop.maxHarvest && crop.chanceForExtraCrops > 0)
                        summary.Add($"-drops {crop.minHarvest} to {crop.maxHarvest} ({Math.Round(crop.chanceForExtraCrops * 100, 2)}% chance of extra crops)");
                    else if (crop.minHarvest > 1)
                        summary.Add($"-drops {crop.minHarvest}");

                    // crop sale price
                    Item drop = GameHelper.GetObjectBySpriteIndex(crop.indexOfHarvest);
                    summary.Add($"-sells for {SaleValueField.GetSummary(this.GetSaleValue(drop, false), 1)}");

                    // generate field
                    yield return new GenericField("Crop", string.Join(Environment.NewLine, summary));
                }
            }

            // crafting
            if (obj?.heldObject != null)
            {
                if (obj is Cask)
                {
                    // get cask data
                    Cask cask = (Cask)obj;
                    Object agingObj = cask.heldObject;
                    ItemQuality currentQuality = (ItemQuality)agingObj.quality;

                    // calculate aging schedule
                    float effectiveAge = metadata.Constants.CaskAgeSchedule.Values.Max() - cask.daysToMature;
                    var schedule =
                        (
                            from entry in metadata.Constants.CaskAgeSchedule
                            let quality = entry.Key
                            let baseDays = entry.Value
                            where baseDays > effectiveAge
                            orderby baseDays ascending
                            let daysLeft = (int)Math.Ceiling((baseDays - effectiveAge) / cask.agingRate)
                            select new
                            {
                                Quality = quality,
                                DaysLeft = daysLeft,
                                HarvestDate = GameHelper.GetDayOffset(daysLeft, metadata.Constants.DaysInSeason)
                            }
                        )
                        .ToArray();

                    // display fields
                    yield return new ItemIconField("Contents", obj.heldObject);
                    if (cask.minutesUntilReady <= 0 || !schedule.Any())
                        yield return new GenericField("Aging", $"{currentQuality.GetName()} quality ready");
                    else
                    {
                        string scheduleStr = string.Join(Environment.NewLine, (from entry in schedule select $"-{entry.Quality.GetName()} {GameHelper.Pluralise(entry.DaysLeft, "tomorrow", $"in {entry.DaysLeft} days")} ({entry.HarvestDate.Item1} {entry.HarvestDate.Item2})"));
                        yield return new GenericField("Aging", $"-{currentQuality.GetName()} now (use pickaxe to stop aging){Environment.NewLine}" + scheduleStr);
                    }
                }
                else
                    yield return new ItemIconField("Contents", obj.heldObject, $"{obj.heldObject.Name} " + (obj.minutesUntilReady > 0 ? "in " + GenericField.GetString(TimeSpan.FromMinutes(obj.minutesUntilReady)) : "ready"));
            }

            // item
            if (showInventoryFields)
            {
                var giftTastes = this.GetGiftTastes(item);
                if (!isCrop)
                    yield return new SaleValueField("Sells for", this.GetSaleValue(item, this.KnownQuality), item.Stack);
                yield return new ItemGiftTastesField("Loves this", giftTastes, GiftTaste.Love);
                yield return new ItemGiftTastesField("Likes this", giftTastes, GiftTaste.Like);
            }

            // fence
            if (item is Fence)
            {
                Fence fence = (Fence)item;

                // health
                if (Game1.getFarm().isBuildingConstructed(Constant.BuildingNames.GoldClock))
                    yield return new GenericField("Health", "no decay with Gold Clock");
                else
                {
                    float maxHealth = fence.isGate ? fence.maxHealth * 2 : fence.maxHealth;
                    float health = fence.health / maxHealth;
                    float daysLeft = fence.health * metadata.Constants.FenceDecayRate / 60 / 24;
                    yield return new PercentageBarField("Health", (int)fence.health, (int)maxHealth, Color.Green, Color.Red, $"{Math.Round(health * 100)}% (roughly {Math.Round(daysLeft)} days left)");
                }
            }

            // recipes
            if (obj != null && obj.bigCraftable != true)
            {
                RecipeModel[] recipes = GameHelper.GetRecipesForIngredient(this.DisplayItem).ToArray();
                if (recipes.Any())
                    yield return new RecipesForIngredientField("Recipes", item, recipes);
            }
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            Item item = this.DisplayItem;

            // draw stackable object
            if ((item as Object)?.stack > 1)
            {
                Object obj = (Object)item;
                obj = new Object(obj.parentSheetIndex, 1, obj.isRecipe, obj.price, obj.quality) { bigCraftable = obj.bigCraftable }; // remove stack number (doesn't play well with clipped content)
                obj.drawInMenu(spriteBatch, position, 1);
                return true;
            }

            // draw generic item
            item.drawInMenu(spriteBatch, position, 1);
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the equivalent menu item for the specified target. (For example, the inventory item matching a fence object.)<</summary>
        /// <param name="item">The target item.</param>
        private Item GetMenuItem(Item item)
        {
            // fence
            if (item is Fence)
            {
                Fence fence = (Fence)item;

                // get equivalent object's sprite ID
                FenceType fenceType = (FenceType)fence.whichType;
                int? spriteID = null;
                if (fence.isGate)
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
                    return new Object(spriteID.Value, 1);
            }

            return item;
        }

        /// <summary>Get the item description.</summary>
        /// <param name="item">The item.</param>
        private string GetDescription(Item item)
        {
            try
            {
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
            string type = item.getCategoryName();
            if (string.IsNullOrWhiteSpace(type) && item is Object)
                type = ((Object)item).type;
            return type;
        }

        /// <summary>Get the possible sale values for an item.</summary>
        /// <param name="item">The item.</param>
        /// <param name="qualityIsKnown">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        private IDictionary<ItemQuality, int> GetSaleValue(Item item, bool qualityIsKnown)
        {
            Func<Item, int> getPrice = i =>
            {
                int price = (i as Object)?.sellToStorePrice() ?? i.salePrice();
                return price > 0 ? price : 0;
            };

            // single quality
            if (!GameHelper.CanHaveQuality(item) || qualityIsKnown)
            {
                ItemQuality quality = qualityIsKnown && item is Object
                    ? (ItemQuality)((Object)item).quality
                    : ItemQuality.Normal;

                return new Dictionary<ItemQuality, int> { [quality] = getPrice(item) };
            }

            // multiple qualities
            return new Dictionary<ItemQuality, int>
            {
                [ItemQuality.Normal] = getPrice(new Object(item.parentSheetIndex, 1)),
                [ItemQuality.Silver] = getPrice(new Object(item.parentSheetIndex, 1, quality: (int)ItemQuality.Silver)),
                [ItemQuality.Gold] = getPrice(new Object(item.parentSheetIndex, 1, quality: (int)ItemQuality.Gold)),
                [ItemQuality.Iridium] = getPrice(new Object(item.parentSheetIndex, 1, quality: (int)ItemQuality.Iridium))
            };
        }

        /// <summary>Get how much each NPC likes receiving an item as a gift.</summary>
        /// <param name="item">The potential gift item.</param>
        private IDictionary<GiftTaste, string[]> GetGiftTastes(Item item)
        {
            return GameHelper.GetGiftTastes(item)
                .GroupBy(p => p.Value, p => p.Key)
                .ToDictionary(p => p.Key, p => p.ToArray());
        }
    }
}
