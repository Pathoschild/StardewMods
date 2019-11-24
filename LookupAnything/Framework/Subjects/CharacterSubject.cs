using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
{
    /// <summary>Describes an NPC (including villagers, monsters, and pets).</summary>
    internal class CharacterSubject : BaseSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>The NPC type.s</summary>
        private readonly TargetType TargetType;

        /// <summary>The lookup target.</summary>
        private readonly NPC Target;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Whether to only show content once the player discovers it.</summary>
        private readonly bool ProgressionMode;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="npc">The lookup target.</param>
        /// <param name="type">The NPC type.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        /// <param name="progressionMode">Whether to only show content once the player discovers it.</param>
        /// <remarks>Reverse engineered from <see cref="NPC"/>.</remarks>
        public CharacterSubject(GameHelper gameHelper, NPC npc, TargetType type, Metadata metadata, ITranslationHelper translations, IReflectionHelper reflectionHelper, bool progressionMode)
            : base(gameHelper, translations)
        {
            this.Reflection = reflectionHelper;
            this.ProgressionMode = progressionMode;

            // get display type
            string typeName;
            if (type == TargetType.Villager)
                typeName = L10n.Types.Villager();
            else if (type == TargetType.Monster)
                typeName = L10n.Types.Monster();
            else
                typeName = npc.GetType().Name;

            // initialize
            this.Target = npc;
            this.TargetType = type;
            CharacterData overrides = metadata.GetCharacter(npc, type);
            string name = npc.getName();
            string description = overrides?.DescriptionKey != null ? translations.Get(overrides.DescriptionKey) : null;
            this.Initialize(name, description, typeName);
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            NPC npc = this.Target;
            switch (this.TargetType)
            {
                case TargetType.Villager:
                    if (npc is Child child)
                        return this.GetDataForChild(child, metadata);
                    else if (npc is TrashBear trashBear)
                        return this.GetDataForTrashBear(trashBear);
                    else
                        return this.GetDataForVillager(npc, metadata);

                case TargetType.Pet:
                    return this.GetDataForPet((Pet)npc);

                case TargetType.Monster:
                    return this.GetDataForMonster((Monster)npc, metadata);

                default:
                    return Enumerable.Empty<ICustomField>();
            }
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<IDebugField> GetDebugFields(Metadata metadata)
        {
            NPC target = this.Target;
            Pet pet = target as Pet;

            // pinned fields
            yield return new GenericDebugField("facing direction", this.Stringify((FacingDirection)target.FacingDirection), pinned: true);
            yield return new GenericDebugField("walking towards player", this.Stringify(target.IsWalkingTowardPlayer), pinned: true);
            if (Game1.player.friendshipData.ContainsKey(target.Name))
            {
                FriendshipModel friendship = this.GameHelper.GetFriendshipForVillager(Game1.player, target, Game1.player.friendshipData[target.Name], metadata);
                yield return new GenericDebugField("friendship", $"{friendship.Points} (max {friendship.MaxPoints})", pinned: true);
            }
            if (pet != null)
                yield return new GenericDebugField("friendship", $"{pet.friendshipTowardFarmer} of {Pet.maxFriendship})", pinned: true);

            // raw fields
            foreach (IDebugField field in this.GetDebugFieldsFrom(target))
                yield return field;
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            NPC npc = this.Target;

            // use character portrait (most NPCs)
            if (npc.Portrait != null)
            {
                spriteBatch.DrawSprite(npc.Portrait, new Rectangle(0, 0, NPC.portrait_width, NPC.portrait_height), position.X, position.Y, Color.White, size.X / NPC.portrait_width);
                return true;
            }

            // else draw sprite (e.g. for pets)
            npc.Sprite.draw(spriteBatch, position, 1, 0, 0, Color.White, scale: size.X / npc.Sprite.getWidth());
            return true;
        }


        /*********
        ** Private methods
        *********/
        /*****
        ** Data fields
        ****/
        /// <summary>Get the fields to display for a child.</summary>
        /// <param name="child">The child for which to show info.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <remarks>Derived from <see cref="Child.dayUpdate"/>.</remarks>
        private IEnumerable<ICustomField> GetDataForChild(Child child, Metadata metadata)
        {
            // birthday
            SDate birthday = SDate.Now().AddDays(-child.daysOld.Value);
            yield return new GenericField(this.GameHelper, L10n.Npc.Birthday(), this.Text.Stringify(birthday, withYear: true));

            // age
            {
                ChildAge stage = (ChildAge)child.Age;
                int daysOld = child.daysOld.Value;
                int daysToNext = this.GetDaysToNextChildGrowth(stage, daysOld);
                bool isGrown = daysToNext == -1;
                int daysAtNext = daysOld + (isGrown ? 0 : daysToNext);

                string ageDesc = isGrown
                    ? L10n.NpcChild.AgeDescriptionGrown(label: stage)
                    : L10n.NpcChild.AgeDescriptionPartial(label: stage, count: daysToNext, nextLabel: stage + 1);

                yield return new PercentageBarField(this.GameHelper, L10n.NpcChild.Age(), child.daysOld.Value, daysAtNext, Color.Green, Color.Gray, ageDesc);
            }

            // friendship
            if (Game1.player.friendshipData.ContainsKey(child.Name))
            {
                FriendshipModel friendship = this.GameHelper.GetFriendshipForVillager(Game1.player, child, Game1.player.friendshipData[child.Name], metadata);
                yield return new CharacterFriendshipField(this.GameHelper, L10n.Npc.Friendship(), friendship, this.Text);
                yield return new GenericField(this.GameHelper, L10n.Npc.TalkedToday(), this.Stringify(Game1.player.friendshipData[child.Name].TalkedToToday));
            }
        }

        /// <summary>Get the fields to display for a monster.</summary>
        /// <param name="monster">The monster for which to show info.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <remarks>Derived from <see cref="Monster.parseMonsterInfo"/>.</remarks>
        private IEnumerable<ICustomField> GetDataForMonster(Monster monster, Metadata metadata)
        {
            // basic info
            bool canRerollDrops = Game1.player.isWearingRing(Ring.burglarsRing);

            yield return new GenericField(this.GameHelper, L10n.Monster.Invincible(), L10n.Generic.Seconds(count: this.Reflection.GetField<int>(monster, "invincibleCountdown").GetValue()), hasValue: monster.isInvincible());
            yield return new PercentageBarField(this.GameHelper, L10n.Monster.Health(), monster.Health, monster.MaxHealth, Color.Green, Color.Gray, L10n.Generic.PercentRatio(percent: (int)Math.Round((monster.Health / (monster.MaxHealth * 1f) * 100)), value: monster.Health, max: monster.MaxHealth));
            yield return new ItemDropListField(this.GameHelper, L10n.Monster.Drops(), this.GetMonsterDrops(monster), fadeNonGuaranteed: true, crossOutNonGuaranteed: !canRerollDrops, defaultText: L10n.Monster.DropsNothing());
            yield return new GenericField(this.GameHelper, L10n.Monster.Experience(), this.Stringify(monster.ExperienceGained));
            yield return new GenericField(this.GameHelper, L10n.Monster.Defense(), this.Stringify(monster.resilience.Value));
            yield return new GenericField(this.GameHelper, L10n.Monster.Attack(), this.Stringify(monster.DamageToFarmer));

            // Adventure Guild quest
            AdventureGuildQuestData adventureGuildQuest = metadata.GetAdventurerGuildQuest(monster.Name);
            if (adventureGuildQuest != null)
            {
                int kills = adventureGuildQuest.Targets.Select(p => Game1.stats.getMonstersKilled(p)).Sum();
                yield return new GenericField(this.GameHelper, L10n.Monster.AdventureGuild(), $"{(kills >= adventureGuildQuest.RequiredKills ? L10n.Monster.AdventureGuildComplete() : L10n.Monster.AdventureGuildIncomplete())} ({L10n.Monster.AdventureGuildProgress(count: kills, requiredCount: adventureGuildQuest.RequiredKills)})");
            }
        }

        /// <summary>Get the fields to display for a pet.</summary>
        /// <param name="pet">The pet for which to show info.</param>
        /// <remarks>Derived from <see cref="Pet.checkAction"/> and <see cref="Pet.dayUpdate"/>.</remarks>
        private IEnumerable<ICustomField> GetDataForPet(Pet pet)
        {
            Farm farm = Game1.getFarm();

            // friendship
            yield return new CharacterFriendshipField(this.GameHelper, L10n.Pet.Love(), this.GameHelper.GetFriendshipForPet(Game1.player, pet), this.Text);

            // petted today / last petted
            int? lastDayPetted = this.GetLastDayPetted(pet, Game1.player.UniqueMultiplayerID);
            yield return new GenericField(this.GameHelper, L10n.Pet.PettedToday(), lastDayPetted == Game1.Date.TotalDays ? L10n.Pet.LastPettedYes() : this.Stringify(false));
            if (!lastDayPetted.HasValue)
                yield return new GenericField(this.GameHelper, L10n.Pet.LastPetted(), L10n.Pet.LastPettedNever());
            else if (lastDayPetted != Game1.Date.TotalDays)
            {
                int daysSincePetted = Game1.Date.TotalDays - lastDayPetted.Value;
                yield return new GenericField(this.GameHelper, L10n.Pet.LastPetted(), daysSincePetted == 1 ? L10n.Generic.Yesterday() : L10n.Pet.LastPettedDaysAgo(daysSincePetted));
            }

            // water bowl
            yield return new GenericField(this.GameHelper, L10n.Pet.WaterBowl(), farm.petBowlWatered.Value ? L10n.Pet.WaterBowlFilled() : L10n.Pet.WaterBowlEmpty());
        }

        /// <summary>Get the fields to display for the trash bear.</summary>
        /// <param name="trashBear">The trash bear for which to show info.</param>
        /// <remarks>Derived from <see cref="TrashBear.checkAction"/>.</remarks>
        private IEnumerable<ICustomField> GetDataForTrashBear(TrashBear trashBear)
        {
            // get number of quests completed
            const int maxQuests = 4;
            int questsDone = 0;
            if (NetWorldState.checkAnywhereForWorldStateID("trashBear1"))
                questsDone = 1;
            if (NetWorldState.checkAnywhereForWorldStateID("trashBear2"))
                questsDone = 2;
            if (NetWorldState.checkAnywhereForWorldStateID("trashBear3"))
                questsDone = 3;
            if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
                questsDone = 4;

            // show item wanted
            if (questsDone < maxQuests)
            {
                this.Reflection.GetMethod(trashBear, "updateItemWanted").Invoke();
                int itemWantedIndex = this.Reflection.GetField<int>(trashBear, "itemWantedIndex").GetValue();
                yield return new ItemIconField(this.GameHelper, L10n.TrashBear.ItemWanted(), new SObject(itemWantedIndex, 1));
            }

            // show progress
            yield return new GenericField(this.GameHelper, L10n.TrashBear.QuestProgress(), L10n.Generic.Ratio(questsDone, maxQuests));
        }

        /// <summary>Get the fields to display for a villager NPC.</summary>
        /// <param name="npc">The NPC for which to show info.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        private IEnumerable<ICustomField> GetDataForVillager(NPC npc, Metadata metadata)
        {
            if (!metadata.Constants.AsocialVillagers.Contains(npc.Name))
            {
                // birthday
                if (npc.Birthday_Season != null)
                {
                    SDate birthday = new SDate(npc.Birthday_Day, npc.Birthday_Season);
                    yield return new GenericField(this.GameHelper, L10n.Npc.Birthday(), this.Text.Stringify(birthday));
                }

                // friendship
                if (Game1.player.friendshipData.ContainsKey(npc.Name))
                {
                    // friendship/romance
                    FriendshipModel friendship = this.GameHelper.GetFriendshipForVillager(Game1.player, npc, Game1.player.friendshipData[npc.Name], metadata);
                    yield return new GenericField(this.GameHelper, L10n.Npc.CanRomance(), friendship.IsSpouse ? L10n.Npc.CanRomanceMarried() : friendship.IsHousemate ? L10n.Npc.CanRomanceHousemate() : this.Stringify(friendship.CanDate));
                    yield return new CharacterFriendshipField(this.GameHelper, L10n.Npc.Friendship(), friendship, this.Text);

                    // talked/gifted today
                    yield return new GenericField(this.GameHelper, L10n.Npc.TalkedToday(), this.Stringify(friendship.TalkedToday));
                    yield return new GenericField(this.GameHelper, L10n.Npc.GiftedToday(), this.Stringify(friendship.GiftsToday > 0));

                    // kissed/hugged today
                    if (friendship.IsSpouse || friendship.IsHousemate)
                        yield return new GenericField(this.GameHelper, friendship.IsSpouse ? L10n.Npc.KissedToday() : L10n.Npc.HuggedToday(), this.Stringify(npc.hasBeenKissedToday.Value));

                    // gifted this week
                    if (!friendship.IsSpouse && !friendship.IsHousemate)
                        yield return new GenericField(this.GameHelper, L10n.Npc.GiftedThisWeek(), L10n.Generic.Ratio(value: friendship.GiftsThisWeek, max: NPC.maxGiftsPerWeek));
                }
                else
                    yield return new GenericField(this.GameHelper, L10n.Npc.Friendship(), L10n.Npc.FriendshipNotMet());

                // gift tastes
                IDictionary<GiftTaste, GiftTasteModel[]> giftTastes = this.GetGiftTastes(npc, metadata);
                yield return new CharacterGiftTastesField(this.GameHelper, L10n.Npc.LovesGifts(), giftTastes, GiftTaste.Love, onlyRevealed: this.ProgressionMode);
                yield return new CharacterGiftTastesField(this.GameHelper, L10n.Npc.LikesGifts(), giftTastes, GiftTaste.Like, onlyRevealed: this.ProgressionMode);
                yield return new CharacterGiftTastesField(this.GameHelper, L10n.Npc.NeutralGifts(), giftTastes, GiftTaste.Neutral, onlyRevealed: this.ProgressionMode);
                if (this.ProgressionMode)
                {
                    yield return new CharacterGiftTastesField(this.GameHelper, L10n.Npc.DislikesGifts(), giftTastes, GiftTaste.Dislike, onlyRevealed: this.ProgressionMode);
                    yield return new CharacterGiftTastesField(this.GameHelper, L10n.Npc.HatesGifts(), giftTastes, GiftTaste.Hate, onlyRevealed: this.ProgressionMode);
                }
            }
        }

        /*****
        ** Other
        ****/
        /// <summary>Get how much an NPC likes receiving each item as a gift.</summary>
        /// <param name="npc">The NPC.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        private IDictionary<GiftTaste, GiftTasteModel[]> GetGiftTastes(NPC npc, Metadata metadata)
        {
            return this.GameHelper.GetGiftTastes(npc, metadata)
                .GroupBy(entry => entry.Taste)
                .ToDictionary(
                    tasteGroup => tasteGroup.Key,
                    tasteGroup => tasteGroup.ToArray()
                );
        }

        /// <summary>Get the number of days until a child grows to the next stage.</summary>
        /// <param name="stage">The child's current growth stage.</param>
        /// <param name="daysOld">The child's current age in days.</param>
        /// <returns>Returns a number of days, or <c>-1</c> if the child won't grow any further.</returns>
        /// <remarks>Derived from <see cref="Child.dayUpdate"/>.</remarks>
        private int GetDaysToNextChildGrowth(ChildAge stage, int daysOld)
        {
            switch (stage)
            {
                case ChildAge.Newborn:
                    return 13 - daysOld;
                case ChildAge.Baby:
                    return 27 - daysOld;
                case ChildAge.Crawler:
                    return 55 - daysOld;
                default:
                    return -1;
            }
        }

        /// <summary>Get the last day when the given player petted the pet.</summary>
        /// <param name="pet">The pet to check.</param>
        /// <param name="playerID">The unique multiplayer ID for the player to check.</param>
        private int? GetLastDayPetted(Pet pet, long playerID)
        {
            return pet.lastPetDay.TryGetValue(playerID, out int lastDay)
                ? lastDay
                : null as int?;
        }

        /// <summary>Get a monster's possible drops.</summary>
        /// <param name="monster">The monster whose drops to get.</param>
        private IEnumerable<ItemDropData> GetMonsterDrops(Monster monster)
        {
            int[] drops = monster.objectsToDrop.ToArray();
            ItemDropData[] possibleDrops = this.GameHelper.GetMonsterData().First(p => p.Name == monster.Name).Drops;

            return (
                from possibleDrop in possibleDrops
                let isGuaranteed = drops.Contains(possibleDrop.ItemID)
                select new ItemDropData(
                    itemID: possibleDrop.ItemID,
                    minDrop: 1,
                    maxDrop: possibleDrop.MaxDrop,
                    probability: isGuaranteed ? 1 : possibleDrop.Probability
                )
            );
        }
    }
}
