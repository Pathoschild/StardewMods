using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.LookupAnything.Framework;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Data;
using Pathoschild.LookupAnything.Framework.Models;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything
{
    /// <summary>Parses the raw game data into usable models. These may be expensive operations and should be cached.</summary>
    internal class DataParser
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get parsed data about the friendship between a player and NPC.</summary>
        /// <param name="player">The player.</param>
        /// <param name="npc">The NPC.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public static FriendshipModel GetFriendshipForVillager(Farmer player, NPC npc, Metadata metadata)
        {
            return new FriendshipModel(player, npc, metadata.Constants);
        }

        /// <summary>Get parsed data about the friendship between a player and NPC.</summary>
        /// <param name="player">The player.</param>
        /// <param name="pet">The pet.</param>
        public static FriendshipModel GetFriendshipForPet(Farmer player, Pet pet)
        {
            return new FriendshipModel(pet.friendshipTowardFarmer, Pet.maxFriendship / 10, Pet.maxFriendship);
        }

        /// <summary>Get parsed data about the friendship between a player and NPC.</summary>
        /// <param name="player">The player.</param>
        /// <param name="animal">The farm animal.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public static FriendshipModel GetFriendshipForAnimal(Farmer player, FarmAnimal animal, Metadata metadata)
        {
            return new FriendshipModel(animal.friendshipTowardFarmer, metadata.Constants.AnimalFriendshipPointsPerLevel, metadata.Constants.AnimalFriendshipMaxPoints);
        }

        /// <summary>Parse gift tastes.</summary>
        /// <param name="objects">The game's object data.</param>
        /// <remarks>
        /// Reverse engineered from <c>Data\NPCGiftTastes</c> and <see cref="StardewValley.NPC.getGiftTasteForThisItem"/>.
        /// The game decides a villager's gift taste using a complicated algorithm which boils down to the first match out of:
        ///   1. A villager's personal taste by item ID.
        ///   2. A universal taste by item ID.
        ///   3. A villager's personal taste by category.
        ///   4. A universal taste by category (if not neutral).
        ///   5. If the item's edibility is less than 0 (but not -300), hate.
        ///   6. If the item's price is less than 20, dislike.
        ///   7. If the item is an artifact...
        ///      7a. and the NPC is Penny, like.
        ///      7b. else neutral.
        /// 
        /// For each rule, their tastes are checked in this order: love, hate, like, dislike, or
        /// neutral. (That is, if an NPC both loves and hates an item, love wins.)
        /// </remarks>
        public static IEnumerable<GiftTasteModel> GetGiftTastes(ObjectModel[] objects)
        {
            // extract raw values
            string[] giftableVillagers;
            var tastes = new List<RawGiftTasteModel>();
            {
                // define data schema
                var universal = new Dictionary<string, GiftTaste>
                {
                    ["Universal_Love"] = GiftTaste.Love,
                    ["Universal_Like"] = GiftTaste.Like,
                    ["Universal_Neutral"] = GiftTaste.Neutral,
                    ["Universal_Dislike"] = GiftTaste.Dislike,
                    ["Universal_Hate"] = GiftTaste.Hate
                };
                var personalMetadataKeys = new Dictionary<int, GiftTaste>
                {
                    // metadata is paired: odd values contain a list of item references, even values contain the reaction dialogue
                    [1] = GiftTaste.Love,
                    [3] = GiftTaste.Like,
                    [5] = GiftTaste.Dislike,
                    [7] = GiftTaste.Hate,
                    [9] = GiftTaste.Neutral
                };

                // get data
                IDictionary<string, string> data = Game1.NPCGiftTastes;
                giftableVillagers = data.Keys.Except(universal.Keys).ToArray();

                // extract raw tastes
                foreach (string villager in data.Keys)
                {
                    string tasteStr = data[villager];

                    if (universal.ContainsKey(villager))
                    {
                        GiftTaste taste = universal[villager];
                        tastes.AddRange(
                            from refID in tasteStr.Split(' ')
                            select new RawGiftTasteModel(taste, "*", int.Parse(refID), isUniversal: true)
                        );
                    }
                    else
                    {
                        string[] personalData = tasteStr.Split('/');
                        foreach (KeyValuePair<int, GiftTaste> taste in personalMetadataKeys)
                        {
                            tastes.AddRange(
                                from refID in
                                    personalData[taste.Key].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                select new RawGiftTasteModel(taste.Value, villager, int.Parse(refID))
                            );
                        }
                    }
                }
            }

            // order by precedence (lower is better)
            tastes = tastes
                .OrderBy(entry =>
                {
                    bool isPersonal = !entry.IsUniversal;
                    bool isSpecific = !entry.IsCategory;

                    // precedence between preferences
                    int precedence;
                    switch (entry.Taste)
                    {
                        case GiftTaste.Love:
                            precedence = 1;
                            break;
                        case GiftTaste.Hate:
                            precedence = 2;
                            break;
                        case GiftTaste.Like:
                            precedence = 3;
                            break;
                        case GiftTaste.Dislike:
                            precedence = 4;
                            break;
                        default:
                            precedence = 5;
                            break;
                    }

                    // personal taste by item ID
                    if (isPersonal && isSpecific)
                        return 10 + precedence;

                    // else universal taste by item ID
                    if (entry.IsUniversal && isSpecific)
                        return 20 + precedence;

                    // else personal taste by category
                    if (isPersonal)
                        return 30 + precedence;

                    // else universal taste by category (if not neutral)
                    if (entry.IsUniversal && entry.Taste != GiftTaste.Neutral)
                        return 40 + precedence;

                    // else
                    return 50 + precedence;
                })
                .ToList();

            // get effective tastes
            {
                // get item lookups
                IDictionary<int, ObjectModel> objectsByID = objects.ToDictionary(p => p.ParentSpriteIndex);
                IDictionary<int, int[]> objectsByCategory =
                    (
                        from entry in objects
                        where entry.Category < 0
                        group entry by entry.Category into items
                        select new { Category = items.Key, Items = items.Select(item => item.ParentSpriteIndex).ToArray() }
                    )
                    .ToDictionary(p => p.Category, p => p.Items);

                // get tastes by precedence
                IDictionary<string, HashSet<int>> seenItemIDs = giftableVillagers.ToDictionary(name => name, name => new HashSet<int>());
                foreach (RawGiftTasteModel entry in tastes)
                {
                    // ignore nonexistent items
                    if (entry.IsCategory && !objectsByCategory.ContainsKey(entry.RefID))
                        continue;
                    if (!entry.IsCategory && !objectsByID.ContainsKey(entry.RefID))
                        continue;

                    // get item IDs
                    int[] itemIDs = entry.IsCategory
                        ? objectsByCategory[entry.RefID]
                        : new[] { entry.RefID };

                    // get affected villagers
                    string[] villagers = entry.IsUniversal
                        ? giftableVillagers
                        : new[] { entry.Villager };

                    // yield if no conflict
                    foreach (string villager in villagers)
                    {
                        foreach (int itemID in itemIDs)
                        {
                            // ignore if conflicts with a preceding taste
                            if (seenItemIDs[villager].Contains(itemID))
                                continue;
                            seenItemIDs[villager].Add(itemID);

                            // yield taste
                            yield return new GiftTasteModel(entry.Taste, villager, itemID);
                        }
                    }
                }
            }
        }

        /// <summary>Parse monster data.</summary>
        /// <remarks>Reverse engineered from <see cref="StardewValley.Monsters.Monster.parseMonsterInfo"/>, <see cref="GameLocation.monsterDrop"/>, and the <see cref="Debris"/> constructor.</remarks>
        public static IEnumerable<MonsterData> GetMonsters()
        {
            Dictionary<string, string> data = Game1.content.Load<Dictionary<string, string>>("Data\\Monsters");

            foreach (var entry in data)
            {
                // monster fields
                string[] fields = entry.Value.Split('/');
                string name = entry.Key;
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
                    int itemID = int.Parse(dropFields[i]);
                    float chance = float.Parse(dropFields[i + 1]);
                    int maxDrops = 1;

                    // if itemID is negative, game randomly drops 1-3
                    if (itemID < 0)
                    {
                        itemID = -itemID;
                        maxDrops = 3;
                    }

                    // some item IDs have special meaning
                    if (itemID == Debris.copperDebris)
                        itemID = Object.copper;
                    else if (itemID == Debris.ironDebris)
                        itemID = Object.iron;
                    else if (itemID == Debris.coalDebris)
                        itemID = Object.coal;
                    else if (itemID == Debris.goldDebris)
                        itemID = Object.gold;
                    else if (itemID == Debris.coinsDebris)
                        continue; // no drop
                    else if (itemID == Debris.iridiumDebris)
                        itemID = Object.iridium;
                    else if (itemID == Debris.woodDebris)
                        itemID = Object.wood;
                    else if (itemID == Debris.stoneDebris)
                        itemID = Object.stone;

                    // add drop
                    drops.Add(new ItemDropData(itemID, maxDrops, chance));
                }
                if (isMineMonster && Game1.player.timesReachedMineBottom >= 1)
                {
                    drops.Add(new ItemDropData(Object.diamondIndex, 1, 0.008f));
                    drops.Add(new ItemDropData(Object.prismaticShardIndex, 1, 0.008f));
                }

                // yield data
                yield return new MonsterData(
                    name: name,
                    health: health,
                    damageToFarmer: damageToFarmer,
                    isGlider: isGlider,
                    durationOfRandomMovements: durationOfRandomMovements,
                    resilience: resilience,
                    jitteriness: jitteriness,
                    moveTowardsPlayerThreshold: moveTowardsPlayerThreshold,
                    speed: speed,
                    missChance: missChance,
                    isMineMonster: isMineMonster,
                    drops: drops
                );
            }
        }

        /// <summary>Parse gift tastes.</summary>
        /// <remarks>Derived from the <see cref="StardewValley.CraftingRecipe.createItem"/>.</remarks>
        public static IEnumerable<ObjectModel> GetObjects()
        {
            Dictionary<int, string> data = Game1.objectInformation;

            foreach (var pair in data)
            {
                int parentSpriteIndex = pair.Key;
                string[] fields = pair.Value.Split('/');

                // ring
                if (parentSpriteIndex >= Ring.ringLowerIndexRange && parentSpriteIndex <= Ring.ringUpperIndexRange)
                {
                    yield return new ObjectModel(
                        parentSpriteIndex: parentSpriteIndex,
                        name: fields[0],
                        description: fields[1],
                        price: int.Parse(fields[2]),
                        edibility: -300,
                        type: fields[3],
                        category: Object.ringCategory
                    );
                }

                // any other object
                else
                {
                    string name = fields[Object.objectInfoNameIndex];
                    int price = int.Parse(fields[Object.objectInfoPriceIndex]);
                    int edibility = int.Parse(fields[Object.objectInfoEdibilityIndex]);
                    string description = fields[Object.objectInfoDescriptionIndex];

                    // type & category
                    string[] typeParts = fields[Object.objectTypeIndex].Split(' ');
                    string typeName = typeParts[0];
                    int category = 0;
                    if (typeParts.Length > 1)
                        category = int.Parse(typeParts[1]);

                    yield return new ObjectModel(parentSpriteIndex, name, description, price, edibility, typeName, category);
                }
            }
        }

        /// <summary>Get the recipe ingredients.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public static RecipeModel[] GetRecipes(Metadata metadata)
        {
            List<RecipeModel> recipes = new List<RecipeModel>();

            // cooking recipes
            recipes.AddRange(
                from entry in CraftingRecipe.cookingRecipes
                let recipe = new CraftingRecipe(entry.Key, isCookingRecipe: true)
                select new RecipeModel(recipe)
            );

            // crafting recipes
            recipes.AddRange(
                from entry in CraftingRecipe.craftingRecipes
                let recipe = new CraftingRecipe(entry.Key, isCookingRecipe: false)
                select new RecipeModel(recipe)
            );

            // recipes not available from game data
            recipes.AddRange(
                from entry in metadata.Recipes
                select new RecipeModel(entry.Name, entry.Type, entry.Ingredients, () => GameHelper.GetObjectBySpriteIndex(entry.Output), false, entry.ExceptIngredients)
            );

            return recipes.OrderBy(p => p.Name).ToArray();
        }
    }
}
