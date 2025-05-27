using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Lexing;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens;

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

    /// <summary>The 'valueAt' argument key.</summary>
    internal const string ValueAtKey = "valueAt";

    /// <summary>The 'inputSeparator' argument key.</summary>
    internal const string InputSeparatorKey = "inputSeparator";

    /// <summary>The argument names handled by Content Patcher.</summary>
    private static readonly IInvariantSet ReservedArgKeys = InvariantSets.From([
        InputArguments.ContainsKey,
        InputArguments.ValueAtKey,
        InputArguments.InputSeparatorKey
    ]);

    /****
    ** State
    ****/
    /// <summary>The last raw value that was parsed.</summary>
    private string? LastRawValue;

    /// <summary>The last tokenizable value that was parsed.</summary>
    private string? LastParsedValue;

    /// <summary>The raw input argument segment containing positional arguments, after parsing tokens but before splitting into individual arguments.</summary>
    private string? PositionalSegment;

    /// <summary>The backing field for <see cref="PositionalArgs"/>.</summary>
    private string[] PositionalArgsImpl = [];

    /// <summary>The backing field for <see cref="NamedArgs"/>.</summary>
    private InvariantDictionary<IInputArgumentValue> NamedArgsImpl = new();

    /// <summary>The backing field for <see cref="ReservedArgs"/>.</summary>
    private InvariantDictionary<IInputArgumentValue> ReservedArgsImpl = new();

    /// <summary>The backing field for <see cref="ReservedArgsList"/>.</summary>
    private KeyValuePair<string, IInputArgumentValue>[] ReservedArgsListImpl = [];


    /*********
    ** Accessors
    *********/
    /// <summary>A singleton instance representing zero input arguments.</summary>
    public static IInputArguments Empty { get; } = new EmptyInputArguments();

    /****
    ** Values
    ****/
    /// <inheritdoc />
    public ITokenString? TokenString { get; }

    /// <inheritdoc />
    public string[] PositionalArgs => this.ParseIfNeeded().PositionalArgsImpl;

    /// <inheritdoc />
    public IDictionary<string, IInputArgumentValue> NamedArgs => this.ParseIfNeeded().NamedArgsImpl;

    /// <inheritdoc />
    public IDictionary<string, IInputArgumentValue> ReservedArgs => this.ParseIfNeeded().ReservedArgsImpl;

    /// <inheritdoc />
    public KeyValuePair<string, IInputArgumentValue>[] ReservedArgsList => this.ParseIfNeeded().ReservedArgsListImpl;

    /****
    ** Metadata
    ****/
    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(InputArguments.TokenString))]
    public bool HasNamedArgs => this.NamedArgs.Any();

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(InputArguments.TokenString))]
    public bool HasPositionalArgs => this.PositionalArgs.Any();

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(InputArguments.TokenString))]
    public bool IsMutable => this.TokenString?.IsMutable ?? false;

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(InputArguments.TokenString))]
    public bool IsReady => this.TokenString?.IsReady ?? false;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="tokenString">The underlying tokenized string.</param>
    public InputArguments(ITokenString tokenString)
    {
        this.TokenString = tokenString;
    }

    /// <inheritdoc />
    public string? GetFirstPositionalArg()
    {
        return this.PositionalArgs.FirstOrDefault();
    }

    /// <summary>Get the raw value for a named argument, if any.</summary>
    /// <param name="key">The argument name.</param>
    public string? GetRawArgumentValue(string key)
    {
        return this.NamedArgs.TryGetValue(key, out IInputArgumentValue? value)
            ? value.Raw
            : null;
    }

    /// <inheritdoc />
    public string? GetPositionalSegment()
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
            InputArguments.Parse(this.TokenString, out this.PositionalSegment, out this.PositionalArgsImpl, out this.NamedArgsImpl, out this.ReservedArgsImpl, out var reservedArgsList);
            this.ReservedArgsListImpl = reservedArgsList.ToArray();
            this.LastParsedValue = this.TokenString?.Value;
            this.LastRawValue = this.TokenString?.Raw;
        }

        return this;
    }

    /// <summary>Parse arguments from a tokenized string.</summary>
    /// <param name="input">The tokenized string to parse.</param>
    /// <param name="positionalSegment">The raw input argument segment containing positional arguments, after parsing tokens but before splitting into individual arguments.</param>
    /// <param name="positionalArgs">The positional arguments.</param>
    /// <param name="namedArgs">The named arguments.</param>
    /// <param name="reservedArgs">The named arguments handled by Content Patcher.</param>
    /// <param name="reservedArgsList">An ordered list of the <paramref name="reservedArgs"/>, including duplicate args.</param>
    private static void Parse(ITokenString? input, out string positionalSegment, out string[] positionalArgs, out InvariantDictionary<IInputArgumentValue> namedArgs, out InvariantDictionary<IInputArgumentValue> reservedArgs, out List<KeyValuePair<string, IInputArgumentValue>> reservedArgsList)
    {
        Lexer lexer = Lexer.Instance;
        InputArguments.GetRawArguments(input, lexer, out positionalSegment, out InvariantDictionary<string> rawNamedArgs);

        // get value separator
        if (!rawNamedArgs.TryGetValue(InputArguments.InputSeparatorKey, out string? inputSeparator) || string.IsNullOrWhiteSpace(inputSeparator))
            inputSeparator = ",";

        // parse args
        positionalArgs = lexer.SplitLexically(positionalSegment, delimiter: inputSeparator).ToArray();
        namedArgs = new InvariantDictionary<IInputArgumentValue>();
        reservedArgs = new InvariantDictionary<IInputArgumentValue>();
        reservedArgsList = [];
        foreach ((string key, string value) in rawNamedArgs)
        {
            var values = new InputArgumentValue(value, lexer.SplitLexically(value, delimiter: inputSeparator).ToArray());

            if (InputArguments.ReservedArgKeys.Contains(key))
            {
                reservedArgs[key] = values;
                reservedArgsList.Add(new KeyValuePair<string, IInputArgumentValue>(key, values));
            }
            else
                namedArgs[key] = values;
        }
    }

    /// <summary>Get the raw positional and named argument strings.</summary>
    /// <param name="input">The tokenized string to parse.</param>
    /// <param name="lexer">The lexer with which to tokenize the string.</param>
    /// <param name="rawPositional">The positional arguments string.</param>
    /// <param name="rawNamed">The named argument string.</param>
    private static void GetRawArguments(ITokenString? input, Lexer lexer, out string rawPositional, out InvariantDictionary<string> rawNamed)
    {
        // get token text
        string? raw = input?.IsReady == true
            ? input.Value
            : input?.Raw;
        raw = raw?.Trim() ?? string.Empty;

        // split into positional and named segments
        string positionalSegment;
        string[] namedSegments;
        {
            string[] parts = lexer.SplitLexically(raw, delimiter: "|", ignoreEmpty: false, trim: false).ToArray();
            positionalSegment = parts[0].Trim();
            namedSegments = parts.Skip(1).Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
        }

        // extract raw arguments
        rawPositional = positionalSegment;
        rawNamed = new InvariantDictionary<string>();
        foreach (string arg in namedSegments)
        {
            string[] parts = lexer.SplitLexically(arg, delimiter: "=", ignoreEmpty: true, trim: false).ToArray();

            if (parts.Length == 1)
                rawNamed[arg] = string.Empty;
            else
            {
                string key = parts[0].Trim();
                string value = string.Join("=", parts.Skip(1)).Trim();
                rawNamed[key] = value;
            }
        }
    }
}
