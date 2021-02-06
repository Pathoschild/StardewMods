using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Migrations;
using StardewModdingAPI;

namespace ContentPatcher.Framework
{
    /// <summary>A content pack being loaded.</summary>
    internal class RawContentPack
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The managed content pack instance.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>The raw content configuration for this content pack.</summary>
        public ContentConfig Content { get; }

        /// <summary>The migrations to apply for the content pack version.</summary>
        public IMigration Migrator { get; }

        /// <summary>The content pack's index in the load order.</summary>
        public int Index { get; }

        /// <summary>The content pack's manifest.</summary>
        public IManifest Manifest => this.ContentPack.Manifest;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPack">The managed content pack instance.</param>
        /// <param name="content">The raw content configuration for this content pack.</param>
        /// <param name="migrator">The migrations to apply for the content pack version.</param>
        /// <param name="index">The content pack's index in the load order.</param>
        public RawContentPack(IContentPack contentPack, ContentConfig content, IMigration migrator, int index)
        {
            this.ContentPack = contentPack;
            this.Content = content;
            this.Migrator = migrator;
            this.Index = index;
        }
    }
}
