using Newtonsoft.Json;

namespace ContentPatcher.Framework.ConfigModels;

/// <summary>The input settings for a <see cref="PatchConfig.MoveEntries"/> field.</summary>
internal class PatchMoveEntryConfig
{
    /*********
    ** Accessors
    *********/
    /// <summary>The entry ID to move.</summary>
    public string? ID { get; }

    /// <summary>The ID of another entry this one should be inserted before.</summary>
    public string? BeforeId { get; }

    /// <summary>The ID of another entry this one should be inserted after.</summary>
    public string? AfterId { get; }

    /// <summary>The position to set.</summary>
    public string? ToPosition { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="id">The entry ID to move.</param>
    /// <param name="beforeId">The ID of another entry this one should be inserted before.</param>
    /// <param name="afterId">The ID of another entry this one should be inserted after.</param>
    /// <param name="toPosition">The position to set.</param>
    [JsonConstructor]
    public PatchMoveEntryConfig(string? id, string? beforeId, string? afterId, string? toPosition)
    {
        this.ID = id;
        this.BeforeId = beforeId;
        this.AfterId = afterId;
        this.ToPosition = toPosition;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="other">The other instance to copy.</param>
    public PatchMoveEntryConfig(PatchMoveEntryConfig other)
        : this(
            id: other.ID,
            beforeId: other.BeforeId,
            afterId: other.AfterId,
            toPosition: other.ToPosition
        )
    { }
}
