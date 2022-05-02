namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>A mod which adds custom machine recipes and requires a separate automation component.</summary>
    internal class DataModelIntegration
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique ID of the main mod.</summary>
        public string Id { get; }

        /// <summary>The display name for the main mod.</summary>
        public string Name { get; }

        /// <summary>The unique ID of the suggested bridge mod.</summary>
        public string SuggestedId { get; }

        /// <summary>The display name for the suggested bridge mod.</summary>
        public string SuggestedName { get; }

        /// <summary>The mod page for the suggested bridge mod.</summary>
        public string SuggestedUrl { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The unique ID of the main mod.</param>
        /// <param name="name">The display name for the main mod.</param>
        /// <param name="suggestedId">The unique ID of the suggested bridge mod.</param>
        /// <param name="suggestedName">The display name for the suggested bridge mod.</param>
        /// <param name="suggestedUrl">The mod page for the suggested bridge mod.</param>
        public DataModelIntegration(string id, string name, string suggestedId, string suggestedName, string suggestedUrl)
        {
            this.Id = id;
            this.Name = name;
            this.SuggestedId = suggestedId;
            this.SuggestedName = suggestedName;
            this.SuggestedUrl = suggestedUrl;
        }
    }
}
