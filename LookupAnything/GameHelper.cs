using System.Linq;
using System.Reflection;
using StardewValley;
using InvalidOperationException = System.InvalidOperationException;

namespace Pathoschild.LookupAnything
{
    /// <summary>Provides utility methods for interacting with the game code.</summary>
    internal class GameHelper
    {
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
            for (System.Type type = parent.GetType(); type != null && field == null; type = type.BaseType)
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
    }
}