using System;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Migrations;
using StardewModdingAPI;

namespace ContentPatcher.Framework
{
    /// <summary>A content pack being loaded.</summary>
    internal class RawContentPack
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the migrations for a content config.</summary>
        private readonly Func<ContentConfig, IMigration[]> GetMigrations;

        /// <summary>The backing field for <see cref="Content"/>.</summary>
        private ContentConfig ContentImpl;

        /// <summary>The backing field for <see cref="Migrator"/>.</summary>
        private IMigration MigratorImpl;


        /*********
        ** Accessors
        *********/
        /// <summary>The managed content pack instance.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>The root breadcrumb path for the content pack.</summary>
        public LogPathBuilder LogPath { get; }

        /// <summary>The raw content configuration for this content pack.</summary>
        public ContentConfig Content => this.ContentImpl ?? throw new InvalidOperationException($"Must call {nameof(RawContentPack)}.{nameof(this.TryReloadContent)} before accessing the {nameof(this.Content)} field.");

        /// <summary>The migrations to apply for the content pack version.</summary>
        public IMigration Migrator => this.MigratorImpl ?? throw new InvalidOperationException($"Must call {nameof(RawContentPack)}.{nameof(this.TryReloadContent)} before accessing the {nameof(this.Migrator)} field.");

        /// <summary>The content pack's index in the load order.</summary>
        public int Index { get; }

        /// <summary>The content pack's manifest.</summary>
        public IManifest Manifest => this.ContentPack.Manifest;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPack">The managed content pack instance.</param>
        /// <param name="index">The content pack's index in the load order.</param>
        /// <param name="getMigrations">Get the migrations for a content config.</param>
        public RawContentPack(IContentPack contentPack, int index, Func<ContentConfig, IMigration[]> getMigrations)
        {
            this.ContentPack = contentPack;
            this.Index = index;
            this.GetMigrations = getMigrations;
            this.LogPath = new LogPathBuilder(contentPack.Manifest.Name);
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="copyFrom">A content pack to clone.</param>
        public RawContentPack(RawContentPack copyFrom)
            : this(copyFrom.ContentPack, copyFrom.Index, copyFrom.GetMigrations)
        {
            this.ContentImpl = copyFrom.Content;
            this.MigratorImpl = copyFrom.Migrator;
        }

        /// <summary>Parse the underlying <c>content.json</c> file and set the <see cref="Content"/> and <see cref="Migrator"/> fields.</summary>
        /// <param name="error">The error indicating why the content could not be reloaded, if applicable.</param>
        public bool TryReloadContent(out string error)
        {
            const string filename = "content.json";

            // load raw file
            ContentConfig content = this.ContentPack.ReadJsonFile<ContentConfig>(filename);
            if (content == null)
            {
                error = $"content pack has no {filename} file";
                return false;
            }

            // validate base fields
            if (content.Format == null)
            {
                error = $"content pack doesn't specify the required {nameof(ContentConfig.Format)} field.";
                return false;
            }
            if (!content.Changes.Any() && !content.CustomLocations.Any())
            {
                error = $"content pack must specify the {nameof(ContentConfig.Changes)} or {nameof(ContentConfig.CustomLocations)} fields.";
                return false;
            }

            // apply migrations
            IMigration migrator = new AggregateMigration(content.Format, this.GetMigrations(content));
            if (!migrator.TryMigrate(content, out error))
                return false;

            // load content
            this.ContentImpl = content;
            this.MigratorImpl = migrator;
            return true;
        }
    }
}
