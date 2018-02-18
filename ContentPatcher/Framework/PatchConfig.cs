using Microsoft.Xna.Framework;

namespace ContentPatcher.Framework
{
    /// <summary>The input settings for a patch from the configuration file.</summary>
    internal class PatchConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The patch type to apply.</summary>
        public string Action { get; set; }

        /// <summary>The asset key to change.</summary>
        public string Target { get; set; }

        /// <summary>The local file to load.</summary>
        public string FromFile { get; set; }

        /// <summary>The sprite area from which to read an image.</summary>
        public Rectangle FromArea { get; set; }

        /// <summary>The sprite area to overwrite.</summary>
        public Rectangle ToArea { get; set; }
    }
}
