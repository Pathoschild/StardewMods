using System;
using System.Linq;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Commands
{
    /// <summary>A summary of low-level patch info shown in the SMAPI console.</summary>
    internal class PatchBaseInfo
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The parsed conditions (if available).</summary>
        public Condition[] ParsedConditions { get; }

        /// <summary>Whether the patch should be applied in the current context.</summary>
        public bool MatchesContext { get; }

        /// <summary>Diagnostic info about the patch.</summary>
        public IContextualState State { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="parsedConditions">The parsed conditions (if available).</param>
        /// <param name="matchesContext">Whether the patch should be applied in the current context.</param>
        /// <param name="state">Diagnostic info about the patch.</param>
        public PatchBaseInfo(Condition[]? parsedConditions, bool matchesContext, IContextualState state)
        {
            this.ParsedConditions = parsedConditions ?? Array.Empty<Condition>();
            this.MatchesContext = matchesContext;
            this.State = state;
        }

        /// <summary>Get a human-readable reason that the patch isn't applied.</summary>
        public virtual string? GetReasonNotLoaded()
        {
            IContextualState state = this.State;

            // state error
            if (state.InvalidTokens.Any())
                return $"invalid tokens: {string.Join(", ", state.InvalidTokens.OrderByHuman())}";
            if (state.UnreadyTokens.Any())
                return $"tokens not ready: {string.Join(", ", state.UnreadyTokens.OrderByHuman())}";
            if (state.Errors.Any())
                return string.Join("; ", state.Errors);

            // conditions not matched
            if (!this.MatchesContext && this.ParsedConditions.Any())
            {
                string[] failedConditions = (
                    from condition in this.ParsedConditions
                    let displayText = !condition.Is(ConditionType.HasFile) && !string.IsNullOrWhiteSpace(condition.Input.TokenString?.Raw)
                        ? $"{condition.Name}:{condition.Input.TokenString.Raw}"
                        : condition.Name
                    orderby displayText
                    where !condition.IsMatch
                    select $"{displayText}"
                ).ToArray();

                if (failedConditions.Any())
                    return $"conditions don't match: {string.Join(", ", failedConditions)}";
            }

            // fallback to unavailable tokens (should never happen due to HasMod check)
            if (state.UnavailableModTokens.Any())
                return $"tokens provided by an unavailable mod: {string.Join(", ", state.UnavailableModTokens.OrderByHuman())}";

            // non-matching for an unknown reason
            if (!this.MatchesContext)
                return "doesn't match context (unknown reason)";

            // seems fine, just not applied yet
            return null;
        }
    }
}
