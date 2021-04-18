using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Commands;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework.Api
{
    /// <summary>A set of parsed conditions linked to the Content Patcher context for an API consumer, which assume they're always run on the same screen.</summary>
    internal class ApiManagedConditionsForSingleScreen : IManagedConditions
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying conditions.</summary>
        private readonly Condition[] Conditions;

        /// <summary>The context with which to update conditions.</summary>
        private readonly IContext Context;

        /// <summary>A contextual manager for the underlying conditions.</summary>
        private readonly AggregateContextual Contextuals;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public bool IsValid { get; }

        /// <inheritdoc />
        public string ValidationError { get; }

        /// <inheritdoc />
        public bool IsReady => this.Contextuals.IsReady;

        /// <inheritdoc />
        public bool IsMatch { get; private set; }

        /// <inheritdoc />
        public bool IsMutable => this.Contextuals.IsMutable;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="conditions">The underlying conditions.</param>
        /// <param name="context">The context with which to update conditions.</param>
        /// <param name="isValid">Whether the conditions were parsed successfully (regardless of whether they're in scope currently).</param>
        /// <param name="validationError">If <paramref name="isValid"/> is false, an error phrase indicating why the conditions failed to parse.</param>
        public ApiManagedConditionsForSingleScreen(Condition[] conditions, IContext context, bool isValid = true, string validationError = null)
        {
            this.Conditions = conditions;
            this.Context = context;
            this.IsValid = isValid;
            this.ValidationError = validationError;

            this.Contextuals = new AggregateContextual().Add(conditions);
        }

        /// <inheritdoc />
        public IEnumerable<int> UpdateContext()
        {
            bool wasMatch = this.IsMatch;

            if (this.IsValid)
            {
                this.Contextuals.UpdateContext(this.Context);
                this.IsMatch = this.IsReady && this.Conditions.All(p => p.IsMatch);
            }

            return this.IsMatch != wasMatch
                ? new[] { StardewModdingAPI.Context.ScreenId }
                : Enumerable.Empty<int>();
        }

        /// <inheritdoc />
        public string GetReasonNotMatched()
        {
            if (this.IsMatch)
                return null;

            PatchBaseInfo patchInfo = new(this.Conditions, this.IsMatch, this.Contextuals.GetDiagnosticState());
            return patchInfo.GetReasonNotLoaded();
        }
    }
}
