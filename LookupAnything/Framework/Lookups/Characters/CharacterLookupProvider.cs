using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
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
            // Gourmand NPC
            if (location is IslandFarmCave islandFarmCave && islandFarmCave.gourmand != null)
            {
                NPC gourmand = islandFarmCave.gourmand;
                yield return new CharacterTarget(this.GameHelper, this.GetSubjectType(gourmand), gourmand, gourmand.getTileLocation(), this.Reflection, () => this.BuildSubject(gourmand));
            }

            // NPCs
            foreach (NPC npc in location.characters)
            {
                Vector2 entityTile = npc.getTileLocation();
                if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new CharacterTarget(this.GameHelper, this.GetSubjectType(npc), npc, entityTile, this.Reflection, () => this.BuildSubject(npc));
            }

            // animals
            foreach (FarmAnimal animal in (location as Farm)?.animals.Values ?? (location as AnimalHouse)?.animals.Values ?? Enumerable.Empty<FarmAnimal>())
            {
                Vector2 entityTile = animal.getTileLocation();
                if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new FarmAnimalTarget(this.GameHelper, animal, entityTile, () => this.BuildSubject(animal));
            }

            // players
            foreach (Farmer farmer in location.farmers)
            {
                Vector2 entityTile = farmer.getTileLocation();
                if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new FarmerTarget(this.GameHelper, farmer, () => this.BuildSubject(farmer));
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
                    return this.BuildSubject(Game1.player);

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
                                return this.BuildSubject(player);
                            }
                            else if (socialID is string villagerName)
                            {
                                NPC npc = this.GameHelper.GetAllCharacters().FirstOrDefault(p => p.isVillager() && p.Name == villagerName);
                                if (npc != null)
                                    return this.BuildSubject(npc);
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
                            return this.BuildSubject(target);
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
        public override ISubject GetSubjectFor(object entity)
        {
            return entity switch
            {
                FarmAnimal animal => this.BuildSubject(animal),
                Farmer player => this.BuildSubject(player),
                NPC npc => this.BuildSubject(npc),
                _ => null
            };
        }

        /// <inheritdoc />
        public override IEnumerable<ISubject> GetSearchSubjects()
        {
            // NPCs
            foreach (NPC npc in Utility.getAllCharacters())
                yield return this.BuildSubject(npc);

            // animals
            foreach (var location in CommonHelper.GetLocations())
            {
                IEnumerable<FarmAnimal> animals =
                    (location as Farm)?.animals.Values
                    ?? (location as AnimalHouse)?.animals.Values;

                if (animals != null)
                {
                    foreach (var animal in animals)
                        yield return this.BuildSubject(animal);
                }
            }

            // players
            foreach (Farmer player in Game1.getAllFarmers())
                yield return this.BuildSubject(player);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Build a subject.</summary>
        /// <param name="player">The entity to look up.</param>
        private ISubject BuildSubject(Farmer player)
        {
            return new FarmerSubject(this.GameHelper, player);
        }

        /// <summary>Build a subject.</summary>
        /// <param name="animal">The entity to look up.</param>
        private ISubject BuildSubject(FarmAnimal animal)
        {
            return new FarmAnimalSubject(this.GameHelper, animal);
        }

        /// <summary>Build a subject.</summary>
        /// <param name="npc">The entity to look up.</param>
        private ISubject BuildSubject(NPC npc)
        {
            return new CharacterSubject(this.GameHelper, npc, this.GetSubjectType(npc), this.GameHelper.Metadata, this.Reflection, this.Config.ProgressionMode, this.Config.HighlightUnrevealedGiftTastes);
        }

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
