namespace ContentPatcher.Framework.Patches.EditData
{
    /// <summary>The result of a move attempt.</summary>
    internal enum MoveResult
    {
        /// <summary>The entry was successfully moved.</summary>
        Success,

        /// <summary>The entry to be moved wasn't found.</summary>
        TargetNotFound,

        /// <summary>The anchor entry is the same as the target entry.</summary>
        AnchorIsMain,

        /// <summary>The anchor entry used to decide the position wasn't found.</summary>
        AnchorNotFound
    }
}
