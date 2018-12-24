using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token for the player's wallet items.</summary>
    internal class HasWalletItemToken : BaseToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>Get whether the player data is available in the current context.</summary>
        private readonly Func<bool> IsPlayerDataAvailable;

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
        /// <param name="isPlayerDataAvailable">Get whether the player data is available in the current context.</param>
        public HasWalletItemToken(Func<bool> isPlayerDataAvailable)
            : base(ConditionType.HasWalletItem.ToString(), canHaveMultipleRootValues: true)
        {
            this.IsPlayerDataAvailable = isPlayerDataAvailable;
            this.EnableSubkeys(required: false, canHaveMultipleValues: false);
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.IsValidInContext = this.IsPlayerDataAvailable();
        }

        /// <summary>Get the allowed subkeys (or <c>null</c> if any value is allowed).</summary>
        protected override InvariantHashSet GetAllowedSubkeys()
        {
            return new InvariantHashSet(this.WalletItems.Keys.Select(p => p.ToString()));
        }

        /// <summary>Get the allowed values for a token name (or <c>null</c> if any value is allowed).</summary>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override InvariantHashSet GetAllowedValues(TokenName name)
        {
            return name.HasSubkey()
                ? InvariantHashSet.Boolean()
                : this.GetAllowedSubkeys();
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override IEnumerable<string> GetValues(TokenName name)
        {
            this.AssertTokenName(name);

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
    }
}
