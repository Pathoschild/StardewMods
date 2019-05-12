using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ContentPatcher.Framework
{
    /// <summary>Internal constant values.</summary>
    public static class InternalConstants
    {
        /*********
        ** Fields
        *********/
        /// <summary>The character uses as an input argument separator.</summary>
        public const string InputArgSeparator = ":";


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
                default:
                    PropertyInfo property = entity.GetType().GetProperty("ID");
                    if (property != null)
                        return property.GetValue(entity)?.ToString();

                    throw new NotSupportedException($"No ID implementation for list asset value type {typeof(TValue).FullName}.");
            }
        }
    }
}
