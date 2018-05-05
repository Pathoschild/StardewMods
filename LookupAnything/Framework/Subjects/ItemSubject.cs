using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
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
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        /// <param name="item">The underlying target.</param>
        /// <param name="context">The context of the object being looked up.</param>
        /// <param name="knownQuality">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        /// <param name="fromCrop">The crop associated with the item (if applicable).</param>
        public ItemSubject(ITranslationHelper translations, Item item, ObjectContext context, bool knownQuality, Crop fromCrop = null)
            : base(translations)
        {
            this.Target = item;
            this.DisplayItem = this.GetMenuItem(item);
            this.FromCrop = fromCrop;
            if ((item as SObject)?.Type == "Seeds")
                this.SeedForCrop = new Crop(item.ParentSheetIndex, 0, 0);
            this.Context = context;
            this.KnownQuality = knownQuality;
            this.Initialise(this.DisplayItem.DisplayName, this.GetDescription(this.DisplayItem), this.GetTypeValue(this.DisplayItem));
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            // get data
            Item item = this.Target;
            SObject obj = item as SObject;
            bool isObject = obj != null;
            bool isCrop = this.FromCrop != null;
            bool isSeed = this.SeedForCrop != null;
            bool isDeadCrop = this.FromCrop?.dead.Value == true;
            bool canSell = obj?.canBeShipped() == true || metadata.Shops.Any(shop => shop.BuysCategories.Contains(item.Category));

            // get overrides
            bool showInventoryFields = true;
            {
                ObjectData objData = metadata.GetObject(item, this.Context);
                if (objData != null)
                {
                    this.Name = objData.NameKey != null ? this.Translate(objData.NameKey) : this.Name;
                    this.Description = objData.DescriptionKey != null ? this.Translate(objData.DescriptionKey) : this.Description;
                    this.Type = objData.TypeKey != null ? this.Translate(objData.TypeKey) : this.Type;
                    showInventoryFields = objData.ShowInventoryFields ?? true;
                }
            }

            // don't show data for dead crop
            if (isDeadCrop)
            {
                yield return new GenericField(this.Translate(L10n.Crop.Summary), this.Translate(L10n.Crop.SummaryDead));
                yield break;
            }

            // crop fields
            if (isCrop || isSeed)
            {
                // get crop
                Crop crop = this.FromCrop ?? this.SeedForCrop;

                // get harvest schedule
                int harvestablePhase = crop.phaseDays.Count - 1;
                bool canHarvestNow = (crop.currentPhase.Value >= harvestablePhase) && (!crop.fullyGrown.Value || crop.dayOfCurrentPhase.Value <= 0);
                int daysToFirstHarvest = crop.phaseDays.Take(crop.phaseDays.Count - 1).Sum(); // ignore harvestable phase

                // add next-harvest field
                if (isCrop)
                {
                    // calculate next harvest
                    int daysToNextHarvest = 0;
                    SDate dayOfNextHarvest = null;
                    if (!canHarvestNow)
                    {
                        // calculate days until next harvest
                        int daysUntilLastPhase = daysToFirstHarvest - crop.dayOfCurrentPhase.Value - crop.phaseDays.Take(crop.currentPhase.Value).Sum();
                        {
                            // growing: days until next harvest
                            if (!crop.fullyGrown.Value)
                                daysToNextHarvest = daysUntilLastPhase;

                            // regrowable crop harvested today
                            else if (crop.dayOfCurrentPhase.Value >= crop.regrowAfterHarvest.Value)
                                daysToNextHarvest = crop.regrowAfterHarvest.Value;

                            // regrowable crop
                            else
                                daysToNextHarvest = crop.dayOfCurrentPhase.Value; // dayOfCurrentPhase decreases to 0 when fully grown, where <=0 is harvestable
                        }
                        dayOfNextHarvest = SDate.Now().AddDays(daysToNextHarvest);
                    }

                    // generate field
                    string summary;
                    if (canHarvestNow)
                        summary = this.Translate(L10n.Crop.HarvestNow);
                    else if (Game1.currentLocation.Name != Constant.LocationNames.Greenhouse && !crop.seasonsToGrowIn.Contains(dayOfNextHarvest.Season))
                        summary = this.Translate(L10n.Crop.HarvestTooLate, new { date = this.Stringify(dayOfNextHarvest) });
                    else
                        summary = $"{this.Stringify(dayOfNextHarvest)} ({this.Text.GetPlural(daysToNextHarvest, L10n.Generic.Tomorrow, L10n.Generic.InXDays).Tokens(new { count = daysToNextHarvest })})";

                    yield return new GenericField(this.Translate(L10n.Crop.Harvest), summary);
                }

                // crop summary
                {
                    List<string> summary = new List<string>();

                    // harvest
                    summary.Add(crop.regrowAfterHarvest.Value == -1
                        ? this.Translate(L10n.Crop.SummaryHarvestOnce, new { daysToFirstHarvest = daysToFirstHarvest })
                        : this.Translate(L10n.Crop.SummaryHarvestMulti, new { daysToFirstHarvest = daysToFirstHarvest, daysToNextHarvests = crop.regrowAfterHarvest })
                    );

                    // seasons
                    summary.Add(this.Translate(L10n.Crop.SummarySeasons, new { seasons = string.Join(", ", this.Text.GetSeasonNames(crop.seasonsToGrowIn)) }));

                    // drops
                    if (crop.minHarvest != crop.maxHarvest && crop.chanceForExtraCrops.Value > 0)
                        summary.Add(this.Translate(L10n.Crop.SummaryDropsXToY, new { min = crop.minHarvest, max = crop.maxHarvest, percent = Math.Round(crop.chanceForExtraCrops.Value * 100, 2) }));
                    else if (crop.minHarvest.Value > 1)
                        summary.Add(this.Translate(L10n.Crop.SummaryDropsX, new { count = crop.minHarvest }));

                    // crop sale price
                    Item drop = GameHelper.GetObjectBySpriteIndex(crop.indexOfHarvest.Value);
                    summary.Add(this.Translate(L10n.Crop.SummarySellsFor, new { price = GenericField.GetSaleValueString(this.GetSaleValue(drop, false, metadata), 1, this.Text) }));

                    // generate field
                    yield return new GenericField(this.Translate(L10n.Crop.Summary), "-" + string.Join($"{Environment.NewLine}-", summary));
                }
            }

            // crafting
            if (obj?.heldObject?.Value != null)
            {
                if (obj is Cask cask)
                {
                    // get cask data
                    SObject agingObj = cask.heldObject.Value;
                    ItemQuality curQuality = (ItemQuality)agingObj.Quality;
                    string curQualityName = this.Translate(L10n.For(curQuality));

                    // calculate aging schedule
                    float effectiveAge = metadata.Constants.CaskAgeSchedule.Values.Max() - cask.daysToMature.Value;
                    var schedule =
                        (
                            from entry in metadata.Constants.CaskAgeSchedule
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
                    yield return new ItemIconField(this.Translate(L10n.Item.Contents), obj.heldObject.Value);
                    if (cask.MinutesUntilReady <= 0 || !schedule.Any())
                        yield return new GenericField(this.Translate(L10n.Item.CaskSchedule), this.Translate(L10n.Item.CaskScheduleNow, new { quality = curQualityName }));
                    else
                    {
                        string scheduleStr = string.Join(Environment.NewLine, (
                            from entry in schedule
                            let tokens = new { quality = this.Translate(L10n.For(entry.Quality)), count = entry.DaysLeft, date = entry.HarvestDate }
                            let str = this.Text.GetPlural(entry.DaysLeft, L10n.Item.CaskScheduleTomorrow, L10n.Item.CaskScheduleInXDays).Tokens(tokens)
                            select $"-{str}"
                        ));
                        yield return new GenericField(this.Translate(L10n.Item.CaskSchedule), this.Translate(L10n.Item.CaskSchedulePartial, new { quality = curQualityName }) + Environment.NewLine + scheduleStr);
                    }
                }
                else if (obj is Furniture)
                {
                    string summary = this.Translate(L10n.Item.ContentsPlaced, new { name = obj.heldObject.Value.DisplayName });
                    yield return new ItemIconField(this.Translate(L10n.Item.Contents), obj.heldObject.Value, summary);
                }
                else if (obj.ParentSheetIndex == Constant.ObjectIndexes.AutoGrabber)
                {
                    string readyText = this.Text.Stringify(obj.heldObject.Value is Chest output && output.items.Any());
                    yield return new GenericField(this.Translate(L10n.Item.Contents), readyText);
                }
                else
                {
                    string summary = obj.MinutesUntilReady <= 0
                        ? this.Translate(L10n.Item.ContentsReady, new { name = obj.heldObject.Value.DisplayName })
                        : this.Translate(L10n.Item.ContentsPartial, new { name = obj.heldObject.Value.DisplayName, time = this.Stringify(TimeSpan.FromMinutes(obj.MinutesUntilReady)) });
                    yield return new ItemIconField(this.Translate(L10n.Item.Contents), obj.heldObject.Value, summary);
                }
            }

            // item
            if (showInventoryFields)
            {
                // needed for
                {
                    List<string> neededFor = new List<string>();

                    // bundles
                    if (isObject)
                    {
                        string[] bundles = (from bundle in this.GetUnfinishedBundles(obj) orderby bundle.Area, bundle.DisplayName select $"{this.GetTranslatedBundleArea(bundle)}: {bundle.DisplayName}").ToArray();
                        if (bundles.Any())
                            neededFor.Add(this.Translate(L10n.Item.NeededForCommunityCenter, new { bundles = string.Join(", ", bundles) }));
                    }

                    // polyculture achievement
                    if (isObject && metadata.Constants.PolycultureCrops.Contains(obj.ParentSheetIndex))
                    {
                        int needed = metadata.Constants.PolycultureCount - GameHelper.GetShipped(obj.ParentSheetIndex);
                        if (needed > 0)
                            neededFor.Add(this.Translate(L10n.Item.NeededForPolyculture, new { count = needed }));
                    }

                    // full shipment achievement
                    if (isObject && GameHelper.GetFullShipmentAchievementItems().Any(p => p.Key == obj.ParentSheetIndex && !p.Value))
                        neededFor.Add(this.Translate(L10n.Item.NeededForFullShipment));

                    // a full collection achievement
                    LibraryMuseum museum = Game1.locations.OfType<LibraryMuseum>().FirstOrDefault();
                    if (museum != null && museum.isItemSuitableForDonation(obj))
                        neededFor.Add(this.Translate(L10n.Item.NeededForFullCollection));

                    // yield
                    if (neededFor.Any())
                        yield return new GenericField(this.Translate(L10n.Item.NeededFor), string.Join(", ", neededFor));
                }

                // sale data
                if (canSell && !isCrop)
                {
                    // sale price
                    string saleValueSummary = GenericField.GetSaleValueString(this.GetSaleValue(item, this.KnownQuality, metadata), item.Stack, this.Text);
                    yield return new GenericField(this.Translate(L10n.Item.SellsFor), saleValueSummary);

                    // sell to
                    List<string> buyers = new List<string>();
                    if (obj?.canBeShipped() == true)
                        buyers.Add(this.Translate(L10n.Item.SellsToShippingBox));
                    buyers.AddRange(
                        from shop in metadata.Shops
                        where shop.BuysCategories.Contains(item.Category)
                        let name = this.Translate(shop.DisplayKey).ToString()
                        orderby name
                        select name
                    );
                    yield return new GenericField(this.Translate(L10n.Item.SellsTo), string.Join(", ", buyers));
                }

                // gift tastes
                var giftTastes = this.GetGiftTastes(item, metadata);
                yield return new ItemGiftTastesField(this.Translate(L10n.Item.LovesThis), giftTastes, GiftTaste.Love);
                yield return new ItemGiftTastesField(this.Translate(L10n.Item.LikesThis), giftTastes, GiftTaste.Like);
            }

            // fence
            if (item is Fence fence)
            {
                string healthLabel = this.Translate(L10n.Item.FenceHealth);

                // health
                if (Game1.getFarm().isBuildingConstructed(Constant.BuildingNames.GoldClock))
                    yield return new GenericField(healthLabel, this.Translate(L10n.Item.FenceHealthGoldClock));
                else
                {
                    float maxHealth = fence.isGate.Value ? fence.maxHealth.Value * 2 : fence.maxHealth.Value;
                    float health = fence.health.Value / maxHealth;
                    double daysLeft = Math.Round(fence.health.Value * metadata.Constants.FenceDecayRate / 60 / 24);
                    double percent = Math.Round(health * 100);
                    yield return new PercentageBarField(healthLabel, (int)fence.health.Value, (int)maxHealth, Color.Green, Color.Red, this.Translate(L10n.Item.FenceHealthSummary, new { percent = percent, count = daysLeft }));
                }
            }

            // recipes
            if (item.GetSpriteType() == ItemSpriteType.Object)
            {
                RecipeModel[] recipes = GameHelper.GetRecipesForIngredient(this.DisplayItem).ToArray();
                if (recipes.Any())
                    yield return new RecipesForIngredientField(this.Translate(L10n.Item.Recipes), item, recipes, this.Text);
            }

            // owned
            if (showInventoryFields && !isCrop && !(item is Tool))
                yield return new GenericField(this.Translate(L10n.Item.Owned), this.Translate(L10n.Item.OwnedSummary, new { count = GameHelper.CountOwnedItems(item) }));

            // see also crop
            bool seeAlsoCrop =
                isSeed
                && item.ParentSheetIndex != this.SeedForCrop.indexOfHarvest.Value // skip seeds which produce themselves (e.g. coffee beans)
                && !(item.ParentSheetIndex >= 495 && item.ParentSheetIndex <= 497) // skip random seasonal seeds
                && item.ParentSheetIndex != 770; // skip mixed seeds
            if (seeAlsoCrop)
            {
                Item drop = GameHelper.GetObjectBySpriteIndex(this.SeedForCrop.indexOfHarvest.Value);
                yield return new LinkField(this.Translate(L10n.Item.SeeAlso), drop.DisplayName, () => new ItemSubject(this.Text, drop, ObjectContext.Inventory, false, this.SeedForCrop));
            }
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<IDebugField> GetDebugFields(Metadata metadata)
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
            Item item = this.DisplayItem;

            // draw stackable object
            if ((item as SObject)?.Stack > 1)
            {
                // remove stack number (doesn't play well with clipped content)
                SObject obj = (SObject)item;
                obj = new SObject(obj.ParentSheetIndex, 1, obj.IsRecipe, obj.Price, obj.Quality);
                obj.bigCraftable.Value = obj.bigCraftable.Value;
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
                : this.Translate(L10n.Types.Other);
        }

        /// <summary>Get unfinished bundles which require this item.</summary>
        /// <param name="item">The item for which to find bundles.</param>
        private IEnumerable<BundleModel> GetUnfinishedBundles(SObject item)
        {
            // no bundles for Joja members
            if (Game1.player.hasOrWillReceiveMail(Constant.MailLetters.JojaMember))
                yield break;

            // get community center
            CommunityCenter communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            if (communityCenter.areAllAreasComplete())
                yield break;

            // get bundles
            if (item.GetType() == typeof(SObject) && !item.bigCraftable.Value) // avoid false positives with hats, furniture, etc
            {
                foreach (BundleModel bundle in DataParser.GetBundles())
                {
                    // ignore completed bundle
                    if (communityCenter.isBundleComplete(bundle.ID))
                        continue;

                    // get ingredient
                    BundleIngredientModel ingredient = bundle.Ingredients.FirstOrDefault(p => p.ItemID == item.ParentSheetIndex && p.Quality <= (ItemQuality)item.Quality);
                    if (ingredient == null)
                        continue;

                    // yield if missing
                    if (!communityCenter.bundles[bundle.ID][ingredient.Index])
                        yield return bundle;
                }
            }
        }

        /// <summary>Get the translated name for a bundle's area.</summary>
        /// <param name="bundle">The bundle.</param>
        private string GetTranslatedBundleArea(BundleModel bundle)
        {
            switch (bundle.Area)
            {
                case "Pantry":
                    return this.Translate(L10n.BundleAreas.Pantry);
                case "Crafts Room":
                    return this.Translate(L10n.BundleAreas.CraftsRoom);
                case "Fish Tank":
                    return this.Translate(L10n.BundleAreas.FishTank);
                case "Boiler Room":
                    return this.Translate(L10n.BundleAreas.BoilerRoom);
                case "Vault":
                    return this.Translate(L10n.BundleAreas.Vault);
                case "Bulletin Board":
                    return this.Translate(L10n.BundleAreas.BulletinBoard);
                default:
                    return bundle.Area;
            }
        }

        /// <summary>Get the possible sale values for an item.</summary>
        /// <param name="item">The item.</param>
        /// <param name="qualityIsKnown">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        private IDictionary<ItemQuality, int> GetSaleValue(Item item, bool qualityIsKnown, Metadata metadata)
        {
            // get sale price
            // derived from ShopMenu::receiveLeftClick
            int GetPrice(Item i)
            {
                int price = (i as SObject)?.sellToStorePrice() ?? (i.salePrice() / 2);
                return price > 0 ? price : 0;
            }

            // single quality
            if (!GameHelper.CanHaveQuality(item) || qualityIsKnown)
            {
                ItemQuality quality = qualityIsKnown && item is SObject obj
                    ? (ItemQuality)obj.Quality
                    : ItemQuality.Normal;

                return new Dictionary<ItemQuality, int> { [quality] = GetPrice(item) };
            }

            // multiple qualities
            int[] iridiumItems = metadata.Constants.ItemsWithIridiumQuality;
            var prices = new Dictionary<ItemQuality, int>
            {
                [ItemQuality.Normal] = GetPrice(new SObject(item.ParentSheetIndex, 1)),
                [ItemQuality.Silver] = GetPrice(new SObject(item.ParentSheetIndex, 1, quality: (int)ItemQuality.Silver)),
                [ItemQuality.Gold] = GetPrice(new SObject(item.ParentSheetIndex, 1, quality: (int)ItemQuality.Gold))
            };
            if (item.GetSpriteType() == ItemSpriteType.Object && (iridiumItems.Contains(item.Category) || iridiumItems.Contains(item.ParentSheetIndex)))
                prices[ItemQuality.Iridium] = GetPrice(new SObject(item.ParentSheetIndex, 1, quality: (int)ItemQuality.Iridium));
            return prices;
        }

        /// <summary>Get how much each NPC likes receiving an item as a gift.</summary>
        /// <param name="item">The potential gift item.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        private IDictionary<GiftTaste, string[]> GetGiftTastes(Item item, Metadata metadata)
        {
            return GameHelper.GetGiftTastes(item, metadata)
                .GroupBy(p => p.Value, p => p.Key.getName())
                .ToDictionary(p => p.Key, p => p.Distinct().ToArray());
        }
    }
}
