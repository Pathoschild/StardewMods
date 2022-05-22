using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>Diagnostic info about a contextual object.</summary>
    internal class ContextualState : IContextualState
    {
        /*********
        ** Fields
        *********/
        /// <summary>The backing field for <see cref="InvalidTokens"/>.</summary>
        private readonly MutableInvariantSet InvalidTokensImpl = new();

        /// <summary>The backing field for <see cref="UnreadyTokens"/>.</summary>
        private readonly MutableInvariantSet UnreadyTokensImpl = new();

        /// <summary>The backing field for <see cref="UnavailableModTokens"/>.</summary>
        private readonly MutableInvariantSet UnavailableModTokensImpl = new();

        /// <summary>The backing field for <see cref="Errors"/>.</summary>
        private readonly MutableInvariantSet ErrorsImpl = new();


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public bool IsValid => !this.InvalidTokens.Any() && !this.UnavailableModTokens.Any();

        /// <inheritdoc />
        public bool IsInScope => this.IsValid && !this.UnreadyTokens.Any();

        /// <inheritdoc />
        public bool IsReady => this.IsInScope && !this.Errors.Any();

        /// <inheritdoc />
        public IInvariantSet InvalidTokens => this.InvalidTokensImpl.GetImmutable();

        /// <inheritdoc />
        public IInvariantSet UnreadyTokens => this.UnreadyTokensImpl.GetImmutable();

        /// <inheritdoc />
        public IInvariantSet UnavailableModTokens => this.UnavailableModTokensImpl.GetImmutable();

        /// <inheritdoc />
        public IInvariantSet Errors => this.ErrorsImpl.GetImmutable();


        /*********
        ** Public methods
        *********/
        /// <summary>Mark the instance valid.</summary>
        public ContextualState Reset()
        {
            this.InvalidTokensImpl.Clear();
            this.UnreadyTokensImpl.Clear();
            this.ErrorsImpl.Clear();
            return this;
        }

        /// <summary>Create a deep clone of the instance.</summary>
        public ContextualState Clone()
        {
            return new ContextualState().MergeFrom(this);
        }

        /// <summary>Merge the data from another instance into this instance.</summary>
        /// <param name="other">The other contextual state to copy.</param>
        public ContextualState MergeFrom(IContextualState? other)
        {
            if (other is ContextualState otherState)
            {
                // avoid creating immutable copies unnecessarily
                this.AddRange(this.InvalidTokensImpl, otherState.InvalidTokensImpl);
                this.AddRange(this.UnreadyTokensImpl, otherState.UnreadyTokensImpl);
                this.AddRange(this.ErrorsImpl, otherState.ErrorsImpl);
            }
            else if (other != null)
            {
                this.AddRange(this.InvalidTokensImpl, other.InvalidTokens);
                this.AddRange(this.UnreadyTokensImpl, other.UnreadyTokens);
                this.AddRange(this.ErrorsImpl, other.Errors);
            }
            return this;
        }

        /// <summary>Add unknown tokens required by the instance.</summary>
        /// <param name="tokens">The tokens to add.</param>
        public ContextualState AddInvalidTokens(params string[]? tokens)
        {
            this.AddRange(this.InvalidTokensImpl, tokens);
            return this;
        }

        /// <summary>Add valid tokens required by the instance which aren't available in the current context.</summary>
        /// <param name="tokens">The tokens to add.</param>
        public ContextualState AddUnreadyTokens(params string[]? tokens)
        {
            this.AddRange(this.UnreadyTokensImpl, tokens);
            return this;
        }

        /// <summary>Add tokens which are provided by a mod which isn't installed, if any.</summary>
        /// <param name="tokens">The tokens to add.</param>
        public ContextualState AddUnavailableModTokens(params string[]? tokens)
        {
            this.AddRange(this.UnavailableModTokensImpl, tokens);
            return this;
        }

        /// <summary>Add error phrases indicating why the instance is not ready to use.</summary>
        /// <param name="errors">The tokens to add.</param>
        public ContextualState AddErrors(params string[]? errors)
        {
            this.AddRange(this.ErrorsImpl, errors);
            return this;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Add a range of values to a target set.</summary>
        /// <param name="target">The set to update.</param>
        /// <param name="source">The values to add.</param>
        private void AddRange(MutableInvariantSet target, IEnumerable<string>? source)
        {
            if (source == null)
                return;

            target.AddMany(source);
        }
    }
}
