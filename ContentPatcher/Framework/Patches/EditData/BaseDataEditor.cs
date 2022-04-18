#nullable disable

using System;
using Newtonsoft.Json.Linq;

namespace ContentPatcher.Framework.Patches.EditData
{
    /// <summary>Provides utility methods for implementing <see cref="IKeyValueEditor"/>.</summary>
    internal abstract class BaseDataEditor : IKeyValueEditor
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public bool CanMoveEntries { get; protected set; }

        /// <inheritdoc />
        public bool CanAddEntries { get; protected set; } = true;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public abstract object ParseKey(string key);

        /// <inheritdoc />
        public virtual bool HasEntry(object key)
        {
            return this.GetEntry(key) != null;
        }

        /// <inheritdoc />
        public abstract object GetEntry(object key);

        /// <inheritdoc />
        public abstract Type GetEntryType(object key);

        /// <inheritdoc />
        public abstract void RemoveEntry(object key);

        /// <inheritdoc />
        public abstract void SetEntry(object key, JToken value);

        /// <inheritdoc />
        public virtual MoveResult MoveEntry(object key, MoveEntryPosition toPosition)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual MoveResult MoveEntry(object key, object anchorKey, bool afterAnchor)
        {
            throw new NotImplementedException();
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Parse a raw key into the data type expected by the data editor.</summary>
        /// <typeparam name="TKey">The expected key type.</typeparam>
        /// <param name="key">The raw key to parse.</param>
        protected object ParseKey<TKey>(string key)
        {
            return typeof(TKey) == typeof(string)
                ? key
                : (TKey)Convert.ChangeType(key, typeof(TKey));
        }
    }
}
