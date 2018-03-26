using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The input settings for a patch from the configuration file.</summary>
    internal class PatchConfig
    {
        /*********
        ** Accessors
        *********/
        /****
        ** All actions
        ****/
        /// <summary>A name for this patch shown in log messages.</summary>
        public string LogName { get; set; }

        /// <summary>The patch type to apply.</summary>
        public string Action { get; set; }

        /// <summary>The asset key to change.</summary>
        public string Target { get; set; }

        /// <summary>Whether to apply this patch.</summary>
        /// <remarks>This must be a string to support config tokens.</remarks>
        public string Enabled { get; set; } = "true";

        /// <summary>The criteria to apply. See readme for valid values.</summary>
        public InvariantDictionary<string> When { get; set; }

        /****
        ** Some actions
        ****/
        /// <summary>The local file to load.</summary>
        public string FromFile { get; set; }

        /****
        ** EditImage
        ****/
        /// <summary>The sprite area from which to read an image.</summary>
        public Rectangle FromArea { get; set; }

        /// <summary>The sprite area to overwrite.</summary>
        public Rectangle ToArea { get; set; }

        /// <summary>Indicates how the image should be patched.</summary>
        public string PatchMode { get; set; }

        /****
        ** EditData
        ****/
        /// <summary>The data records to edit.</summary>
        public IDictionary<string, string> Entries { get; set; }

        /// <summary>The individual fields to edit in data records.</summary>
        public IDictionary<string, IDictionary<int, string>> Fields { get; set; }
    }
}
