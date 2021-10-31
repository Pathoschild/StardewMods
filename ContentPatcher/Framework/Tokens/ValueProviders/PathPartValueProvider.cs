using System;
using System.Collections.Generic;
using System.IO;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which extracts part of a path from the given path.</summary>
    internal class PathPartValueProvider : BaseValueProvider
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public PathPartValueProvider()
            : base(ConditionType.PathPart, mayReturnMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: true, mayReturnMultipleValues: false, maxPositionalArgs: 2);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            bool changed = !this.IsReady;
            this.MarkReady(true);
            return changed;
        }

        /// <inheritdoc />
        public override bool TryValidateInput(IInputArguments input, out string error)
        {
            if (!base.TryValidateInput(input, out error))
                return false;

            if (input.PositionalArgs.Length == 0)
                error = $"The {this.Name} token requires a path argument.";
            else if (input.PositionalArgs.Length == 1)
                error = $"The {this.Name} token requires a fragment type (one of {string.Join(", ", Enum.GetNames(typeof(PathFragment)))}).";
            else if (!Enum.TryParse(input.PositionalArgs[1], ignoreCase: true, out PathFragment _) && !int.TryParse(input.PositionalArgs[1], out _))
                error = $"Invalid path fragment type '{input.PositionalArgs[1]}'; expected a numeric index, or one of {string.Join(", ", Enum.GetNames(typeof(PathFragment)))}.";

            return error == null;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            if (!this.TryGetPart(input.PositionalArgs[0], input.PositionalArgs[1], out string part, out string error))
                throw new InvalidOperationException(error); // shouldn't happen since we check the input in TryValidateInput

            if (part != null)
                yield return part;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get part of the path if the input arguments are valid.</summary>
        /// <param name="path">The path to check.</param>
        /// <param name="rawPart">The path fragment or index position.</param>
        /// <param name="part">The extracted path part. This may be null even if valid to indicate no value (e.g. retrieved the fourth index from a two-segment path).</param>
        /// <param name="error">The error indicating why the input arguments are invalid.</param>
        private bool TryGetPart(string path, string rawPart, out string part, out string error)
        {
            error = null;

            // empty path
            if (string.IsNullOrWhiteSpace(path))
            {
                part = null;
                return true;
            }

            // fragment
            if (Enum.TryParse(rawPart, ignoreCase: true, out PathFragment fragment))
            {
                part = fragment switch
                {
                    PathFragment.DirectoryPath => Path.GetDirectoryName(path),
                    PathFragment.FileName => Path.GetFileName(path),
                    PathFragment.FilenameWithoutExtension => Path.GetFileNameWithoutExtension(path),
                    _ => null
                };

                if (part == null)
                    error = $"Invalid path fragment type '{rawPart}'; expected a numeric index, or one of {string.Join(", ", Enum.GetNames(typeof(PathFragment)))}.";

                return part != null;
            }

            // index
            if (int.TryParse(rawPart, out int index))
            {
                string[] segments = PathUtilities.GetSegments(rawPart);
                if (Math.Abs(index) >= segments.Length)
                {
                    part = null;
                    return true;
                }

                // get value at index (negative index = from end)
                part = index >= 0
                    ? segments[index]
                    : segments[segments.Length + index];

                return true;
            }

            // invalid input
            part = null;
            error = $"Invalid path fragment type '{rawPart}'; expected a numeric index, or one of {string.Join(", ", Enum.GetNames(typeof(PathFragment)))}.";
            return false;
        }
    }
}
