using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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

        /// <summary>Get the unique key for an entry, if available.</summary>
        private readonly Lazy<Func<TValue, string?>?> GetAssetKeyImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="data">The underlying data.</param>
        public ListKeyValueEditor(IList<TValue> data)
        {
            this.Data = data;
            this.CanMoveEntries = true;
            this.GetAssetKeyImpl = new(this.GetAssetKeyFunc);
        }

        /// <inheritdoc />
        public override object ParseKey(string key)
        {
            // array index (like "#5")
            string[] parts = key.Split('#', count: 2);
            if (parts.Length == 2 && string.IsNullOrWhiteSpace(parts[0]) && int.TryParse(parts[1], out int index))
                return index;

            // else entry key
            return key;
        }

        /// <inheritdoc />
        public override object? GetEntry(object key)
        {
            return this.TryGetEntry(key, out TValue? value, out _)
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
            if (this.TryGetEntry(key, out _, out int index))
                this.Data.RemoveAt(index);
        }

        /// <inheritdoc />
        public override void SetEntry(object key, object value)
        {
            if (this.TryGetEntry(key, out _, out int index))
                this.Data[index] = (TValue)value;
            else
                this.Data.Add((TValue)value);
        }

        /// <inheritdoc />
        public override MoveResult MoveEntry(object key, MoveEntryPosition toPosition)
        {
            // get entry
            if (!this.TryGetEntry(key, out TValue? entry, out int index))
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
            if (!this.TryGetEntry(key, out TValue? entry, out int entryIndex))
                return MoveResult.TargetNotFound;
            if (!this.TryGetEntry(anchorKey, out _, out int anchorIndex))
                return MoveResult.AnchorNotFound;
            if (entryIndex == anchorIndex)
                return MoveResult.AnchorIsMain;

            // move to position
            int newIndex = afterAnchor
                ? anchorIndex + 1
                : anchorIndex;

            if (entryIndex < anchorIndex)
                newIndex--; // list will shift up when we remove the old entry

            this.Data.RemoveAt(entryIndex);
            this.Data.Insert(newIndex, entry);
            return MoveResult.Success;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the key for a list asset entry.</summary>
        /// <param name="entry">The entity whose ID to fetch.</param>
        private string? GetKey(TValue entry)
        {
            Func<TValue, string?>? getKey = this.GetAssetKeyImpl.Value;
            if (getKey != null)
                return getKey(entry);

            throw new NotSupportedException($"No ID implementation for list value type {typeof(TValue).FullName}.");
        }

        /// <summary>Get a strongly-typed entry from the list.</summary>
        /// <param name="keyOrIndex">The entry key or array index.</param>
        /// <param name="value">The entry found in the list.</param>
        /// <param name="index">The entry's index within the list.</param>
        private bool TryGetEntry(object keyOrIndex, [NotNullWhen(true)] out TValue? value, out int index)
        {
            // get entry by key or index
            switch (keyOrIndex)
            {
                case int searchIndex:
                    if (searchIndex >= 0 && searchIndex < this.Data.Count)
                    {
                        value = this.Data[searchIndex]!;
                        index = searchIndex;
                        return true;
                    }
                    break;

                case string key:
                    for (int i = 0; i < this.Data.Count; i++)
                    {
                        value = this.Data[i]!;

                        if (this.GetKey(value) == key)
                        {
                            index = i;
                            return true;
                        }
                    }
                    break;
            }

            // not found
            value = default;
            index = -1;
            return false;
        }

        /// <summary>Get a function which returns the unique key for an entry, if available.</summary>
        public Func<TValue, string?>? GetAssetKeyFunc()
        {
            // ID property
            {
                PropertyInfo? property = typeof(TValue).GetProperty("Id") ?? typeof(TValue).GetProperty("ID");
                if (property?.GetMethod != null)
                    return entry => property.GetValue(entry)?.ToString();
            }

            // ID field
            {
                FieldInfo? field = typeof(TValue).GetField("Id") ?? typeof(TValue).GetField("ID");
                if (field != null)
                    return entry => field.GetValue(entry)?.ToString();
            }

            return null;
        }
    }
}
