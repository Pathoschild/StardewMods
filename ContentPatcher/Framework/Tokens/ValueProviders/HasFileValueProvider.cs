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
            : base(ConditionType.HasFile, canHaveMultipleValuesForRoot: false)
        {
            this.RelativePathExists = relativePathExists;
            this.EnableInputArguments(required: true, canHaveMultipleValues: false);
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            bool changed = !this.IsReady;
            this.MarkReady(true);
            return changed;
        }

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="IValueProvider.HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override bool HasBoundedValues(ITokenString input, out InvariantHashSet allowedValues)
        {
            allowedValues = input.IsMeaningful()
                ? InvariantHashSet.Boolean()
                : null;
            return allowedValues != null;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            yield return this.GetPathExists(input).ToString();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the given file path exists.</summary>
        /// <param name="input">The relative file path.</param>
        /// <exception cref="InvalidOperationException">The path is not relative or contains directory climbing (../).</exception>
        private bool GetPathExists(ITokenString input)
        {
            if (!input.IsMeaningful() || !input.IsReady)
                return false;

            // get normalized path
            string path = PathUtilities.NormalizePathSeparators(input.Value);

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
