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

        /// <summary>The valid input arguments.</summary>
        private readonly InvariantHashSet ValidInputs = new InvariantHashSet(Enum.GetNames(typeof(WalletItem)));


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="isPlayerDataAvailable">Get whether the player data is available in the current context.</param>
        public HasWalletItemValueProvider(Func<bool> isPlayerDataAvailable)
            : base(ConditionType.HasWalletItem, mayReturnMultipleValuesForRoot: true)
        {
            this.IsPlayerDataAvailable = isPlayerDataAvailable;
            this.EnableInputArguments(required: false, mayReturnMultipleValues: false, maxPositionalArgs: 1);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override InvariantHashSet GetValidPositionalArgs()
        {
            return this.ValidInputs;
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            allowedValues = input.HasPositionalArgs
                ? InvariantHashSet.Boolean()
                : this.GetValidPositionalArgs();
            return true;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            if (input.HasPositionalArgs)
            {
                bool hasItem = this.TryParseEnum(input.GetFirstPositionalArg(), out WalletItem item) && this.Values[item];
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
