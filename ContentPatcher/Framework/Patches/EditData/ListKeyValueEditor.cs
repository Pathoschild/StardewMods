using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ContentPatcher.Framework.Patches.EditData;

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

    private Dictionary<string, int> KeyCache = [];


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
        {
            this.Data.RemoveAt(index);
            // TODO: Modify entries to offset their values
            this.KeyCache.Clear();
        }
    }

    /// <inheritdoc />
    public override void SetEntry(object key, object value)
    {
        if (this.TryGetEntry(key, out var oldVal, out int index))
        {
            this.Data[index] = (TValue)value;

            // TODO: Add null handling
            this.KeyCache.Remove(this.GetKey(oldVal));
            this.KeyCache[this.GetKey((TValue)value)] = index;
        }
        else
        {
            this.Data.Add((TValue)value);
            this.KeyCache[this.GetKey((TValue)value)] = this.Data.Count - 1;
        }
            
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
                this.KeyCache.Clear();
                this.Data.RemoveAt(index);
                this.Data.Insert(0, entry);
                break;

            case MoveEntryPosition.Bottom:
                this.KeyCache.Clear();
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

        this.KeyCache.Clear();

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
                if (this.KeyCache.TryGetValue(key, out int cachedIndex))
                {
                    value = this.Data[cachedIndex]!;
                    index = cachedIndex;
                    return true;
                }
                if (this.KeyCache.Count == this.Data.Count)
                {
                    // all entries are cached, so the key doesn't exist
                    break;
                }
                for (int i = 0; i < this.Data.Count; i++)
                {
                    value = this.Data[i]!;

                    string? actualKey = this.GetKey(value);
                    if (actualKey != null)
                    {
                        this.KeyCache[actualKey] = i;
                    }

                    if (actualKey == key)
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
        Type type = typeof(TValue);

        // string
        if (type == typeof(string))
            return entry => entry as string;

        // simple value type
        if (type.IsValueType && (type.IsPrimitive || type.IsEnum))
            return entry => entry?.ToString();

        // ID property
        {
            PropertyInfo? property = type.GetProperty("Id") ?? type.GetProperty("ID");
            if (property?.GetMethod != null)
                return entry => property.GetValue(entry)?.ToString();
        }

        // ID field
        {
            FieldInfo? field = type.GetField("Id") ?? type.GetField("ID");
            if (field != null)
                return entry => field.GetValue(entry)?.ToString();
        }

        return null;
    }
}
