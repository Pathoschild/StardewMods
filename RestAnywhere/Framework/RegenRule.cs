using System;

namespace RestAnywhere.Framework
{
    /// <summary>Contains information for a specific regen condition.</summary>
    internal class RegenRule
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The health regen per millisecond to apply.</summary>
        public float HealthRegen { get; }

        /// <summary>The stamina regen per millisecond to apply.</summary>
        public float StaminaRegen { get; }

        /// <summary>Whether the regen should be applied for the given context.</summary>
        public Func<RegenContext, bool> Applies { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="healthRegen">The health regen per millisecond to apply.</param>
        /// <param name="staminaRegen">The stamina regen per millisecond to apply.</param>
        /// <param name="applies">Whether the regen should be applied for the given context.</param>
        public RegenRule(float healthRegen, float staminaRegen, Func<RegenContext, bool> applies)
        {
            this.Applies = applies;
            this.HealthRegen = healthRegen;
            this.StaminaRegen = staminaRegen;
        }
    }
}
