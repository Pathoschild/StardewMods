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
        /// <summary>Handles reading info from the current save.</summary>
        private readonly TokenSaveReader SaveReader;

        /// <summary>The names and genders of NPCs/players having children.</summary>
        private readonly SortedSet<string> PartnersHavingChild = new(HumanSortComparer.DefaultIgnoreCase);

        /// <summary>Whether to only include pregnant NPCs.</summary>
        private readonly bool PregnancyOnly;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type (must be <see cref="ConditionType.HavingChild"/> or <see cref="ConditionType.Pregnant"/>).</param>
        /// <param name="saveReader">Handles reading info from the current save.</param>
        public HavingChildValueProvider(ConditionType type, TokenSaveReader saveReader)
            : base(type, mayReturnMultipleValuesForRoot: true)
        {
            if (type != ConditionType.HavingChild && type != ConditionType.Pregnant)
                throw new ArgumentException($"The condition type must be {ConditionType.HavingChild} or {ConditionType.Pregnant}.");

            this.SaveReader = saveReader;
            this.PregnancyOnly = type == ConditionType.Pregnant;
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            const string playerPrefix = InternalConstants.PlayerNamePrefix;
            return this.IsChanged(this.PartnersHavingChild, () =>
            {
                this.PartnersHavingChild.Clear();
                if (this.MarkReady(this.SaveReader.IsReady))
                {
                    foreach (Farmer player in this.SaveReader.GetAllPlayers())
                    {
                        // get spouse info
                        if (!this.SaveReader.TryGetSpouseInfo(player, out string spouseName, out Friendship relationship, out Gender spouseGender, out bool isPlayerSpouse))
                            continue;

                        // check for pregnancy
                        if (relationship.NextBirthingDate == null || relationship.NextBirthingDate <= Game1.Date)
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
    }
}
