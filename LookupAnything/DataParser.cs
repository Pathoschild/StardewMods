using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.ExtraMachineConfig;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Machines;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TokenizableStrings;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything;

/// <summary>Parses the raw game data into usable models. These may be expensive operations and should be cached.</summary>
internal class DataParser
{
    /*********
    ** Fields
    *********/
    /// <summary>The placeholder item ID for a recipe which can't be parsed due to its complexity.</summary>
    public const string ComplexRecipeId = "__COMPLEX_RECIPE__";


    /*********
    ** Public methods
    *********/
    /// <summary>Read parsed data about the Community Center bundles.</summary>
    /// <param name="monitor">The monitor with which to log errors.</param>
    /// <remarks>Derived from the <see cref="StardewValley.Locations.CommunityCenter"/> constructor and <see cref="StardewValley.Menus.JunimoNoteMenu.openRewardsMenu"/>.</remarks>
    public IEnumerable<BundleModel> GetBundles(IMonitor monitor)
    {
        foreach ((string key, string? value) in Game1.netWorldState.Value.BundleData)
        {
            if (value is null)
                continue;

            BundleModel bundle;
            try
            {
                // parse key
                string[] keyParts = key.Split('/');
                string area = ArgUtility.Get(keyParts, 0);
                int id = ArgUtility.GetInt(keyParts, 1);

                // parse bundle info
                string[] valueParts = value.Split('/');
                string name = ArgUtility.Get(valueParts, Bundle.NameIndex);
                string reward = ArgUtility.Get(valueParts, Bundle.RewardIndex);
                string displayName = ArgUtility.Get(valueParts, Bundle.DisplayNameIndex);

                // parse ingredients
                List<BundleIngredientModel> ingredients = [];
                string[] ingredientData = ArgUtility.SplitBySpace(ArgUtility.Get(valueParts, 2));
                for (int i = 0; i < ingredientData.Length; i += 3)
                {
                    int index = i / 3;
                    string itemID = ArgUtility.Get(ingredientData, i);
                    int stack = ArgUtility.GetInt(ingredientData, i + 1);
                    ItemQuality quality = ArgUtility.GetEnum<ItemQuality>(ingredientData, i + 2);
                    ingredients.Add(new BundleIngredientModel(index, itemID, stack, quality));
                }

                // create bundle
                bundle = new BundleModel(
                    ID: id,
                    Name: name,
                    DisplayName: displayName,
                    Area: area,
                    RewardData: reward,
                    Ingredients: ingredients.ToArray()
                );
            }
            catch (Exception ex)
            {
                monitor.LogOnce($"Couldn't parse community center bundle '{key}' due to an invalid format.\nRecipe data: '{value}'\nError: {ex}", LogLevel.Warn);
                continue;
            }

            yield return bundle;
        }
    }

    /// <summary>Read parsed data about a fish pond's population gates for a specific fish.</summary>
    /// <param name="data">The fish pond data.</param>
    public IEnumerable<FishPondPopulationGateData> GetFishPondPopulationGates(FishPondData data)
    {
        if (data.PopulationGates is null)
            yield break;

        foreach ((int minPopulation, List<string?>? rawData) in data.PopulationGates)
        {
            if (rawData is null)
                continue;

            // get required items
            FishPondPopulationGateQuestItemData[] questItems = rawData
                .Select(entry =>
                {
                    // parse ID
                    string[] parts = ArgUtility.SplitBySpace(entry);
                    if (parts.Length is < 1 or > 3)
                        return null;

                    // parse counts
                    string id = ArgUtility.Get(parts, 0);
                    int minCount = ArgUtility.GetInt(parts, 1, 1);
                    int maxCount = ArgUtility.GetInt(parts, 2, 1);

                    // normalize counts
                    minCount = Math.Max(1, minCount);
                    maxCount = Math.Max(1, maxCount);
                    if (maxCount < minCount)
                        maxCount = minCount;

                    // build entry
                    return new FishPondPopulationGateQuestItemData(id, minCount, maxCount);
                })
                .WhereNotNull()
                .ToArray();

            // build entry
            yield return new FishPondPopulationGateData(minPopulation, questItems);
        }
    }

