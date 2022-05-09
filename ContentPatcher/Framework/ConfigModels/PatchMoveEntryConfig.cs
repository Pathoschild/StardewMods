using Newtonsoft.Json;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The input settings for a <see cref="PatchConfig.MoveEntries"/> field.</summary>
    internal class PatchMoveEntryConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The entry ID to move.</summary>
        public string? ID { get; }

        /// <summary>The ID of another entry this one should be inserted before.</summary>
        public string? BeforeID { get; }

        /// <summary>The ID of another entry this one should be inserted after.</summary>
        public string? AfterID { get; }

        /// <summary>The position to set.</summary>
        public string? ToPosition { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The entry ID to move.</param>
        /// <param name="beforeID">The ID of another entry this one should be inserted before.</param>
        /// <param name="afterID">The ID of another entry this one should be inserted after.</param>
        /// <param name="toPosition">he position to set.</param>
        [JsonConstructor]
        public PatchMoveEntryConfig(string? id, string? beforeID, string? afterID, string? toPosition)
        {
            this.ID = id;
            this.BeforeID = beforeID;
            this.AfterID = afterID;
            this.ToPosition = toPosition;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other instance to copy.</param>
        public PatchMoveEntryConfig(PatchMoveEntryConfig other)
            : this(
                  id: other.ID,
                  beforeID: other.BeforeID,
                  afterID: other.AfterID,
                  toPosition: other.ToPosition
            )
        { }
    }
}
