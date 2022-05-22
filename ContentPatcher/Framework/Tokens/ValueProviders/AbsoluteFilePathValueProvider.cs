using System;
using System.Collections.Generic;
using System.IO;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which gets the absolute path for a file in the content pack's folder.</summary>
    internal class AbsoluteFilePathValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The absolute path to the content pack's folder.</summary>
        private readonly string BaseDirPath;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="baseDirPath">The absolute path to the content pack's folder.</param>
        public AbsoluteFilePathValueProvider(string baseDirPath)
            : base(ConditionType.AbsoluteFilePath, mayReturnMultipleValuesForRoot: false)
        {
            this.BaseDirPath = baseDirPath;
            this.EnableInputArguments(required: true, mayReturnMultipleValues: false, maxPositionalArgs: null);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            bool changed = !this.IsReady;
            this.MarkReady(true);
            return changed;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            string? path = this.GetAbsolutePath(input.GetPositionalSegment());

            return path != null
                ? InvariantSets.FromValue(path)
                : InvariantSets.Empty;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the absolute path for a file in the content pack with validation.</summary>
        /// <param name="path">The relative file path.</param>
        /// <exception cref="InvalidOperationException">The path is not relative or contains directory climbing (../).</exception>
        private string? GetAbsolutePath(string? path)
        {
            // get normalized path
            path = PathUtilities.NormalizePath(path);
            if (string.IsNullOrWhiteSpace(path))
                return path;

            // validate
            if (Path.IsPathRooted(path))
                throw new InvalidOperationException($"The {this.Name} token requires a relative path.");
            if (!PathUtilities.IsSafeRelativePath(path))
                throw new InvalidOperationException($"The {this.Name} token requires a relative path and cannot contain directory climbing (../).");

            // get path
            return Path.GetFullPath(
                Path.Combine(this.BaseDirPath, path)
            );
        }
    }
}
