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

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters;

/// <summary>Provides lookup data for in-game characters.</summary>
internal class CharacterLookupProvider : BaseLookupProvider
{
    /*********
    ** Fields methods
    *********/
    /// <summary>The mod configuration.</summary>
    private readonly Func<ModConfig> Config;

    /// <summary>Provides subject entries.</summary>
    private readonly ISubjectRegistry Codex;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="reflection">Simplifies access to private game code.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="config">The mod configuration.</param>
    /// <param name="codex">Provides subject entries.</param>
    public CharacterLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, Func<ModConfig> config, ISubjectRegistry codex)
        : base(reflection, gameHelper)
    {
        this.Config = config;
        this.Codex = codex;
    }

    /// <inheritdoc />
    public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
    {
        // Gourmand NPC
        if (location is IslandFarmCave { gourmand: not null } islandFarmCave)
        {
            NPC gourmand = islandFarmCave.gourmand;
            yield return new CharacterTarget(this.GameHelper, this.GetSubjectType(gourmand), gourmand, gourmand.Tile, () => this.BuildSubject(gourmand));
        }

        // NPCs
        foreach (NPC npc in Game1.CurrentEvent?.actors ?? (IEnumerable<NPC>)location.characters)
        {
            Vector2 entityTile = npc.Tile;
            if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                yield return new CharacterTarget(this.GameHelper, this.GetSubjectType(npc), npc, entityTile, () => this.BuildSubject(npc));
        }

        // animals
        foreach (FarmAnimal animal in location.Animals.Values)
        {
            Vector2 entityTile = animal.Tile;
            if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                yield return new FarmAnimalTarget(this.GameHelper, animal, entityTile, () => this.BuildSubject(animal));
        }

        // players
        foreach (Farmer farmer in location.farmers)
        {
            Vector2 entityTile = farmer.Tile;
            if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                yield return new FarmerTarget(this.GameHelper, farmer, () => this.BuildSubject(farmer));
        }
    }

    /// <inheritdoc />
    public override ISubject? GetSubject(IClickableMenu menu, int cursorX, int cursorY)
    {
        IClickableMenu targetMenu = this.GameHelper.GetGameMenuPage(menu) ?? menu;
        switch (targetMenu)
        {
            /****
            ** GameMenu
            ****/
            // skills tab
            case SkillsPage:
                return this.BuildSubject(Game1.player);

            // profile tab
            case ProfileMenu profileMenu:
                if (profileMenu.hoveredItem == null)
                {
                    Character character = profileMenu.Current.Character;
                    if (character != null)
                        return this.Codex.GetByEntity(character, character.currentLocation);
                }
                break;

            // social tab
            case SocialPage socialPage:
                foreach (ClickableTextureComponent slot in socialPage.characterSlots)
                {
                    if (slot.containsPoint(cursorX, cursorY))
                    {
                        SocialPage.SocialEntry entry = socialPage.SocialEntries[slot.myID];

                        switch (entry.Character)
                        {
                            case Farmer player:
                                return this.BuildSubject(player);

                            case NPC npc:
                                return this.BuildSubject(npc);
                        }
                    }
                }
                break;

            /****
            ** Calendar
            ****/
            case Billboard { calendarDays: not null } billboard: // Billboard used for both calendar and 'help wanted'
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
                    NPC? target = this.GameHelper
                        .GetAllCharacters()
                        .Where(p => p.Birthday_Season == Game1.currentSeason && p.Birthday_Day == selectedDay)
                        .MaxBy(p => p.CanSocialize); // SVE duplicates the Marlon NPC, but only one of them is marked social
                    if (target != null)
                        return this.BuildSubject(target);
                }
                break;

            /****
            ** Load menu
            ****/
            case TitleMenu when TitleMenu.subMenu is LoadGameMenu loadMenu:
                {
                    ClickableComponent? button = loadMenu.slotButtons.FirstOrDefault(p => p.containsPoint(cursorX, cursorY));
                    if (button != null)
                    {
                        int index = loadMenu.currentItemIndex + int.Parse(button.name);
                        LoadGameMenu.SaveFileSlot? slot = loadMenu.MenuSlots[index] as LoadGameMenu.SaveFileSlot;
                        if (slot?.Farmer != null)
                            return new FarmerSubject(this.GameHelper, slot.Farmer, isLoadMenu: true);
                    }
                }
                break;

            /****
            ** mod: Animal Social Menu
            ****/
            case not null when targetMenu.GetType().FullName == "AnimalSocialMenu.Framework.AnimalSocialPage":
                {
                    int slotOffset = this.Reflection.GetField<int>(targetMenu, "SlotPosition").GetValue();
                    List<ClickableTextureComponent> slots = this.Reflection.GetField<List<ClickableTextureComponent>>(targetMenu, "Sprites").GetValue();
                    List<object> animalIds = this.Reflection.GetField<List<object>>(targetMenu, "Names").GetValue();

                    for (int i = slotOffset; i < slots.Count; i++)
                    {
                        if (slots[i].containsPoint(cursorX, cursorY))
                        {
                            if (animalIds.TryGetIndex(i, out object? rawId) && rawId is long id)
                            {
                                FarmAnimal? animal = Game1
                                    .getFarm()
                                    .getAllFarmAnimals()
                                    .FirstOrDefault(p => p.myID.Value == id);

                                if (animal != null)
                                    return this.BuildSubject(animal);
                            }
                            break;
                        }
                    }
                }
                break;

            /****
            ** By convention for mod menus
            ****/
            case not null:
                {
                    NPC? npc =
                        this.Reflection.GetField<NPC>(targetMenu, "hoveredNpc", required: false)?.GetValue()
                        ?? this.Reflection.GetField<NPC>(targetMenu, "HoveredNpc", required: false)?.GetValue();
                    if (npc is not null)
                        return this.BuildSubject(npc);
                }
                break;
        }

        return null;
    }

    /// <inheritdoc />
    public override ISubject? GetSubjectFor(object entity, GameLocation? location)
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
        HashSet<string> seen = [];

        // NPCs
        foreach (NPC npc in Utility.getAllCharacters())
        {
            if (seen.Add($"NPC:{npc.Name}"))
                yield return this.BuildSubject(npc);
        }

        // animals
        foreach (GameLocation location in CommonHelper.GetLocations())
        {
            foreach (FarmAnimal animal in location.Animals.Values)
            {
                if (seen.Add($"Animal:{animal.myID}"))
                    yield return this.BuildSubject(animal);
            }
        }

        // players
        foreach (Farmer player in Game1.getAllFarmers())
        {
            if (seen.Add($"Farmer:{player.UniqueMultiplayerID}"))
                yield return this.BuildSubject(player);
        }
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
        return new FarmAnimalSubject(this.Codex, this.GameHelper, animal);
    }

    /// <summary>Build a subject.</summary>
    /// <param name="npc">The entity to look up.</param>
    private ISubject BuildSubject(NPC npc)
    {
        ModConfig config = this.Config();

        return new CharacterSubject(
            codex: this.Codex,
            gameHelper: this.GameHelper,
            npc: npc,
            type: this.GetSubjectType(npc),
            metadata: this.GameHelper.Metadata,
            showUnknownGiftTastes: config.ShowUnknownGiftTastes,
            highlightUnrevealedGiftTastes: config.HighlightUnrevealedGiftTastes,
            showGiftTastes: config.ShowGiftTastes,
            collapseFieldsConfig: config.CollapseLargeFields,
            enableTargetRedirection: config.EnableTargetRedirection,
            showUnownedGifts: config.ShowUnownedGifts
        );
    }

    /// <summary>Get the subject type for an NPC.</summary>
    /// <param name="npc">The NPC instance.</param>
    private SubjectType GetSubjectType(NPC npc)
    {
        return npc switch
        {
            Horse => SubjectType.Horse,
            Junimo => SubjectType.Junimo,
            Pet => SubjectType.Pet,
            Monster => SubjectType.Monster,
            _ => SubjectType.Villager
        };
    }
}
