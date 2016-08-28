using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Pathoschild.LookupAnything.Framework.Constants;
using StardewValley;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything
{
    /// <summary>Provides utility methods for interacting with the game code.</summary>
    internal static class GameHelper
    {
        /*********
        ** Properties
        *********/
        /// <summary>The cached villagers' gift tastes, indexed by taste and then villager name. Each item reference is a category (negative value) or parent sprite index (positive value).</summary>
        private static Lazy<IDictionary<GiftTaste, IDictionary<string, int[]>>> GiftTastes;

        /// <summary>The cached list of characters who can receive gifts.</summary>
        private static Lazy<NPC[]> GiftableVillagers;


        /*********
        ** Public methods
        *********/
        /// <summary>Reset the low-level cache used to store expensive query results, so the data is recalculated on demand.</summary>
        public static void ResetCache()
        {
            GameHelper.GiftTastes = new Lazy<IDictionary<GiftTaste, IDictionary<string, int[]>>>(GameHelper.FetchGiftTastes);
            GameHelper.GiftableVillagers = new Lazy<NPC[]>(GameHelper.FetchGiftableVillagers);
        }

        /// <summary>Get how much each NPC likes receiving an item as a gift.</summary>
        /// <param name="item">The item to check.</param>
        public static IDictionary<NPC, GiftTaste> GetGiftTastes(Item item)
        {
            // can't be gifted
            if (!item.canBeGivenAsGift())
                return new Dictionary<NPC, GiftTaste>();

            // fetch game data
            var giftTastes = GameHelper.GiftTastes.Value;
            var giftableVillagers = GameHelper.GiftableVillagers.Value;

            // get tastes
            IDictionary<NPC, GiftTaste> tastes = new Dictionary<NPC, GiftTaste>();
            foreach (NPC npc in giftableVillagers)
            {
                // get taste
                foreach (GiftTaste taste in Enum.GetValues(typeof(GiftTaste)))
                {
                    if (giftTastes[taste][npc.getName()].Contains(item.category) || giftTastes[taste][npc.getName()].Contains(item.parentSheetIndex))
                    {
                        tastes[npc] = taste;
                        break;
                    }
                }

                // default to neutral
                if (!tastes.ContainsKey(npc))
                    tastes[npc] = GiftTaste.Neutral;
            }
            return tastes;
        }

        /// <summary>Get the items a specified NPC can receive.</summary>
        /// <param name="npc">The NPC to check.</param>
        public static IDictionary<Item, GiftTaste> GetGiftTastes(NPC npc)
        {
            // get game data
            var giftTastes = GameHelper.GiftTastes.Value;
            var giftableVillagers = GameHelper.GiftableVillagers.Value;
            if (!giftableVillagers.Contains(npc))
                return new Dictionary<Item, GiftTaste>();

            // get tastes
            IDictionary<Item, GiftTaste> tastes = new Dictionary<Item, GiftTaste>();
            foreach (GiftTaste taste in Enum.GetValues(typeof(GiftTaste)))
            {
                foreach (Object item in giftTastes[taste][npc.getName()].SelectMany(GameHelper.GetObjectsByReferenceID))
                    tastes[item] = taste;
            }
            return tastes;
        }

        /// <summary>Get all objects matching the reference ID.</summary>
        /// <param name="refID">The reference ID. This can be a category (negative value) or parent sprite index (positive value)</param>
        public static IEnumerable<Object> GetObjectsByReferenceID(int refID)
        {
            // category
            if (refID < 0)
            {
                return (
                    from pair in Game1.objectInformation
                    where Regex.IsMatch(pair.Value, $"\b{refID}\b")
                    select new Object(pair.Key, 1)
                );
            }

            // parent sprite index
            return new[] { new Object(refID, 1) };
        }

        /// <summary>Get whether an item can have a quality (which increases its sale price).</summary>
        /// <param name="item">The item.</param>
        public static bool CanHaveQuality(Item item)
        {
            // check category
            if (new[] { "Artifact", "Trash", "Crafting", "Seed", "Decor", "Resource", "Fertilizer", "Bait", "Fishing Tackle" }.Contains(item.getCategoryName()))
                return false;

            // check type
            if (new[] { "Crafting", "asdf" /*dig spots*/, "Quest" }.Contains((item as Object)?.Type))
                return false;

            return true;
        }

        /// <summary>Get a private field value.</summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="parent">The parent object.</param>
        /// <param name="name">The field name.</param>
        public static T GetPrivateField<T>(object parent, string name)
        {
            if (parent == null)
                return default(T);

            // get field from hierarchy
            FieldInfo field = null;
            for (Type type = parent.GetType(); type != null && field == null; type = type.BaseType)
                field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);

            // validate
            if (field == null)
                throw new InvalidOperationException($"The {parent.GetType().Name} object doesn't have a private '{name}' field.");

            // get value
            return (T)field.GetValue(parent);
        }

        /// <summary>Select the correct plural form for a word.</summary>
        /// <param name="count">The number.</param>
        /// <param name="single">The singular form.</param>
        /// <param name="plural">The plural form.</param>
        public static string Pluralise(int count, string single, string plural)
        {
            return count == 1 ? single : plural;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a list of characters who can receive gifts.</summary>
        private static NPC[] FetchGiftableVillagers()
        {
            // NPCs are giftable if they have at least one preference
            var uniqueKeys = new HashSet<string>(
                GameHelper.GiftTastes.Value
                    .SelectMany(p => p.Value)
                    .Select(p => p.Key)
            );

            // get characters matching keys
            return Utility.getAllCharacters()
                .Where(npc => npc.isVillager() && uniqueKeys.Contains(npc.getName()))
                .ToArray();
        }

        /// <summary>Get the villagers' gift tastes.</summary>
        /// <remarks>Reverse engineered from <c>Data\NPCGiftTastes</c> and <see cref="NPC.getGiftTasteForThisItem"/>.</remarks>
        private static IDictionary<GiftTaste, IDictionary<string, int[]>> FetchGiftTastes()
        {
            // parse game data
            var universalTastes = new Dictionary<GiftTaste, int[]>();
            var personalTastes = new Dictionary<string, Dictionary<GiftTaste, int[]>>();
            {
                // define keys
                var universalKeys = new Dictionary<string, GiftTaste>
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

                // read data
                foreach (string key in Game1.NPCGiftTastes.Keys)
                {
                    // universal tastes
                    if (universalKeys.ContainsKey(key))
                    {
                        GiftTaste taste = universalKeys[key];
                        universalTastes[taste] = Game1.NPCGiftTastes[key]
                            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse)
                            .ToArray();
                    }

                    // personal tastes
                    else
                    {
                        personalTastes[key] = new Dictionary<GiftTaste, int[]>();
                        string[] metadata = Game1.NPCGiftTastes[key].Split('/');
                        foreach (int i in personalMetadataKeys.Keys)
                        {
                            GiftTaste taste = personalMetadataKeys[i];
                            personalTastes[key][taste] = metadata[i]
                                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(int.Parse)
                                .ToArray();
                        }
                    }
                }
            }

            // merge data structures
            var giftTastes = new Dictionary<GiftTaste, IDictionary<string, int[]>>
            {
                [GiftTaste.Love] = new Dictionary<string, int[]>(),
                [GiftTaste.Like] = new Dictionary<string, int[]>(),
                [GiftTaste.Neutral] = new Dictionary<string, int[]>(),
                [GiftTaste.Dislike] = new Dictionary<string, int[]>(),
                [GiftTaste.Hate] = new Dictionary<string, int[]>()
            };
            foreach (string villagerName in personalTastes.Keys)
            {
                foreach (GiftTaste taste in personalTastes[villagerName].Keys)
                    giftTastes[taste][villagerName] = personalTastes[villagerName][taste];
            }
            foreach (GiftTaste taste in universalTastes.Keys)
            {
                foreach (string villagerName in giftTastes[taste].Keys.ToArray())
                    giftTastes[taste][villagerName] = giftTastes[taste][villagerName].Concat(universalTastes[taste]).ToArray();
            }

            return giftTastes;
        }
    }
}