namespace Pathoschild.LookupAnything.Framework.Data
{
    /// <summary>Provides override metadata about a game NPC.</summary>
    internal class CharacterData
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Identify object
        ****/
        /// <summary>The NPC identifier, like "Horse" (any NPCs of type Horse) or "Villager::Gunther" (any NPCs of type Villager with the name "Gunther").</summary>
        public string ID { get; set; }


        /****
        ** Overrides
        ****/
        /// <summary>The overridden NPC name (if any).</summary>
        public string Name { get; set; }

        /// <summary>The overridden NPC description (if any).</summary>
        public string Description { get; set; }
    }
}