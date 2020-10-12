using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Monsters;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters
{
    /// <summary>Provides lookup data for in-game characters.</summary>
    internal class CharacterLookupProvider : BaseLookupProvider
    {
        /*********
        ** Fields methods
        *********/
        /// <summary>The mod configuration.</summary>
        private readonly ModConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="config">The mod configuration.</param>
        public CharacterLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, ModConfig config)
            : base(reflection, gameHelper)
        {
            this.Config = config;
        }

        /// <inheritdoc />
        public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
        {
            // NPCs
            foreach (NPC npc in location.characters)
            {
                Vector2 entityTile = npc.getTileLocation();
                if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new CharacterTarget(this.GameHelper, this.GetSubjectType(npc), npc, entityTile, this.Reflection);
            }

            // animals
            foreach (FarmAnimal animal in (location as Farm)?.animals.Values ?? (location as AnimalHouse)?.animals.Values ?? Enumerable.Empty<FarmAnimal>())
            {
                Vector2 entityTile = animal.getTileLocation();
                if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new FarmAnimalTarget(this.GameHelper, animal, entityTile);
            }

            // players
            foreach (Farmer farmer in location.farmers)
            {
                Vector2 entityTile = farmer.getTileLocation();
                if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new FarmerTarget(this.GameHelper, farmer);
            }
        }

        /// <inheritdoc />
        public override ISubject GetSubject(IClickableMenu menu, int cursorX, int cursorY)
        {
            IClickableMenu targetMenu = (menu as GameMenu)?.GetCurrentPage() ?? menu;
            switch (targetMenu)
            {
                /****
                ** GameMenu
                ****/
                // skills tab
                case SkillsPage _:
                    return this.GetSubjectFor(Game1.player);

                // social tab
                case SocialPage socialPage:
                    {
                        // get villagers on current page
                        int scrollOffset = this.Reflection.GetField<int>(socialPage, "slotPosition").GetValue();
                        ClickableTextureComponent[] entries = this.Reflection
                            .GetField<List<ClickableTextureComponent>>(socialPage, "sprites")
                            .GetValue()
                            .Skip(scrollOffset)
                            .ToArray();

                        // find hovered villager
                        ClickableTextureComponent entry = entries.FirstOrDefault(p => p.containsPoint(cursorX, cursorY));
                        if (entry != null)
                        {
                            int index = Array.IndexOf(entries, entry) + scrollOffset;
                            object socialID = this.Reflection.GetField<List<object>>(socialPage, "names").GetValue()[index];
                            if (socialID is long playerID)
                            {
                                Farmer player = Game1.getFarmer(playerID);
                                return this.GetSubjectFor(player);
                            }
                            else if (socialID is string villagerName)
                            {
                                NPC npc = this.GameHelper.GetAllCharacters().FirstOrDefault(p => p.isVillager() && p.Name == villagerName);
                                if (npc != null)
                                    return this.GetSubjectFor(npc);
                            }
                        }
                    }
                    break;


                /****
                ** Other menus
                ****/
                // calendar
                case Billboard billboard:
                    {
                        // get target day
                        int selectedDay = -1;
                        for (int i = 0; i < billboard.calendarDays.Count; i++)
                        {
                            if (billboard.calendarDays[i].containsPoint(cursorX, cursorY))
                            {
                                selectedDay = i + 1;
                                break;
                            }
                        }
                        if (selectedDay == -1)
                            return null;

                        // get villager with a birthday on that date
                        NPC target = this.GameHelper.GetAllCharacters().FirstOrDefault(p => p.Birthday_Season == Game1.currentSeason && p.Birthday_Day == selectedDay);
                        if (target != null)
                            return this.GetSubjectFor(target);
                    }
                    break;

                // load menu
                case TitleMenu _ when TitleMenu.subMenu is LoadGameMenu loadMenu:
                    {
                        ClickableComponent button = loadMenu.slotButtons.FirstOrDefault(p => p.containsPoint(cursorX, cursorY));
                        if (button != null)
                        {
                            int index = this.Reflection.GetField<int>(loadMenu, "currentItemIndex").GetValue() + int.Parse(button.name);
                            var slots = this.Reflection.GetProperty<List<LoadGameMenu.MenuSlot>>(loadMenu, "MenuSlots").GetValue();
                            LoadGameMenu.SaveFileSlot slot = slots[index] as LoadGameMenu.SaveFileSlot;
                            if (slot?.Farmer != null)
                                return new FarmerSubject(this.GameHelper, slot.Farmer, isLoadMenu: true);
                        }
                    }
                    break;
            }

            return null;
        }

        /// <inheritdoc />
        public override ISubject GetSubject(ITarget target)
        {
            return target switch
            {
                FarmerTarget player => this.GetSubjectFor(player.Value),
                FarmAnimalTarget animal => this.GetSubjectFor(animal.Value),
                CharacterTarget npc => this.GetSubjectFor(npc.Value),
                _ => null
            };
        }

        /// <inheritdoc />
        public override ISubject GetSubjectFor(object entity)
        {
            return entity switch
            {
                FarmAnimal animal => new FarmAnimalSubject(this.GameHelper, animal),
                Farmer player => new FarmerSubject(this.GameHelper, player),
                NPC npc => new CharacterSubject(this.GameHelper, npc, this.GetSubjectType(npc), this.GameHelper.Metadata, this.Reflection, this.Config.ProgressionMode, this.Config.HighlightUnrevealedGiftTastes),
                _ => null
            };
        }

        /// <inheritdoc />
        public override IEnumerable<ISubject> GetSearchSubjects()
        {
            // NPCs
            foreach (NPC npc in Utility.getAllCharacters())
                yield return this.GetSubjectFor(npc);

            // animals
            foreach (var location in CommonHelper.GetLocations())
            {
                IEnumerable<FarmAnimal> animals =
                    (location as Farm)?.animals.Values
                    ?? (location as AnimalHouse)?.animals.Values;

                if (animals != null)
                {
                    foreach (var animal in animals)
                        yield return this.GetSubjectFor(animal);
                }
            }

            // players
            foreach (Farmer player in Game1.getAllFarmers())
                yield return this.GetSubjectFor(player);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the subject type for an NPC.</summary>
        /// <param name="npc">The NPC instance.</param>
        private SubjectType GetSubjectType(NPC npc)
        {
            return npc switch
            {
                Horse _ => SubjectType.Horse,
                Junimo _ => SubjectType.Junimo,
                Pet _ => SubjectType.Pet,
                Monster _ => SubjectType.Monster,
                _ => SubjectType.Villager
            };
        }
    }
}
