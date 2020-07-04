using System;
using System.Reflection;
using StardewValley.GameData.Crafting;
using StardewValley.GameData.FishPond;
using StardewValley.GameData.Movies;

namespace ContentPatcher.Framework
{
    /// <summary>Internal constant values.</summary>
    public static class InternalConstants
    {
        /*********
        ** Fields
        *********/
        /// <summary>The character used as a separator between the token name and positional input arguments.</summary>
        public const string PositionalInputArgSeparator = ":";

        /// <summary>The character used as a separator between the token name (or positional input arguments) and named input arguments.</summary>
        public const string NamedInputArgSeparator = "|";

        /// <summary>The character used as a separator between the mod ID and token name for a mod-provided token.</summary>
        public const string ModTokenSeparator = "/";

        /// <summary>A prefix for player names when specified as an input argument.</summary>
        public const string PlayerNamePrefix = "@";


        /*********
        ** Methods
        *********/
        /// <summary>Get the key for a list asset entry.</summary>
        /// <typeparam name="TValue">The list value type.</typeparam>
        /// <param name="entity">The entity whose ID to fetch.</param>
        public static string GetListAssetKey<TValue>(TValue entity)
        {
            switch (entity)
            {
                case ConcessionTaste entry:
                    return entry.Name;

                case FishPondData entry:
                    return string.Join(",", entry.RequiredTags);

                case MovieCharacterReaction entry:
                    return entry.NPCName;

                case TailorItemRecipe entry:
                    return string.Join(",", entry.FirstItemTags) + "|" + string.Join(",", entry.SecondItemTags);

                default:
                    PropertyInfo property = entity.GetType().GetProperty("ID");
                    if (property != null)
                        return property.GetValue(entity)?.ToString();

                    throw new NotSupportedException($"No ID implementation for list asset value type {typeof(TValue).FullName}.");
            }
        }
    }
}
