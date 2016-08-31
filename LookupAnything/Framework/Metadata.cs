using System.Collections.Generic;
using System.Linq;
using Pathoschild.LookupAnything.Framework.Data;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>Provides metadata that's not available from the game data directly (e.g. because it's buried in the logic).</summary>
    public class Metadata
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Corrects metadata for 'big craftable' objects (including furniture, crafting stations, scarecrows, etc).</summary>
        public IDictionary<int, ObjectData> BigCraftables { get; set; }

        /// <summary>Corrects metadata for most game objects except <see cref="BigCraftables"/> (including inventory items, terrain features, crops, trees, and other map objects).</summary>
        public IDictionary<int, ObjectData> Objects { get; set; }

        /// <summary>Information about Adventure Guild monster-slaying quests.</summary>
        public AdventureGuildQuestData[] AdventureGuildQuests { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get overrides for an object.</summary>
        /// <param name="item">The item for which to get overrides.</param>
        public ObjectData GetOverrides(Item item)
        {
            // big craftable
            if ((item as Object)?.bigCraftable == true)
            {
                return this.Objects.ContainsKey(item.parentSheetIndex)
                    ? this.Objects[item.parentSheetIndex]
                    : null;
            }

            // object
            return this.Objects.ContainsKey(item.parentSheetIndex)
                ? this.Objects[item.parentSheetIndex]
                : null;
        }

        /// <summary>Get the adventurer guild quest for the specified monster (if any).</summary>
        /// <param name="monster">The monster name.</param>
        public AdventureGuildQuestData GetAdventurerGuildQuest(string monster)
        {
            return this.AdventureGuildQuests.FirstOrDefault(p => p.Targets.Contains(monster));
        }
    }
}
