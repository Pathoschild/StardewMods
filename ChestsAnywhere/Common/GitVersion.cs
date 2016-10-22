using System;
using Newtonsoft.Json;

namespace ChestsAnywhere.Common
{
    /// <summary>Metadata about a GitHub release tag.</summary>
    internal class GitRelease
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The display name.</summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>The semantic version number.</summary>
        [JsonProperty("tag_name")]
        public string Version { get; set; }

        /// <summary>The text summarising changes in this release.</summary>
        [JsonProperty("body")]
        public string Summary { get; set; }

        [JsonIgnore]
        public bool Errored { get; set; }

        /*********
        ** Public methods
        *********/
        /// <summary>Get whether this release supercedes the specified version.</summary>
        /// <param name="version">The potentially superceded version.</param>
        public bool IsNewerThan(string version)
        {
            return string.Compare(version, this.Version, StringComparison.InvariantCultureIgnoreCase) == -1;
        }
    }
}