    /// <summary>Read parsed data about a fish pond's item drops for a specific fish.</summary>
    /// <param name="data">The fish pond data.</param>
    public IEnumerable<FishPondDropData> GetFishPondDrops(FishPondData data)
    {
        if (data.ProducedItems is null)
            yield break;

        foreach (FishPondReward? drop in data.ProducedItems)
        {
            if (drop is null)
                continue;

            IList<ItemQueryResult> itemQueryResults = ItemQueryResolver.TryResolve(drop, new ItemQueryContext(), ItemQuerySearchMode.AllOfTypeItem);

            float chance = drop.Chance * (1f / itemQueryResults.Count);
            foreach (ItemQueryResult result in itemQueryResults)
            {
                if (result.Item is Item item)
                    yield return new FishPondDropData(drop.RequiredPopulation, drop.Precedence, item, drop.MinStack, drop.MaxStack, chance, drop.Condition);
            }
        }
    }

    /// <summary>Read parsed data about the spawn rules for a specific fish.</summary>
    /// <param name="fish">The fish item data.</param>
    /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
    /// <remarks>Derived from <see cref="GameLocation.getFish"/>.</remarks>
    public FishSpawnData GetFishSpawnRules(ParsedItemData fish, Metadata metadata)
    {
        // parse location and condition data
        var locations = new List<FishSpawnLocationData>();
        bool isLegendaryFamily = false;
        foreach ((string locationId, LocationData? data) in Game1.locationData)
        {
            if (metadata.IgnoreFishingLocations.Contains(locationId))
                continue; // ignore event data

            List<FishSpawnLocationData> curLocations = [];
            if (data?.Fish is not null)
            {
                foreach (SpawnFishData spawn in data.Fish)
                {
                    if (spawn is null)
                        continue;

                    ParsedItemData? spawnItemData = ItemRegistry.GetData(spawn.ItemId);
                    if (spawnItemData?.ObjectType != "Fish" || spawnItemData.QualifiedItemId != fish.QualifiedItemId)
                        continue;

                    if (spawn.Season.HasValue)
                    {
                        curLocations.Add(new FishSpawnLocationData(locationId, spawn.FishAreaId, new[] { spawn.Season.Value.ToString() }));
                    }
                    else if (spawn.Condition != null)
                    {
                        foreach (GameStateQuery.ParsedGameStateQuery condition in GameStateQuery.Parse(spawn.Condition))
                        {
                            if (condition.Query.Length == 0)
                                continue;

                            // season
                            if (GameStateQuery.SeasonQueryKeys.Contains(condition.Query[0]))
                            {
                                var seasons = new List<string>();
                                foreach (string season in new[] { "spring", "summer", "fall", "winter" })
                                {
                                    if (!condition.Negated && condition.Query.Any(word => word.Equals(season, StringComparison.OrdinalIgnoreCase)))
                                        seasons.Add(season);
                                }
                                curLocations.Add(new FishSpawnLocationData(locationId, spawn.FishAreaId, seasons.ToArray()));
                            }

                            // Qi's Extended Family quest
                            else if (!isLegendaryFamily && condition is { Negated: false, Query: ["PLAYER_SPECIAL_ORDER_RULE_ACTIVE", "Current", "LEGENDARY_FAMILY"] })
                                isLegendaryFamily = true;
                        }
                    }
                    else
                        curLocations.Add(new FishSpawnLocationData(locationId, spawn.FishAreaId, new[] { "spring", "summer", "fall", "winter" }));
                }
            }

            // combine seasons for same area
            if (curLocations.Count > 0)
            {
                locations.AddRange(
                    from areaGroup in curLocations.GroupBy(p => p.Area)
                    let seasons = areaGroup.SelectMany(p => p.Seasons).Distinct().ToArray()
                    select new FishSpawnLocationData(locationId, areaGroup.Key, seasons)
                );
            }
        }

        // parse fish data
        var timesOfDay = new List<FishSpawnTimeOfDayData>();
        FishSpawnWeather weather = FishSpawnWeather.Both;
        int minFishingLevel = 0;
        bool isUnique = false;
        if (fish.HasTypeObject())
        {
            if (locations.Any()) // ignore default spawn criteria if the fish doesn't spawn naturally; in that case it should be specified explicitly in custom data below (if any)
            {
                if (DataLoader.Fish(Game1.content).TryGetValue(fish.ItemId, out string? rawData) && rawData is not null)
                {
                    string[] fishFields = rawData.Split('/');

                    // times of day
                    string[] timeFields = ArgUtility.SplitBySpace(ArgUtility.Get(fishFields, 5));
                    for (int i = 0, last = timeFields.Length + 1; i + 1 < last; i += 2)
                    {
                        if (ArgUtility.TryGetInt(timeFields, i, out int minTime, out _) && ArgUtility.TryGetInt(timeFields, i + 1, out int maxTime, out _))
                            timesOfDay.Add(new FishSpawnTimeOfDayData(minTime, maxTime));
                    }

                    // weather
                    if (!ArgUtility.TryGetEnum(fishFields, 7, out weather, out _))
                        weather = FishSpawnWeather.Both;

                    // min fishing level
                    if (!ArgUtility.TryGetInt(fishFields, 12, out minFishingLevel, out _))
                        minFishingLevel = 0;
                }
            }
        }

        // read custom data
        if (metadata.CustomFishSpawnRules.TryGetValue(fish.QualifiedItemId, out FishSpawnData? customRules))
        {
            if (customRules.MinFishingLevel > minFishingLevel)
                minFishingLevel = customRules.MinFishingLevel;

            if (customRules.Weather != FishSpawnWeather.Unknown)
                weather = customRules.Weather;

            isUnique = isUnique || customRules.IsUnique;

            if (customRules.TimesOfDay != null)
                timesOfDay.AddRange(customRules.TimesOfDay);

            if (customRules.Locations != null)
                locations.AddRange(customRules.Locations);
        }


        // build model
        return new FishSpawnData(
            FishItem: fish,
            Locations: locations.ToArray(),
            TimesOfDay: timesOfDay.ToArray(),
            Weather: weather,
            MinFishingLevel: minFishingLevel,
            IsUnique: isUnique,
            IsLegendaryFamily: isLegendaryFamily
        );
    }

