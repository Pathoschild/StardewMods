namespace ContentPatcher.Framework.Patches
{
    /// <summary>A predefined position when moving entries.</summary>
    public enum MoveEntryPosition
    {
        /// <summary>Don't move the entry.</summary>
        None,

        /// <summary>Insert the entry at index 0.</summary>
        Top,

        /// <summary>Append the entry to the end of the list.</summary>
        Bottom
    }
}
