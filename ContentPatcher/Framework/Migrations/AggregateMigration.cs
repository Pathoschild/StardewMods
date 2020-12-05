using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens.Json;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Aggregates content pack migrations.</summary>
    internal class AggregateMigration : IMigration
    {
        /*********
        ** Fields
        *********/
        /// <summary>The valid format versions.</summary>
        private readonly HashSet<string> ValidVersions;

        /// <summary>The migrations to apply.</summary>
        private readonly IMigration[] Migrations;


        /*********
        ** Accessors
        *********/
        /// <summary>The version to which this migration applies.</summary>
        public ISemanticVersion Version { get; }


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
            this.Migrations = migrations.Where(m => m.Version.IsNewerThan(version)).ToArray();
        }

        /// <inheritdoc />
        public bool TryMigrate(ContentConfig content, out string error)
        {
            // validate format version
            if (!this.ValidVersions.Contains(content.Format.ToString()))
            {
                error = $"unsupported format {content.Format} (supported version: {string.Join(", ", this.ValidVersions)}).";
                return false;
            }

            // apply migrations
            foreach (IMigration migration in this.Migrations)
            {
                if (!migration.TryMigrate(content, out error))
                    return false;
            }

            // no issues found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryMigrate(ILexToken lexToken, out string error)
        {
            // apply migrations
            foreach (IMigration migration in this.Migrations)
            {
                if (!migration.TryMigrate(lexToken, out error))
                    return false;
            }

            // no issues found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryMigrate(IManagedTokenString tokenStr, out string error)
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
        public bool TryMigrate(TokenizableJToken tokenStructure, out string error)
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
        public bool TryMigrate(Condition condition, out string error)
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
    }
}
