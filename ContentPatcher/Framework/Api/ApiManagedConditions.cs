using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Api
{
    /// <summary>A set of parsed conditions linked to the Content Patcher context for an API consumer. This implementation is <strong>per-screen</strong>, so the result depends on the screen that's active when calling the members.</summary>
    internal class ApiManagedConditions : IManagedConditions
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying conditions.</summary>
        private readonly PerScreen<IManagedConditions> Conditions;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        [MemberNotNullWhen(false, nameof(ApiManagedConditions.ValidationError))]
        public bool IsValid => this.Conditions.Value.IsValid;

        /// <inheritdoc />
        public string? ValidationError => this.Conditions.Value.ValidationError;

        /// <inheritdoc />
        public bool IsReady => this.Conditions.Value.IsReady;

        /// <inheritdoc />
        public bool IsMatch => this.Conditions.Value.IsMatch;

        /// <inheritdoc />
        public bool IsMutable => this.Conditions.Value.IsMutable;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="parse">Get parsed conditions for the currently active screen.</param>
        public ApiManagedConditions(Func<IManagedConditions> parse)
        {
            this.Conditions = new PerScreen<IManagedConditions>(parse);
        }

        /// <inheritdoc />
        public IEnumerable<int> UpdateContext()
        {
            return new HashSet<int>(
                this.Conditions
                    .GetActiveValues()
                    .Select(p => p.Value)
                    .SelectMany(p => p.UpdateContext())
            );
        }

        /// <inheritdoc />
        public string? GetReasonNotMatched()
        {
            return this.Conditions.Value.GetReasonNotMatched();
        }
    }
}
