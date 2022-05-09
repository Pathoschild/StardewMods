using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ContentPatcher.Framework.Patches.EditData
{
    /// <summary>Encapsulates applying key/value edits to a dictionary.</summary>
    /// <typeparam name="TKey">The dictionary key type.</typeparam>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    internal class DictionaryKeyValueEditor<TKey, TValue> : BaseDataEditor
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying data.</summary>
        private readonly IDictionary<TKey, TValue> Data;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="data">The underlying data.</param>
        public DictionaryKeyValueEditor(IDictionary<TKey, TValue> data)
        {
            this.Data = data;
        }

        /// <inheritdoc />
        public override object ParseKey(string key)
        {
            return this.ParseKey<TKey>(key);
        }

        /// <inheritdoc />
        public override bool HasEntry(object key)
        {
            TKey parsedKey = (TKey)key;

            return this.Data.ContainsKey(parsedKey);
        }

        /// <inheritdoc />
        public override object? GetEntry(object key)
        {
            TKey parsedKey = (TKey)key;

            return this.Data.TryGetValue(parsedKey, out TValue? value)
                ? value
                : default;
        }

        /// <inheritdoc />
        public override Type GetEntryType(object key)
        {
            return typeof(TValue);
        }

        /// <inheritdoc />
        public override void RemoveEntry(object key)
        {
            TKey parsedKey = (TKey)key;

            this.Data.Remove(parsedKey);
        }

        /// <inheritdoc />
        public override void SetEntry(object key, JToken value)
        {
            TKey parsedKey = (TKey)key;

            this.Data[parsedKey] = value.ToObject<TValue>()!;
        }
    }
}
