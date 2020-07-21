using System;
using System.Collections.Generic;
using System.IO;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which checks whether a file exists in the content pack's folder.</summary>
    internal class HasFileValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get whether a relative file path exists in the content pack.</summary>
        private readonly Func<string, bool> RelativePathExists;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="relativePathExists">Get whether a relative file path exists in the content pack.</param>
        public HasFileValueProvider(Func<string, bool> relativePathExists)
            : base(ConditionType.HasFile, mayReturnMultipleValuesForRoot: false)
        {
            this.RelativePathExists = relativePathExists;
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
        public override bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            allowedValues = InvariantHashSet.Boolean();
            return true;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            yield return this.GetPathExists(input.GetPositionalSegment()).ToString();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the given file path exists.</summary>
        /// <param name="path">The relative file path.</param>
        /// <exception cref="InvalidOperationException">The path is not relative or contains directory climbing (../).</exception>
        private bool GetPathExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // get normalized path
            path = PathUtilities.NormalizePathSeparators(path);

            // validate
            if (Path.IsPathRooted(path))
                throw new InvalidOperationException($"The {ConditionType.HasFile} token requires a relative path.");
            if (!PathUtilities.IsSafeRelativePath(path))
                throw new InvalidOperationException($"The {ConditionType.HasFile} token requires a relative path and cannot contain directory climbing (../).");

            // check file existence
            return this.RelativePathExists(path);
        }
    }
}
