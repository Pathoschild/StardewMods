namespace ContentPatcher.Framework.TriggerActions
{
    /// <summary>A data ID type which can be migrated using <see cref="MigrateIdsAction"/>.</summary>
    public enum MigrateIdType
    {
        /// <summary>Migrate cooking recipe IDs.</summary>
        CookingRecipes,

        /// <summary>Migrate crafting recipe IDs.</summary>
        CraftingRecipes,

        /// <summary>Migrate event IDs.</summary>
        Events,

        /// <summary>Migrate item local IDs.</summary>
        Items,

        /// <summary>Migrate mail IDs.</summary>
        Mail,

        /// <summary>Migrate songs-heard cue names.</summary>
        Songs
    }
}
