using System.Collections.Generic;
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

        /// <summary>The language code to patch (or <c>null</c> for any language).</summary>
        public string Locale { get; set; }

        /// <summary>The local file to load.</summary>
        public string FromFile { get; set; }

        /// <summary>Whether to apply this patch.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>The sprite area from which to read an image.</summary>
        public Rectangle FromArea { get; set; }

        /// <summary>The sprite area to overwrite.</summary>
        public Rectangle ToArea { get; set; }

        /// <summary>The data records to edit.</summary>
        public IDictionary<string, string> Entries { get; set; }

        /// <summary>The individual fields to edit in data records.</summary>
        public IDictionary<string, IDictionary<int, string>> Fields { get; set; }
    }
}
