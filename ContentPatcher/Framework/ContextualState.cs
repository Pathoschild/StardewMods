using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley.Extensions;

namespace ContentPatcher.Framework
{
    /// <summary>Diagnostic info about a contextual object.</summary>
    internal class ContextualState : IContextualState
    {
        /*********
        ** Fields
        *********/
        /// <summary>The backing field for <see cref="InvalidTokens"/>.</summary>
        private readonly MutableInvariantSet InvalidTokensImpl = [];

        /// <summary>The backing field for <see cref="UnreadyTokens"/>.</summary>
        private readonly MutableInvariantSet UnreadyTokensImpl = [];

        /// <summary>The backing field for <see cref="UnavailableModTokens"/>.</summary>
        private readonly MutableInvariantSet UnavailableModTokensImpl = [];

        /// <summary>The backing field for <see cref="Errors"/>.</summary>
        private readonly MutableInvariantSet ErrorsImpl = [];


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

        /// <summary>Add an unknown token required by the instance.</summary>
        /// <param name="token">The token name to add.</param>
        public ContextualState AddInvalidToken(string token)
        {
            this.InvalidTokensImpl.Add(token);
            return this;
        }

        /// <summary>Add a valid token required by the instance which isn't available in the current context.</summary>
        /// <param name="token">The token name to add.</param>
        public ContextualState AddUnreadyToken(string token)
        {
            this.UnreadyTokensImpl.Add(token);
            return this;
        }

        /// <summary>Add a token which is provided by a mod which isn't installed, if any.</summary>
        /// <param name="token">The token name to add.</param>
        public ContextualState AddUnavailableModToken(string token)
        {
            this.UnavailableModTokensImpl.Add(token);
            return this;
        }

        /// <summary>Add an error phrase indicating why the instance is not ready to use.</summary>
        /// <param name="error">The error phrase to add.</param>
        public ContextualState AddError(string error)
        {
            this.ErrorsImpl.Add(error);
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

            target.AddRange(source);
        }
    }
}
