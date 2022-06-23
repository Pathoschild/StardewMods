namespace ContentPatcher.Framework.Constants
{
    /// <summary>A text operation type.</summary>
    internal enum TextOperationType
    {
        /// <summary>Append text after the target value.</summary>
        Append,

        /// <summary>Prepend text before the target value.</summary>
        Prepend,

        /// <summary>Remove the first text occurrence in the target value.</summary>
        RemoveFirstOccurrence,

        /// <summary>Remove the last text occurrence in the target value.</summary>
        RemoveLastOccurrence,

        /// <summary>Remove all occurrences of the text in the target value.</summary>
        RemoveAllOccurrences
    }
}
