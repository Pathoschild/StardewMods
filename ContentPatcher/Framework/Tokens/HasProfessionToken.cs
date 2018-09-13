using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token for the player's professions.</summary>
    internal class HasProfessionToken : BaseToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The player's current professions.</summary>
        private readonly HashSet<Profession> Professions = new HashSet<Profession>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public HasProfessionToken()
            : base(ConditionType.HasProfession.ToString(), canHaveMultipleValues: true, requiresSubkeys: false) { }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.Professions.Clear();
            this.IsValidInContext = Context.IsWorldReady;
            if (this.IsValidInContext)
            {
                foreach (int professionID in Game1.player.professions)
                    this.Professions.Add((Profession)professionID);
            }

        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check, if applicable.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or this token require a subkeys and <paramref name="name"/> does not specify one.</exception>
        public override IEnumerable<string> GetValues(TokenName? name = null)
        {
            this.AssertTokenName(name);

            foreach (Profession profession in this.Professions)
                yield return profession.ToString();
        }

        /// <summary>Perform custom validation on a set of input values.</summary>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public override bool TryCustomValidation(InvariantHashSet values, out string error)
        {
            if (!base.TryCustomValidation(values, out error))
                return false;

            string[] invalidValues = this.GetInvalidValues(values).ToArray();
            if (invalidValues.Any())
            {
                error = $"can't parse some values ({string.Join(", ", invalidValues)}) as profession IDs; must be a predefined name ({string.Join(", ", Enum.GetNames(typeof(Profession)).OrderBy(p => p))}) or integer ID.";
                return false;
            }

            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the values which can't be parsed as a profession ID.</summary>
        /// <param name="values">The values to check.</param>
        private IEnumerable<string> GetInvalidValues(IEnumerable<string> values)
        {
            foreach (string value in values)
            {
                if (!Enum.TryParse(value, true, out Profession _)) // either a defined Profession constant or arbitrary int value are OK
                    yield return value;
            }
        }
    }
}
