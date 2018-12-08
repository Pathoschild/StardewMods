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
    /// <summary>A token for the player's wallet items.</summary>
    internal class HasWalletItemToken : BaseToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The defined wallet items and whether the player has them.</summary>
        private readonly IDictionary<WalletItem, Func<bool>> WalletItems = new Dictionary<WalletItem, Func<bool>>
        {
            [WalletItem.DwarvishTranslationGuide] = () => Game1.player.canUnderstandDwarves,
            [WalletItem.RustyKey] = () => Game1.player.hasRustyKey,
            [WalletItem.ClubCard] = () => Game1.player.hasClubCard,
            [WalletItem.SpecialCharm] = () => Game1.player.hasSpecialCharm,
            [WalletItem.SkullKey] = () => Game1.player.hasSkullKey,
            [WalletItem.MagnifyingGlass] = () => Game1.player.hasMagnifyingGlass,
            [WalletItem.DarkTalisman] = () => Game1.player.hasDarkTalisman,
            [WalletItem.MagicInk] = () => Game1.player.hasMagicInk,
            [WalletItem.BearsKnowledge] = () => Game1.player.eventsSeen.Contains(2120303),
            [WalletItem.SpringOnionMastery] = () => Game1.player.eventsSeen.Contains(3910979)
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public HasWalletItemToken()
            : base(ConditionType.HasWalletItem.ToString(), canHaveMultipleRootValues: true)
        {
            this.EnableSubkeys(required: false, canHaveMultipleValues: false);
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.IsValidInContext = Context.IsWorldReady;
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override IEnumerable<string> GetValues(TokenName name)
        {
            this.AssertTokenName(name);
            if (!Context.IsWorldReady)
                yield break;

            if (name.HasSubkey())
            {
                bool hasItem = this.TryParseEnum(name.Subkey, out WalletItem item) && this.WalletItems[item]();
                yield return hasItem.ToString();
            }
            else
            {
                foreach (KeyValuePair<WalletItem, Func<bool>> pair in this.WalletItems)
                {
                    if (pair.Value())
                        yield return pair.Key.ToString();
                }
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
                error = $"can't parse some values ({string.Join(", ", invalidValues)}) as wallet items; must be one of [{string.Join(", ", Enum.GetNames(typeof(WalletItem)).OrderByIgnoreCase(p => p))}].";
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
                if (!this.TryParseEnum(value, out WalletItem item) || !this.WalletItems.ContainsKey(item))
                    yield return value;
            }
        }
    }
}
