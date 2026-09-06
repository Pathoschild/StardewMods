using System.Collections.Generic;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations;

/// <summary>A text operation which parses a field's current value as a delimited list of values, and removes those matching a search value.</summary>
internal class RegexTextOperation : BaseTextOperation
{
    /*********
    ** Accessors
    *********/
    /// <summary>The value to append or prepend.</summary>
    public ITokenString Value { get; }

    /// <summary>The search expression to match.</summary>
    public ITokenString Search { get; }

    /*********
    ** Protected methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="search">The regex to match.</param>
    /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
    public RegexTextOperation(ICollection<IManagedTokenString> target, IManagedTokenString value, IManagedTokenString search) : base(TextOperationType.Regex, target)
    {
        this.Value = value;
        this.Search = search;
    }

    /// <inheritdoc />
    public override string? Apply(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        string? search = this.Search.Value;
        if (string.IsNullOrEmpty(search))
            return text;

        string value = this.Value.Value ?? "";

        return Regex.Replace(text, search, value);
    }
}
