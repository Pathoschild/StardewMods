using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token which check whether a file exists in the content pack's folder.</summary>
    internal class HasFileToken : BaseToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod folder from which to load assets.</summary>
        private readonly string ModFolder;

        /// <summary>The context for this token instance.</summary>
        private IContext TokenContext;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modFolder">The absolute path to the mod folder.</param>
        public HasFileToken(string modFolder)
            : base(ConditionType.HasFile.ToString(), canHaveMultipleRootValues: false)
        {
            this.ModFolder = modFolder;
            this.EnableSubkeys(required: true, canHaveMultipleValues: false);
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.TokenContext = context;
            this.IsValidInContext = true;
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override IEnumerable<string> GetValues(TokenName name)
        {
            this.AssertTokenName(name);

            yield return this.GetPathExists(name.Subkey).ToString();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the given file path exists.</summary>
        /// <param name="path">A relative file path.</param>
        /// <exception cref="InvalidOperationException">The path is not relative or contains directory climbing (../).</exception>
        private bool GetPathExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // parse tokens
            TokenString tokenStr = new TokenString(path, this.TokenContext);
            if (tokenStr.InvalidTokens.Any())
                return false;
            tokenStr.UpdateContext(this.TokenContext);
            path = tokenStr.Value;

            // get normalised path
            if (string.IsNullOrWhiteSpace(path))
                return false;
            path = PathUtilities.NormalisePathSeparators(path);

            // validate
            if (Path.IsPathRooted(path))
                throw new InvalidOperationException($"The {ConditionType.HasFile} token requires a relative path.");
            if (!PathUtilities.IsSafeRelativePath(path))
                throw new InvalidOperationException($"The {ConditionType.HasFile} token requires a relative path and cannot contain directory climbing (../).");

            // check file existence
            string fullPath = Path.Combine(this.ModFolder, PathUtilities.NormalisePathSeparators(path));
            return File.Exists(fullPath);
        }
    }
}
