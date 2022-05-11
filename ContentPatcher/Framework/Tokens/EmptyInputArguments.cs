using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>An optimized empty arguments structure for a token.</summary>
    internal class EmptyInputArguments : IInputArguments
    {
        /*********
        ** Fields
        *********/
        /// <summary>An empty immutable dictionary of input argument values.</summary>
        private static readonly IDictionary<string, IInputArgumentValue> EmptyArgValues = new ReadOnlyDictionary<string, IInputArgumentValue>(new Dictionary<string, IInputArgumentValue>());


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public ITokenString? TokenString => null;

        /// <inheritdoc />
        public string[] PositionalArgs => Array.Empty<string>();

        /// <inheritdoc />
        public IDictionary<string, IInputArgumentValue> NamedArgs => EmptyInputArguments.EmptyArgValues;

        /// <inheritdoc />
        public IDictionary<string, IInputArgumentValue> ReservedArgs => EmptyInputArguments.EmptyArgValues;

        /// <inheritdoc />
        public KeyValuePair<string, IInputArgumentValue>[] ReservedArgsList => Array.Empty<KeyValuePair<string, IInputArgumentValue>>();

        /// <inheritdoc />
        public bool HasNamedArgs => false;

        /// <inheritdoc />
        public bool HasPositionalArgs => false;

        /// <inheritdoc />
        public bool IsMutable => false;

        /// <inheritdoc />
        public bool IsReady => true;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public string? GetFirstPositionalArg()
        {
            return null;
        }

        /// <inheritdoc />
        public string? GetRawArgumentValue(string key)
        {
            return null;
        }

        /// <inheritdoc />
        public string? GetPositionalSegment()
        {
            return null;
        }
    }
}
