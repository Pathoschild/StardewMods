using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.LookupAnything.Framework;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Models;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything
{
    /// <summary>Parses the raw game data into usable models.</summary>
    internal class DataParser
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Parse gift tastes.</summary>
        /// <remarks>Reverse engineered from <c>Data\NPCGiftTastes</c> and <see cref="StardewValley.NPC.getGiftTasteForThisItem"/>.</remarks>
        public static IEnumerable<GiftTasteModel> GetGiftTastes()
        {
            IDictionary<string, string> data = Game1.NPCGiftTastes;

            // define schema
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

            // parse
            foreach (string villager in data.Keys)
            {
                string tasteStr = data[villager];

                if (universal.ContainsKey(villager))
                {
                    GiftTaste taste = universal[villager];
                    foreach (string value in tasteStr.Split(' '))
                        yield return new GiftTasteModel(taste, "*", int.Parse(value), isUniversal: true);
                }
                else
                {
                    string[] personalData = tasteStr.Split('/');
                    foreach (KeyValuePair<int, GiftTaste> taste in personalMetadataKeys)
                    {
                        foreach (string value in personalData[taste.Key].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                            yield return new GiftTasteModel(taste.Value, villager, int.Parse(value));
                    }
                }
            }
        }

        /// <summary>Parse monster data.</summary>
        /// <remarks>Reverse engineered from <see cref="StardewValley.Monsters.Monster.parseMonsterInfo"/> and <see cref="GameLocation.monsterDrop"/>.</remarks>
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
                    int itemID = int.Parse(dropFields[i]);
                    float chance = float.Parse(dropFields[i + 1]);
                    int maxDrops = 1;
                    if (itemID < 0)
                    {
                        itemID = -itemID;
                        maxDrops = 3; // if item ID is negative, game randomly drops 1-3
                    }

                    drops.Add(new ItemDropData(itemID, maxDrops, chance));
                }
                if (isMineMonster && Game1.player.timesReachedMineBottom >= 1)
                {
                    drops.Add(new ItemDropData(72, 1, 0.008f));
                    drops.Add(new ItemDropData(74, 1, 0.008f));
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
