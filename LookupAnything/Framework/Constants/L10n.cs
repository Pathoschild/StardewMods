using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Constants
{
    /// <summary>Localisation Keys matching the mod's <c>i18n</c> schema.</summary>
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass", Justification = "Irrelevant in this context.")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named to keep translation keys short.")]
    internal static class L10n
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying translation helper.</summary>
        private static ITranslationHelper Helper;


        /*********
        ** Accessors
        *********/
        /// <summary>Generic field value translations.</summary>
        public static class Generic
        {
            /// <summary>A value like <c>{{seasonName}} {{dayNumber}}</c>.</summary>
            /// <param name="seasonName">The name of the current season.</param>
            /// <param name="seasonNumber">The internal season number.</param>
            /// <param name="dayNumber">The day of month.</param>
            /// <param name="year">The year.</param>
            public static Translation Date(string seasonName, int seasonNumber, int dayNumber, int year) => L10n.Helper.Get("generic.date", new { seasonName, seasonNumber, dayNumber, year });

            /// <summary>A value like <c>{{seasonName}} {{dayNumber}} in year {{Year}}</c>.</summary>
            /// <param name="seasonName">The name of the current season.</param>
            /// <param name="seasonNumber">The internal season number.</param>
            /// <param name="dayNumber">The day of month.</param>
            /// <param name="year">The year.</param>
            public static Translation DateWithYear(string seasonName, int seasonNumber, int dayNumber, int year) => L10n.Helper.Get("generic.date-with-year", new { seasonName, seasonNumber, dayNumber, year });

            /// <summary>A value like <c>{{percent}}%</c>.</summary>
            /// <param name="percent">The percentage chance.</param>
            public static Translation Percent(int percent) => L10n.Helper.Get("generic.percent", new { percent });

            /// <summary>A value like <c>{{percent}}% chance of {{label}}</c>.</summary>
            /// <param name="percent">The percentage chance.</param>
            /// <param name="label">The thing for which a chance is being shown.</param>
            public static Translation PercentChanceOf(int percent, string label) => L10n.Helper.Get("generic.percent-chance-of", new { percent, label });

            /// <summary>A value like <c>{{percent}}% ({{value}} of {{max}})</c>.</summary>
            /// <param name="percent">The percentage ratio.</param>
            /// <param name="value">The fraction numerator.</param>
            /// <param name="max">The fraction denominator.</param>
            public static Translation PercentRatio(int percent, int value, int max) => L10n.Helper.Get("generic.percent-ratio", new { percent, value, max });

            /// <summary>A value like <c>{{value}} of {{max}}</c>.</summary>
            /// <param name="value">The fraction numerator.</param>
            /// <param name="max">The fraction denominator.</param>
            public static Translation Ratio(int value, int max) => L10n.Helper.Get("generic.ratio", new { value, max });

            /// <summary>A value like <c>{{min}} to {{max}}</c>.</summary>
            /// <param name="min">The minimum value.</param>
            /// <param name="max">The maximum value.</param>
            public static Translation Range(int min, int max) => L10n.Helper.Get("generic.range", new { min, max });

            /// <summary>A value like <c>yes</c>.</summary>
            public static Translation Yes() => L10n.Helper.Get("generic.yes");

            /// <summary>A value like <c>no</c>.</summary>
            public static Translation No() => L10n.Helper.Get("generic.no");

            /// <summary>A value like <c>{{count}} seconds</c>.</summary>
            public static Translation Seconds(int count) => L10n.Helper.Get("generic.seconds", new { count });

            /// <summary>A value like <c>{{count}} minutes</c>.</summary>
            public static Translation Minutes(int count) => L10n.Helper.Get("generic.minutes", new { count });

            /// <summary>A value like <c>{{count}} hours</c>.</summary>
            public static Translation Hours(int count) => L10n.Helper.Get("generic.hours", new { count });

            /// <summary>A value like <c>{{count}} days</c>.</summary>
            public static Translation Days(int count) => L10n.Helper.Get("generic.days", new { count });

            /// <summary>A value like <c>in {{count}} days</c>.</summary>
            public static Translation InXDays(int count) => L10n.Helper.Get("generic.in-x-days", new { count });

            /// <summary>A value like <c>tomorrow</c>.</summary>
            public static Translation Tomorrow() => L10n.Helper.Get("generic.tomorrow");

            /// <summary>A value like <c>{{price}}g</c>.</summary>
            public static Translation Price(int price) => L10n.Helper.Get("generic.price", new { price });

            /// <summary>A value like <c>{{price}}g ({{quality}})</c>.</summary>
            public static Translation PriceForQuality(int price, ItemQuality quality) => L10n.Helper.Get("generic.price-for-quality", new { price, quality = L10n.For(quality) });

            /// <summary>A value like <c>{{price}}g for stack of {{count}}</c>.</summary>
            public static Translation PriceForStack(int price, int count) => L10n.Helper.Get("generic.price-for-stack", new { price, count });
        }

        /// <summary>Lookup subject types.</summary>
        public static class Types
        {
            /// <summary>A value like <c>Building</c>.</summary>
            public static Translation Building() => L10n.Helper.Get("type.building");

            /// <summary>A value like <c>{{fruitName}} Tree</c>.</summary>
            public static Translation FruitTree() => L10n.Helper.Get("type.fruit-tree");

            /// <summary>A value like <c>Monster</c>.</summary>
            public static Translation Monster() => L10n.Helper.Get("type.monster");

            /// <summary>A value like <c>Player</c>.</summary>
            public static Translation Player() => L10n.Helper.Get("type.player");

            /// <summary>A value like <c>Map tile</c>.</summary>
            public static Translation Tile() => L10n.Helper.Get("type.map-tile");

            /// <summary>A value like <c>Tree</c>.</summary>
            public static Translation Tree() => L10n.Helper.Get("type.tree");

            /// <summary>A value like <c>Villager</c>.</summary>
            public static Translation Villager() => L10n.Helper.Get("type.villager");

            /// <summary>A value like <c>Other</c>.</summary>
            public static Translation Other() => L10n.Helper.Get("type.other");
        }

        /// <summary>Community Center bundle areas.</summary>
        public static class BundleAreas
        {
            /// <summary>A value like <c>Pantry</c>.</summary>
            public static Translation Pantry() => L10n.Helper.Get("bundle-area.pantry");

            /// <summary>A value like <c>Crafts Room</c>.</summary>
            public static Translation CraftsRoom() => L10n.Helper.Get("bundle-area.crafts-room");

            /// <summary>A value like <c>Fish Tank</c>.</summary>
            public static Translation FishTank() => L10n.Helper.Get("bundle-area.fish-tank");

            /// <summary>A value like <c>Boiler Room</c>.</summary>
            public static Translation BoilerRoom() => L10n.Helper.Get("bundle-area.boiler-room");

            /// <summary>A value like <c>Vault</c>.</summary>
            public static Translation Vault() => L10n.Helper.Get("bundle-area.vault");

            /// <summary>A value like <c>Bulletin Board</c>.</summary>
            public static Translation BulletinBoard() => L10n.Helper.Get("bundle-area.bulletin-board");
        }

        /// <summary>Recipe types.</summary>
        public static class RecipeTypes
        {
            /// <summary>A value like <c>Cooking</c>.</summary>
            public static Translation Cooking() => L10n.Helper.Get("recipe-type.cooking");

            /// <summary>A value like <c>Crafting</c>.</summary>
            public static Translation Crafting() => L10n.Helper.Get("recipe-type.crafting");
        }

        /// <summary>Animal lookup translations.</summary>
        public static class Animal
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>Love</c>.</summary>
            public static Translation Love() => L10n.Helper.Get("animal.love");

            /// <summary>A value like <c>Happiness</c>.</summary>
            public static Translation Happiness() => L10n.Helper.Get("animal.happiness");

            /// <summary>A value like <c>Mood today</c>.</summary>
            public static Translation Mood() => L10n.Helper.Get("animal.mood");

            /// <summary>A value like <c>Complaints</c>.</summary>
            public static Translation Complaints() => L10n.Helper.Get("animal.complaints");

            /// <summary>A value like <c>Produce ready</c>.</summary>
            public static Translation ProduceReady() => L10n.Helper.Get("animal.produce-ready");

            /// <summary>A value like <c>Growth</c>.</summary>
            public static Translation Growth() => L10n.Helper.Get("animal.growth");

            /// <summary>A value like <c>Sells for</c>.</summary>
            public static Translation SellsFor() => L10n.Helper.Get("animal.sells-for");

            /****
            ** Values
            ****/
            /// <summary>A value like <c>was disturbed by {{name}}</c>.</summary>
            public static Translation ComplaintsWildAnimalAttack() => L10n.Helper.Get("animal.complaints.wild-animal-attack");

            /// <summary>A value like <c>wasn't fed yesterday</c>.</summary>
            public static Translation ComplaintsHungry() => L10n.Helper.Get("animal.complaints.hungry");

            /// <summary>A value like <c>was left outside last night</c>.</summary>
            public static Translation ComplaintsLeftOut() => L10n.Helper.Get("animal.complaints.left-out");

            /// <summary>A value like <c>moved into new home</c>.</summary>
            public static Translation ComplaintsNewHome() => L10n.Helper.Get("animal.complaints.new-home");

            /// <summary>A value like <c>no heater in winter</c>.</summary>
            public static Translation ComplaintsNoHeater() => L10n.Helper.Get("animal.complaints.no-heater");

            /// <summary>A value like <c>hasn't been petted today</c>.</summary>
            public static Translation ComplaintsNotPetted() => L10n.Helper.Get("animal.complaints.not-petted");
        }

        /// <summary>building lookup translations.</summary>
        public static class Building
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>Animals</c>.</summary>
            public static Translation Animals() => L10n.Helper.Get("building.animals");

            /// <summary>A value like <c>Construction</c>.</summary>
            public static Translation Construction() => L10n.Helper.Get("building.construction");

            /// <summary>A value like <c>Feed trough</c>.</summary>
            public static Translation FeedTrough() => L10n.Helper.Get("building.feed-trough");

            /// <summary>A value like <c>Horse</c>.</summary>
            public static Translation Horse() => L10n.Helper.Get("building.horse");

            /// <summary>A value like <c>Horse</c>.</summary>
            public static Translation HorseLocation() => L10n.Helper.Get("building.horse-location");

            /// <summary>A value like <c>Harvesting enabled</c>.</summary>
            public static Translation JunimoHarvestingEnabled() => L10n.Helper.Get("building.junimo-harvesting-enabled");

            /// <summary>A value like <c>Owner</c>.</summary>
            public static Translation Owner() => L10n.Helper.Get("building.owner");

            /// <summary>A value like <c>Produce ready</c>.</summary>
            public static Translation OutputProcessing() => L10n.Helper.Get("building.output-processing");

            /// <summary>A value like <c>Produce ready</c>.</summary>
            public static Translation OutputReady() => L10n.Helper.Get("building.output-ready");

            /// <summary>A value like <c>Slimes</c>.</summary>
            public static Translation Slimes() => L10n.Helper.Get("building.slimes");

            /// <summary>A value like <c>Stored hay</c>.</summary>
            public static Translation StoredHay() => L10n.Helper.Get("building.stored-hay");

            /// <summary>A value like <c>Upgrades</c>.</summary>
            public static Translation Upgrades() => L10n.Helper.Get("building.upgrades");

            /// <summary>A value like <c>Water trough</c>.</summary>
            public static Translation WaterTrough() => L10n.Helper.Get("building.water-trough");

            /****
            ** Values
            ****/
            /// <summary>A value like <c>{{count}} of max {{max}} animals</c>.</summary>
            public static Translation AnimalsSummary(int count, int max) => L10n.Helper.Get("building.animals.summary", new { count, max });

            /// <summary>A value like <c>ready on {{date}}</c>.</summary>
            public static Translation ConstructionSummary(SDate date) => L10n.Helper.Get("building.construction.summary", new { date });

            /// <summary>A value like <c>automated</c>.</summary>
            public static Translation FeedTroughAutomated() => L10n.Helper.Get("building.feed-trough.automated");

            /// <summary>A value like <c>{{filled}} of {{max}} feed slots filled</c>.</summary>
            public static Translation FeedTroughSummary(int filled, int max) => L10n.Helper.Get("building.feed-trough.summary", new { filled, max });

            /// <summary>A value like <c>{{location}} ({{x}}, {{y}})</c>.</summary>
            public static Translation HorseLocationSummary(string location, int x, int y) => L10n.Helper.Get("building.horse-location.summary", new { location, x, y });

            /// <summary>A value like <c>no owner</c>.</summary>
            public static Translation OwnerNone() => L10n.Helper.Get("building.owner.none");

            /// <summary>A value like <c>{{count}} of max {{max}} slimes</c>.</summary>
            public static Translation SlimesSummary(int count, int max) => L10n.Helper.Get("building.slimes.summary", new { count, max });

            /// <summary>A value like <c>{{hayCount}} hay (max capacity: {{maxHay}})</c>.</summary>
            public static Translation StoredHaySummaryOneSilo(int hayCount, int maxHay) => L10n.Helper.Get("building.stored-hay.summary-one-silo", new { hayCount, maxHay });

            /// <summary>A value like <c>{{hayCount}} hay in {{siloCount}} silos (max capacity: {{maxHay}})</c>.</summary>
            public static Translation StoredHaySummaryMultipleSilos(int hayCount, int maxHay, int siloCount) => L10n.Helper.Get("building.stored-hay.summary-multiple-silos", new { hayCount, maxHay, siloCount });

            /// <summary>A value like <c>up to 4 animals, add cows</c>.</summary>
            public static Translation UpgradesBarn0() => L10n.Helper.Get("building.upgrades.barn.0");

            /// <summary>A value like <c>up to 8 animals, add pregnancy and goats</c>.</summary>
            public static Translation UpgradesBarn1() => L10n.Helper.Get("building.upgrades.barn.1");

            /// <summary>A value like <c>up to 12 animals, add autofeed, pigs, and sheep"</c>.</summary>
            public static Translation UpgradesBarn2() => L10n.Helper.Get("building.upgrades.barn.2");

            /// <summary>A value like <c>initial cabin</c>.</summary>
            public static Translation UpgradesCabin0() => L10n.Helper.Get("building.upgrades.cabin.0");

            /// <summary>A value like <c>add kitchen, enable marriage</c>.</summary>
            public static Translation UpgradesCabin1() => L10n.Helper.Get("building.upgrades.cabin.1");

            /// <summary>A value like <c>enable children</c>.</summary>
            public static Translation UpgradesCabin2() => L10n.Helper.Get("building.upgrades.cabin.2");

            /// <summary>A value like <c>up to 4 animals; add chickens</c>.</summary>
            public static Translation UpgradesCoop0() => L10n.Helper.Get("building.upgrades.coop.0");

            /// <summary>A value like <c>up to 8 animals; add incubator, dinosaurs, and ducks</c>.</summary>
            public static Translation UpgradesCoop1() => L10n.Helper.Get("building.upgrades.coop.1");

            /// <summary>A value like <c>up to 12 animals; add autofeed and rabbits</c>.</summary>
            public static Translation UpgradesCoop2() => L10n.Helper.Get("building.upgrades.coop.2");

            /// <summary>A value like <c>{{filled}} of {{max}} water troughs filled</c>.</summary>
            public static Translation WaterTroughSummary(int filled, int max) => L10n.Helper.Get("building.water-trough.summary", new { filled, max });
        }

        /// <summary>Fruit tree lookup translations.</summary>
        public static class FruitTree
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>Complaints</c>.</summary>
            public static Translation Complaints() => L10n.Helper.Get("fruit-tree.complaints");

            /// <summary>A value like <c>Growth</c>.</summary>
            public static Translation Growth() => L10n.Helper.Get("fruit-tree.growth");

            /// <summary>A value like <c>{{fruitName}} Tree</c>.</summary>
            public static Translation Name(string fruitName) => L10n.Helper.Get("fruit-tree.name", new { fruitName });

            /// <summary>A value like <c>Next fruit</c>.</summary>
            public static Translation NextFruit() => L10n.Helper.Get("fruit-tree.next-fruit");

            /// <summary>A value like <c>Season</c>.</summary>
            public static Translation Season() => L10n.Helper.Get("fruit-tree.season");

            /// <summary>A value like <c>Quality</c>.</summary>
            public static Translation Quality() => L10n.Helper.Get("fruit-tree.quality");

            /****
            ** Values
            ****/
            /// <summary>A value like <c>can't grow because there are adjacent objects</c>.</summary>
            public static Translation ComplaintsAdjacentObjects() => L10n.Helper.Get("fruit-tree.complaints.adjacent-objects");

            /// <summary>A value like <c>mature on {{date}}</c>.</summary>
            public static Translation GrowthSummary(string date) => L10n.Helper.Get("fruit-tree.growth.summary", new { date });

            /// <summary>A value like <c>struck by lightning! Will recover in {{count}} days.</c>.</summary>
            public static Translation NextFruitStruckByLightning(int count) => L10n.Helper.Get("fruit-tree.next-fruit.struck-by-lightning", new { count });

            /// <summary>A value like <c>out of season</c>.</summary>
            public static Translation NextFruitOutOfSeason() => L10n.Helper.Get("fruit-tree.next-fruit.out-of-season");

            /// <summary>A value like <c>won't grow any more fruit until you harvest those it has</c>.</summary>
            public static Translation NextFruitMaxFruit() => L10n.Helper.Get("fruit-tree.next-fruit.max-fruit");

            /// <summary>A value like <c>too young to bear fruit</c>.</summary>
            public static Translation NextFruitTooYoung() => L10n.Helper.Get("fruit-tree.next-fruit.too-young");

            /// <summary>A value like <c>{{quality}} now</c>.</summary>
            public static Translation QualityNow(ItemQuality quality) => L10n.Helper.Get("fruit-tree.quality.now", new { quality = L10n.For(quality) });

            /// <summary>A value like <c>{{quality}} on {{date}}</c>.</summary>
            public static Translation QualityOnDate(ItemQuality quality, string date) => L10n.Helper.Get("fruit-tree.quality.on-date", new { quality = L10n.For(quality), date });

            /// <summary>A value like <c>{{quality}} on {{date}} next year</c>.</summary>
            public static Translation QualityOnDateNextYear(ItemQuality quality, string date) => L10n.Helper.Get("fruit-tree.quality.on-date-next-year", new { quality = L10n.For(quality), date });

            /// <summary>A value like <c>{{season}} (or anytime in greenhouse)</c>.</summary>
            public static Translation SeasonSummary(string season) => L10n.Helper.Get("fruit-tree.season.summary", new { season });
        }

        /// <summary>Crop lookup translations.</summary>
        public static class Crop
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>Crop</c>.</summary>
            public static Translation Summary() => L10n.Helper.Get("crop.summary");

            /// <summary>A value like <c>Harvest</c>.</summary>
            public static Translation Harvest() => L10n.Helper.Get("crop.harvest");

            /****
            ** Values
            ****/
            /// <summary>A value like <c>This crop is dead.</c>.</summary>
            public static Translation SummaryDead() => L10n.Helper.Get("crop.summary.dead");

            /// <summary>A value like <c>drops {{count}}</c>.</summary>
            public static Translation SummaryDropsX(int count) => L10n.Helper.Get("crop.summary.drops-x", new { count });

            /// <summary>A value like <c>drops {{min}} to {{max}} ({{percent}}% chance of extra crops)</c>.</summary>
            public static Translation SummaryDropsXToY(int min, int max, int percent) => L10n.Helper.Get("crop.summary.drops-x-to-y", new { min, max, percent });

            /// <summary>A value like <c>harvest after {{daysToFirstHarvest}} days</c>.</summary>
            public static Translation SummaryHarvestOnce(int daysToFirstHarvest) => L10n.Helper.Get("crop.summary.harvest-once", new { daysToFirstHarvest });

            /// <summary>A value like <c>harvest after {{daysToFirstHarvest}} days, then every {{daysToNextHarvests}} days</c>.</summary>
            public static Translation SummaryHarvestMulti(int daysToFirstHarvest, int daysToNextHarvests) => L10n.Helper.Get("crop.summary.harvest-multi", new { daysToFirstHarvest, daysToNextHarvests });

            /// <summary>A value like <c>grows in {{seasons}}</c>.</summary>
            public static Translation SummarySeasons(string seasons) => L10n.Helper.Get("crop.summary.seasons", new { season = seasons });

            /// <summary>A value like <c>sells for {{price}}</c>.</summary>
            public static Translation SummarySellsFor(string price) => L10n.Helper.Get("crop.summary.sells-for", new { price });

            /// <summary>A value like <c>now</c>.</summary>
            public static Translation HarvestNow() => L10n.Helper.Get("crop.harvest.now");

            /// <summary>A value like <c>too late in the season for the next harvest (would be on {{date}})</c>.</summary>
            public static Translation HarvestTooLate(string date) => L10n.Helper.Get("crop.harvest.too-late", new { date });
        }

        /// <summary>Item lookup translations.</summary>
        public static class Item
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>Aging</c>.</summary>
            public static Translation CaskSchedule() => L10n.Helper.Get("item.cask-schedule");

            /// <summary>A value like <c>Bait</c>.</summary>
            public static Translation CrabpotBait() => L10n.Helper.Get("item.crabpot-bait");

            /// <summary>A value like <c>Needs bait!</c>.</summary>
            public static Translation CrabpotBaitNeeded() => L10n.Helper.Get("item.crabpot-bait-needed");

            /// <summary>A value like <c>Not needed due to Luremaster profession.</c>.</summary>
            public static Translation CrabpotBaitNotNeeded() => L10n.Helper.Get("item.crabpot-bait-not-needed");

            /// <summary>A value like <c>Contents</c>.</summary>
            public static Translation Contents() => L10n.Helper.Get("item.contents");

            /// <summary>A value like <c>Needed for</c>.</summary>
            public static Translation NeededFor() => L10n.Helper.Get("item.needed-for");

            /// <summary>A value like <c>Sells for</c>.</summary>
            public static Translation SellsFor() => L10n.Helper.Get("item.sells-for");

            /// <summary>A value like <c>Sells to</c>.</summary>
            public static Translation SellsTo() => L10n.Helper.Get("item.sells-to");

            /// <summary>A value like <c>Likes this</c>.</summary>
            public static Translation LikesThis() => L10n.Helper.Get("item.likes-this");

            /// <summary>A value like <c>Loves this</c>.</summary>
            public static Translation LovesThis() => L10n.Helper.Get("item.loves-this");

            /// <summary>A value like <c>Health</c>.</summary>
            public static Translation FenceHealth() => L10n.Helper.Get("item.fence-health");

            /// <summary>A value like <c>Recipes</c>.</summary>
            public static Translation Recipes() => L10n.Helper.Get("item.recipes");

            /// <summary>A value like <c>Owned</c>.</summary>
            public static Translation Owned() => L10n.Helper.Get("item.number-owned");

            /// <summary>A value like <c>Cooked</c>.</summary>
            public static Translation Cooked() => L10n.Helper.Get("item.number-cooked");

            /// <summary>A value like <c>Crafted</c>.</summary>
            public static Translation Crafted() => L10n.Helper.Get("item.number-crafted");

            /// <summary>A value like <c>See also</c>.</summary>
            public static Translation SeeAlso() => L10n.Helper.Get("item.see-also");

            /****
            ** Values
            ****/
            /// <summary>A value like <c>{{quality}} ready now</c>.</summary>
            public static Translation CaskScheduleNow(ItemQuality quality) => L10n.Helper.Get("item.cask-schedule.now", new { quality = L10n.For(quality) });

            /// <summary>A value like <c>{{quality}} now (use pickaxe to stop aging)</c>.</summary>
            public static Translation CaskSchedulePartial(ItemQuality quality) => L10n.Helper.Get("item.cask-schedule.now-partial", new { quality = L10n.For(quality) });

            /// <summary>A value like <c>{{quality}} tomorrow</c>.</summary>
            public static Translation CaskScheduleTomorrow(ItemQuality quality) => L10n.Helper.Get("item.cask-schedule.tomorrow", new { quality = L10n.For(quality) });

            /// <summary>A value like <c>{{quality}} in {{count}} days ({{date}})</c>.</summary>
            public static Translation CaskScheduleInXDays(ItemQuality quality, int count, SDate date) => L10n.Helper.Get("item.cask-schedule.in-x-days", new { quality = L10n.For(quality), count, date });

            /// <summary>A value like <c>has {{name}}</c>.</summary>
            public static Translation ContentsPlaced(string name) => L10n.Helper.Get("item.contents.placed", new { name });

            /// <summary>A value like <c>{{name}} ready</c>.</summary>
            public static Translation ContentsReady(string name) => L10n.Helper.Get("item.contents.ready", new { name });

            /// <summary>A value like <c>{{name}} in {{time}}</c>.</summary>
            public static Translation ContentsPartial(string name, string time) => L10n.Helper.Get("item.contents.partial", new { name, time });

            /// <summary>A value like <c>community center ({{bundles}})</c>.</summary>
            public static Translation NeededForCommunityCenter(string bundles) => L10n.Helper.Get("item.needed-for.community-center", new { bundles });

            /// <summary>A value like <c>full shipment achievement (ship one)</c>.</summary>
            public static Translation NeededForFullShipment() => L10n.Helper.Get("item.needed-for.full-shipment");

            /// <summary>A value like <c>polyculture achievement (ship {{count}} more)</c>.</summary>
            public static Translation NeededForPolyculture(int count) => L10n.Helper.Get("item.needed-for.polyculture", new { count });

            /// <summary>A value like <c>full collection achievement (donate one to museum)</c>.</summary>
            public static Translation NeededForFullCollection() => L10n.Helper.Get("item.needed-for.full-collection");

            /// <summary>A value like <c>gourmet chef achievement (cook {{recipes}})</c>.</summary>
            public static Translation NeededForGourmetChef(string recipes) => L10n.Helper.Get("item.needed-for.gourmet-chef", new { recipes });

            /// <summary>A value like <c>craft master achievement (make {{recipes}})</c>.</summary>
            public static Translation NeededForCraftMaster(string recipes) => L10n.Helper.Get("item.needed-for.craft-master", new { recipes });

            /// <summary>A value like <c>shipping box</c>.</summary>
            public static Translation SellsToShippingBox() => L10n.Helper.Get("item.sells-to.shipping-box");

            /// <summary>A value like <c>no decay with Gold Clock</c>.</summary>
            public static Translation FenceHealthGoldClock() => L10n.Helper.Get("item.fence-health.gold-clock");

            /// <summary>A value like <c>{{percent}}% (roughly {{count}} days left)</c>.</summary>
            public static Translation FenceHealthSummary(int percent, int count) => L10n.Helper.Get("item.fence-health.summary", new { percent, count });

            /// <summary>A value like <c>{{name}} (needs {{count}})</c>.</summary>
            public static Translation RecipesEntry(string name, int count) => L10n.Helper.Get("item.recipes.entry", new { name, count });

            /// <summary>A value like <c>you own {{count}} of these</c>.</summary>
            public static Translation OwnedSummary(int count) => L10n.Helper.Get("item.number-owned.summary", new { count });

            /// <summary>A value like <c>you made {{count}} of these</c>.</summary>
            public static Translation CraftedSummary(int count) => L10n.Helper.Get("item.number-crafted.summary", new { count });
        }

        /// <summary>Monster lookup translations.</summary>
        public static class Monster
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>Invincible</c>.</summary>
            public static Translation Invincible() => L10n.Helper.Get("monster.invincible");

            /// <summary>A value like <c>Health</c>.</summary>
            public static Translation Health() => L10n.Helper.Get("monster.health");

            /// <summary>A value like <c>Drops</c>.</summary>
            public static Translation Drops() => L10n.Helper.Get("monster.drops");

            /// <summary>A value like <c>XP</c>.</summary>
            public static Translation Experience() => L10n.Helper.Get("monster.experience");

            /// <summary>A value like <c>Defence</c>.</summary>
            public static Translation Defence() => L10n.Helper.Get("monster.defence");

            /// <summary>A value like <c>Attack</c>.</summary>
            public static Translation Attack() => L10n.Helper.Get("monster.attack");

            /// <summary>A value like <c>Adventure Guild</c>.</summary>
            public static Translation AdventureGuild() => L10n.Helper.Get("monster.adventure-guild");

            /****
            ** Values
            ****/
            /// <summary>A value like <c>nothing</c>.</summary>
            public static Translation DropsNothing() => L10n.Helper.Get("monster.drops.nothing");

            /// <summary>A value like <c>complete</c>.</summary>
            public static Translation AdventureGuildComplete() => L10n.Helper.Get("monster.adventure-guild.complete");

            /// <summary>A value like <c>in progress</c>.</summary>
            public static Translation AdventureGuildIncomplete() => L10n.Helper.Get("monster.adventure-guild.incomplete");

            /// <summary>A value like <c>killed {{count}} of {{requiredCount}}</c>.</summary>
            public static Translation AdventureGuildProgress(int count, int requiredCount) => L10n.Helper.Get("monster.adventure-guild.progress", new { count, requiredCount });
        }

        /// <summary>NPC lookup translations.</summary>
        public static class Npc
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>Birthday</c>.</summary>
            public static Translation Birthday() => L10n.Helper.Get("npc.birthday");

            /// <summary>A value like <c>Can romance</c>.</summary>
            public static Translation CanRomance() => L10n.Helper.Get("npc.can-romance");

            /// <summary>A value like <c>Friendship</c>.</summary>
            public static Translation Friendship() => L10n.Helper.Get("npc.friendship");

            /// <summary>A value like <c>Talked today</c>.</summary>
            public static Translation TalkedToday() => L10n.Helper.Get("npc.talked-today");

            /// <summary>A value like <c>Gifted today</c>.</summary>
            public static Translation GiftedToday() => L10n.Helper.Get("npc.gifted-today");

            /// <summary>A value like <c>Kissed today</c>.</summary>
            public static Translation KissedToday() => L10n.Helper.Get("npc.kissed-today");

            /// <summary>A value like <c>Gifted this week</c>.</summary>
            public static Translation GiftedThisWeek() => L10n.Helper.Get("npc.gifted-this-week");

            /// <summary>A value like <c>Likes gifts</c>.</summary>
            public static Translation LikesGifts() => L10n.Helper.Get("npc.likes-gifts");

            /// <summary>A value like <c>Loves gifts</c>.</summary>
            public static Translation LovesGifts() => L10n.Helper.Get("npc.loves-gifts");

            /// <summary>A value like <c>Neutral gifts</c>.</summary>
            public static Translation NeutralGifts() => L10n.Helper.Get("npc.neutral-gifts");

            /****
            ** Values
            ****/
            /// <summary>A value like <c>You're married! &lt;</c>.</summary>
            public static Translation CanRomanceMarried() => L10n.Helper.Get("npc.can-romance.married");

            /// <summary>A value like <c>You haven't met them yet.</c>.</summary>
            public static Translation FriendshipNotMet() => L10n.Helper.Get("npc.friendship.not-met");

            /// <summary>A value like <c>need bouquet for next</c>.</summary>
            public static Translation FriendshipNeedBouquet() => L10n.Helper.Get("npc.friendship.need-bouquet");

            /// <summary>A value like <c>next in {{count}} pts</c>.</summary>
            public static Translation FriendshipNeedPoints(int count) => L10n.Helper.Get("npc.friendship.need-points", new { count });
        }

        /// <summary>NPC child lookup translations.</summary>
        public static class NpcChild
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>Age</c>.</summary>
            public static Translation Age() => L10n.Helper.Get("npc.child.age");

            /****
            ** Values
            ****/
            /// <summary>A value like <c>{{label}} ({{count}} days to {{nextLabel}})</c>.</summary>
            public static Translation AgeDescriptionPartial(ChildAge label, int count, ChildAge nextLabel) => L10n.Helper.Get("npc.child.age.description-partial", new { label = L10n.For(label), count, nextLabel = L10n.For(nextLabel) });

            /// <summary>A value like <c>{{label}}</c>.</summary>
            public static Translation AgeDescriptionGrown(ChildAge label) => L10n.Helper.Get("npc.child.age.description-grown", new { label = L10n.For(label) });

            /// <summary>A value like <c>newborn</c>.</summary>
            public static Translation AgeNewborn() => L10n.Helper.Get("npc.child.age.newborn");

            /// <summary>A value like <c>baby</c>.</summary>
            public static Translation AgeBaby() => L10n.Helper.Get("npc.child.age.baby");

            /// <summary>A value like <c>crawler</c>.</summary>
            public static Translation AgeCrawler() => L10n.Helper.Get("npc.child.age.crawler");

            /// <summary>A value like <c>toddler</c>.</summary>
            public static Translation AgeToddler() => L10n.Helper.Get("npc.child.age.toddler");
        }

        /// <summary>Pet lookup translations.</summary>
        public static class Pet
        {
            /// <summary>A value like <c>Love</c>.</summary>
            public static Translation Love() => L10n.Helper.Get("pet.love");

            /// <summary>A value like <c>Petted today</c>.</summary>
            public static Translation PettedToday() => L10n.Helper.Get("pet.petted-today");
        }

        /// <summary>Player lookup translations.</summary>
        public static class Player
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>Farm name</c>.</summary>
            public static Translation FarmName() => L10n.Helper.Get("player.farm-name");

            /// <summary>A value like <c>Farm map</c>.</summary>
            public static Translation FarmMap() => L10n.Helper.Get("player.farm-map");

            /// <summary>A value like <c>Favourite thing</c>.</summary>
            public static Translation FavoriteThing() => L10n.Helper.Get("player.favorite-thing");

            /// <summary>A value like <c>Gender</c>.</summary>
            public static Translation Gender() => L10n.Helper.Get("player.gender");

            /// <summary>A value like <c>Spouse</c>.</summary>
            public static Translation Spouse() => L10n.Helper.Get("player.spouse");

            /// <summary>A value like <c>Combat skill</c>.</summary>
            public static Translation CombatSkill() => L10n.Helper.Get("player.combat-skill");

            /// <summary>A value like <c>Farming skill</c>.</summary>
            public static Translation FarmingSkill() => L10n.Helper.Get("player.farming-skill");

            /// <summary>A value like <c>Foraging skill</c>.</summary>
            public static Translation ForagingSkill() => L10n.Helper.Get("player.foraging-skill");

            /// <summary>A value like <c>Fishing skill</c>.</summary>
            public static Translation FishingSkill() => L10n.Helper.Get("player.fishing-skill");

            /// <summary>A value like <c>Mining skill</c>.</summary>
            public static Translation MiningSkill() => L10n.Helper.Get("player.mining-skill");

            /// <summary>A value like <c>Luck</c>.</summary>
            public static Translation Luck() => L10n.Helper.Get("player.luck");

            /****
            ** Values
            ****/
            /// <summary>A value like <c>Custom</c>.</summary>
            public static Translation FarmMapCustom() => L10n.Helper.Get("player.farm-map.custom");

            /// <summary>A value like <c>male</c>.</summary>
            public static Translation GenderMale() => L10n.Helper.Get("player.gender.male");

            /// <summary>A value like <c>female</c>.</summary>
            public static Translation GenderFemale() => L10n.Helper.Get("player.gender.female");

            /// <summary>A value like <c>({{percent}}% to many random checks)</c>.</summary>
            public static Translation LuckSummary(string percent) => L10n.Helper.Get("player.luck.summary", new { percent });

            /// <summary>A value like <c>level {{level}} ({{expNeeded}} XP to next)</c>.</summary>
            public static Translation SkillProgress(int level, int expNeeded) => L10n.Helper.Get("player.skill.progress", new { level, expNeeded });

            /// <summary>A value like <c>level {{level}}</c>.</summary>
            public static Translation SkillProgressLast(int level) => L10n.Helper.Get("player.skill.progress-last", new { level });
        }

        /// <summary>Tile lookup translations.</summary>
        public static class Tile
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>A tile position on the map. This is displayed because you enabled tile lookups in the configuration.</c>.</summary>
            public static Translation Description() => L10n.Helper.Get("tile.description");

            /// <summary>A value like <c>Map name</c>.</summary>
            public static Translation MapName() => L10n.Helper.Get("tile.map-name");

            /// <summary>A value like <c>Tile</c>.</summary>
            public static Translation TileField() => L10n.Helper.Get("tile.tile");

            /// <summary>A value like <c>{{layerName}}: tile index</c>.</summary>
            public static Translation TileIndex(string layerName) => L10n.Helper.Get("tile.tile-index", new { layerName });

            /// <summary>A value like <c>{{layerName}}: tilesheet</c>.</summary>
            public static Translation TileSheet(string layerName) => L10n.Helper.Get("tile.tilesheet", new { layerName });

            /// <summary>A value like <c>{{layerName}}: blend mode</c>.</summary>
            public static Translation BlendMode(string layerName) => L10n.Helper.Get("tile.blend-mode", new { layerName });

            /// <summary>A value like <c>{{layerName}}: ix props: {{propertyName}}</c>.</summary>
            public static Translation IndexProperty(string layerName, string propertyName) => L10n.Helper.Get("tile.index-property", new { layerName, propertyName });

            /// <summary>A value like <c>{{layerName}}: props: {{propertyName}}</c>.</summary>
            public static Translation TileProperty(string layerName, string propertyName) => L10n.Helper.Get("tile.tile-property", new { layerName, propertyName });

            /****
            ** Values
            ****/
            /// <summary>A value like <c>no tile here</c>.</summary>
            public static Translation TileFieldNoneFound() => L10n.Helper.Get("tile.tile.none-here");
        }

        /// <summary>Wild tree lookup translations.</summary>
        public static class Tree
        {
            /****
            ** Labels
            ****/
            /// <summary>A value like <c>Maple Tree</c>.</summary>
            public static Translation NameMaple() => L10n.Helper.Get("tree.name.maple");

            /// <summary>A value like <c>Oak Tree</c>.</summary>
            public static Translation NameOak() => L10n.Helper.Get("tree.name.oak");

            /// <summary>A value like <c>Pine Tree</c>.</summary>
            public static Translation NamePine() => L10n.Helper.Get("tree.name.pine");

            /// <summary>A value like <c>Palm Tree</c>.</summary>
            public static Translation NamePalm() => L10n.Helper.Get("tree.name.palm");

            /// <summary>A value like <c>Big Mushroom</c>.</summary>
            public static Translation NameBigMushroom() => L10n.Helper.Get("tree.name.big-mushroom");

            /// <summary>A value like <c>Unknown Tree</c>.</summary>
            public static Translation NameUnknown() => L10n.Helper.Get("tree.name.unknown");

            /// <summary>A value like <c>Growth stage</c>.</summary>
            public static Translation Stage() => L10n.Helper.Get("tree.stage");

            /// <summary>A value like <c>Next growth</c>.</summary>
            public static Translation NextGrowth() => L10n.Helper.Get("tree.next-growth");

            /// <summary>A value like <c>Has seed</c>.</summary>
            public static Translation HasSeed() => L10n.Helper.Get("tree.has-seed");

            /****
            ** Values
            ****/
            /// <summary>A value like <c>Fully grown</c>.</summary>
            public static Translation StageDone() => L10n.Helper.Get("tree.stage.done");

            /// <summary>A value like <c>{{stageName}} ({{step}} of {{max}})</c>.</summary>
            public static Translation StagePartial(string stageName, int step, int max) => L10n.Helper.Get("tree.stage.partial", new { stageName, step, max });

            /// <summary>A value like <c>can't grow in winter outside greenhouse</c>.</summary>
            public static Translation NextGrowthWinter() => L10n.Helper.Get("tree.next-growth.winter");

            /// <summary>A value like <c>can't grow because other trees are too close</c>.</summary>
            public static Translation NextGrowthAdjacentTrees() => L10n.Helper.Get("tree.next-growth.adjacent-trees");

            /// <summary>A value like <c>20% chance to grow into {{stage}} tomorrow</c>.</summary>
            public static Translation NextGrowthRandom(string stage) => L10n.Helper.Get("tree.next-growth.random", new { stage });
        }

        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the static helper.</summary>
        /// <param name="translation">The translation helper.</param>
        public static void Init(ITranslationHelper translation)
        {
            L10n.Helper = translation;
        }

        /// <summary>Get a translation for an enum value.</summary>
        /// <param name="stage">The tree growth stage.</param>
        public static Translation For(WildTreeGrowthStage stage)
        {
            return L10n.Helper.Get($"tree.stages.{stage}");
        }

        /// <summary>Get a translation for an enum value.</summary>
        /// <param name="quality">The item quality.</param>
        public static Translation For(ItemQuality quality)
        {
            return L10n.Helper.Get($"quality.{quality.GetName()}");
        }

        /// <summary>Get a translation for an enum value.</summary>
        /// <param name="status">The friendship status.</param>
        public static Translation For(FriendshipStatus status)
        {
            return L10n.Helper.Get($"friendship-status.{status.ToString().ToLower()}");
        }

        /// <summary>Get a translation for an enum value.</summary>
        /// <param name="age">The child age.</param>
        public static Translation For(ChildAge age)
        {
            return L10n.Helper.Get($"npc.child.age.{age.ToString().ToLower()}");
        }

        /// <summary>Get a translation for the current locale.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An anonymous object containing token key/value pairs, like <c>new { value = 42, name = "Cranberries" }</c>.</param>
        /// <exception cref="KeyNotFoundException">The <paramref name="key" /> doesn't match an available translation.</exception>
        public static Translation GetRaw(string key, object tokens = null)
        {
            return L10n.Helper.Get(key, tokens);
        }
    }
}
