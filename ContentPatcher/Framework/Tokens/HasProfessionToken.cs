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
            : base(ConditionType.HasProfession.ToString(), canHaveMultipleRootValues: true)
        {
            this.EnableSubkeys(required: false, canHaveMultipleValues: false);
        }

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
        /// <param name="name">The token name to check.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override IEnumerable<string> GetValues(TokenName name)
        {
            this.AssertTokenName(name);

            if (name.HasSubkey())
            {
                bool hasProfession = this.TryParseEnum(name.Subkey, out Profession profession, mustBeNamed: false) && this.Professions.Contains(profession);
                yield return hasProfession.ToString();
            }
            else
            {
                foreach (Profession profession in this.Professions)
                    yield return profession.ToString();
            }
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
                error = $"can't parse some values ({string.Join(", ", invalidValues)}) as profession IDs; must be a predefined name ({string.Join(", ", Enum.GetNames(typeof(Profession)).OrderByIgnoreCase(p => p))}) or integer ID.";
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
                if (!this.TryParseEnum(value, out Profession _, mustBeNamed: false))
                    yield return value;
            }
        }
    }
}
