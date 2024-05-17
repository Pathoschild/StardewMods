using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.DataParsers;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Items
{
    /// <summary>Describes a Stardew Valley item.</summary>
    internal class ItemSubject : BaseSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>The lookup target.</summary>
        private readonly Item Target;

        /// <summary>The crop which will drop the item (if applicable).</summary>
        private readonly Crop? FromCrop;

        /// <summary>The dirt containing the crop (if applicable).</summary>
        private readonly HoeDirt? FromDirt;

        /// <summary>The crop grown by this seed item (if applicable).</summary>
        private readonly Crop? SeedForCrop;

        /// <summary>The context of the object being looked up.</summary>
        private readonly ObjectContext Context;

        /// <summary>Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</summary>
        private readonly bool KnownQuality;

        /// <summary>The location containing the item, if applicable.</summary>
        private readonly GameLocation? Location;

        /// <summary>Whether to only show content once the player discovers it.</summary>
        private readonly bool ProgressionMode;

        /// <summary>Whether to highlight item gift tastes which haven't been revealed in the NPC profile.</summary>
        private readonly bool HighlightUnrevealedGiftTastes;

        /// <summary>Which gift taste levels to show.</summary>
        private readonly ModGiftTasteConfig ShowGiftTastes;

        /// <summary>The configured minimum field values needed before they're auto-collapsed.</summary>
        private readonly ModCollapseLargeFieldsConfig CollapseFieldsConfig;

        /// <summary>Provides subject entries.</summary>
        private readonly ISubjectRegistry Codex;

        /// <summary>Get a lookup subject for a crop.</summary>
        private readonly Func<Crop, ObjectContext, HoeDirt?, ISubject> GetCropSubject;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="progressionMode">Whether to only show content once the player discovers it.</param>
        /// <param name="highlightUnrevealedGiftTastes">Whether to highlight item gift tastes which haven't been revealed in the NPC profile.</param>
        /// <param name="showGiftTastes">Which gift taste levels to show.</param>
        /// <param name="collapseFieldsConfig">The configured minimum field values needed before they're auto-collapsed.</param>
        /// <param name="item">The underlying target.</param>
        /// <param name="context">The context of the object being looked up.</param>
        /// <param name="knownQuality">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        /// <param name="location">The location containing the item, if applicable.</param>
        /// <param name="getCropSubject">Get a lookup subject for a crop.</param>
        /// <param name="fromCrop">The crop associated with the item (if applicable).</param>
        /// <param name="fromDirt">The dirt containing the crop (if applicable).</param>
        public ItemSubject(ISubjectRegistry codex, GameHelper gameHelper, bool progressionMode, bool highlightUnrevealedGiftTastes, ModGiftTasteConfig showGiftTastes, ModCollapseLargeFieldsConfig collapseFieldsConfig, Item item, ObjectContext context, bool knownQuality, GameLocation? location, Func<Crop, ObjectContext, HoeDirt?, ISubject> getCropSubject, Crop? fromCrop = null, HoeDirt? fromDirt = null)
            : base(gameHelper)
        {
            this.Codex = codex;
            this.ProgressionMode = progressionMode;
            this.HighlightUnrevealedGiftTastes = highlightUnrevealedGiftTastes;
            this.ShowGiftTastes = showGiftTastes;
            this.CollapseFieldsConfig = collapseFieldsConfig;
            this.Target = item;
            this.FromCrop = fromCrop ?? fromDirt?.crop;
            this.FromDirt = fromDirt;
            this.Context = context;
            this.Location = location;
            this.KnownQuality = knownQuality;
            this.GetCropSubject = getCropSubject;

            this.SeedForCrop = item.QualifiedItemId != "(O)433" || this.FromCrop == null // ignore unplanted coffee beans (to avoid "see also: coffee beans" loop)
                ? this.TryGetCropForSeed(item, location)
                : null;

            this.Initialize(this.Target.DisplayName, this.GetDescription(this.Target), this.GetTypeValue(this.Target));
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<ICustomField> GetData()
        {
            // get data
            Item item = this.Target;
            SObject? obj = item as SObject;
            bool isCrop = this.FromCrop != null;
            bool isSeed = this.SeedForCrop != null;
            bool isDeadCrop = this.FromCrop?.dead.Value == true;
            bool canSell = obj?.canBeShipped() == true || this.Metadata.Shops.Any(shop => shop.BuysCategories.Contains(item.Category));
            bool isMovieTicket = obj?.QualifiedItemId == "(O)809";

            // get overrides
            bool showInventoryFields = obj?.IsBreakableStone() != true;
            {
                ItemData? objData = this.Metadata.GetObject(item, this.Context);
                if (objData != null)
                {
                    this.Name = objData.NameKey != null ? I18n.GetByKey(objData.NameKey) : this.Name;
                    this.Description = objData.DescriptionKey != null ? I18n.GetByKey(objData.DescriptionKey) : this.Description;
                    this.Type = objData.TypeKey != null ? I18n.GetByKey(objData.TypeKey) : this.Type;
                    showInventoryFields = objData.ShowInventoryFields ?? showInventoryFields;
                }
            }

            // show item ID
            {
                IModInfo? fromMod = this.GameHelper.TryGetModFromItemId(item.ItemId);
                if (fromMod != null)
                    yield return new GenericField(I18n.Item_CustomItem(), I18n.Item_CustomItem_Summary(modName: fromMod.Manifest.Name));
            }

            // don't show data for dead crop
            if (isDeadCrop)
            {
                yield return new GenericField(I18n.Crop_Summary(), I18n.Crop_Summary_Dead());
                yield break;
            }

            // crop fields
            foreach (ICustomField field in this.GetCropFields(this.FromDirt, this.FromCrop ?? this.SeedForCrop, isSeed))
                yield return field;

            // indoor pot crop
            if (obj is IndoorPot pot)
            {
                Crop? potCrop = pot.hoeDirt.Value.crop;
                Bush? potBush = pot.bush.Value;

                if (potCrop != null)
                {
                    string dropName = ItemRegistry.GetDataOrErrorItem(potCrop.indexOfHarvest.Value).DisplayName;
                    yield return new LinkField(I18n.Item_Contents(), dropName, () => this.GetCropSubject(potCrop, ObjectContext.World, pot.hoeDirt.Value));
                }

                if (potBush != null)
                {
                    ISubject? subject = this.Codex.GetByEntity(potBush, this.Location ?? potBush.Location);
                    if (subject != null)
                        yield return new LinkField(I18n.Item_Contents(), subject.Name, () => subject);
                }

            }

            // machine output
            foreach (ICustomField field in this.GetMachineOutputFields(obj))
                yield return field;

            // music blocks
            if (obj?.Name == "Flute Block")
                yield return new GenericField(I18n.Item_MusicBlock_Pitch(), I18n.Generic_Ratio(value: obj.preservedParentSheetIndex.Value, max: 2300));
            else if (obj?.Name == "Drum Block")
                yield return new GenericField(I18n.Item_MusicBlock_DrumType(), I18n.Generic_Ratio(value: obj.preservedParentSheetIndex.Value, max: 6));

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
                    string? saleValueSummary = GenericField.GetSaleValueString(this.GetSaleValue(item, this.KnownQuality), item.Stack);
                    yield return new GenericField(I18n.Item_SellsFor(), saleValueSummary);

                    // sell to
                    List<string> buyers = new();
                    if (obj?.canBeShipped() == true)
                        buyers.Add(I18n.Item_SellsTo_ShippingBox());
                    buyers.AddRange(
                        from shop in this.Metadata.Shops
                        where shop.BuysCategories.Contains(item.Category)
                        let name = I18n.GetByKey(shop.DisplayKey).ToString()
                        orderby name
                        select name
                    );
                    yield return new GenericField(I18n.Item_SellsTo(), string.Join(", ", buyers));
                }

                // clothing
                if (item is Clothing clothing)
                    yield return new GenericField(I18n.Item_CanBeDyed(), this.Stringify(clothing.dyeable.Value));

                // gift tastes
                if (!isMovieTicket)
                {
                    IDictionary<GiftTaste, GiftTasteModel[]> giftTastes = this.GetGiftTastes(item);
                    if (this.ShowGiftTastes.Loved)
                        yield return new ItemGiftTastesField(I18n.Item_LovesThis(), giftTastes, GiftTaste.Love, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes);
                    if (this.ShowGiftTastes.Liked)
                        yield return new ItemGiftTastesField(I18n.Item_LikesThis(), giftTastes, GiftTaste.Like, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes);
                    if (this.ShowGiftTastes.Neutral)
                        yield return new ItemGiftTastesField(I18n.Item_NeutralAboutThis(), giftTastes, GiftTaste.Neutral, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes);
                    if (this.ShowGiftTastes.Disliked)
                        yield return new ItemGiftTastesField(I18n.Item_DislikesThis(), giftTastes, GiftTaste.Dislike, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes);
                    if (this.ShowGiftTastes.Hated)
                        yield return new ItemGiftTastesField(I18n.Item_HatesThis(), giftTastes, GiftTaste.Hate, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes);
                }
            }

            // weapon
            if (item is MeleeWeapon weapon && !weapon.isScythe())
            {
                int accuracy = weapon.addedPrecision.Value;
                float critChance = weapon.critChance.Value;
                float critMultiplier = weapon.critMultiplier.Value;
                int damageMin = weapon.minDamage.Value;
                int damageMax = weapon.maxDamage.Value;
                int defense = weapon.addedDefense.Value;
                float knockback = weapon.knockback.Value;
                int speed = weapon.speed.Value;
                int reach = weapon.addedAreaOfEffect.Value;

                int shownKnockback = (int)Math.Ceiling(Math.Abs(knockback - weapon.defaultKnockBackForThisType(weapon.type.Value)) * 10); // as shown in game UI
                int shownSpeed = (speed - (weapon.type.Value == MeleeWeapon.club ? MeleeWeapon.baseClubSpeed : 0)) / 2; // as shown in game UI

                string AddSign(float value) => (value > 0 ? "+" : "") + value;
                yield return new GenericField(I18n.Item_MeleeWeapon_Damage(), damageMin != damageMax ? I18n.Generic_Range(damageMin, damageMax) : damageMin.ToString());
                yield return new GenericField(I18n.Item_MeleeWeapon_CriticalChance(), I18n.Generic_Percent(critChance * 100f));
                yield return new GenericField(I18n.Item_MeleeWeapon_CriticalDamage(), I18n.Item_MeleeWeapon_CriticalDamage_Label(critMultiplier));
                yield return new GenericField(I18n.Item_MeleeWeapon_Defense(), defense == 0 ? "0" : I18n.Item_MeleeWeapon_Defense_Label(AddSign(weapon.addedDefense.Value)));

                if (speed == 0)
                    yield return new GenericField(I18n.Item_MeleeWeapon_Speed(), "0");
                else
                {
                    string speedLabel = I18n.Item_MeleeWeapon_Speed_Summary(speed: AddSign(speed), milliseconds: AddSign(-speed * MeleeWeapon.millisecondsPerSpeedPoint));
                    if (speed != shownSpeed)
                        speedLabel = I18n.Item_MeleeWeapon_Speed_ShownVsActual(shownSpeed: AddSign(shownSpeed), actualSpeed: speedLabel, lineBreak: Environment.NewLine);

                    yield return new GenericField(I18n.Item_MeleeWeapon_Speed(), speedLabel);
                }

                yield return new GenericField(I18n.Item_MeleeWeapon_Knockback(), (knockback > 1 ? I18n.Item_MeleeWeapon_Knockback_Label(amount: AddSign(shownKnockback), multiplier: knockback) : "0"));
                yield return new GenericField(I18n.Item_MeleeWeapon_Reach(), reach > 0 ? I18n.Item_MeleeWeapon_Reach_Label(AddSign(reach)) : "0");
                yield return new GenericField(I18n.Item_MeleeWeapon_Accuracy(), AddSign(accuracy));
            }

            // recipes
            if (showInventoryFields)
            {
                RecipeModel[] recipes =
                    // recipes that take this item as ingredient
                    this.GameHelper.GetRecipesForIngredient(item)

                    // recipes which produce this item
                    .Concat(this.GameHelper.GetRecipesForOutput(item))

                    // recipes for a machine
                    .Concat(this.GameHelper.GetRecipesForMachine(item as SObject))
                    .ToArray();

                if (recipes.Length > 0)
                {
                    var field = new ItemRecipesField(this.GameHelper, I18n.Item_Recipes(), item, recipes, this.ProgressionMode);

                    // calculate count of recipes that will be shown, in case we're in progression mode and some are hidden
                    int shownRecipesCount = recipes.Count(recipe => !this.ProgressionMode || recipe.IsKnown());
                    if (this.CollapseFieldsConfig.Enabled && shownRecipesCount >= this.CollapseFieldsConfig.ItemRecipes)
                        field.CollapseByDefault(I18n.Generic_ShowXResults(count: shownRecipesCount));
                    yield return field;
                }
            }

            // fish spawn rules
            if (item.Category == SObject.FishCategory)
                yield return new FishSpawnRulesField(this.GameHelper, I18n.Item_FishSpawnRules(), item.ItemId);

            // fish pond data
            // derived from FishPond::doAction
            if (item.HasTypeObject() && (item.Category is SObject.FishCategory || item.QualifiedItemId is "(O)393"/*coral*/ or "(O)397"/*sea urchin*/))
            {
                FishPondData? fishPondData = FishPond.GetRawData(item.ItemId);
                if (fishPondData is not null)
                {
                    int minChanceOfAnyDrop = (int)Math.Round(Utility.Lerp(0.15f, 0.95f, 1 / 10f) * 100);
                    int maxChanceOfAnyDrop = (int)Math.Round(Utility.Lerp(0.15f, 0.95f, FishPond.MAXIMUM_OCCUPANCY / 10f) * 100);
                    string preface = I18n.Building_FishPond_Drops_Preface(chance: I18n.Generic_Range(min: minChanceOfAnyDrop, max: maxChanceOfAnyDrop));
                    yield return new FishPondDropsField(this.GameHelper, I18n.Item_FishPondDrops(), -1, fishPondData, preface);
                }
            }

            // fence
            if (item is Fence fence)
            {
                string healthLabel = I18n.Item_FenceHealth();

                // health
                if (Game1.getFarm().isBuildingConstructed(Constant.BuildingNames.GoldClock))
                    yield return new GenericField(healthLabel, I18n.Item_FenceHealth_GoldClock());
                else
                {
                    float maxHealth = fence.isGate.Value ? fence.maxHealth.Value * 2 : fence.maxHealth.Value;
                    float health = fence.health.Value / maxHealth;
                    double daysLeft = Math.Round(fence.health.Value * this.Constants.FenceDecayRate / 60 / 24);
                    double percent = Math.Round(health * 100);
                    yield return new PercentageBarField(healthLabel, (int)fence.health.Value, (int)maxHealth, Color.Green, Color.Red, I18n.Item_FenceHealth_Summary(percent: (int)percent, count: (int)daysLeft));
                }
            }

            // movie ticket
            if (isMovieTicket)
            {
                MovieData movie = MovieTheater.GetMovieForDate(Game1.Date);
                if (movie == null)
                    yield return new GenericField(I18n.Item_MovieTicket_MovieThisWeek(), I18n.Item_MovieTicket_MovieThisWeek_None());
                else
                {
                    // movie this week
                    yield return new GenericField(I18n.Item_MovieTicket_MovieThisWeek(), new IFormattedText[]
                    {
                        new FormattedText(TokenParser.ParseText(movie.Title), bold: true),
                        new FormattedText(Environment.NewLine),
                        new FormattedText(TokenParser.ParseText(movie.Description))
                    });

                    // movie tastes
                    const GiftTaste rejectKey = (GiftTaste)(-1);
                    IDictionary<GiftTaste, string[]> tastes = this.GameHelper.GetMovieTastes()
                        .GroupBy(entry => entry.Value ?? rejectKey)
                        .ToDictionary(group => group.Key, group => group.Select(p => p.Key.displayName).OrderBy(p => p).ToArray());

                    yield return new MovieTastesField(I18n.Item_MovieTicket_LovesMovie(), tastes, GiftTaste.Love);
                    yield return new MovieTastesField(I18n.Item_MovieTicket_LikesMovie(), tastes, GiftTaste.Like);
                    yield return new MovieTastesField(I18n.Item_MovieTicket_DislikesMovie(), tastes, GiftTaste.Dislike);
                    yield return new MovieTastesField(I18n.Item_MovieTicket_RejectsMovie(), tastes, rejectKey);
                }
            }

            // dyes
            if (showInventoryFields)
                yield return new ColorField(I18n.Item_ProducesDye(), item);

            // owned and times cooked/crafted
            if (showInventoryFields && !isCrop)
            {
                // owned
                yield return new GenericField(I18n.Item_NumberOwned(), I18n.Item_NumberOwned_Summary(count: this.GameHelper.CountOwnedItems(item)));

                // times crafted
                RecipeModel[] recipes = this.GameHelper
                    .GetRecipes()
                    .Where(recipe => recipe.OutputQualifiedItemId == this.Target.QualifiedItemId)
                    .ToArray();
                if (recipes.Any())
                {
                    string label = recipes.First().Type == RecipeType.Cooking ? I18n.Item_NumberCooked() : I18n.Item_NumberCrafted();
                    int timesCrafted = recipes.Sum(recipe => recipe.GetTimesCrafted(Game1.player));
                    if (timesCrafted >= 0) // negative value means not available for this recipe type
                        yield return new GenericField(label, I18n.Item_NumberCrafted_Summary(count: timesCrafted));
                }
            }

            // see also crop
            bool seeAlsoCrop =
                isSeed
                && item.ItemId != this.SeedForCrop!.indexOfHarvest.Value // skip seeds which produce themselves (e.g. coffee beans)
                && item.ItemId is not ("495" or "496" or "497") // skip random seasonal seeds
                && item.ItemId != "770"; // skip mixed seeds
            if (seeAlsoCrop)
            {
                string dropName = ItemRegistry.GetDataOrErrorItem(this.SeedForCrop!.indexOfHarvest.Value).DisplayName;
                yield return new LinkField(I18n.Item_SeeAlso(), dropName, () => this.GetCropSubject(this.SeedForCrop, ObjectContext.Inventory, null));
            }
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<IDebugField> GetDebugFields()
        {
            Item target = this.Target;
            SObject? obj = target as SObject;
            Crop? crop = this.FromCrop ?? this.SeedForCrop;

            // pinned fields
            yield return new GenericDebugField("item ID", target.QualifiedItemId, pinned: true);
            yield return new GenericDebugField("sprite index", target.ParentSheetIndex, pinned: true);
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
            this.Target.drawInMenu(spriteBatch, position, 1, 1f, 1f, StackDrawType.Hide, Color.White, false);
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the item description.</summary>
        /// <param name="item">The item.</param>
        [SuppressMessage("ReSharper", "AssignmentIsFullyDiscarded", Justification = "Discarding the value is deliberate. We need to call the property to trigger the data load, but we don't actually need the result.")]
        private string? GetDescription(Item item)
        {
            try
            {
                _ = item.DisplayName; // force display name to load, which is needed to get the description outside the inventory for some reason
                return item is MeleeWeapon weapon && !weapon.isScythe()
                    ? weapon.Description
                    : item.getDescription();
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

        /// <summary>Get the crop which grows from the given seed, if applicable.</summary>
        /// <param name="seed">The potential seed item to check.</param>
        /// <param name="location">The location containing the crop, if applicable.</param>
        private Crop? TryGetCropForSeed(Item seed, GameLocation? location)
        {
            if (!seed.HasTypeId(ItemRegistry.type_object))
                return null;

            try
            {
                return Crop.TryGetData(seed.ItemId, out _)
                    ? new(seed.ItemId, 0, 0, location ?? Game1.getFarm())
                    : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Get the custom fields for a crop.</summary>
        /// <param name="dirt">The dirt the crop is planted in, if applicable.</param>
        /// <param name="crop">The crop to represent.</param>
        /// <param name="isSeed">Whether the crop being displayed is for an unplanted seed.</param>
        private IEnumerable<ICustomField> GetCropFields(HoeDirt? dirt, Crop? crop, bool isSeed)
        {
            if (crop == null)
                yield break;

            var data = new CropDataParser(crop, isPlanted: !isSeed);
            bool isForage = CommonHelper.IsItemId(crop.whichForageCrop.Value, allowZero: false) && crop.fullyGrown.Value; // show crop fields for growing mixed seeds

            // add next-harvest field
            if (!isSeed && !isForage)
            {
                // get next harvest
                SDate nextHarvest = data.GetNextHarvest();

                // generate field
                string summary;
                if (data.CanHarvestNow)
                    summary = I18n.Generic_Now();
                else if (!Game1.currentLocation.SeedsIgnoreSeasonsHere() && !data.Seasons.Contains(nextHarvest.Season))
                    summary = I18n.Crop_Harvest_TooLate(date: this.Stringify(nextHarvest));
                else
                    summary = $"{this.Stringify(nextHarvest)} ({this.GetRelativeDateStr(nextHarvest)})";

                yield return new GenericField(I18n.Crop_Harvest(), summary);
            }

            // crop summary
            if (!isForage)
            {
                List<string> summary = new();

                // harvest
                if (!crop.forageCrop.Value)
                {
                    summary.Add(data.HasMultipleHarvests
                        ? I18n.Crop_Summary_HarvestMulti(daysToFirstHarvest: data.DaysToFirstHarvest, daysToNextHarvests: data.DaysToSubsequentHarvest)
                        : I18n.Crop_Summary_HarvestOnce(daysToFirstHarvest: data.DaysToFirstHarvest)
                    );
                }

                // seasons
                summary.Add(I18n.Crop_Summary_Seasons(seasons: string.Join(", ", I18n.GetSeasonNames(data.Seasons))));

                // drops
                if (data.CropData is not null)
                {
                    int minStack = data.CropData.HarvestMinStack;
                    int maxStack = data.CropData.HarvestMaxStack;
                    double extraHarvestChance = data.CropData.ExtraHarvestChance;

                    // TODO 1.6: update for new combinations (e.g. min/max without extra chance) and extra per farming level
                    if (minStack != maxStack)
                        summary.Add(I18n.Crop_Summary_DropsXToY(min: minStack, max: maxStack, percent: (int)Math.Round(extraHarvestChance * 100, 2)));
                    else if (minStack > 1)
                        summary.Add(I18n.Crop_Summary_DropsX(count: minStack));
                }
                else
                    summary.Add(I18n.Crop_Summary_DropsX(count: 1));


                // crop sale price
                Item drop = data.GetSampleDrop();
                summary.Add(I18n.Crop_Summary_SellsFor(price: GenericField.GetSaleValueString(this.GetSaleValue(drop, false), 1)!));

                // generate field
                yield return new GenericField(I18n.Crop_Summary(), "-" + string.Join($"{Environment.NewLine}-", summary));
            }

            // dirt water/fertilizer state
            if (dirt != null && !isForage)
            {
                // watered
                yield return new GenericField(I18n.Crop_Watered(), this.Stringify(dirt.state.Value == HoeDirt.watered));

                // fertilizer
                string[] appliedFertilizers = this.GetAppliedFertilizers(dirt)
                    .Select(GameI18n.GetObjectName)
                    .Distinct()
                    .DefaultIfEmpty(this.Stringify(false))
                    .OrderBy(p => p)
                    .ToArray();

                yield return new GenericField(I18n.Crop_Fertilized(), string.Join(", ", appliedFertilizers));
            }
        }

        /// <summary>Get the fertilizer item IDs applied to a dirt tile.</summary>
        /// <param name="dirt">The dirt tile to check.</param>
        private IEnumerable<string> GetAppliedFertilizers(HoeDirt dirt)
        {
            if (this.GameHelper.MultiFertilizer.IsLoaded)
                return this.GameHelper.MultiFertilizer.GetAppliedFertilizers(dirt);

            if (ItemRegistry.QualifyItemId(dirt.fertilizer.Value) != null)
                return new[] { dirt.fertilizer.Value };

            return Enumerable.Empty<string>();
        }

        /// <summary>Get the custom fields for machine output.</summary>
        /// <param name="machine">The machine whose output to represent.</param>
        private IEnumerable<ICustomField> GetMachineOutputFields(SObject? machine)
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
                    yield return new ItemIconField(this.GameHelper, I18n.Item_Contents(), heldObj, this.Codex);
                    if (minutesLeft <= 0 || !schedule.Any())
                        yield return new GenericField(I18n.Item_CaskSchedule(), I18n.Item_CaskSchedule_Now(quality: I18n.For(curQuality)));
                    else
                    {
                        string scheduleStr = string.Join(Environment.NewLine, (
                            from entry in schedule
                            let str = I18n.GetPlural(entry.DaysLeft, I18n.Item_CaskSchedule_Tomorrow(quality: I18n.For(entry.Quality)), I18n.Item_CaskSchedule_InXDays(quality: I18n.For(entry.Quality), count: entry.DaysLeft, date: this.Stringify(entry.HarvestDate)))
                            select $"-{str}"
                        ));
                        yield return new GenericField(I18n.Item_CaskSchedule(), $"{I18n.Item_CaskSchedule_NowPartial(quality: I18n.For(curQuality))}{Environment.NewLine}{scheduleStr}");
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
                        yield return new ItemIconField(this.GameHelper, I18n.Item_CrabpotBait(), pot.bait.Value, this.Codex);
                    else if (Game1.player.professions.Contains(11)) // no bait needed if luremaster
                        yield return new GenericField(I18n.Item_CrabpotBait(), I18n.Item_CrabpotBaitNotNeeded());
                    else
                        yield return new GenericField(I18n.Item_CrabpotBait(), I18n.Item_CrabpotBaitNeeded());
                }

                // output item
                if (heldObj != null)
                {
                    string summary = I18n.Item_Contents_Ready(name: heldObj.DisplayName);
                    yield return new ItemIconField(this.GameHelper, I18n.Item_Contents(), heldObj, this.Codex, summary);
                }
            }

            // furniture
            else if (machine is Furniture)
            {
                // displayed item
                if (heldObj != null)
                {
                    string summary = I18n.Item_Contents_Placed(name: heldObj.DisplayName);
                    yield return new ItemIconField(this.GameHelper, I18n.Item_Contents(), heldObj, this.Codex, summary);
                }
            }

            // auto-grabber
            else if (machine.QualifiedItemId == $"{ItemRegistry.type_bigCraftable}{Constant.ObjectIndexes.AutoGrabber}")
            {
                string? readyText = I18n.Stringify(heldObj is Chest output && output.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any());
                yield return new GenericField(I18n.Item_Contents(), readyText);
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
                    yield return new ItemIconField(this.GameHelper, I18n.Item_Contents(), heldObj, this.Codex, summary);
                }
            }
        }

        /// <summary>Get the custom fields indicating what an item is needed for.</summary>
        /// <param name="obj">The machine whose output to represent.</param>
        private IEnumerable<ICustomField> GetNeededForFields(SObject? obj)
        {
            if (obj == null || obj.TypeDefinitionId != ItemRegistry.type_object)
                yield break;

            List<string> neededFor = new();

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
            if (this.Constants.PolycultureCrops.Contains(obj.QualifiedItemId))
            {
                int needed = this.Constants.PolycultureCount - this.GameHelper.GetShipped(obj.ItemId);
                if (needed > 0)
                    neededFor.Add(I18n.Item_NeededFor_Polyculture(count: needed));
            }

            // full shipment achievement (ship every item)
            if (this.GameHelper.GetFullShipmentAchievementItems().Any(p => p.Key == obj.QualifiedItemId && !p.Value))
                neededFor.Add(I18n.Item_NeededFor_FullShipment());

            // full collection achievement (donate every artifact)
            if (obj.needsToBeDonated())
                neededFor.Add(I18n.Item_NeededFor_FullCollection());

            // recipe achievements
            {
                RecipeData[] missingRecipes =
                    (
                        from recipe in this.GameHelper.GetRecipesForIngredient(this.Target)
                        let item = recipe.TryCreateItem(this.Target)
                        let timesCrafted = recipe.GetTimesCrafted(Game1.player)
                        where item != null && timesCrafted <= 0
                        orderby item.DisplayName
                        select new RecipeData(recipe.Type, item.DisplayName, TimesCrafted: timesCrafted, IsKnown: recipe.IsKnown())
                    )
                    .ToArray();

                // gourmet chef achievement (cook every recipe)
                {
                    RecipeData[] missingCookingRecipes = missingRecipes.Where(recipe => recipe.Type == RecipeType.Cooking).ToArray();
                    if (missingCookingRecipes.Length > 0)
                    {
                        string missingRecipesText = this.GetNeededForRecipeText(missingCookingRecipes);
                        neededFor.Add(I18n.Item_NeededFor_GourmetChef(missingRecipesText));
                    }
                }

                // craft master achievement (craft every item)
                {
                    RecipeData[] missingCraftingRecipes = missingRecipes.Where(recipe => recipe.Type == RecipeType.Crafting).ToArray();
                    if (missingCraftingRecipes.Length > 0)
                    {
                        string missingRecipesText = this.GetNeededForRecipeText(missingCraftingRecipes);
                        neededFor.Add(I18n.Item_NeededFor_CraftMaster(missingRecipesText));
                    }
                }
            }

            // quests
            {
                string[] quests = this.GameHelper
                    .GetQuestsWhichNeedItem(obj)
                    .Select(p => p.DisplayText)
                    .OrderBy(p => p)
                    .ToArray();
                if (quests.Any())
                    neededFor.Add(I18n.Item_NeededFor_Quests(quests: string.Join(", ", quests)));
            }

            // yield
            if (neededFor.Any())
                yield return new GenericField(I18n.Item_NeededFor(), string.Join(", ", neededFor));
        }

        /// <summary>Get a text representation for missing recipes. If progression mode is enabled, includes the count of unknown recipes.</summary>
        /// <param name="missingRecipes">A list of recipes that haven't been cooked or crafted yet.</param>
        private string GetNeededForRecipeText(RecipeData[] missingRecipes)
        {
            if (!this.ProgressionMode)
            {
                // return all recipe names
                string[] recipeNames = (from recipe in missingRecipes select (string)recipe.DisplayName).ToArray();
                return string.Join(", ", recipeNames);
            }

            RecipeData[] knownMissingRecipes = missingRecipes.Where(recipe => recipe.IsKnown).ToArray();
            int unknownMissingRecipeCount = missingRecipes.Length - knownMissingRecipes.Length;

            if (knownMissingRecipes.Any())
            {
                // return learned recipe names + count of unlearned recipes
                string[] knownRecipeNames = (from recipe in knownMissingRecipes select (string)recipe.DisplayName).ToArray();
                return string.Join(", ", knownRecipeNames.Append(I18n.Item_UnknownRecipes(unknownMissingRecipeCount)));
            }

            // return just the count of unlearned recipes
            return I18n.Item_UnknownRecipes(unknownMissingRecipeCount);
        }

        /// <summary>Get unfinished bundles which require this item.</summary>
        /// <param name="item">The item for which to find bundles.</param>
        private IEnumerable<BundleModel> GetUnfinishedBundles(SObject item)
        {
            // no bundles for Joja members
            if (Game1.player.hasOrWillReceiveMail(Constant.MailLetters.JojaMember))
                yield break;

            // avoid false positives
            if (item.bigCraftable.Value || item is Cask or Fence or Furniture or IndoorPot or Sign or Torch or Wallpaper)
                yield break; // avoid false positives

            // get community center
            CommunityCenter communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            bool IsBundleOpen(int id)
            {
                try
                {
                    return !communityCenter.isBundleComplete(id);
                }
                catch
                {
                    return false; // invalid bundle data
                }
            }

            // get bundles
            if (!communityCenter.areAllAreasComplete() || IsBundleOpen(36))
            {
                foreach (BundleModel bundle in this.GameHelper.GetBundleData())
                {
                    if (!IsBundleOpen(bundle.ID))
                        continue;

                    bool isMissing = this.GetIngredientsFromBundle(bundle, item).Any(p => this.IsIngredientNeeded(bundle, p));
                    if (isMissing)
                        yield return bundle;
                }
            }
        }

        /// <summary>Get the translated name for a bundle's area.</summary>
        /// <param name="bundle">The bundle.</param>
        private string GetTranslatedBundleArea(BundleModel bundle)
        {
            return bundle.Area switch
            {
                "Pantry" => I18n.BundleArea_Pantry(),
                "Crafts Room" => I18n.BundleArea_CraftsRoom(),
                "Fish Tank" => I18n.BundleArea_FishTank(),
                "Boiler Room" => I18n.BundleArea_BoilerRoom(),
                "Vault" => I18n.BundleArea_Vault(),
                "Bulletin Board" => I18n.BundleArea_BulletinBoard(),
                "Abandoned Joja Mart" => I18n.BundleArea_AbandonedJojaMart(),
                _ => bundle.Area
            };
        }

        /// <summary>Get the possible sale values for an item.</summary>
        /// <param name="item">The item.</param>
        /// <param name="qualityIsKnown">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        private IDictionary<ItemQuality, int> GetSaleValue(Item item, bool qualityIsKnown)
        {
            SObject? obj = item.getOne() as SObject;

            // single quality
            if (obj == null || !this.GameHelper.CanHaveQuality(item) || qualityIsKnown)
            {
                ItemQuality quality = qualityIsKnown && obj != null
                    ? (ItemQuality)obj.Quality
                    : ItemQuality.Normal;
                return new Dictionary<ItemQuality, int> { [quality] = this.GetRawSalePrice(item) };
            }

            // multiple qualities
            {
                string[] iridiumItems = this.Constants.ItemsWithIridiumQuality;
                var prices = new Dictionary<ItemQuality, int>();
                var sample = (SObject)item.getOne();
                foreach (ItemQuality quality in CommonHelper.GetEnumValues<ItemQuality>())
                {
                    if (quality == ItemQuality.Iridium && !iridiumItems.Contains(item.QualifiedItemId) && !iridiumItems.Contains(item.Category.ToString()))
                        continue;

                    sample.Quality = (int)quality;
                    prices[quality] = this.GetRawSalePrice(sample);
                }
                return prices;
            }
        }

        /// <summary>Get the sale price for a specific item instance.</summary>
        /// <param name="item">The item instance.</param>
        /// <remarks>Derived from <see cref="Utility.getSellToStorePriceOfItem(Item, bool)"/>.</remarks>
        private int GetRawSalePrice(Item item)
        {
            int price = item is SObject obj
                ? obj.sellToStorePrice()
                : (item.salePrice() / 2);

            return price > 0
                ? price
                : 0;
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
                .Where(required =>
                {
                    if (required.ItemId is (null or "-1"))
                        return false; // monetary bundle

                    if (!ItemRegistry.HasItemId(item, required.ItemId) && required.ItemId != item.Category.ToString())
                        return false;

                    if ((ItemQuality)item.Quality < required.Quality)
                        return false;

                    return true;
                });
        }

        /// <summary>Get whether an ingredient is still needed for a bundle.</summary>
        /// <param name="bundle">The bundle to check.</param>
        /// <param name="ingredient">The ingredient to check.</param>
        private bool IsIngredientNeeded(BundleModel bundle, BundleIngredientModel ingredient)
        {
            CommunityCenter communityCenter = Game1.locations.OfType<CommunityCenter>().First();

            // handle rare edge case where item is required in the bundle data, but it's not
            // present in the community center data. This seems to be caused by some mods like
            // Challenging Community Center Bundles in some cases.
            if (!communityCenter.bundles.TryGetValue(bundle.ID, out bool[] items) || ingredient.Index >= items.Length)
                return true;

            return !items[ingredient.Index];
        }

        /// <summary>Get the number of an ingredient needed for a bundle.</summary>
        /// <param name="bundle">The bundle to check.</param>
        /// <param name="item">The ingredient to check.</param>
        private int GetIngredientCountNeeded(BundleModel bundle, SObject item)
        {
            return this
                .GetIngredientsFromBundle(bundle, item)
                .Where(p => this.IsIngredientNeeded(bundle, p))
                .Sum(p => p.Stack);
        }

        /// <summary>The basic metadata for a recipe.</summary>
        /// <param name="Type">The recipe type.</param>
        /// <param name="DisplayName">The translated display name for the produced item, including metadata like the "(Recipe)" suffix.</param>
        /// <param name="TimesCrafted">How many times the recipe was cooked or crafted by the player.</param>
        /// <param name="IsKnown">Whether the player knows this recipe.</param>
        public record RecipeData(RecipeType Type, string DisplayName, int TimesCrafted, bool IsKnown);
    }
}
