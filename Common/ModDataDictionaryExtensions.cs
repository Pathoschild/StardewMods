using System;
using StardewValley;

namespace Pathoschild.Stardew.Common
{
    /// <summary>Provides extension methods for reading and writing values in <see cref="ModDataDictionary"/> fields.</summary>
    internal static class ModDataDictionaryExtensions
    {
        /*********
        ** Public fields
        *********/
        /// <summary>Read a field from the mod data dictionary.</summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="data">The mod data dictionary to read.</param>
        /// <param name="key">The dictionary key to read.</param>
        /// <param name="parse">Convert the raw string value into the expected type.</param>
        /// <param name="defaultValue">The default value to return if the data field isn't set.</param>
        public static T ReadField<T>(this ModDataDictionary data, string key, Func<string, T> parse, T defaultValue = default)
        {
            return data.TryGetValue(key, out string rawValue)
                ? parse(rawValue)
                : defaultValue;
        }

        /// <summary>Read a field from the mod data dictionary.</summary>
        /// <param name="data">The mod data dictionary to read.</param>
        /// <param name="key">The dictionary key to read.</param>
        /// <param name="defaultValue">The default value to return if the data field isn't set.</param>
        public static string ReadField(this ModDataDictionary data, string key, string defaultValue = null)
        {
            return data.TryGetValue(key, out string rawValue)
                ? rawValue
                : defaultValue;
        }

        /// <summary>Write a field to a mod data dictionary, or remove it if null.</summary>
        /// <param name="data">The mod data dictionary to update.</param>
        /// <param name="key">The dictionary key to write.</param>
        /// <param name="value">The value to write, or <c>null</c> to remove it.</param>
        public static ModDataDictionary WriteField(this ModDataDictionary data, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                data.Remove(key);
            else
                data[key] = value;

            return data;
        }
    }
}
