using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData;
using StardewValley.GameData.Pets;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters
{
    /// <summary>Describes an NPC (including villagers, monsters, and pets).</summary>
    internal class CharacterSubject : BaseSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>The NPC type.</summary>
        private readonly SubjectType TargetType;

        /// <summary>The lookup target.</summary>
        private readonly NPC Target;

        /// <summary>Provides subject entries.</summary>
        private readonly ISubjectRegistry Codex;

        /// <summary>Whether to only show content once the player discovers it.</summary>
        private readonly bool ProgressionMode;

        /// <summary>Whether to highlight item gift tastes which haven't been revealed in the NPC profile.</summary>
        private readonly bool HighlightUnrevealedGiftTastes;

        /// <summary>Which gift taste levels to show.</summary>
        private readonly ModGiftTasteConfig ShowGiftTastes;

        /// <summary>The configured minimum field values needed before they're auto-collapsed.</summary>
        private readonly ModCollapseLargeFieldsConfig CollapseFieldsConfig;

        /// <summary>Whether to look up the original entity when the game spawns a temporary copy.</summary>
        private readonly bool EnableTargetRedirection;

        /// <summary>Whether to show gift tastes that the player doesn't own somewhere in the world.</summary>
        private readonly bool ShowUnownedGifts;

        /// <summary>Whether the NPC is Gourmand in the Fern Islands farm cave.</summary>
        private readonly bool IsGourmand;

        /// <summary>Whether the NPC is a haunted skull monster.</summary>
        private readonly bool IsHauntedSkull;

        /// <summary>Whether the NPC is a magma sprite monster.</summary>
        private readonly bool IsMagmaSprite;

        /// <summary>Whether to disable portraits for this NPC, even if they'd normally be used.</summary>
        private readonly bool DisablePortraits;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="npc">The lookup target.</param>
        /// <param name="type">The NPC type.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <param name="progressionMode">Whether to only show content once the player discovers it.</param>
        /// <param name="highlightUnrevealedGiftTastes">Whether to highlight item gift tastes which haven't been revealed in the NPC profile.</param>
        /// <param name="showGiftTastes">Which gift taste levels to show.</param>
        /// <param name="collapseFieldsConfig">The configured minimum field values needed before they're auto-collapsed.</param>
        /// <param name="enableTargetRedirection">Whether to look up the original entity when the game spawns a temporary copy.</param>
        /// <param name="showUnownedGifts">Whether to show gift tastes that the player doesn't own somewhere in the world.</param>
        /// <remarks>Reverse engineered from <see cref="NPC"/>.</remarks>
        public CharacterSubject(ISubjectRegistry codex, GameHelper gameHelper, NPC npc, SubjectType type, Metadata metadata, bool progressionMode, bool highlightUnrevealedGiftTastes, ModGiftTasteConfig showGiftTastes, ModCollapseLargeFieldsConfig collapseFieldsConfig, bool enableTargetRedirection, bool showUnownedGifts)
            : base(gameHelper)
        {
            this.Codex = codex;
            this.ProgressionMode = progressionMode;
            this.HighlightUnrevealedGiftTastes = highlightUnrevealedGiftTastes;
            this.ShowGiftTastes = showGiftTastes;
            this.CollapseFieldsConfig = collapseFieldsConfig;
            this.EnableTargetRedirection = enableTargetRedirection;
            this.ShowUnownedGifts = showUnownedGifts;

            // initialize
            this.Target = npc;
            this.TargetType = type;
            CharacterData? overrides = metadata.GetCharacter(npc, type);
            this.Initialize(
                name: npc.getName(),
                description: overrides?.DescriptionKey != null ? (string?)I18n.GetByKey(overrides.DescriptionKey) : null,
                type: CharacterSubject.GetTypeName(npc, type)
            );

            // detect special cases
            if (npc is Bat bat)
            {
                this.IsHauntedSkull = bat.hauntedSkull.Value;
                this.IsMagmaSprite = bat.magmaSprite.Value;
            }
            else
                this.IsGourmand = type == SubjectType.Villager && npc.Name == "Gourmand" && npc.currentLocation.Name == nameof(IslandFarmCave);

            this.DisablePortraits = CharacterSubject.ShouldDisablePortraits(npc, this.IsGourmand);
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<ICustomField> GetData()
        {
            NPC npc = this.Target;
            return this.TargetType switch
            {
                SubjectType.Monster => this.GetDataForMonster((Monster)npc),
                SubjectType.Pet => this.GetDataForPet((Pet)npc),
                SubjectType.Villager => npc switch
                {
                    Child child => this.GetDataForChild(child),
                    TrashBear trashBear => this.GetDataForTrashBear(trashBear),
                    _ when this.IsGourmand => this.GetDataForGourmand(),
                    _ => this.GetDataForVillager(npc)
                },
                _ => Enumerable.Empty<ICustomField>()
            };
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
        public override IEnumerable<IDebugField> GetDebugFields()
        {
            NPC target = this.Target;
            Pet? pet = target as Pet;

            // pinned fields
            yield return new GenericDebugField("facing direction", this.Stringify((FacingDirection)target.FacingDirection), pinned: true);
            yield return new GenericDebugField("walking towards player", this.Stringify(target.IsWalkingTowardPlayer), pinned: true);
            if (Game1.player.friendshipData.ContainsKey(target.Name))
            {
                FriendshipModel friendship = this.GameHelper.GetFriendshipForVillager(Game1.player, target, Game1.player.friendshipData[target.Name]);
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

            // special cases
            if (this.IsHauntedSkull || this.IsMagmaSprite)
            {
                var sourceRect = Game1.getSourceRectForStandardTileSheet(npc.Sprite.Texture, 4, 16, 16);
                spriteBatch.Draw(npc.Sprite.Texture, position: position, sourceRectangle: sourceRect, color: Color.White, rotation: 0, origin: Vector2.Zero, scale: new Vector2(size.X / 16), effects: SpriteEffects.None, layerDepth: 1);
                return true;
            }

            // use character portrait (most villager NPCs)
            if (npc.IsVillager && !this.DisablePortraits)
            {
                spriteBatch.DrawSprite(npc.Portrait, new Rectangle(0, 0, NPC.portrait_width, NPC.portrait_height), position.X, position.Y, new Point(NPC.portrait_width, NPC.portrait_height), Color.White, size.X / NPC.portrait_width);
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
        /// <remarks>Derived from <see cref="Child.dayUpdate"/>.</remarks>
        private IEnumerable<ICustomField> GetDataForChild(Child child)
        {
            // birthday
            yield return new GenericField(I18n.Npc_Birthday(), this.GetChildBirthdayString(child));

            // age
            {
                ChildAge stage = (ChildAge)child.Age;
                int daysOld = child.daysOld.Value;
                int daysToNext = this.GetDaysToNextChildGrowth(stage, daysOld);
                bool isGrown = daysToNext == -1;
                int daysAtNext = daysOld + (isGrown ? 0 : daysToNext);

                string ageDesc = isGrown
                    ? I18n.Npc_Child_Age_DescriptionGrown(label: I18n.For(stage))
                    : I18n.Npc_Child_Age_DescriptionPartial(label: I18n.For(stage), count: daysToNext, nextLabel: I18n.For(stage + 1));

                yield return new PercentageBarField(I18n.Npc_Child_Age(), child.daysOld.Value, daysAtNext, Color.Green, Color.Gray, ageDesc);
            }

            // friendship
            if (Game1.player.friendshipData.ContainsKey(child.Name))
            {
                FriendshipModel friendship = this.GameHelper.GetFriendshipForVillager(Game1.player, child, Game1.player.friendshipData[child.Name]);
                yield return new CharacterFriendshipField(I18n.Npc_Friendship(), friendship);
                yield return new GenericField(I18n.Npc_TalkedToday(), this.Stringify(Game1.player.friendshipData[child.Name].TalkedToToday));
            }
        }

        /// <summary>Get the fields to display for the gourmand frog.</summary>
        /// <remarks>Derived from <see cref="IslandFarmCave.IndexForRequest"/>.</remarks>
        private IEnumerable<ICustomField> GetDataForGourmand()
        {
            // get cave
            IslandFarmCave? cave = (IslandFarmCave?)Game1.getLocationFromName("IslandFarmCave");
            if (cave == null)
                yield break;
            int questsDone = cave.gourmandRequestsFulfilled.Value;
            int maxQuests = IslandFarmCave.TOTAL_GOURMAND_REQUESTS;

            // show items wanted
            if (questsDone <= maxQuests)
            {
                var checkboxes = new List<KeyValuePair<IFormattedText[], bool>>();
                for (int i = 0; i < maxQuests; i++)
                {
                    string wantedKey = cave.IndexForRequest(i);
                    if (!CommonHelper.IsItemId(wantedKey))
                        continue;

                    checkboxes.Add(
                        CheckboxListField.Checkbox(
                            text: ItemRegistry.GetDataOrErrorItem(wantedKey).DisplayName,
                            value: questsDone > i
                        )
                    );
                }

                if (checkboxes.Any())
                    yield return new CheckboxListField(I18n.TrashBearOrGourmand_ItemWanted(), checkboxes);
            }

            // show progress
            yield return new GenericField(I18n.TrashBearOrGourmand_QuestProgress(), I18n.Generic_Ratio(questsDone, maxQuests));
        }

        /// <summary>Get the fields to display for a monster.</summary>
        /// <param name="monster">The monster for which to show info.</param>
        /// <remarks>Derived from <see cref="Monster.parseMonsterInfo"/>.</remarks>
        private IEnumerable<ICustomField> GetDataForMonster(Monster monster)
        {
            // basic info
            bool canRerollDrops = Game1.player.isWearingRing(Ring.BurglarsRingId);

            yield return new GenericField(I18n.Monster_Invincible(), I18n.Generic_Seconds(count: monster.invincibleCountdown), hasValue: monster.isInvincible());
            yield return new PercentageBarField(I18n.Monster_Health(), monster.Health, monster.MaxHealth, Color.Green, Color.Gray, I18n.Generic_PercentRatio(percent: (int)Math.Round((monster.Health / (monster.MaxHealth * 1f) * 100)), value: monster.Health, max: monster.MaxHealth));
            yield return new ItemDropListField(this.GameHelper, I18n.Monster_Drops(), this.GetMonsterDrops(monster), fadeNonGuaranteed: true, crossOutNonGuaranteed: !canRerollDrops, defaultText: I18n.Monster_Drops_Nothing());
            yield return new GenericField(I18n.Monster_Experience(), this.Stringify(monster.ExperienceGained));
            yield return new GenericField(I18n.Monster_Defense(), this.Stringify(monster.resilience.Value));
            yield return new GenericField(I18n.Monster_Attack(), this.Stringify(monster.DamageToFarmer));

            // Adventure Guild quest
            foreach (MonsterSlayerQuestData questData in DataLoader.MonsterSlayerQuests(Game1.content).Values)
            {
                if (questData.Targets?.Contains(monster.Name) is not true)
                    continue;

                int kills = questData.Targets.Sum(Game1.stats.getMonstersKilled);
                string goalName = TokenParser.ParseText(questData.DisplayName);
                var checkbox = CheckboxListField.Checkbox(
                    text: I18n.Monster_AdventureGuild_EradicationGoal(name: goalName, count: kills, requiredCount: questData.Count),
                    value: kills >= questData.Count
                );
                yield return new CheckboxListField(I18n.Monster_AdventureGuild(), checkbox);
            }
        }

        /// <summary>Get the fields to display for a pet.</summary>
        /// <param name="pet">The pet for which to show info.</param>
        /// <remarks>Derived from <see cref="Pet.checkAction"/> and <see cref="Pet.dayUpdate"/>.</remarks>
        private IEnumerable<ICustomField> GetDataForPet(Pet pet)
        {
            // friendship
            yield return new CharacterFriendshipField(I18n.Pet_Love(), this.GameHelper.GetFriendshipForPet(Game1.player, pet));

            // petted today / last petted
            int? lastDayPetted = this.GetLastDayPetted(pet, Game1.player.UniqueMultiplayerID);
            yield return new GenericField(I18n.Pet_PettedToday(), lastDayPetted == Game1.Date.TotalDays ? I18n.Pet_LastPetted_Yes() : this.Stringify(false));
            if (!lastDayPetted.HasValue)
                yield return new GenericField(I18n.Pet_LastPetted(), I18n.Pet_LastPetted_Never());
            else if (lastDayPetted != Game1.Date.TotalDays)
            {
                int daysSincePetted = Game1.Date.TotalDays - lastDayPetted.Value;
                yield return new GenericField(I18n.Pet_LastPetted(), daysSincePetted == 1 ? I18n.Generic_Yesterday() : I18n.Pet_LastPetted_DaysAgo(daysSincePetted));
            }

            // water bowl
            PetBowl bowl = pet.GetPetBowl();
            if (bowl != null)
                yield return new GenericField(I18n.Pet_WaterBowl(), bowl.watered.Value ? I18n.Pet_WaterBowl_Filled() : I18n.Pet_WaterBowl_Empty());
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
                trashBear.updateItemWanted();
                yield return new ItemIconField(this.GameHelper, I18n.TrashBearOrGourmand_ItemWanted(), ItemRegistry.Create(trashBear.itemWantedIndex), this.Codex);
            }

            // show progress
            yield return new GenericField(I18n.TrashBearOrGourmand_QuestProgress(), I18n.Generic_Ratio(questsDone, maxQuests));
        }

        /// <summary>Get the fields to display for a villager NPC.</summary>
        /// <param name="npc">The NPC for which to show info.</param>
        private IEnumerable<ICustomField> GetDataForVillager(NPC npc)
        {
            // special case: Abigail in the mines is a temporary instance with the name
            // 'AbigailMine', so the info shown will be incorrect.
            if (this.EnableTargetRedirection && npc.Name == "AbigailMine" && npc.currentLocation?.Name == "UndergroundMine20")
                npc = Game1.getCharacterFromName("Abigail") ?? npc;

            // social fields (birthday, friendship, gifting, etc)
            if (this.GameHelper.IsSocialVillager(npc))
            {
                // birthday
                if (this.GameHelper.TryGetDate(npc.Birthday_Day, npc.Birthday_Season, out SDate birthday))
                    yield return new GenericField(I18n.Npc_Birthday(), I18n.Stringify(birthday));

                // friendship
                if (Game1.player.friendshipData.ContainsKey(npc.Name))
                {
                    // friendship/romance
                    FriendshipModel friendship = this.GameHelper.GetFriendshipForVillager(Game1.player, npc, Game1.player.friendshipData[npc.Name]);
                    yield return new GenericField(I18n.Npc_CanRomance(), friendship.IsSpouse ? I18n.Npc_CanRomance_Married() : friendship.IsHousemate ? I18n.Npc_CanRomance_Housemate() : this.Stringify(friendship.CanDate));
                    yield return new CharacterFriendshipField(I18n.Npc_Friendship(), friendship);

                    // talked/gifted today
                    yield return new GenericField(I18n.Npc_TalkedToday(), this.Stringify(friendship.TalkedToday));
                    yield return new GenericField(I18n.Npc_GiftedToday(), this.Stringify(friendship.GiftsToday > 0));

                    // kissed/hugged today
                    if (friendship.IsSpouse || friendship.IsHousemate)
                        yield return new GenericField(friendship.IsSpouse ? I18n.Npc_KissedToday() : I18n.Npc_HuggedToday(), this.Stringify(npc.hasBeenKissedToday.Value));

                    // gifted this week
                    if (!friendship.IsSpouse && !friendship.IsHousemate)
                        yield return new GenericField(I18n.Npc_GiftedThisWeek(), I18n.Generic_Ratio(value: friendship.GiftsThisWeek, max: NPC.maxGiftsPerWeek));
                }
                else
                    yield return new GenericField(I18n.Npc_Friendship(), I18n.Npc_Friendship_NotMet());

                // gift tastes
                {
                    IDictionary<GiftTaste, GiftTasteModel[]> giftTastes = this.GetGiftTastes(npc);
                    IDictionary<string, bool> ownedItems = CharacterGiftTastesField.GetOwnedItemsCache(this.GameHelper);

                    if (this.ShowGiftTastes.Loved)
                        yield return this.GetGiftTasteField(I18n.Npc_LovesGifts(), giftTastes, ownedItems, GiftTaste.Love);
                    if (this.ShowGiftTastes.Liked)
                        yield return this.GetGiftTasteField(I18n.Npc_LikesGifts(), giftTastes, ownedItems, GiftTaste.Like);
                    if (this.ShowGiftTastes.Neutral)
                        yield return this.GetGiftTasteField(I18n.Npc_NeutralGifts(), giftTastes, ownedItems, GiftTaste.Neutral);
                    if (this.ShowGiftTastes.Disliked)
                        yield return this.GetGiftTasteField(I18n.Npc_DislikesGifts(), giftTastes, ownedItems, GiftTaste.Dislike);
                    if (this.ShowGiftTastes.Hated)
                        yield return this.GetGiftTasteField(I18n.Npc_HatesGifts(), giftTastes, ownedItems, GiftTaste.Hate);
                }
            }
        }

        /// <summary>Get a list of gift tastes for an NPC.</summary>
        /// <param name="label">The field label.</param>
        /// <param name="giftTastes">The gift taste data.</param>
        /// <param name="ownedItemsCache">A lookup cache for owned items, as created by <see cref="CharacterGiftTastesField.GetOwnedItemsCache"/>.</param>
        /// <param name="taste">The gift taste to display.</param>
        private ICustomField GetGiftTasteField(string label, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, IDictionary<string, bool> ownedItemsCache, GiftTaste taste)
        {
            var field = new CharacterGiftTastesField(label, giftTastes, taste, onlyRevealed: this.ProgressionMode, highlightUnrevealed: this.HighlightUnrevealedGiftTastes, onlyOwned: !this.ShowUnownedGifts, ownedItemsCache);
            if (this.CollapseFieldsConfig.Enabled && giftTastes.TryGetValue(taste, out GiftTasteModel[]? tastes) && tastes.Length >= this.CollapseFieldsConfig.NpcGiftTastes)
                field.CollapseByDefault(I18n.Generic_ShowXResults(tastes.Length));
            return field;
        }

        /*****
        ** Other
        ****/
        /// <summary>Get the display type for a character.</summary>
        /// <param name="npc">The lookup target.</param>
        /// <param name="type">The NPC type.</param>
        private static string GetTypeName(Character npc, SubjectType type)
        {
            switch (type)
            {
                case SubjectType.Villager:
                    return I18n.Type_Villager();

                case SubjectType.Horse:
                    return GameI18n.GetString("Strings\\StringsFromCSFiles:StrengthGame.cs.11665");

                case SubjectType.Monster:
                    return I18n.Type_Monster();

                case SubjectType.Pet when npc is Pet pet && Pet.TryGetData(pet.petType.Value, out PetData petData):
                    {
                        string typeName = TokenParser.ParseText(petData.DisplayName);
                        if (typeName.Length > 1)
                            typeName = char.ToUpperInvariant(typeName[0]) + typeName.Substring(1);
                        return typeName;
                    }

                default:
                    return npc.GetType().Name;
            }
        }

        /// <summary>Get whether to disable portraits for an NPC, even if they'd normally be shown.</summary>
        /// <param name="npc">The NPC to check.</param>
        /// <param name="isGourmand">Whether the NPC is Gourmand in the Fern Islands farm cave.</param>
        /// <remarks>Most code should use the cached <see cref="DisablePortraits"/> instead.</remarks>
        private static bool ShouldDisablePortraits(NPC npc, bool isGourmand)
        {
            // at the Spirit's Eve festival, the monsters are spawned as villagers
            if (Game1.CurrentEvent?.id is "festival_fall27" && npc.Name is "Mummy" or "Stone Golem" or "Wilderness Golem")
                return true;

            // Gourmand uses Professor Snail's portraits
            if (isGourmand)
                return true;

            // if the portraits fail to load, this will log the warning once instead of failing on every draw loop
            return npc.Portrait is null;
        }

        /// <summary>Get how much an NPC likes receiving each item as a gift.</summary>
        /// <param name="npc">The NPC.</param>
        private IDictionary<GiftTaste, GiftTasteModel[]> GetGiftTastes(NPC npc)
        {
            return this.GameHelper.GetGiftTastes(npc)
                .GroupBy(entry => entry.Taste)
                .ToDictionary(
                    tasteGroup => tasteGroup.Key,
                    tasteGroup => tasteGroup.ToArray()
                );
        }

        /// <summary>Get a child's translated birthday based on their <see cref="Child.daysOld"/> field.</summary>
        /// <param name="child">The child instance.</param>
        private string GetChildBirthdayString(Child child)
        {
            int daysOld = child.daysOld.Value;

            try
            {
                return SDate
                    .Now()
                    .AddDays(-daysOld)
                    .ToLocaleString(withYear: true);
            }
            catch (ArithmeticException)
            {
                // The player probably changed the game date, so the birthday would be before the
                // game started. We'll just drop the year number from the output in that case.
                return new SDate(Game1.dayOfMonth, Game1.season, 100_000_000)
                    .AddDays(-daysOld)
                    .ToLocaleString(withYear: false);
            }
        }

        /// <summary>Get the number of days until a child grows to the next stage.</summary>
        /// <param name="stage">The child's current growth stage.</param>
        /// <param name="daysOld">The child's current age in days.</param>
        /// <returns>Returns a number of days, or <c>-1</c> if the child won't grow any further.</returns>
        /// <remarks>Derived from <see cref="Child.dayUpdate"/>.</remarks>
        private int GetDaysToNextChildGrowth(ChildAge stage, int daysOld)
        {
            return stage switch
            {
                ChildAge.Newborn => 13 - daysOld,
                ChildAge.Baby => 27 - daysOld,
                ChildAge.Crawler => 55 - daysOld,
                _ => -1
            };
        }

        /// <summary>Get the last day when the given player petted the pet.</summary>
        /// <param name="pet">The pet to check.</param>
        /// <param name="playerID">The unique multiplayer ID for the player to check.</param>
        private int? GetLastDayPetted(Pet pet, long playerID)
        {
            return pet.lastPetDay.TryGetValue(playerID, out int lastDay)
                ? lastDay
                : null;
        }

        /// <summary>Get a monster's possible drops.</summary>
        /// <param name="monster">The monster whose drops to get.</param>
        private IEnumerable<ItemDropData> GetMonsterDrops(Monster monster)
        {
            // get possible drops
            ItemDropData[]? possibleDrops = this.GameHelper.GetMonsterData().FirstOrDefault(p => p.Name == monster.Name)?.Drops;
            if (this.IsHauntedSkull)
                possibleDrops ??= this.GameHelper.GetMonsterData().FirstOrDefault(p => p.Name == "Lava Bat")?.Drops; // haunted skulls use lava bat data
            possibleDrops ??= Array.Empty<ItemDropData>();

            // get actual drops
            IDictionary<string, List<ItemDropData>> dropsLeft = monster
                .objectsToDrop
                .Select(this.GetActualDrop)
                .GroupBy(p => p.ItemID)
                .ToDictionary(group => group.Key, group => group.ToList());

            // return possible drops
            foreach (var drop in possibleDrops.OrderByDescending(p => p.Probability))
            {
                bool isGuaranteed = dropsLeft.TryGetValue(drop.ItemID, out List<ItemDropData>? actualDrops) && actualDrops.Any();
                if (isGuaranteed)
                {
                    ItemDropData[] matches = actualDrops!.Where(p => p.MinDrop >= drop.MinDrop && p.MaxDrop <= drop.MaxDrop).ToArray();
                    ItemDropData? bestMatch =
                        matches.FirstOrDefault(p => p.MinDrop == drop.MinDrop && p.MaxDrop == drop.MaxDrop)
                        ?? matches.FirstOrDefault();
                    if (bestMatch is not null)
                        actualDrops!.Remove(bestMatch);
                }

                yield return new ItemDropData(
                    ItemID: drop.ItemID,
                    MinDrop: 1,
                    MaxDrop: drop.MaxDrop,
                    Probability: isGuaranteed ? 1 : drop.Probability
                );
            }

            // special case: return guaranteed drops that weren't matched
            foreach (var pair in dropsLeft.Where(p => p.Value.Any()))
            {
                foreach (var drop in pair.Value)
                    yield return drop;
            }
        }

        /// <summary>Get the drop info for a <see cref="Monster.objectsToDrop"/> ID, if it's valid.</summary>
        /// <param name="id">The ID to parse.</param>
        /// <remarks>Derived from <see cref="GameLocation.monsterDrop"/> and the <see cref="Debris"/> constructor.</remarks>
        private ItemDropData GetActualDrop(string id)
        {
            // basic info
            int minDrop = 1;
            int maxDrop = 1;

            // negative ID means the monster will drop 1-3 of the item
            if (int.TryParse(id, out int numericId) && numericId < 0)
            {
                id = (-numericId).ToString();
                maxDrop = 3;
            }

            // handle hardcoded ID mappings in Debris constructor
            id = id switch
            {
                "0" => SObject.copper.ToString(),
                "2" => SObject.iron.ToString(),
                "4" => SObject.coal.ToString(),
                "6" => SObject.gold.ToString(),
                "10" => SObject.iridium.ToString(),
                "12" => SObject.wood.ToString(),
                "14" => SObject.stone.ToString(),
                _ => id
            };

            // build model
            return new ItemDropData(ItemID: id, MinDrop: minDrop, MaxDrop: maxDrop, Probability: 1);
        }
    }
}
