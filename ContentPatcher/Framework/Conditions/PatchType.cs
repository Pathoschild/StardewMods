namespace ContentPatcher.Framework.Conditions
{
    /// <summary>The patch type.</summary>
    internal enum PatchType
    {
        /// <summary>Load the initial version of the file.</summary>
        Load,

        /// <summary>Replace an image with bread.</summary>
        Loaf,

        /// <summary>Edit an image.</summary>
        EditImage,

        /// <summary>Edit a data file.</summary>
        EditData,

        /// <summary>Edit a map after it's loaded.</summary>
        EditMap
    }
}
