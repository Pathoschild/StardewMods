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
using StardewValley.TokenizableStrings;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything
{
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
                    string area = keyParts[0];
                    int id = int.Parse(keyParts[1]);

                    // parse bundle info
                    string[] valueParts = value.Split('/');
                    string name = valueParts[0];
                    string reward = valueParts[1];
                    string displayName = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en
                        ? name // field isn't present in English
                        : valueParts.Last(); // number of fields varies, but display name is always last

                    // parse ingredients
                    List<BundleIngredientModel> ingredients = [];
                    string[] ingredientData = valueParts[2].Split(' ');
                    for (int i = 0; i < ingredientData.Length; i += 3)
                    {
                        int index = i / 3;
                        string itemID = ingredientData[i];
                        int stack = int.Parse(ingredientData[i + 1]);
                        ItemQuality quality = (ItemQuality)int.Parse(ingredientData[i + 2]);
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
                        string[]? parts = entry?.Split(' ');
                        if (parts is null || parts.Length is < 1 or > 3)
                            return null;

                        // parse counts
                        int minCount = 1;
                        int maxCount = 1;
                        string id = parts[0];
                        if (parts.Length >= 2)
                            int.TryParse(parts[1], out minCount);
                        if (parts.Length >= 3)
                            int.TryParse(parts[1], out maxCount);

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
                if (drop is not null)
                    yield return new FishPondDropData(drop.RequiredPopulation, drop.ItemId, drop.MinStack, drop.MaxStack, drop.Chance);
            }
        }

        /// <summary>Read parsed data about the spawn rules for a specific fish.</summary>
        /// <param name="fish">The fish item data.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <remarks>Derived from <see cref="GameLocation.getFish"/>.</remarks>
        public FishSpawnData GetFishSpawnRules(ParsedItemData fish, Metadata metadata)
        {
            // parse location data
            var locations = new List<FishSpawnLocationData>();
            foreach ((string locationId, LocationData? data) in DataLoader.Locations(Game1.content))
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
                            var conditionData = GameStateQuery.Parse(spawn.Condition);
                            var seasonalConditions = conditionData.Where(condition => GameStateQuery.SeasonQueryKeys.Contains(condition.Query[0]));
                            foreach (GameStateQuery.ParsedGameStateQuery condition in seasonalConditions)
                            {
                                var seasons = new List<string>();
                                foreach (string season in new[] { "spring", "summer", "fall", "winter" })
                                {
                                    if (!condition.Negated && condition.Query.Any(word => word.Equals(season, StringComparison.OrdinalIgnoreCase)))
                                        seasons.Add(season);
                                }
                                curLocations.Add(new FishSpawnLocationData(locationId, spawn.FishAreaId, seasons.ToArray()));
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
                        string[] timeFields = ArgUtility.Get(fishFields, 5)?.Split(' ') ?? Array.Empty<string>();
                        for (int i = 0, last = timeFields.Length + 1; i + 1 < last; i += 2)
                        {
                            if (int.TryParse(timeFields[i], out int minTime) && int.TryParse(timeFields[i + 1], out int maxTime))
                                timesOfDay.Add(new FishSpawnTimeOfDayData(minTime, maxTime));
                        }

                        // weather
                        if (!Enum.TryParse(ArgUtility.Get(fishFields, 7), true, out weather))
                            weather = FishSpawnWeather.Both;

                        // min fishing level
                        if (!int.TryParse(ArgUtility.Get(fishFields, 12), out minFishingLevel))
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
                IsUnique: isUnique
            );
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
                int health = int.Parse(fields[0]);
                int damageToFarmer = int.Parse(fields[1]);
                //int minCoins = int.Parse(fields[2]);
                //int maxCoins = int.Parse(fields[3]) + 1;
                bool isGlider = bool.Parse(fields[4]);
                int durationOfRandomMovements = int.Parse(fields[5]);
                int resilience = int.Parse(fields[7]);
                double jitteriness = double.Parse(fields[8]);
                int moveTowardsPlayerThreshold = int.Parse(fields[9]);
                int speed = int.Parse(fields[10]);
                double missChance = double.Parse(fields[11]);
                bool isMineMonster = bool.Parse(fields[12]);

                // drops
                var drops = new List<ItemDropData>();
                string[] dropFields = fields[6].Split(' ');
                for (int i = 0; i < dropFields.Length; i += 2)
                {
                    // get drop info
                    string itemID = dropFields[i];
                    float chance = float.Parse(dropFields[i + 1]);
                    int maxDrops = 1;

                    // if itemID is negative, game randomly drops 1-3
                    if (int.TryParse(itemID, out int id) && id < 0)
                    {
                        itemID = (-id).ToString();
                        maxDrops = 3;
                    }

                    // some item IDs have special meaning
                    if (itemID == Debris.copperDebris.ToString())
                        itemID = SObject.copper.ToString();
                    else if (itemID == Debris.ironDebris.ToString())
                        itemID = SObject.iron.ToString();
                    else if (itemID == Debris.coalDebris.ToString())
                        itemID = SObject.coal.ToString();
                    else if (itemID == Debris.goldDebris.ToString())
                        itemID = SObject.gold.ToString();
                    else if (itemID == Debris.coinsDebris.ToString())
                        continue; // no drop
                    else if (itemID == Debris.iridiumDebris.ToString())
                        itemID = SObject.iridium.ToString();
                    else if (itemID == Debris.woodDebris.ToString())
                        itemID = SObject.wood.ToString();
                    else if (itemID == Debris.stoneDebris.ToString())
                        itemID = SObject.stone.ToString();

                    // add drop
                    drops.Add(new ItemDropData(itemID, 1, maxDrops, chance));
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
                    DurationOfRandomMovements: durationOfRandomMovements,
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
                        foreach (MachineItemOutput? outputItem in outputRule.OutputItem)
                        {
                            if (outputItem is null)
                                continue;

                            // get conditions
                            List<string>? conditions = null;
                            {
                                // extract raw conditions
                                string? rawConditions = null;
                                if (!string.IsNullOrWhiteSpace(trigger.Condition))
                                    rawConditions = trigger.Condition;
                                if (!string.IsNullOrWhiteSpace(outputItem.Condition))
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
                                new RecipeIngredientModel(RecipeType.MachineInput, inputId, trigger.RequiredCount, inputContextTags)
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
                            ItemQueryContext itemQueryContext = new();
                            IList<ItemQueryResult> itemQueryResults = ItemQueryResolver.TryResolve(
                                outputItem,
                                itemQueryContext,
                                formatItemId: id => id?.Replace("DROP_IN_ID", "0").Replace("DROP_IN_PRESERVE", "0").Replace("NEARBY_FLOWER_ID", "0")
                            );

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

                        RecipeIngredientModel[] ingredients = [new RecipeIngredientModel(RecipeType.BuildingInput, ingredientId, rule.RequiredCount, ingredientContextTags)];

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
        /// <summary>Get the translated display name for a location and optional fish area.</summary>
        /// <param name="id">The location's internal name.</param>
        /// <param name="data">The location data, if available.</param>
        /// <param name="fishAreaId">The fish area ID within the location, if applicable.</param>
        private string GetLocationDisplayName(string id, LocationData? data, string? fishAreaId)
        {
            // special cases
            {
                // skip: no area set
                if (string.IsNullOrWhiteSpace(fishAreaId))
                    return this.GetLocationDisplayName(id, data);

                // special case: mine level
                if (string.Equals(id, "UndergroundMine", StringComparison.OrdinalIgnoreCase))
                    return I18n.Location_UndergroundMine_Level(level: fishAreaId);
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

        /// <summary>Get the translated display name for a location.</summary>
        /// <param name="id">The location's internal name.</param>
        /// <param name="data">The location data, if available.</param>
        private string GetLocationDisplayName(string id, LocationData? data)
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
}
