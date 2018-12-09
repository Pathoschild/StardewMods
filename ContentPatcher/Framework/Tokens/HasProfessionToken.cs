using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
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

        /// <summary>Perform custom validation on a subkey/value pair.</summary>
        /// <param name="name">The token name to validate.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public override bool TryValidate(TokenName name, string value, out string error)
        {
            if (!base.TryValidate(name, value, out error))
                return false;

            // validate profession IDs
            string profession = name.HasSubkey() ? name.Subkey : value;
            if (!this.TryParseEnum(profession, out Profession _, mustBeNamed: false))
            {
                error = $"can't parse '{profession}' as a profession ID; must be one of [{string.Join(", ", Enum.GetNames(typeof(Profession)).OrderByIgnoreCase(p => p))}] or an integer ID.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
