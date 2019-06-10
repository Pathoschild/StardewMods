using System;
using System.Collections.Generic;
using System.IO;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common;

namespace ContentPatcher.Framework
{
    /// <summary>A context which provides tokens specific to a single patch.</summary>
    internal class SinglePatchContext : IContext
    {
        /*********
        ** Fields
        *********/
        /// <summary>The parent context that provides non-patch-specific tokens.</summary>
        private IContext LastParentContext;

        /// <summary>The token instance for the <see cref="ConditionType.Target"/> token.</summary>
        private readonly DynamicToken TargetToken;

        /// <summary>The token instance for the <see cref="ConditionType.TargetWithoutPath"/> token.</summary>
        private readonly DynamicToken TargetWithoutPathToken;

        /// <summary>The custom tokens provided by this instance.</summary>
        private readonly IToken[] CustomTokens;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="scope">The mod namespace in which the token is accessible.</param>
        /// <param name="parentContext">The initial parent context that provides non-patch-specific tokens, if any.</param>
        public SinglePatchContext(string scope, IContext parentContext = null)
        {
            this.LastParentContext = parentContext;
            this.CustomTokens = new IToken[]
            {
                this.TargetToken = new DynamicToken(ConditionType.Target.ToString(), scope),
                this.TargetWithoutPathToken = new DynamicToken(ConditionType.TargetWithoutPath.ToString(), scope)
            };
        }

        /****
        ** IContext
        ****/
        /// <summary>Update the patch context.</summary>
        /// <param name="parentContext">The parent context that provides non-patch-specific tokens.</param>
        /// <param name="targetName">The asset name intercepted by the patch.</param>
        public void Update(IContext parentContext, ITokenString targetName)
        {
            this.LastParentContext = parentContext;

            if (targetName.IsReady)
            {
                string path = PathUtilities.NormalisePathSeparators(targetName.Value);

                this.TargetToken.SetReady(true);
                this.TargetToken.SetValue(new LiteralString(path));

                this.TargetWithoutPathToken.SetReady(true);
                this.TargetWithoutPathToken.SetValue(new LiteralString(Path.GetFileName(path)));
            }
            else
            {
                this.TargetToken.SetReady(false);
                this.TargetWithoutPathToken.SetReady(false);
            }
        }

        /// <summary>Get whether the context contains the given token.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public bool Contains(string name, bool enforceContext)
        {
            return this.GetToken(name, enforceContext) != null;
        }

        /// <summary>Get the underlying token which handles a key.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        public IToken GetToken(string name, bool enforceContext)
        {
            foreach (IToken token in this.CustomTokens)
            {
                if (token.Name.EqualsIgnoreCase(name))
                    return token;
            }

            return this.LastParentContext?.GetToken(name, enforceContext);
        }

        /// <summary>Get the underlying tokens.</summary>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public IEnumerable<IToken> GetTokens(bool enforceContext)
        {
            foreach (IToken token in this.CustomTokens)
                yield return token;

            if (this.LastParentContext != null)
            {
                foreach (IToken token in this.LastParentContext.GetTokens(enforceContext))
                    yield return token;
            }
        }

        /// <summary>Get the current values of the given token for comparison.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="input">The input argument, if any.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Return the values of the matching token, or an empty list if the token doesn't exist.</returns>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        public IEnumerable<string> GetValues(string name, ITokenString input, bool enforceContext)
        {
            IToken token = this.GetToken(name, enforceContext);
            return token?.GetValues(input) ?? new string[0];
        }
    }
}