    /// <summary>Read parsed data about the spawn rules for fish in a specific location.</summary>
    /// <param name="location">The location for which to get the spawn rules.</param>
    /// <param name="tile">The tile for which to get the spawn rules.</param>
    /// <param name="fishAreaId">The internal ID of the fishing area for which to get the spawn rules.</param>
    /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
    public IEnumerable<FishSpawnData> GetFishSpawnRules(GameLocation location, Vector2 tile, string fishAreaId, Metadata metadata)
    {
        // get fish from game data
        HashSet<string> seenFishIds = [];
        List<SpawnFishData?>? locationFish = location.GetData()?.Fish;
        if (locationFish is not null)
        {
            foreach (SpawnFishData? fishData in locationFish)
            {
                if (fishData?.ItemId is null)
                    continue;

                seenFishIds.Add(fishData.ItemId);

                // skip if fish can't spawn in this body of water
                if (fishData.FishAreaId != null && fishData.FishAreaId != fishAreaId)
                    continue;

                // skip if position doesn't match
                if (fishData.BobberPosition?.Contains(tile) is false)
                    continue;
                if (fishData.PlayerPosition?.Contains(Game1.player.TilePoint) is false)
                    continue;

                // skip if data isn't for a fish or jelly (e.g. furniture)
                ParsedItemData fish = ItemRegistry.GetDataOrErrorItem(fishData.ItemId);
                if (fish.ObjectType != "Fish")
                    continue;

                yield return this.GetFishSpawnRules(fish, metadata);
            }
        }

        // get fish from custom metadata
        foreach ((string fishId, FishSpawnData spawnData) in metadata.CustomFishSpawnRules)
        {
            // skip if we already checked this fish, even if we skipped it (e.g. due to spawning only in a certain fishing area in a location)
            if (seenFishIds.Contains(fishId))
                continue;

            // skip if spawn location doesn't match
            if (!spawnData.MatchesLocation(location.Name))
                continue;

            ParsedItemData fish = ItemRegistry.GetDataOrErrorItem(fishId);
            yield return this.GetFishSpawnRules(fish, metadata);
        }
    }

