using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches.EditData
{
    /// <summary>Encapsulates applying key/value edits to a list.</summary>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    internal class ListKeyValueEditor<TValue> : BaseDataEditor
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying data.</summary>
        private readonly IList<TValue> Data;

        /// <summary>Simplifies dynamic access to game code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="data">The underlying data.</param>
        /// <param name="reflection">Simplifies dynamic access to game code.</param>
        public ListKeyValueEditor(IList<TValue> data, IReflectionHelper reflection)
        {
            this.Data = data;
            this.Reflection = reflection;
            this.CanMoveEntries = true;
        }

        /// <inheritdoc />
        public override object ParseKey(string key)
        {
            return key;
        }

        /// <inheritdoc />
        public override object GetEntry(object key)
        {
            string parsedKey = (string)key;

            return this.TryGetEntry(parsedKey, out TValue value, out _)
                ? value
                : default;
        }

        /// <inheritdoc />
        public override void RemoveEntry(object key)
        {
            string parsedKey = (string)key;

            if (this.TryGetEntry(parsedKey, out _, out int index))
                this.Data.RemoveAt(index);
        }

        /// <inheritdoc />
        public override Type GetEntryType(object key)
        {
            return typeof(TValue);
        }

        /// <inheritdoc />
        public override void SetEntry(object key, JToken value)
        {
            string parsedKey = (string)key;

            if (this.TryGetEntry(parsedKey, out _, out int index))
                this.Data[index] = value.ToObject<TValue>();
            else
                this.Data.Add(value.ToObject<TValue>());
        }

        /// <inheritdoc />
        public override MoveResult MoveEntry(object key, MoveEntryPosition toPosition)
        {
            // get entry
            string parsedKey = (string)key;
            if (!this.TryGetEntry(parsedKey, out TValue entry, out int index))
                return MoveResult.TargetNotFound;

            // move entry
            switch (toPosition)
            {
                case MoveEntryPosition.Top:
                    this.Data.RemoveAt(index);
                    this.Data.Insert(0, entry);
                    break;

                case MoveEntryPosition.Bottom:
                    this.Data.RemoveAt(index);
                    this.Data.Add(entry);
                    break;

                case MoveEntryPosition.None:
                    break;

                default:
                    throw new InvalidOperationException($"Unknown move position '{toPosition}'.");
            }

            return MoveResult.Success;
        }

        /// <inheritdoc />
        public override MoveResult MoveEntry(object key, object anchorKey, bool afterAnchor)
        {
            // get entries
            if (!this.TryGetEntry((string)key, out TValue entry, out int entryIndex))
                return MoveResult.TargetNotFound;
            if (!this.TryGetEntry((string)anchorKey, out _, out int anchorIndex))
                return MoveResult.AnchorNotFound;
            if (entryIndex == anchorIndex)
                return MoveResult.TargetNotFound;

            // move to position
            int newIndex = afterAnchor
                ? anchorIndex + 1
                : anchorIndex;

            this.Data.RemoveAt(entryIndex);
            this.Data.Insert(newIndex, entry);
            return MoveResult.Success;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the key for a list asset entry.</summary>
        /// <param name="entity">The entity whose ID to fetch.</param>
        private string GetKey(TValue entity)
        {
            return InternalConstants.GetListAssetKey(entity, this.Reflection);
        }

        /// <summary>Get a strongly-typed entry from the list.</summary>
        /// <param name="key">The entry key.</param>
        /// <param name="value">The entry found in the list.</param>
        /// <param name="index">The entry's index within the list.</param>
        private bool TryGetEntry(string key, out TValue value, out int index)
        {
            for (int i = 0; i < this.Data.Count; i++)
            {
                value = this.Data[i];

                if (this.GetKey(value) == key)
                {
                    index = i;
                    return true;
                }
            }

            value = default;
            index = -1;
            return false;
        }
    }
}
