using StardewModdingAPI;

namespace ContentPatcher.Framework
{
    /// <summary>The model for a content patch file.</summary>
    internal class ContentConfig
    {
        /// <summary>The format version.</summary>
        public ISemanticVersion Format { get; set; }
        
        /// <summary>Whether to apply this content pack.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>The changes to make.</summary>
        public PatchConfig[] Changes { get; set; }
    }
}
