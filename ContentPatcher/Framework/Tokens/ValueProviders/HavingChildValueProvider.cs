using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for NPCs and players whose relationships have an active adoption or pregnancy.</summary>
    internal class HavingChildValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get whether the player data is available in the current context.</summary>
        private readonly Func<bool> IsPlayerDataAvailable;

        /// <summary>The names and genders of NPCs/players having children.</summary>
        private readonly InvariantHashSet PartnersHavingChild = new InvariantHashSet();

        /// <summary>Whether to only include pregnant NPCs.</summary>
        private readonly bool PregnancyOnly;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type (must be <see cref="ConditionType.HavingChild"/> or <see cref="ConditionType.Pregnant"/>).</param>
        /// <param name="isPlayerDataAvailable">Get whether the player data is available in the current context.</param>
        public HavingChildValueProvider(ConditionType type, Func<bool> isPlayerDataAvailable)
            : base(type, mayReturnMultipleValuesForRoot: true)
        {
            if (type != ConditionType.HavingChild && type != ConditionType.Pregnant)
                throw new ArgumentException($"The condition type must be {ConditionType.HavingChild} or {ConditionType.Pregnant}.");

            this.IsPlayerDataAvailable = isPlayerDataAvailable;
            this.PregnancyOnly = type == ConditionType.Pregnant;
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            const string playerPrefix = InternalConstants.PlayerNamePrefix;
            return this.IsChanged(this.PartnersHavingChild, () =>
            {
                this.PartnersHavingChild.Clear();
                if (this.MarkReady(this.IsPlayerDataAvailable()))
                {
                    foreach (Farmer player in Game1.getAllFarmers())
                    {
                        // get relationship
                        Friendship relationship = player.GetSpouseFriendship();
                        if (relationship == null)
                            continue;

                        // check for pregnancy
                        if (relationship.NextBirthingDate == null || relationship.NextBirthingDate <= Game1.Date)
                            continue;

                        // get spouse info
                        if (!this.TryGetSpouseInfo(player, out string spouseName, out Gender spouseGender, out bool isPlayerSpouse))
                            continue;

                        // track pregnancy/adoption
                        if (this.PregnancyOnly)
                        {
                            Gender playerGender = player.IsMale ? Gender.Male : Gender.Female;
                            if (playerGender != spouseGender)
                            {
                                if (playerGender == Gender.Female)
                                    this.PartnersHavingChild.Add(playerPrefix + player.Name);
                                if (spouseGender == Gender.Female)
                                    this.PartnersHavingChild.Add((isPlayerSpouse ? playerPrefix : "") + spouseName);
                            }
                        }
                        else
                        {
                            this.PartnersHavingChild.Add(playerPrefix + player.Name);
                            this.PartnersHavingChild.Add((isPlayerSpouse ? playerPrefix : "") + spouseName);
                        }
                    }
                }
            });
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            return this.PartnersHavingChild;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the name and gender of the player's spouse, if they're married</summary>
        /// <param name="player">The player whose spouse to check.</param>
        /// <param name="name">The spouse name.</param>
        /// <param name="gender">The spouse gender.</param>
        /// <param name="isPlayer">Whether the spouse is a player character.</param>
        /// <returns>Returns true if the player's spouse info was successfully found.''</returns>
        private bool TryGetSpouseInfo(Farmer player, out string name, out Gender gender, out bool isPlayer)
        {
            long? spousePlayerID = Game1.player.team.GetSpouse(player.UniqueMultiplayerID);
            if (spousePlayerID.HasValue)
            {
                Farmer spouse = Game1.getFarmerMaybeOffline(spousePlayerID.Value);
                if (spouse != null)
                {
                    name = spouse.Name;
                    gender = spouse.IsMale ? Gender.Male : Gender.Female;
                    isPlayer = true;
                    return true;
                }
            }
            else
            {
                NPC spouse = Game1.getCharacterFromName(player.spouse, mustBeVillager: true);
                if (spouse != null)
                {
                    name = spouse.Name;
                    gender = spouse.Gender == NPC.male ? Gender.Male : Gender.Female;
                    isPlayer = false;
                    return true;
                }
            }

            name = null;
            gender = Gender.Male;
            isPlayer = false;
            return false;
        }
    }
}
