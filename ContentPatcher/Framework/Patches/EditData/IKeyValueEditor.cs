using System;
using Newtonsoft.Json.Linq;

namespace ContentPatcher.Framework.Patches.EditData
{
    /// <summary>Encapsulates applying key/value edits to a data structure.</summary>
    internal interface IKeyValueEditor
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether entries in the data can be reordered.</summary>
        public bool CanMoveEntries { get; }

        /// <summary>Whether new entries can be added to the data model.</summary>
        public bool CanAddEntries { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Parse a raw key into the data type expected by the data editor.</summary>
        /// <param name="key">The raw key to parse.</param>
        object ParseKey(string key);

        /// <summary>Get whether the underlying asset has an entry.</summary>
        /// <param name="key">The entry key parsed by <see cref="ParseKey"/>.</param>
        bool HasEntry(object key);

        /// <summary>Get the value of an entry.</summary>
        /// <param name="key">The entry key parsed by <see cref="ParseKey"/>.</param>
        object? GetEntry(object key);

        /// <summary>Get the expected type for an entry.</summary>
        /// <param name="key">The entry key parsed by <see cref="ParseKey"/>.</param>
        Type? GetEntryType(object key);

        /// <summary>Remove an entry.</summary>
        /// <param name="key">The entry key parsed by <see cref="ParseKey"/>.</param>
        void RemoveEntry(object key);

        /// <summary>Replace the value of an entry.</summary>
        /// <param name="key">The entry key parsed by <see cref="ParseKey"/>.</param>
        /// <param name="value">The new value.</param>
        void SetEntry(object key, JToken value);

        /// <summary>Move an entry within the data.</summary>
        /// <param name="key">The key of the entry to move.</param>
        /// <param name="toPosition">The position to which to move it within the data.</param>
        MoveResult MoveEntry(object key, MoveEntryPosition toPosition);

        /// <summary>Move an entry within the data.</summary>
        /// <param name="key">The key of the entry to move.</param>
        /// <param name="anchorKey">The key of the entry to which the move is relative.</param>
        /// <param name="afterAnchor">Whether to move the entry so it's right after the <paramref name="anchorKey"/>; else move it right before the <paramref name="anchorKey"/>.</param>
        MoveResult MoveEntry(object key, object anchorKey, bool afterAnchor);
    }
}