    /// <summary>Get parsed data about the friendship between a player and NPC.</summary>
    /// <param name="player">The player.</param>
    /// <param name="npc">The NPC.</param>
    /// <param name="friendship">The current friendship data.</param>
    /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
    public FriendshipModel GetFriendshipForVillager(SFarmer player, NPC npc, Friendship friendship, Metadata metadata)
    {
        return new FriendshipModel(player, npc, friendship, metadata.Constants);
    }

    /// <summary>Get parsed data about the friendship between a player and NPC.</summary>
    /// <param name="player">The player.</param>
    /// <param name="pet">The pet.</param>
    public FriendshipModel GetFriendshipForPet(SFarmer player, Pet pet)
    {
        return new FriendshipModel(pet.friendshipTowardFarmer.Value, Pet.maxFriendship / 10, Pet.maxFriendship);
    }

    /// <summary>Get parsed data about the friendship between a player and NPC.</summary>
    /// <param name="player">The player.</param>
    /// <param name="animal">The farm animal.</param>
    /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
    public FriendshipModel GetFriendshipForAnimal(SFarmer player, FarmAnimal animal, Metadata metadata)
    {
        return new FriendshipModel(animal.friendshipTowardFarmer.Value, metadata.Constants.AnimalFriendshipPointsPerLevel, metadata.Constants.AnimalFriendshipMaxPoints);
    }

    /// <summary>Get the translated display name for a fish spawn location.</summary>
    /// <param name="fishSpawnData">The location-specific spawn rules for which to get a location name.</param>
    /// <exception cref="NotSupportedException">If the location ID of fishSpawnData does not exist in the game data.</exception>
    public string GetLocationDisplayName(FishSpawnLocationData fishSpawnData)
    {
        if (!Game1.locationData.TryGetValue(fishSpawnData.LocationId, out LocationData? locationData))
            locationData = null;

        return this.GetLocationDisplayName(fishSpawnData.LocationId, locationData, fishSpawnData.Area);
    }

