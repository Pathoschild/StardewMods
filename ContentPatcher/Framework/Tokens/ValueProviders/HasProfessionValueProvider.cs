using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for the player's professions.</summary>
    internal class HasProfessionValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get whether the player data is available in the current context.</summary>
        private readonly Func<bool> IsPlayerDataAvailable;

        /// <summary>The player's current professions.</summary>
        private readonly HashSet<Profession> Professions = new HashSet<Profession>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="isPlayerDataAvailable">Get whether the player data is available in the current context.</param>
        public HasProfessionValueProvider(Func<bool> isPlayerDataAvailable)
            : base(ConditionType.HasProfession, mayReturnMultipleValuesForRoot: true)
        {
            this.IsPlayerDataAvailable = isPlayerDataAvailable;
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(this.Professions, () =>
            {
                this.Professions.Clear();
                if (this.MarkReady(this.IsPlayerDataAvailable()))
                {
                    foreach (int professionID in Game1.player.professions)
                        this.Professions.Add((Profession)professionID);
                }
            });
        }

        /// <inheritdoc />
        public override bool TryValidateValues(IInputArguments input, InvariantHashSet values, out string error)
        {
            if (!base.TryValidateValues(input, values, out error))
                return false;

            // validate profession IDs
            foreach (string value in values)
            {
                if (!this.TryParseEnum(value, out Profession _, mustBeNamed: false))
                {
                    error = $"can't parse '{value}' as a profession ID; must be one of [{string.Join(", ", Enum.GetNames(typeof(Profession)).OrderByIgnoreCase(p => p))}] or an integer ID.";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            foreach (Profession profession in this.Professions)
                yield return profession.ToString();
        }
    }
}
