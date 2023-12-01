using System;
using System.Collections.Generic;
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
            if (state.InvalidTokens.Count > 0 || state.UnreadyTokens.Count > 0 || state.Errors.Count > 0)
            {
                List<string> reasons = new();

                if (state.InvalidTokens.Any())
                    reasons.Add($"invalid tokens: {string.Join(", ", state.InvalidTokens.OrderByHuman())}");
                if (state.UnreadyTokens.Any())
                    reasons.Add($"tokens not ready: {string.Join(", ", state.UnreadyTokens.OrderByHuman())}");
                if (state.Errors.Any())
                    reasons.Add(string.Join("; ", state.Errors));

                if (reasons.Any())
                    return string.Join("; ", reasons);
            }

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
