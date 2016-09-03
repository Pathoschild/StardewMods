using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Components;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Data;
using Pathoschild.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes an NPC (including villagers, monsters, and pets).</summary>
    public class CharacterSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The lookup target.</summary>
        private readonly Target<NPC> Target;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="target">The lookup target.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <remarks>Reverse engineered from <see cref="NPC"/>.</remarks>
        public CharacterSubject(Target<NPC> target, Metadata metadata)
            : base(target.Value.getName(), null, "NPC")
        {
            this.Target = target;
            NPC npc = target.Value;

            switch (target.Type)
            {
                case TargetType.Villager:
                    this.Type = "Villager";
                    this.AddCustomFields(
                        new GenericField("Birthday", $"{Utility.capitalizeFirstLetter(npc.birthday_Season)} {npc.birthday_Day}"),
                        new GenericField("Can romance", npc.datable),
                        new GenericField("Love interest", npc.loveInterest != "null" ? npc.loveInterest : "none"),
                        new CharacterGiftTastesField("Best gifts", this.GetGiftTastes(npc)),
                        new CharacterFriendshipField("Friendship", Game1.player.friendships[npc.name][0], NPC.friendshipPointsPerHeartLevel, NPC.maxFriendshipPoints),
                        new GenericField("Talked today", Game1.player.friendships[npc.name][2] == 1),
                        new GenericField("Gifted today", Game1.player.friendships[npc.name][3] > 0),
                        new GenericField("Gifted this week", $"{Game1.player.friendships[npc.name][1]} of {NPC.maxGiftsPerWeek}")
                    );
                    break;

                case TargetType.Pet:
                    this.Type = target.Value.GetType().Name;
                    Pet pet = (Pet)target.Value;
                    this.AddCustomFields(
                        new CharacterFriendshipField("Love", pet.friendshipTowardFarmer, Pet.maxFriendship / 10, Pet.maxFriendship),
                        new GenericField("Petted today", GameHelper.GetPrivateField<bool>(pet, "wasPetToday"))
                    );
                    break;

                case TargetType.Monster:
                    this.Type = "Monster";

                    // basic info
                    Monster monster = (Monster)target.Value;
                    string[] drops = (from id in monster.objectsToDrop let item = GameHelper.GetObjectBySpriteIndex(id) orderby item.Name select item.Name).ToArray();
                    this.AddCustomFields(
                        new GenericField("Invincible", $"For {GameHelper.GetPrivateField<int>(monster, "invincibleCountdown")} seconds", hasValue: monster.isInvincible()),
                        new PercentageBarField("Health", monster.health, monster.maxHealth, Color.Green, Color.Gray, $"{Math.Round((monster.health / (monster.maxHealth * 1f) * 100))}% ({monster.health} of {monster.maxHealth})"),
                        new GenericField("Will drop", drops.Any() ? string.Join(", ", drops) : "nothing"),
                        new GenericField("XP", monster.experienceGained),
                        new GenericField("Defence", monster.resilience),
                        new GenericField("Attack", monster.damageToFarmer)
                    );

                    // Adventure Guild quest
                    AdventureGuildQuestData adventureGuildQuest = metadata.GetAdventurerGuildQuest(monster.name);
                    if (adventureGuildQuest != null)
                    {
                        int kills = adventureGuildQuest.Targets.Select(p => Game1.stats.getMonstersKilled(p)).Sum();
                        this.AddCustomFields(new GenericField("Adventure Guild", $"{(kills >= adventureGuildQuest.RequiredKills ? "complete" : "in progress")} (killed {kills} of {adventureGuildQuest.RequiredKills})"));
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unknown NPC type '{target.Type}'");
            }
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch sprites, Vector2 position, Vector2 size)
        {
            NPC npc = this.Target.Value;

            // use character portrait (most NPCs)
            if (npc.Portrait != null)
            {
                sprites.DrawBlock(npc.Portrait, new Rectangle(0, 0, NPC.portrait_width, NPC.portrait_height), position.X, position.Y, Color.White, size.X / NPC.portrait_width);
                return true;
            }

            // else draw sprite (e.g. for pets)
            npc.Sprite.draw(sprites, position, 1, 0, 0, Color.White, scale: size.X / npc.Sprite.getWidth());
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get how much an NPC likes receiving each item as a gift.</summary>
        /// <param name="npc">The NPC.</param>
        private IDictionary<GiftTaste, Item[]> GetGiftTastes(NPC npc)
        {
            IDictionary<GiftTaste, List<Item>> tastes = new Dictionary<GiftTaste, List<Item>>();
            foreach (var objectInfo in Game1.objectInformation)
            {
                Object item = GameHelper.GetObjectBySpriteIndex(objectInfo.Key);
                if (!npc.canReceiveThisItemAsGift(item))
                    continue;
                try
                {
                    GiftTaste taste = (GiftTaste)npc.getGiftTasteForThisItem(item);
                    if (!tastes.ContainsKey(taste))
                        tastes[taste] = new List<Item>();
                    tastes[taste].Add(item);
                }
                catch (Exception)
                {
                    // some NPCs (e.g. dog) claim to allow gifts, but crash if you check their preference
                }
            }
            return tastes.ToDictionary(p => p.Key, p => p.Value.ToArray());
        }
    }
}