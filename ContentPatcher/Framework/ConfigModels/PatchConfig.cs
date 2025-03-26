using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.ConfigModels;

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
    public string? LogName { get; set; }

    /// <summary>The patch type to apply.</summary>
    public string? Action { get; set; }

    /// <summary>The asset name to change.</summary>
    public string? Target { get; set; }

    /// <summary>The locale code in the target asset's name to match (like <c>fr-FR</c> to target <c>Characters/Dialogue/Abigail.fr-FR</c>), or an empty string to match only the base unlocalized asset, or <c>null</c> to match all localized or unlocalized variants of the <see cref="Target"/>.</summary>
    public string? TargetLocale { get; set; }

    /// <summary>Indicates when a patch should be updated.</summary>
    public string? Update { get; set; }

    /// <summary>The local file to load.</summary>
    public string? FromFile { get; set; }

    /// <summary>Whether to apply this patch.</summary>
    /// <remarks>This must be a string to support config tokens.</remarks>
    public string? Enabled { get; set; }

    /// <summary>The criteria to apply. See readme for valid values.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public InvariantDictionary<string?> When { get; } = new();

    /// <summary>The priority for this patch when multiple patches apply.</summary>
    public string? Priority { get; set; }

    /// <summary>The local token values to use for this patch, in addition to the pre-existing tokens.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public InvariantDictionary<string?>? LocalTokens { get; set; }

    /****
    ** Multiple actions
    ****/
    /// <summary>The text operations to apply.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public List<TextOperationConfig?> TextOperations { get; } = [];

    /****
    ** EditImage
    ****/
    /// <summary>The sprite area from which to read an image.</summary>
    public PatchRectangleConfig? FromArea { get; set; }

    /// <summary>The sprite area to overwrite.</summary>
    public PatchRectangleConfig? ToArea { get; set; }

    /// <summary>Indicates how the image should be patched.</summary>
    public string? PatchMode { get; set; }

    /****
    ** EditData
    ****/
    /// <summary>The data records to edit.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public InvariantDictionary<JToken?> Entries { get; } = new();

    /// <summary>The individual fields to edit in data records.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public InvariantDictionary<InvariantDictionary<JToken?>?> Fields { get; } = new();

    /// <summary>The records to reorder, if the target is a list asset.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public List<PatchMoveEntryConfig?> MoveEntries { get; } = [];

    /// <summary>The field within the data asset to which edits should be applied, or empty to apply to the root asset.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public List<string> TargetField { get; } = [];

    /****
    ** EditMap
    ****/
    /// <summary>The map properties to edit.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public InvariantDictionary<string?> MapProperties { get; } = new();

    /// <summary>The NPC-only warps to add to the location.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public List<string?> AddNpcWarps { get; } = [];

    /// <summary>The warps to add to the location.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public List<string?> AddWarps { get; } = [];

    /// <summary>The map tiles to edit.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public List<PatchMapTileConfig?> MapTiles { get; } = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    [JsonConstructor]
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
        this.Priority = other.Priority;
        this.LocalTokens = other.LocalTokens?.Clone();

        // multiple actions
        this.TextOperations = other.TextOperations.Select(p => p != null ? new TextOperationConfig(p) : null).ToList();

        // EditImage
        this.FromArea = other.FromArea != null ? new PatchRectangleConfig(other.FromArea) : null;
        this.ToArea = other.ToArea != null ? new PatchRectangleConfig(other.ToArea) : null;
        this.PatchMode = other.PatchMode;

        // EditData
        this.Entries = other.Entries.Clone(value => value?.DeepClone());
        this.Fields = other.Fields.Clone(
            entryFields => entryFields?.Clone(value => value?.DeepClone())
        );
        this.MoveEntries = other.MoveEntries.Select(p => p != null ? new PatchMoveEntryConfig(p) : null).ToList();
        this.TargetField = other.TargetField.ToList();

        // EditMap
        this.MapProperties = other.MapProperties.Clone();
        this.AddNpcWarps = other.AddNpcWarps.ToList();
        this.AddWarps = other.AddWarps.ToList();
        this.MapTiles = other.MapTiles.Select(p => p != null ? new PatchMapTileConfig(p) : null).ToList();
    }
}
