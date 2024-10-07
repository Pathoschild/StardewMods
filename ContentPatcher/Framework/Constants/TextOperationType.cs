namespace ContentPatcher.Framework.Constants
{
    /// <summary>A text operation type.</summary>
    internal enum TextOperationType
    {
        /// <summary>Append text after the target value.</summary>
        Append,

        /// <summary>Prepend text before the target value.</summary>
        Prepend,

        /// <summary>Parse the target text into a list of delimited values, and remove the values matching the search.</summary>
        RemoveDelimited,

        /// <summary>Parse the target text into a list of delimited values, and replace the values matching the search with a new value.</summary>
        ReplaceDelimited
    }
}
