using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens.Json;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Aggregates content pack migrations.</summary>
    internal class AggregateMigration : IRuntimeMigration
    {
        /*********
        ** Fields
        *********/
        /// <summary>The valid format versions.</summary>
        private readonly HashSet<string> ValidVersions;

        /// <summary>The latest format version.</summary>
        private readonly string LatestVersion;

        /// <summary>The migrations to apply.</summary>
        private readonly IMigration[] Migrations;

        /// <summary>The migrations to apply at runtime.</summary>
        private readonly IRuntimeMigration[] RuntimeMigrations;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public ISemanticVersion Version { get; }

        /// <inheritdoc />
        public string[] MigrationWarnings { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="version">The content pack version.</param>
        /// <param name="migrations">The migrations to apply.</param>
        public AggregateMigration(ISemanticVersion version, IMigration[] migrations)
        {
            this.Version = version;
            this.ValidVersions = new HashSet<string>(migrations.Select(p => p.Version.ToString()));
            this.LatestVersion = migrations.Last().Version.ToString();
            this.Migrations = migrations.Where(m => m.Version.IsNewerThan(version)).ToArray();
            this.RuntimeMigrations = this.Migrations.OfType<IRuntimeMigration>().ToArray();
            this.MigrationWarnings = this.Migrations.SelectMany(p => p.MigrationWarnings).Distinct().ToArray();
        }

        /****
        ** IMigration
        ****/
        /// <inheritdoc />
        public bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error)
        {
            // validate format version
            if (!this.ValidVersions.Contains(new SemanticVersion(content.Format!.MajorVersion, content.Format.MinorVersion, 0).ToString()))
            {
                string latestVersion = this.LatestVersion;
                error = content.Format.IsNewerThan(latestVersion)
                    ? $"unsupported format version {content.Format}, you may need to update Content Patcher"
                    : $"unsupported format version {content.Format} (expected {latestVersion})";
                return false;
            }

            // apply migrations
            foreach (IMigration migration in this.Migrations)
            {
                if (!migration.TryMigrateMainContent(content, out error))
                    return false;
            }

            // no issues found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
        {
            // apply migrations
            foreach (IMigration migration in this.Migrations)
            {
                if (!migration.TryMigrate(ref patches, out error))
                    return false;
            }

            // no issues found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
        {
            // apply migrations
            foreach (IMigration migration in this.Migrations)
            {
                if (!migration.TryMigrate(ref lexToken, out error))
                    return false;
            }

            // no issues found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryMigrate(IManagedTokenString tokenStr, [NotNullWhen(false)] out string? error)
        {
            // apply migrations
            foreach (IMigration migration in this.Migrations)
            {
                if (!migration.TryMigrate(tokenStr, out error))
                    return false;
            }

            // no issues found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryMigrate(TokenizableJToken tokenStructure, [NotNullWhen(false)] out string? error)
        {
            // apply migrations
            foreach (IMigration migration in this.Migrations)
            {
                if (!migration.TryMigrate(tokenStructure, out error))
                    return false;
            }

            // no issues found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryMigrate(Condition condition, [NotNullWhen(false)] out string? error)
        {
            // apply migrations
            foreach (IMigration migration in this.Migrations)
            {
                if (!migration.TryMigrate(condition, out error))
                    return false;
            }

            // no issues found
            error = null;
            return true;
        }

        /****
        ** IRuntimeMigration
        ****/
        /// <inheritdoc />
        public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
        {
            IAssetName? target = null;

            foreach (IRuntimeMigration migration in this.RuntimeMigrations)
                target = migration.RedirectTarget(target ?? assetName, patch);

            return target;
        }

        /// <inheritdoc />
        public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
            where T : notnull
        {
            bool anyChanged = false;

            error = null;

            foreach (IRuntimeMigration migration in this.RuntimeMigrations)
            {
                if (migration.TryApplyLoadPatch(patch, assetName, ref asset, out error))
                    anyChanged = true;

                if (error != null)
                    return false;
            }

            return anyChanged;
        }

        /// <inheritdoc />
        public bool TryApplyEditPatch<T>(IPatch patch, IAssetData asset, out string? error)
            where T : notnull
        {
            bool anyChanged = false;

            error = null;

            foreach (IRuntimeMigration migration in this.RuntimeMigrations)
            {
                if (migration.TryApplyEditPatch<T>(patch, asset, out error))
                    anyChanged = true;

                if (error != null)
                    return false;
            }

            return anyChanged;
        }
    }
}
