namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The input settings for a <see cref="PatchConfig.MoveEntries"/> field.</summary>
    internal class PatchMoveEntryConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The entry ID to move.</summary>
        public string ID { get; set; }

        /// <summary>The ID of another entry this one should be inserted before.</summary>
        public string BeforeID { get; set; }

        /// <summary>The ID of another entry this one should be inserted after.</summary>
        public string AfterID { get; set; }

        /// <summary>The position to set.</summary>
        public string ToPosition { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public PatchMoveEntryConfig() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other instance to copy.</param>
        public PatchMoveEntryConfig(PatchMoveEntryConfig other)
        {
            this.ID = other.ID;
            this.BeforeID = other.BeforeID;
            this.AfterID = other.AfterID;
            this.ToPosition = other.ToPosition;
        }
    }
}