    /// <summary>Get the translated display name for a location.</summary>
    /// <param name="id">The location's internal name.</param>
    /// <param name="data">The location data, if available.</param>
    public string GetLocationDisplayName(string id, LocationData? data)
    {
        // from predefined translations
        {
            string name = I18n.GetByKey($"location.{id}").UsePlaceholder(false);
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        // from location data
        if (data != null)
        {
            string name = TokenParser.ParseText(data.DisplayName);
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        // else default to ID
        return id;
    }

    /// <summary>Get the translated display name for a location and optional fish area.</summary>
    /// <param name="id">The location's internal name.</param>
    /// <param name="data">The location data, if available.</param>
    /// <param name="fishAreaId">The fish area ID within the location, if applicable.</param>
    public string GetLocationDisplayName(string id, LocationData? data, string? fishAreaId)
    {
        // special cases
        {
            // mine level
            if (MineShaft.IsGeneratedLevel(id, out int mineLevel))
            {
                string level = fishAreaId ?? mineLevel.ToString(); // sometimes the mine level is provided as the fish area ID, other times it's included in the location name

                return !string.IsNullOrWhiteSpace(level)
                    ? I18n.Location_UndergroundMine_Level(level)
                    : this.GetLocationDisplayName(id, data);
            }

            // no area set
            if (string.IsNullOrWhiteSpace(fishAreaId))
                return this.GetLocationDisplayName(id, data);
        }

        // get base data
        string locationName = this.GetLocationDisplayName(id, data);
        string areaName = TokenParser.ParseText(data?.FishAreas?.GetValueOrDefault(fishAreaId)?.DisplayName);

        // build translation
        string displayName = I18n.GetByKey($"location.{id}.{fishAreaId}", new { locationName }).UsePlaceholder(false); // predefined translation
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = !string.IsNullOrWhiteSpace(areaName)
                ? I18n.Location_FishArea(locationName: locationName, areaName: areaName)
                : I18n.Location_UnknownFishArea(locationName: locationName, id: fishAreaId);
        }
        return displayName;
    }

    /// <summary>Parse monster data.</summary>
    /// <remarks>Reverse engineered from <see cref="StardewValley.Monsters.Monster.parseMonsterInfo"/>, <see cref="GameLocation.monsterDrop"/>, and the <see cref="Debris"/> constructor.</remarks>
    public IEnumerable<MonsterData> GetMonsters()
    {
        foreach ((string name, string? rawData) in DataLoader.Monsters(Game1.content))
        {
            if (rawData is null)
                continue;

            // monster fields
            string[] fields = rawData.Split('/');
            int health = ArgUtility.GetInt(fields, Monster.index_health);
            int damageToFarmer = ArgUtility.GetInt(fields, Monster.index_damageToFarmer);
            bool isGlider = ArgUtility.GetBool(fields, Monster.index_isGlider);
            int resilience = ArgUtility.GetInt(fields, Monster.index_resilience);
            double jitteriness = ArgUtility.GetFloat(fields, Monster.index_jitteriness);
            int moveTowardsPlayerThreshold = ArgUtility.GetInt(fields, Monster.index_distanceThresholdToMoveTowardsPlayer);
            int speed = ArgUtility.GetInt(fields, Monster.index_speed);
            double missChance = ArgUtility.GetFloat(fields, Monster.index_missChance);
            bool isMineMonster = ArgUtility.GetBool(fields, Monster.index_isMineMonster);

            // drops
            var drops = new List<ItemDropData>();
            string[] dropFields = ArgUtility.SplitBySpace(ArgUtility.Get(fields, Monster.index_drops));
            for (int i = 0; i < dropFields.Length; i += 2)
            {
                // get drop info
                string itemId = ArgUtility.Get(dropFields, i);
                float chance = ArgUtility.GetFloat(dropFields, i + 1);
                int maxDrops = 1;

                // if itemID is negative, game randomly drops 1-3
                if (int.TryParse(itemId, out int id) && id < 0)
                {
                    itemId = (-id).ToString();
                    maxDrops = 3;
                }

                // some item IDs have special meaning
                if (itemId == Debris.copperDebris.ToString())
                    itemId = SObject.copper.ToString();
                else if (itemId == Debris.ironDebris.ToString())
                    itemId = SObject.iron.ToString();
                else if (itemId == Debris.coalDebris.ToString())
                    itemId = SObject.coal.ToString();
                else if (itemId == Debris.goldDebris.ToString())
                    itemId = SObject.gold.ToString();
                else if (itemId == Debris.coinsDebris.ToString())
                    continue; // no drop
                else if (itemId == Debris.iridiumDebris.ToString())
                    itemId = SObject.iridium.ToString();
                else if (itemId == Debris.woodDebris.ToString())
                    itemId = SObject.wood.ToString();
                else if (itemId == Debris.stoneDebris.ToString())
                    itemId = SObject.stone.ToString();

                // add drop
                drops.Add(new ItemDropData(itemId, 1, maxDrops, chance));
            }
            if (isMineMonster && Game1.player.timesReachedMineBottom >= 1)
            {
                drops.Add(new ItemDropData(SObject.diamondIndex.ToString(), 1, 1, 0.008f));
                drops.Add(new ItemDropData(SObject.prismaticShardIndex.ToString(), 1, 1, 0.008f));
            }

            // yield data
            yield return new MonsterData(
                Name: name,
                Health: health,
                DamageToFarmer: damageToFarmer,
                IsGlider: isGlider,
                Resilience: resilience,
                Jitteriness: jitteriness,
                MoveTowardsPlayerThreshold: moveTowardsPlayerThreshold,
                Speed: speed,
                MissChance: missChance,
                IsMineMonster: isMineMonster,
                Drops: drops.ToArray()
            );
        }
    }

    /// <summary>Get the recipe ingredients.</summary>
    /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
    /// <param name="monitor">The monitor with which to log errors.</param>
    /// <param name="extraMachineConfig">The Extra Machine Config mod's API.</param>
    public RecipeModel[] GetRecipes(Metadata metadata, IMonitor monitor, ExtraMachineConfigIntegration extraMachineConfig)
    {
        List<RecipeModel> recipes = [];

        // cooking/crafting recipes
        var craftingRecipes =
            (from pair in CraftingRecipe.cookingRecipes select new { pair.Key, pair.Value, IsCookingRecipe = true })
            .Concat(from pair in CraftingRecipe.craftingRecipes select new { pair.Key, pair.Value, IsCookingRecipe = false });
        foreach (var entry in craftingRecipes)
        {
            if (entry.Value is null)
                continue;

            try
            {
                var recipe = new CraftingRecipe(entry.Key, entry.IsCookingRecipe);

                foreach (string itemId in recipe.itemToProduce)
                {
                    string qualifiedItemId = RecipeModel.QualifyRecipeOutputId(recipe, itemId) ?? itemId;
                    recipes.Add(new RecipeModel(recipe, outputQualifiedItemId: qualifiedItemId));
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Couldn't parse {(entry.IsCookingRecipe ? "cooking" : "crafting")} recipe '{entry.Key}' due to an invalid format.\nRecipe data: '{entry.Value}'\nError: {ex}", LogLevel.Warn);
            }
        }

        // machine recipes from Data/Machines
        foreach ((string entryKey, MachineData? machineData) in DataLoader.Machines(Game1.content))
        {
            string qualifiedMachineId = entryKey; // avoid referencing loop variable in closure

            if (!ItemRegistry.Exists(qualifiedMachineId) || machineData?.OutputRules?.Count is not > 0)
                continue;

            RecipeIngredientModel[] additionalConsumedItems =
                machineData.AdditionalConsumedItems?.Select(item => new RecipeIngredientModel(RecipeType.MachineInput, item.ItemId, item.RequiredCount)).ToArray()
                ?? [];

            bool someRulesTooComplex = false;

            foreach (MachineOutputRule? outputRule in machineData.OutputRules)
            {
                if (outputRule?.Triggers?.Count is not > 0 || outputRule.OutputItem?.Count is not > 0)
                    continue;

                foreach (MachineOutputTriggerRule? trigger in outputRule.Triggers)
                {
                    if (trigger is null)
                        continue;

                    // build output list
                    foreach (MachineItemOutput? mainOutputItem in outputRule.OutputItem)
                    {
                        if (mainOutputItem is null)
                            continue;

                        // if there are extra outputs added by the Extra Machine Config mod, add them here
                        MachineItemOutput[] allOutputItems = extraMachineConfig.IsLoaded
                            ? [mainOutputItem, .. extraMachineConfig.ModApi.GetExtraOutputs(mainOutputItem, machineData)]
                            : [mainOutputItem];

                        foreach (MachineItemOutput outputItem in allOutputItems)
                        {
                            // get conditions
                            List<string>? conditions = null;
                            {
                                // extract raw conditions
                                string? rawConditions = null;
                                if (!string.IsNullOrWhiteSpace(trigger.Condition))
                                    rawConditions = trigger.Condition;

                                // add main output's condition
                                if (!string.IsNullOrWhiteSpace(mainOutputItem.Condition))
                                {
                                    rawConditions = rawConditions != null
                                        ? rawConditions + ", " + mainOutputItem.Condition
                                        : mainOutputItem.Condition;
                                }

                                // add secondary output's condition from Extra Machine Config mod
                                if (!string.IsNullOrWhiteSpace(outputItem.Condition) && outputItem.Condition != mainOutputItem.Condition)
                                {
                                    rawConditions = rawConditions != null
                                        ? rawConditions + ", " + outputItem.Condition
                                        : outputItem.Condition;
                                }

                                // parse
                                if (rawConditions != null)
                                    conditions = GameStateQuery.SplitRaw(rawConditions).Distinct().ToList();
                            }

                            // get ingredient
                            if (!this.TryGetMostSpecificIngredientIds(trigger.RequiredItemId, trigger.RequiredTags, ref conditions, out string? inputId, out string[] inputContextTags))
                                continue;

                            // track whether some recipes are too complex to fully display
                            if (outputItem.OutputMethod != null)
                                someRulesTooComplex = true;

                            // add ingredients
                            List<RecipeIngredientModel> ingredients = [
                                new(RecipeType.MachineInput, inputId, trigger.RequiredCount, inputContextTags)
                            ];
                            ingredients.AddRange(additionalConsumedItems);

                            // if there are extra fuels added by the Extra Machine Config mod, add them here
                            if (extraMachineConfig.IsLoaded)
                            {
                                foreach ((string extraItemId, int extraCount) in extraMachineConfig.ModApi.GetExtraRequirements(outputItem))
                                    ingredients.Add(new RecipeIngredientModel(RecipeType.MachineInput, extraItemId, extraCount));

                                foreach ((string extraContextTags, int extraCount) in extraMachineConfig.ModApi.GetExtraTagsRequirements(outputItem))
                                    ingredients.Add(new RecipeIngredientModel(RecipeType.MachineInput, null, extraCount, extraContextTags.Split(",")));
                            }

                            // add produced item
                            IList<ItemQueryResult> itemQueryResults;
                            if (outputItem.ItemId != null || outputItem.RandomItemId != null)
                            {
                                ItemQueryContext itemQueryContext = new();
                                itemQueryResults = ItemQueryResolver.TryResolve(
                                    outputItem,
                                    itemQueryContext,
                                    formatItemId: id => id?.Replace("DROP_IN_ID", "0").Replace("DROP_IN_PRESERVE", "0").Replace("NEARBY_FLOWER_ID", "0")
                                );
                            }
                            else
                            {
                                itemQueryResults = [];
                                someRulesTooComplex = true;
                            }

                            // add to list
                            recipes.AddRange(
                                from result in itemQueryResults
                                select new RecipeModel(
                                    key: null,
                                    type: RecipeType.MachineInput,
                                    displayType: ItemRegistry.GetDataOrErrorItem(qualifiedMachineId).DisplayName,
                                    ingredients,
                                    goldPrice: 0,
                                    item: _ => ItemRegistry.Create(result.Item.QualifiedItemId),
                                    isKnown: () => true,
                                    machineId: qualifiedMachineId,
                                    //exceptIngredients: recipe.ExceptIngredients.Select(id => new RecipeIngredientModel(id!.Value, 1)),
                                    exceptIngredients: null,
                                    outputQualifiedItemId: result.Item.QualifiedItemId,
                                    minOutput: outputItem.MinStack > 0 ? outputItem.MinStack : 1,
                                    maxOutput: outputItem.MaxStack > 0 ? outputItem.MaxStack : null, // TODO: Calculate this better
                                    quality: outputItem.Quality,
                                    outputChance: 100 / outputRule.OutputItem.Count / itemQueryResults.Count,
                                    conditions: conditions?.ToArray()
                                )
                            );
                        }
                    }
                }
            }

            // add placeholder 'too complex to display' recipe
            if (someRulesTooComplex)
            {
                recipes.Add(
                    new RecipeModel(
                        key: null,
                        type: RecipeType.MachineInput,
                        displayType: ItemRegistry.GetDataOrErrorItem(qualifiedMachineId).DisplayName,
                        ingredients: [],
                        goldPrice: 0,
                        item: _ => ItemRegistry.Create(DataParser.ComplexRecipeId),
                        isKnown: () => true,
                        machineId: qualifiedMachineId,
                        outputQualifiedItemId: DataParser.ComplexRecipeId
                    )
                );
            }
        }

        // building recipes from Data/Buildings
        foreach ((string buildingType, BuildingData? buildingData) in Game1.buildingData)
        {
            // construction recipe
            if (buildingData?.BuildCost > 0 || buildingData?.BuildMaterials?.Count > 0)
            {
                RecipeIngredientModel[] ingredients = RecipeModel.ParseIngredients(buildingData);

                Building building;
                try
                {
                    building = new Building(buildingType, Vector2.Zero);
                }
                catch
                {
                    continue; // ignore recipe if the building data is invalid
                }

                recipes.Add(
                    new RecipeModel(building, ingredients, buildingData.BuildCost)
                );
            }

            // processing recipes
            if (buildingData?.ItemConversions?.Count > 0)
            {
                foreach (BuildingItemConversion? rule in buildingData.ItemConversions)
                {
                    if (rule?.ProducedItems?.Count is not > 0 || rule.RequiredTags?.Count is not > 0)
                        continue;

                    List<string>? ruleConditions = null;
                    if (!this.TryGetMostSpecificIngredientIds(null, rule.RequiredTags, ref ruleConditions, out string? ingredientId, out string[] ingredientContextTags))
                        continue;

                    RecipeIngredientModel[] ingredients = [new(RecipeType.BuildingInput, ingredientId, rule.RequiredCount, ingredientContextTags)];

                    foreach (GenericSpawnItemDataWithCondition? outputItem in rule.ProducedItems)
                    {
                        if (outputItem is null)
                            continue;

                        // add produced item
                        IList<ItemQueryResult> itemQueryResults = ItemQueryResolver.TryResolve(outputItem, new ItemQueryContext());

                        // get conditions
                        string[]? conditions = !string.IsNullOrWhiteSpace(outputItem.Condition)
                            ? GameStateQuery.SplitRaw(outputItem.Condition).Distinct().ToArray()
                            : null;

                        // add to list
                        recipes.AddRange(
                            from result in itemQueryResults
                            select new RecipeModel(
                                key: null,
                                type: RecipeType.BuildingInput,
                                displayType: TokenParser.ParseText(buildingData?.Name) ?? buildingType,
                                ingredients,
                                goldPrice: 0,
                                item: _ => ItemRegistry.Create(result.Item.QualifiedItemId),
                                isKnown: () => true,
                                machineId: buildingType,
                                exceptIngredients: null,
                                outputQualifiedItemId: result.Item.QualifiedItemId,
                                minOutput: outputItem.MinStack > 0 ? outputItem.MinStack : 1,
                                maxOutput: outputItem.MaxStack > 0 ? outputItem.MaxStack : null, // TODO: Calculate this better
                                quality: outputItem.Quality,
                                outputChance: 100 / itemQueryResults.Count,
                                conditions: conditions
                            )
                        );
                    }
                }
            }
        }

        return recipes.ToArray();
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Normalize raw ingredient ID and context tags from a machine recipe into the most specific item ID and context tags possible.</summary>
    /// <param name="fromItemId">The ingredient's raw item ID from the machine data.</param>
    /// <param name="fromContextTags">The ingredient's raw context tags from the machine data.</param>
    /// <param name="fromConditions">A game state query which indicates whether an entry is applicable.</param>
    /// <param name="itemId">The item ID matching the item, or <c>null</c> if the recipe is based on <paramref name="contextTags"/>.</param>
    /// <param name="contextTags">The context tags matching the item, or an empty array if it's based on <paramref name="contextTags"/>.</param>
    /// <returns>Returns whether an item ID or any context tags were specified.</returns>
    private bool TryGetMostSpecificIngredientIds(string? fromItemId, List<string?>? fromContextTags, ref List<string>? fromConditions, out string? itemId, out string[] contextTags)
    {
        // normalize values
        contextTags = fromContextTags?.WhereNotNull().ToArray() ?? [];
        itemId = !string.IsNullOrWhiteSpace(fromItemId)
            ? fromItemId
            : null;

        // convert item ID tag to item ID
        if (contextTags.Length == 1 && MachineDataHelper.TryGetUniqueItemFromContextTag(contextTags[0], out ParsedItemData? dataFromTag))
        {
            if (itemId != null && ItemRegistry.QualifyItemId(itemId) != dataFromTag.QualifiedItemId)
                return false; // conflicting item IDs

            itemId = dataFromTag.QualifiedItemId;
            contextTags = [];
        }

        // convert item query to item ID
        if (fromConditions != null)
        {
            for (int i = 0; i < fromConditions.Count; i++)
            {
                if (MachineDataHelper.TryGetUniqueItemFromGameStateQuery(fromConditions[i], out ParsedItemData? data))
                {
                    if (itemId != null && data.QualifiedItemId != ItemRegistry.QualifyItemId(itemId))
                        return false; // conflicting item IDs

                    itemId = data.QualifiedItemId;
                    fromConditions.RemoveAt(i);
                }
            }
        }

        return itemId != null || contextTags.Length > 0;
    }
}
