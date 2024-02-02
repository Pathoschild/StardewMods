using System.Diagnostics.CodeAnalysis;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.Common
{
    /// <summary>Provides access to the game's translations.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named to match convention.")]
    internal static class GameI18n
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the translated name for a big craftable object.</summary>
        /// <param name="id">The big craftable's unqualified ID.</param>
        public static string GetBigCraftableName(string id)
        {
            if (Game1.bigCraftableData == null)
                return "(missing translation: game hasn't loaded bigcraftable data yet)";

            try
            {
                var data = ItemRegistry.GetData(ItemRegistry.ManuallyQualifyItemId(id, ItemRegistry.type_bigCraftable));
                return data?.DisplayName ?? $"(missing translation: no bigcraftable with ID '{id}')";
            }
            catch
            {
                return $"(missing translation: bigcraftable object with ID '{id}' has an invalid format)";
            }
        }

        /// <summary>Get the translated name for a building.</summary>
        /// <param name="id">The object's unqualified ID.</param>
        public static string GetBuildingName(string id)
        {
            if (Game1.buildingData == null)
                return "(missing translation: game hasn't loaded object data yet)";

            try
            {
                if (Game1.buildingData.TryGetValue(id, out BuildingData? data))
                    return TokenParser.ParseText(data?.Name) ?? id;

                return $"(missing translation: no building with ID '{id}')";
            }
            catch
            {
                return $"(missing translation: building with ID '{id}' has an invalid format)";
            }
        }

        /// <summary>Get the translated name for an object.</summary>
        /// <param name="id">The object's unqualified ID.</param>
        public static string GetObjectName(string id)
        {
            if (Game1.objectData == null)
                return "(missing translation: game hasn't loaded object data yet)";

            try
            {
                var data = ItemRegistry.GetData(ItemRegistry.ManuallyQualifyItemId(id, ItemRegistry.type_object));
                return data?.DisplayName ?? $"(missing translation: no object with ID '{id}')";
            }
            catch
            {
                return $"(missing translation: object with ID '{id}' has an invalid format)";
            }
        }

        /// <summary>Get a translation by key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="substitutions">The values for placeholders like <c>{0}</c> in the translation text.</param>
        public static string GetString(string key, params object[] substitutions)
        {
            return Game1.content.LoadString(key, substitutions);
        }
    }
}
