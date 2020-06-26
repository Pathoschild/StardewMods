using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>The parsed input arguments for a token.</summary>
    internal class InputArguments : IInputArguments
    {
        /*********
        ** Fields
        *********/
        /// <summary>The last tokenisable value that was parsed.</summary>
        private string LastParsedValue = null;

        /// <summary>The backing field for <see cref="PositionalArgs"/>.</summary>
        private string[] PositionalArgsImpl = new string[0];

        /// <summary>The backing field for <see cref="NamedArgs"/>.</summary>
        private IDictionary<string, IInputArgumentValue> NamedArgsImpl = new InvariantDictionary<IInputArgumentValue>();


        /*********
        ** Accessors
        *********/
        /// <summary>A singleton instance representing zero input arguments.</summary>
        public static IInputArguments Empty { get; } = new InputArguments(new LiteralString(string.Empty));

        /// <inheritdoc />
        public ITokenString TokenString { get; }

        /// <inheritdoc />
        public string[] PositionalArgs => this.ParseIfNeeded().PositionalArgsImpl;

        /// <inheritdoc />
        public IDictionary<string, IInputArgumentValue> NamedArgs => this.ParseIfNeeded().NamedArgsImpl;

        /// <inheritdoc />
        public bool HasNamedArgs => this.NamedArgs.Any();

        /// <inheritdoc />
        public bool HasPositionalArgs => this.PositionalArgs.Any();

        /// <inheritdoc />
        public bool IsMutable => this.TokenString.IsMutable;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tokenString">The underlying tokenised string.</param>
        public InputArguments(ITokenString tokenString)
        {
            this.TokenString = tokenString;
        }

        /// <inheritdoc />
        public string GetFirstPositionalArg()
        {
            return this.PositionalArgs.FirstOrDefault();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse the underlying token string if it's not already parsed.</summary>
        private InputArguments ParseIfNeeded()
        {
            if (this.LastParsedValue != this.TokenString?.Value)
            {
                InputArguments.Parse(this.TokenString, out this.PositionalArgsImpl, out this.NamedArgsImpl);
                this.LastParsedValue = this.TokenString?.Value;
            }

            return this;
        }

        /// <summary>Parse arguments from a tokenised string.</summary>
        /// <param name="input">The tokenised string to parse.</param>
        /// <param name="positionalArgs">The parsed positional arguments.</param>
        /// <param name="namedArgs">The parsed named arguments.</param>
        private static void Parse(ITokenString input, out string[] positionalArgs, out IDictionary<string, IInputArgumentValue> namedArgs)
        {
            InputArguments.GetRawArguments(input, out string rawPositionalArgs, out string rawNamedArgs);

            positionalArgs = rawPositionalArgs.SplitValuesNonUnique().ToArray();
            namedArgs = new InvariantDictionary<IInputArgumentValue>();

            foreach (string arg in rawNamedArgs.SplitValuesNonUnique('|'))
            {
                string[] parts = arg.Split(new[] { '=' }, 2);

                if (parts.Length == 1)
                    namedArgs[arg] = new InputArgumentValue(string.Empty, new string[0]);
                else
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    namedArgs[key] = new InputArgumentValue(value, value.SplitValuesNonUnique().ToArray());
                }
            }
        }

        /// <summary>Get the raw positional and named argument strings.</summary>
        /// <param name="input">The tokenised string to parse.</param>
        /// <param name="rawPositional">The positional arguments string.</param>
        /// <param name="rawNamed">The named argument string.</param>
        private static void GetRawArguments(ITokenString input, out string rawPositional, out string rawNamed)
        {
            string raw = input?.IsReady == true
                ? input.Value?.Trim() ?? string.Empty
                : string.Empty;

            int splitIndex = raw.IndexOf("|", StringComparison.Ordinal);
            if (splitIndex == -1)
            {
                rawPositional = raw;
                rawNamed = string.Empty;
            }
            else
            {
                string[] parts = raw.Split(new[] { '|' }, 2);
                rawPositional = parts[0].Trim();
                rawNamed = parts[1].Trim();
            }
        }
    }
}
