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
        /****
        ** Constants
        ****/
        /// <summary>The 'contains' argument key.</summary>
        internal const string ContainsKey = "contains";

        /// <summary>The 'inputSeparator' argument key.</summary>
        internal const string InputSeparatorKey = "inputSeparator";

        /// <summary>The argument names handled by Content Patcher.</summary>
        private static readonly ISet<string> ReservedArgKeys = new InvariantHashSet
        {
            InputArguments.ContainsKey,
            InputArguments.InputSeparatorKey
        };

        /****
        ** State
        ****/
        /// <summary>The last raw value that was parsed.</summary>
        private string LastRawValue = null;

        /// <summary>The last tokenisable value that was parsed.</summary>
        private string LastParsedValue = null;

        /// <summary>The raw input argument segment containing positional arguments, after parsing tokens but before splitting into individual arguments.</summary>
        private string PositionalSegment;

        /// <summary>The backing field for <see cref="PositionalArgs"/>.</summary>
        private string[] PositionalArgsImpl = new string[0];

        /// <summary>The backing field for <see cref="NamedArgs"/>.</summary>
        private IDictionary<string, IInputArgumentValue> NamedArgsImpl = new InvariantDictionary<IInputArgumentValue>();

        /// <summary>The backing field for <see cref="ReservedArgs"/>.</summary>
        private IDictionary<string, IInputArgumentValue> ReservedArgsImpl = new InvariantDictionary<IInputArgumentValue>();


        /*********
        ** Accessors
        *********/
        /// <summary>A singleton instance representing zero input arguments.</summary>
        public static IInputArguments Empty { get; } = new InputArguments(new LiteralString(string.Empty, new LogPathBuilder()));

        /****
        ** Values
        ****/
        /// <inheritdoc />
        public ITokenString TokenString { get; }

        /// <inheritdoc />
        public string[] PositionalArgs => this.ParseIfNeeded().PositionalArgsImpl;

        /// <inheritdoc />
        public IDictionary<string, IInputArgumentValue> NamedArgs => this.ParseIfNeeded().NamedArgsImpl;

        /// <inheritdoc />
        public IDictionary<string, IInputArgumentValue> ReservedArgs => this.ParseIfNeeded().ReservedArgsImpl;

        /****
        ** Metadata
        ****/
        /// <inheritdoc />
        public bool HasNamedArgs => this.NamedArgs.Any();

        /// <inheritdoc />
        public bool HasPositionalArgs => this.PositionalArgs.Any();

        /// <inheritdoc />
        public bool IsMutable => this.TokenString?.IsMutable ?? false;

        /// <inheritdoc />
        public bool IsReady => this.TokenString?.IsReady ?? false;


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

        /// <summary>Get the raw value for a named argument, if any.</summary>
        /// <param name="key">The argument name.</param>
        public string GetRawArgumentValue(string key)
        {
            return this.NamedArgs.TryGetValue(key, out IInputArgumentValue value)
                ? value.Raw
                : null;
        }

        /// <inheritdoc />
        public string GetPositionalSegment()
        {
            return this.PositionalSegment;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse the underlying token string if it's not already parsed.</summary>
        private InputArguments ParseIfNeeded()
        {
            if (this.LastParsedValue != this.TokenString?.Value || this.LastRawValue != this.TokenString?.Raw)
            {
                InputArguments.Parse(this.TokenString, out this.PositionalSegment, out this.PositionalArgsImpl, out this.NamedArgsImpl, out this.ReservedArgsImpl);
                this.LastParsedValue = this.TokenString?.Value;
                this.LastRawValue = this.TokenString?.Raw;
            }

            return this;
        }

        /// <summary>Parse arguments from a tokenised string.</summary>
        /// <param name="input">The tokenised string to parse.</param>
        /// <param name="positionalSegment">The raw input argument segment containing positional arguments, after parsing tokens but before splitting into individual arguments.</param>
        /// <param name="positionalArgs">The positional arguments.</param>
        /// <param name="namedArgs">The named arguments.</param>
        /// <param name="reservedArgs">The named arguments handled by Content Patcher.</param>
        private static void Parse(ITokenString input, out string positionalSegment, out string[] positionalArgs, out IDictionary<string, IInputArgumentValue> namedArgs, out IDictionary<string, IInputArgumentValue> reservedArgs)
        {
            InputArguments.GetRawArguments(input, out positionalSegment, out InvariantDictionary<string> rawNamedArgs);

            // get value separator
            if (!rawNamedArgs.TryGetValue(InputArguments.InputSeparatorKey, out string inputSeparator) || string.IsNullOrWhiteSpace(inputSeparator))
                inputSeparator = ",";

            // parse arguments
            positionalArgs = positionalSegment.SplitValuesNonUnique(inputSeparator).ToArray();
            namedArgs = new InvariantDictionary<IInputArgumentValue>();
            reservedArgs = new InvariantDictionary<IInputArgumentValue>();
            foreach (var arg in rawNamedArgs)
            {
                var values = new InputArgumentValue(arg.Value, arg.Value.SplitValuesNonUnique(inputSeparator).ToArray());

                if (InputArguments.ReservedArgKeys.Contains(arg.Key))
                    reservedArgs[arg.Key] = values;
                else
                    namedArgs[arg.Key] = values;
            }
        }

        /// <summary>Get the raw positional and named argument strings.</summary>
        /// <param name="input">The tokenised string to parse.</param>
        /// <param name="rawPositional">The positional arguments string.</param>
        /// <param name="rawNamed">The named argument string.</param>
        private static void GetRawArguments(ITokenString input, out string rawPositional, out InvariantDictionary<string> rawNamed)
        {
            // get token text
            string raw = input?.IsReady == true
                ? input.Value
                : input?.Raw;
            raw = raw?.Trim() ?? string.Empty;

            // split into positional and named segments
            string positionalSegment;
            string namedSegment;
            {
                int splitIndex = raw.IndexOf("|", StringComparison.Ordinal);
                if (splitIndex == -1)
                {
                    positionalSegment = raw;
                    namedSegment = string.Empty;
                }
                else
                {
                    string[] parts = raw.Split(new[] { '|' }, 2);
                    positionalSegment = parts[0].Trim();
                    namedSegment = parts[1].Trim();
                }
            }

            // extract raw arguments
            rawPositional = positionalSegment;
            rawNamed = new InvariantDictionary<string>();
            foreach (string arg in namedSegment.SplitValuesNonUnique("|"))
            {
                string[] parts = arg.Split(new[] { '=' }, 2);

                if (parts.Length == 1)
                    rawNamed[arg] = string.Empty;
                else
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    rawNamed[key] = value;
                }
            }
        }
    }
}
