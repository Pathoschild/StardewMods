using System.Collections.Generic;
using Netcode;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Provides utility extension methods.</summary>
    internal static class InternalExtensions
    {
        /*********
        ** Public methods
        *********/
        /****
        ** Items
        ****/
        /// <summary>Get the sprite sheet to which the item's <see cref="Item.parentSheetIndex"/> refers.</summary>
        /// <param name="item">The item to check.</param>
        public static ItemSpriteType GetSpriteType(this Item item)
        {
            if (item is Object obj)
            {
                if (obj is Furniture)
                    return ItemSpriteType.Furniture;
                if (obj is Wallpaper)
                    return ItemSpriteType.Wallpaper;
                return obj.bigCraftable.Value
                    ? ItemSpriteType.BigCraftable
                    : ItemSpriteType.Object;
            }
            if (item is Boots)
                return ItemSpriteType.Boots;
            if (item is Hat)
                return ItemSpriteType.Hat;
            if (item is Tool)
                return ItemSpriteType.Tool;

            return ItemSpriteType.Unknown;
        }

        /// <summary>Get the value stored for the key. If the value doesn't exist, return that type's default value.</summary>
        /// <typeparam name="TKey">The type of the dictionary keys</typeparam>
        /// <typeparam name="TValue">The type of the dictionary values</typeparam>
        /// <param name="dictionary">This dictionary</param>
        /// <param name="key">The key to look up</param>
        /// <returns>Either the value store for the key or a default</returns>
        public static TValue GetValueOrDefault<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key )
        {
            return dictionary.TryGetValue( key, out var ret ) ? ret : default;
        }

        /// <summary>Get the value stored for the key. If the value doesn't exist, return that type's default value.</summary>
        /// <typeparam name="TKey">The type of the dictionary keys</typeparam>
        /// <typeparam name="TValue">The type of the dictionary values</typeparam>
        /// <param name="dictionary">This dictionary</param>
        /// <param name="key">The key to look up</param>
        /// <returns>Either the value store for the key or a default</returns>
        public static TValue GetValueOrDefault<TKey, TValue, TField, TSerialDict, TSelf> ( this NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> dictionary, TKey key )
            where TField : class, INetObject<INetSerializable>, new()
            where TSerialDict : IDictionary<TKey, TValue>, new()
            where TSelf : NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>
        {
            return dictionary.TryGetValue( key, out var ret ) ? ret : default;
        }
    }
}
