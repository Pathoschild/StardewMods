using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations
{
    /// <summary>The base implementation for a text operation.</summary>
    internal interface ITextOperation : IContextual
    {
        /*********
        ** Properties
        *********/
        /// <summary>The text operation to perform.</summary>
        TextOperationType Operation { get; }

        /// <summary>The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</summary>
        ITokenString[] Target { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Get the first entry in the <see cref="Target"/> as an enum value.</summary>
        TextOperationTargetRoot? GetTargetRoot();

        /// <summary>Get a copy of the input with the text operation applied.</summary>
        /// <param name="text">The input to modify.</param>
        string Apply(string? text);
    }
}
