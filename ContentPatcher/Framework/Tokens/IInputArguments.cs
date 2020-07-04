using System;
using System.Collections.Generic;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>The parsed input arguments for a token.</summary>
    internal interface IInputArguments
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying tokenised string.</summary>
        ITokenString TokenString { get; }

        /// <summary>The positional input arguments.</summary>
        string[] PositionalArgs { get; }

        /// <summary>The named input arguments.</summary>
        IDictionary<string, IInputArgumentValue> NamedArgs { get; }

        /// <summary>The named input arguments handled by Content Patcher. Tokens should generally ignore these.</summary>
        IDictionary<string, IInputArgumentValue> ReservedArgs { get; }

        /// <summary>Whether any named arguments were provided.</summary>
        bool HasNamedArgs { get; }

        /// <summary>Whether any positional arguments were provided.</summary>
        bool HasPositionalArgs { get; }

        /// <summary>Whether the input arguments contain tokens that may change depending on the context.</summary>
        bool IsMutable { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the first positional argument value, if any.</summary>
        string GetFirstPositionalArg();
    }
}
