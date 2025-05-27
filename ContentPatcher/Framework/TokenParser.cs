using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework;

/// <summary>Handles low-level parsing and validation for tokens.</summary>
internal class TokenParser
{
    /*********
    ** Fields
    *********/
    /// <summary>Handles parsing raw strings into tokens.</summary>
    private readonly Lexer Lexer = Lexer.Instance;

    /// <summary>The private <c>_name</c> field on <see cref="JProperty"/> instances, used to update tokenized property names.</summary>
    private static readonly FieldInfo PropertyNameField;


    /*********
    ** Accessors
    *********/
    /// <summary>The manifest for the content pack being parsed.</summary>
    public IManifest ForMod { get; }

    /// <summary>The tokens available for this content pack.</summary>
    public IContext Context { get; }

    /// <summary>The migrator which validates and migrates content pack data.</summary>
    public IMigration Migrator { get; }

    /// <summary>The mod IDs which are currently installed.</summary>
    public IInvariantSet InstalledMods { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Initialize static fields.</summary>
    static TokenParser()
    {
        TokenParser.PropertyNameField =
            typeof(JProperty).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Failed to initialize token parser: required field '{nameof(JProperty)}._name' not found");
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="tokenContext">The tokens available for this content pack.</param>
    /// <param name="forMod">The manifest for the content pack being parsed.</param>
    /// <param name="migrator">The migrator which validates and migrates content pack data.</param>
    /// <param name="installedMods">The mod IDs which are currently installed.</param>
    public TokenParser(IContext tokenContext, IManifest forMod, IMigration migrator, IInvariantSet installedMods)
    {
        this.Context = tokenContext;
        this.ForMod = forMod;
        this.Migrator = migrator;
        this.InstalledMods = installedMods;
    }

    /// <summary>Create a token string instance for the given values.</summary>
    /// <param name="lexTokens">The lexical tokens parsed from the raw string.</param>
    /// <param name="context">The available token context.</param>
    /// <param name="path">The path to the value from the root content file.</param>
    public IManagedTokenString CreateTokenString(ILexToken[] lexTokens, IContext context, LogPathBuilder path)
    {
        return this.CreateTokenStringOrNull(lexTokens, context, path)
            ?? new LiteralString(string.Empty, path);
    }

    /// <summary>Create a token string instance for the given values, or null if the string is empty.</summary>
    /// <param name="lexTokens">The lexical tokens parsed from the raw string.</param>
    /// <param name="context">The available token context.</param>
    /// <param name="path">The path to the value from the root content file.</param>
    public IManagedTokenString? CreateTokenStringOrNull(ILexToken[]? lexTokens, IContext context, LogPathBuilder path)
    {
        if (lexTokens?.Length is null or 0)
            return null;

        if (lexTokens is [LexTokenLiteral literal])
        {
            return string.IsNullOrWhiteSpace(literal.Text)
                ? null
                : new LiteralString(literal.Text, path);
        }

        return new TokenString(lexTokens, context, path);
    }

    /// <summary>Create a token string instance for the given values, or null if the string is empty.</summary>
    /// <param name="inputStr">The token string for the input argument.</param>
    public IInputArguments CreateInputArgs(ITokenString? inputStr)
    {
        return inputStr is not null
            ? new InputArguments(inputStr)
            : InputArguments.Empty;
    }

    /// <summary>Parse a string which can contain tokens or be null, and validate that it's valid.</summary>
    /// <param name="rawValue">The raw string which may contain tokens.</param>
    /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
    /// <param name="path">The path to the value from the root content file.</param>
    /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
    /// <param name="parsed">The parsed value.</param>
    /// <remarks><see cref="TryParseString"/> and <see cref="TryParseNullableString"/> are identical except in their handling of a null value; the former will return an empty tokenized string while the latter will return null.</remarks>
    public bool TryParseNullableString(string? rawValue, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, out IManagedTokenString? parsed)
    {
        if (rawValue is null)
        {
            error = null;
            parsed = null;
            return true;
        }

        return this.TryParseString(rawValue, assumeModIds, path, out error, out parsed);
    }

    /// <summary>Parse a string which can contain tokens, and validate that it's valid.</summary>
    /// <param name="rawValue">The raw string which may contain tokens.</param>
    /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
    /// <param name="path">The path to the value from the root content file.</param>
    /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
    /// <param name="parsed">The parsed value.</param>
    /// <param name="preValidate">Validate a token used in the string after it's been parsed, but before the normal validation is applied. This can return an error string to fail validation, or <c>null</c> to continue with the default validation.</param>
    /// <inheritdoc cref="TryParseNullableString" path="/remarks" />
    public bool TryParseString(string? rawValue, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out IManagedTokenString? parsed, Func<LexTokenToken, string?>? preValidate = null)
    {
        if (!string.IsNullOrEmpty(rawValue))
        {
            // parse lexical bits
            ILexToken[] bits = this.Lexer.ParseBits(rawValue, impliedBraces: false).ToArray();
            for (int i = 0; i < bits.Length; i++)
            {
                if (!this.Migrator.TryMigrate(ref bits[i], out error))
                {
                    parsed = null;
                    return false;
                }
            }

            // get token string
            parsed = this.CreateTokenString(bits, this.Context, path);
            if (!this.Migrator.TryMigrate(parsed, out error))
                return false;

            // validate tokens
            foreach (LexTokenToken lexToken in parsed.GetTokenPlaceholders(recursive: false))
            {
                error = preValidate?.Invoke(lexToken);
                if (error != null)
                    return false;

                if (!this.TryValidateToken(lexToken, assumeModIds, out error))
                    return false;
            }
        }
        else
            parsed = new LiteralString(string.Empty, path);

        // looks OK
        error = null;
        return true;
    }

    /// <summary>Parse a JSON structure which can contain tokens, and validate that it's valid.</summary>
    /// <param name="rawJson">The raw JSON structure which may contain tokens.</param>
    /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
    /// <param name="path">The path to the value from the root content file.</param>
    /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
    /// <param name="parsed">The parsed value, which may be legitimately <c>null</c> even if successful.</param>
    public bool TryParseJson(JToken? rawJson, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, out TokenizableJToken? parsed)
    {
        if (rawJson == null || rawJson.Type == JTokenType.Null || rawJson.Type == JTokenType.Comment)
        {
            error = null;
            parsed = null;
            return true;
        }

        // extract mutable fields
        if (!this.TryInjectJsonProxyFields(rawJson, assumeModIds, path, out error, out TokenizableProxy[]? proxyFields))
        {
            parsed = null;
            return false;
        }

        // build tokenizable structure
        parsed = new TokenizableJToken(rawJson, proxyFields);
        if (!this.Migrator.TryMigrate(parsed, out error))
            return false;

        // validate tokens
        foreach (LexTokenToken lexToken in parsed.GetTokenStrings().SelectMany(p => p.GetTokenPlaceholders(recursive: false)))
        {
            if (!this.TryValidateToken(lexToken, assumeModIds, out error))
                return false;
        }

        // looks OK
        if (parsed.Value.Type == JTokenType.Null)
            parsed = null;
        error = null;
        return true;
    }

    /// <summary>Validate whether a token referenced in a content pack field is valid.</summary>
    /// <param name="lexToken">The lexical token reference to check.</param>
    /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
    /// <param name="error">An error phrase indicating why validation failed (if applicable).</param>
    public bool TryValidateToken(LexTokenToken lexToken, IInvariantSet assumeModIds, [NotNullWhen(false)] out string? error)
    {
        // token doesn't exist
        IToken? token = this.Context.GetToken(lexToken.Name, enforceContext: false);
        if (token == null)
        {
            // special case: requires a missing mod that's checked via HasMod
            string? requiredModId = lexToken.GetProviderModId();
            if (requiredModId != null && assumeModIds.Contains(requiredModId) && !this.InstalledMods.Contains(requiredModId))
            {
                error = null;
                return true;
            }

            // else error
            error = $"'{lexToken}' can't be used as a token because that token could not be found.";
            return false;
        }

        // token requires dependency
        if (token is ModProvidedToken modToken && !this.ForMod.HasDependency(modToken.Mod.UniqueID, canBeOptional: false))
        {
            if (!assumeModIds.Contains(modToken.Mod.UniqueID))
            {
                error = $"'{lexToken}' can't be used because it's provided by {modToken.Mod.Name} (ID: {modToken.Mod.UniqueID}), but {this.ForMod.Name} doesn't list it as a dependency and the patch doesn't have an immutable {ConditionType.HasMod} condition for that mod.";
                return false;
            }
        }

        // no error found
        error = null;
        return true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Find all tokens in a JSON structure and inject proxy fields which can be updated to change the structure.</summary>
    /// <param name="token">The JSON structure to modify.</param>
    /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
    /// <param name="path">The path to the value from the root content file.</param>
    /// <param name="error">An error phrase indicating why validation failed (if applicable).</param>
    /// <param name="proxyFields">The injected proxy fields, if any.</param>
    /// <returns>Returns whether the JSON structure was successfully modified, regardless of whether any proxy fields are needed.</returns>
    private bool TryInjectJsonProxyFields(JToken token, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out TokenizableProxy[]? proxyFields)
    {
        proxyFields = null;

        List<TokenizableProxy?> fields = [];
        switch (token)
        {
            case JValue valueToken:
                if (valueToken.Type != JTokenType.Comment)
                {
                    string? value = valueToken.Value<string?>();
                    if (!this.TryInjectJsonProxyField(value, assumeModIds, val => valueToken.Value = val, path, out error, out TokenizableProxy? proxy))
                        return false;

                    fields.Add(proxy);
                }
                break;

            case JObject objToken:
                foreach (JProperty property in objToken.Properties().ToArray())
                {
                    LogPathBuilder localPath = path.With(property.Name);

                    // resolve property name
                    if (!this.TryInjectJsonProxyField(property.Name, assumeModIds, val => TokenParser.PropertyNameField.SetValue(property, val), localPath.With("key"), out error, out TokenizableProxy? proxyName))
                        return false;
                    fields.Add(proxyName);

                    // resolve property values
                    if (!this.TryInjectJsonProxyFields(property.Value, assumeModIds, localPath.With("value"), out error, out TokenizableProxy[]? proxyValues))
                        return false;
                    fields.AddRange(proxyValues);
                }
                break;

            case JArray arrToken:
                {
                    int i = 0;
                    foreach (JToken valueToken in arrToken)
                    {
                        if (!this.TryInjectJsonProxyFields(valueToken, assumeModIds, path.With(i++.ToString()), out error, out TokenizableProxy[]? proxyValues))
                            return false;
                        fields.AddRange(proxyValues);
                    }
                }
                break;

            default:
                throw new InvalidOperationException($"Unknown JSON token: {token.GetType().FullName} ({token.Type})");
        }

        proxyFields = fields.WhereNotNull().ToArray();
        error = null;
        return true;
    }

    /// <summary>Resolve tokens in a string field, replace immutable tokens with their values, and get mutable tokens.</summary>
    /// <param name="str">The string to scan.</param>
    /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
    /// <param name="setValue">Update the source with a new value.</param>
    /// <param name="path">The path to the value from the root content file.</param>
    /// <param name="error">An error phrase indicating why validation failed (if applicable).</param>
    /// <param name="parsed">The parsed value if needed, or <c>null</c> if the string does not contain any tokens.</param>
    private bool TryInjectJsonProxyField(string? str, IInvariantSet assumeModIds, Action<string> setValue, LogPathBuilder path, [NotNullWhen(false)] out string? error, out TokenizableProxy? parsed)
    {
        // empty values can't contain tokens
        if (string.IsNullOrWhiteSpace(str))
        {
            parsed = null;
            error = null;
            return true;
        }

        // parse string
        if (!this.TryParseString(str, assumeModIds, path, out error, out IManagedTokenString? tokenStr))
        {
            parsed = null;
            return false;
        }

        // handle mutable token
        if (tokenStr.IsMutable)
        {
            parsed = new TokenizableProxy(tokenStr, setValue);
            return true;
        }

        // substitute immutable value
        if (tokenStr.Value != str)
            setValue(tokenStr.Value!);
        parsed = null;
        return true;
    }
}
