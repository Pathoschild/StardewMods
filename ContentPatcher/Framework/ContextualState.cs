using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>Diagnostic info about a contextual object.</summary>
    internal class ContextualState : IContextualState
    {
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
        public InvariantHashSet InvalidTokens { get; } = new InvariantHashSet();

        /// <inheritdoc />
        public InvariantHashSet UnreadyTokens { get; } = new InvariantHashSet();

        /// <inheritdoc />
        public InvariantHashSet UnavailableModTokens { get; } = new InvariantHashSet();

        /// <inheritdoc />
        public InvariantHashSet Errors { get; } = new InvariantHashSet();


        /*********
        ** Public methods
        *********/
        /// <summary>Mark the instance valid.</summary>
        public ContextualState Reset()
        {
            this.InvalidTokens.Clear();
            this.UnreadyTokens.Clear();
            this.Errors.Clear();
            return this;
        }

        /// <summary>Create a deep clone of the instance.</summary>
        public ContextualState Clone()
        {
            return new ContextualState().MergeFrom(this);
        }

        /// <summary>Merge the data from another instance into this instance.</summary>
        /// <param name="other">The other contextual state to copy.</param>
        public ContextualState MergeFrom(IContextualState other)
        {
            if (other != null)
            {
                this.AddRange(this.InvalidTokens, other.InvalidTokens);
                this.AddRange(this.UnreadyTokens, other.UnreadyTokens);
                this.AddRange(this.Errors, other.Errors);
            }
            return this;
        }

        /// <summary>Add unknown tokens required by the instance.</summary>
        /// <param name="tokens">The tokens to add.</param>
        public ContextualState AddInvalidTokens(params string[] tokens)
        {
            this.AddRange(this.InvalidTokens, tokens);
            return this;
        }

        /// <summary>Add valid tokens required by the instance which aren't available in the current context.</summary>
        /// <param name="tokens">The tokens to add.</param>
        public ContextualState AddUnreadyTokens(params string[] tokens)
        {
            this.AddRange(this.UnreadyTokens, tokens);
            return this;
        }

        /// <summary>Add tokens which are provided by a mod which isn't installed, if any.</summary>
        /// <param name="tokens">The tokens to add.</param>
        public ContextualState AddUnavailableModTokens(params string[] tokens)
        {
            this.AddRange(this.UnavailableModTokens, tokens);
            return this;
        }

        /// <summary>Add error phrases indicating why the instance is not ready to use.</summary>
        /// <param name="errors">The tokens to add.</param>
        public ContextualState AddErrors(params string[] errors)
        {
            this.AddRange(this.Errors, errors);
            return this;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Add a range of values to a target set.</summary>
        /// <param name="target">The set to update.</param>
        /// <param name="source">The values to add.</param>
        private void AddRange(ISet<string> target, IEnumerable<string> source)
        {
            if (source == null)
                return;

            target.AddMany(source);
        }
    }
}
