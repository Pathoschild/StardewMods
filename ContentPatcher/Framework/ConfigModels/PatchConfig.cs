using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
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

        /// <summary>Indicates when a patch should be updated.</summary>
        public string Update { get; set; }

        /// <summary>The local file to load.</summary>
        public string FromFile { get; set; }

        /// <summary>Whether to apply this patch.</summary>
        /// <remarks>This must be a string to support config tokens.</remarks>
        public string Enabled { get; set; } = "true";

        /// <summary>The criteria to apply. See readme for valid values.</summary>
        public InvariantDictionary<string> When { get; set; }

        /****
        ** Multiple actions
        ****/
        /// <summary>The text operations to apply.</summary>
        public TextOperationConfig[] TextOperations { get; set; }

        /****
        ** EditImage
        ****/
        /// <summary>The sprite area from which to read an image.</summary>
        public PatchRectangleConfig FromArea { get; set; }

        /// <summary>The sprite area to overwrite.</summary>
        public PatchRectangleConfig ToArea { get; set; }

        /// <summary>Indicates how the image should be patched.</summary>
        public string PatchMode { get; set; }

        /****
        ** EditData
        ****/
        /// <summary>The data records to edit.</summary>
        public InvariantDictionary<JToken> Entries { get; set; }

        /// <summary>The individual fields to edit in data records.</summary>
        public InvariantDictionary<InvariantDictionary<JToken>> Fields { get; set; }

        /// <summary>The records to reorder, if the target is a list asset.</summary>
        public PatchMoveEntryConfig[] MoveEntries { get; set; }

        /****
        ** EditMap
        ****/
        /// <summary>The map properties to edit.</summary>
        public InvariantDictionary<string> MapProperties { get; set; }

        /// <summary>The map tiles to edit.</summary>
        public PatchMapTileConfig[] MapTiles { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public PatchConfig() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other instance to copy.</param>
        public PatchConfig(PatchConfig other)
        {
            // all actions
            this.LogName = other.LogName;
            this.Action = other.Action;
            this.Target = other.Target;
            this.Update = other.Update;
            this.FromFile = other.FromFile;
            this.Enabled = other.Enabled;
            this.When = other.When.Clone();

            // multiple actions
            this.TextOperations = other.TextOperations.Select(p => new TextOperationConfig(p)).ToArray();

            // EditImage
            this.FromArea = other.FromArea != null ? new PatchRectangleConfig(other.FromArea) : null;
            this.ToArea = other.ToArea != null ? new PatchRectangleConfig(other.ToArea) : null;
            this.PatchMode = other.PatchMode;

            // EditData
            this.Entries = other.Entries.Clone(value => value?.DeepClone());
            this.Fields = other.Fields.Clone(
                entryFields => entryFields?.Clone(value => value?.DeepClone())
            );
            this.MoveEntries = other.MoveEntries.Select(p => new PatchMoveEntryConfig(p)).ToArray();

            // EditMap
            this.MapProperties = other.MapProperties.Clone();
            this.MapTiles = other.MapTiles.Select(p => new PatchMapTileConfig(p)).ToArray();
        }

        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            // all actions
            this.When ??= new InvariantDictionary<string>();

            // multiple actions
            this.TextOperations ??= new TextOperationConfig[0];

            // EditData
            this.Entries ??= new InvariantDictionary<JToken>();
            this.Fields ??= new InvariantDictionary<InvariantDictionary<JToken>>();
            this.MoveEntries ??= new PatchMoveEntryConfig[0];

            // EditMap
            this.MapProperties ??= new InvariantDictionary<string>();
            this.MapTiles ??= new PatchMapTileConfig[0];
        }
    }
}
