using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for the player's wallet items.</summary>
    internal class HasWalletItemValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get whether the player data is available in the current context.</summary>
        private readonly Func<bool> IsPlayerDataAvailable;

        /// <summary>The defined wallet items and whether the player has them.</summary>
        private readonly IDictionary<WalletItem, bool> Values = new Dictionary<WalletItem, bool>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="isPlayerDataAvailable">Get whether the player data is available in the current context.</param>
        public HasWalletItemValueProvider(Func<bool> isPlayerDataAvailable)
            : base(ConditionType.HasWalletItem, canHaveMultipleValuesForRoot: true)
        {
            this.IsPlayerDataAvailable = isPlayerDataAvailable;
            this.EnableInputArguments(required: false, canHaveMultipleValues: false);
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(() =>
            {
                IDictionary<WalletItem, bool> oldValues = new Dictionary<WalletItem, bool>(this.Values);

                this.Values.Clear();
                if (this.MarkReady(this.IsPlayerDataAvailable()))
                {
                    this.Values[WalletItem.DwarvishTranslationGuide] = Game1.player.canUnderstandDwarves;
                    this.Values[WalletItem.RustyKey] = Game1.player.hasRustyKey;
                    this.Values[WalletItem.ClubCard] = Game1.player.hasClubCard;
                    this.Values[WalletItem.SpecialCharm] = Game1.player.hasSpecialCharm;
                    this.Values[WalletItem.SkullKey] = Game1.player.hasSkullKey;
                    this.Values[WalletItem.MagnifyingGlass] = Game1.player.hasMagnifyingGlass;
                    this.Values[WalletItem.DarkTalisman] = Game1.player.hasDarkTalisman;
                    this.Values[WalletItem.MagicInk] = Game1.player.hasMagicInk;
                    this.Values[WalletItem.BearsKnowledge] = Game1.player.eventsSeen.Contains(2120303);
                    this.Values[WalletItem.SpringOnionMastery] = Game1.player.eventsSeen.Contains(3910979);

                    return
                        this.Values.Count != oldValues.Count
                        || this.Values.Any(entry => !oldValues.TryGetValue(entry.Key, out bool oldValue) || entry.Value != oldValue);
                }

                return false;
            });
        }

        /// <summary>Get the set of valid input arguments if restricted, or an empty collection if unrestricted.</summary>
        public override InvariantHashSet GetValidInputs()
        {
            return new InvariantHashSet(this.Values.Keys.Select(p => p.ToString()));
        }

        /// <summary>Get the allowed values for an input argument (or <c>null</c> if any value is allowed).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override InvariantHashSet GetAllowedValues(ITokenString input)
        {
            return input.IsMeaningful()
                ? InvariantHashSet.Boolean()
                : this.GetValidInputs();
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            if (input.IsMeaningful())
            {
                bool hasItem = this.TryParseEnum(input.Value, out WalletItem item) && this.Values[item];
                yield return hasItem.ToString();
            }
            else
            {
                foreach (KeyValuePair<WalletItem, bool> pair in this.Values)
                {
                    if (pair.Value)
                        yield return pair.Key.ToString();
                }
            }
        }
    }
}
