namespace ContentPatcher.Framework.Conditions
{
    /// <summary>The patch type.</summary>
    internal enum PatchType
    {
        /// <summary>Load the initial version of the file.</summary>
        Load,

        /// <summary>Edit an image.</summary>
        EditImage,

        /// <summary>Edit a data file.</summary>
        EditData,

        /// <summary>Edit a map after it's loaded.</summary>
        EditMap,

        /// <summary>Include patches from another JSON file.</summary>
        Include,

        /// <summary>Run code.</summary>
        Code,
    }
}
