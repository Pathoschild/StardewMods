namespace ContentPatcher.Framework.Constants
{
    /// <summary>Which delimited values should be removed or replaced by a text operation.</summary>
    internal enum TextOperationReplaceMode
    {
        /// <summary>Remove the first value which matches the search.</summary>
        First,

        /// <summary>Remove the last value which matches the search.</summary>
        Last,

        /// <summary>Remove all values which match the search.</summary>
        All
    }
}
