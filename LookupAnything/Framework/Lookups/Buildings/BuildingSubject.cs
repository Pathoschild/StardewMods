using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FishPonds;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.TokenizableStrings;
using xTile;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Buildings;

/// <summary>Describes a constructed building.</summary>
internal class BuildingSubject : BaseSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The lookup target.</summary>
    private readonly Building Target;

    /// <summary>The building's source rectangle in its spritesheet.</summary>
    private readonly Rectangle SourceRectangle;

    /// <summary>Provides subject entries.</summary>
    private readonly ISubjectRegistry Codex;

    /// <summary>The configured minimum field values needed before they're auto-collapsed.</summary>
    private readonly ModCollapseLargeFieldsConfig CollapseFieldsConfig;

    /// <summary>Whether to show recipes involving error items.</summary>
    private readonly bool ShowInvalidRecipes;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="codex">Provides subject entries.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="building">The lookup target.</param>
    /// <param name="sourceRectangle">The building's source rectangle in its spritesheet.</param>
    /// <param name="collapseFieldsConfig">The configured minimum field values needed before they're auto-collapsed.</param>
    /// <param name="showInvalidRecipes">Whether to show recipes involving error items.</param>
    public BuildingSubject(ISubjectRegistry codex, GameHelper gameHelper, Building building, Rectangle sourceRectangle, ModCollapseLargeFieldsConfig collapseFieldsConfig, bool showInvalidRecipes)
        : base(gameHelper, building.buildingType.Value, null, I18n.Type_Building())
    {
        // init
        this.Codex = codex;
        this.Target = building;
        this.SourceRectangle = sourceRectangle;
        this.CollapseFieldsConfig = collapseFieldsConfig;
        this.ShowInvalidRecipes = showInvalidRecipes;

        // get name/description from data if available
        BuildingData? buildingData = building.GetData();
        this.Name = TokenParser.ParseText(buildingData?.Name) ?? this.Name;
        this.Description = TokenParser.ParseText(buildingData?.Description) ?? this.Description;
    }

    /// <inheritdoc />
    public override IEnumerable<ICustomField> GetData()
    {
        // get info
        Building building = this.Target;
        var data = building.GetData();
        bool built = !building.isUnderConstruction();
        int? upgradeLevel = this.GetUpgradeLevel(building);

        // added by mod
        {
            IModInfo? fromMod = this.GameHelper.TryGetModFromStringId(building.buildingType.Value);
            if (fromMod != null)
                yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(modName: fromMod.Manifest.Name));
        }

        // construction / upgrade
        if (!built || building.daysUntilUpgrade.Value > 0)
        {
            int daysLeft = building.isUnderConstruction() ? building.daysOfConstructionLeft.Value : building.daysUntilUpgrade.Value;
            SDate readyDate = SDate.Now().AddDays(daysLeft);
            yield return new GenericField(I18n.Building_Construction(), I18n.Building_Construction_Summary(date: this.Stringify(readyDate)));
        }

        // owner
        Farmer? owner = this.GetOwner();
        if (owner != null)
            yield return new LinkField(I18n.Building_Owner(), owner.Name, () => this.Codex.GetByEntity(owner, owner.currentLocation)!);
        else if (building.GetIndoors() is Cabin)
            yield return new GenericField(I18n.Building_Owner(), I18n.Building_Owner_None());

        // stable horse
        if (built && building is Stable stable)
        {
            Horse horse = Utility.findHorse(stable.HorseId);
            if (horse != null)
            {
                yield return new LinkField(I18n.Building_Horse(), horse.Name, () => this.Codex.GetByEntity(horse, horse.currentLocation)!);
                yield return new GenericField(I18n.Building_HorseLocation(), I18n.Building_HorseLocation_Summary(location: horse.currentLocation.Name, x: horse.TilePoint.X, y: horse.TilePoint.Y));
            }
        }

        // animals
        if (built && building.GetIndoors() is AnimalHouse animalHouse)
        {
            // animal counts
            yield return new GenericField(I18n.Building_Animals(), I18n.Building_Animals_Summary(count: animalHouse.animalsThatLiveHere.Count, max: animalHouse.animalLimit.Value));

            // feed trough
            if (upgradeLevel >= 2 && (this.IsBarn(building) || this.IsCoop(building)))
                yield return new GenericField(I18n.Building_FeedTrough(), I18n.Building_FeedTrough_Automated());
            else
            {
                this.GetFeedMetrics(animalHouse, out int totalFeedSpaces, out int filledFeedSpaces);
                yield return new GenericField(I18n.Building_FeedTrough(), I18n.Building_FeedTrough_Summary(filled: filledFeedSpaces, max: totalFeedSpaces));
            }
        }

        // slimes
        if (built && building.GetIndoors() is SlimeHutch slimeHutch)
        {
            // slime count
            int slimeCount = slimeHutch.characters.OfType<GreenSlime>().Count();
            yield return new GenericField(I18n.Building_Slimes(), I18n.Building_Slimes_Summary(count: slimeCount, max: 20));

            // water trough
            yield return new GenericField(I18n.Building_WaterTrough(), I18n.Building_WaterTrough_Summary(filled: slimeHutch.waterSpots.Count(p => p), max: slimeHutch.waterSpots.Count));
        }

        // upgrade level
        if (built)
        {
            var upgradeLevelSummary = this.GetUpgradeLevelSummary(building, upgradeLevel).ToArray();
            if (upgradeLevelSummary.Any())
                yield return new CheckboxListField(I18n.Building_Upgrades(), new CheckboxList(upgradeLevelSummary));
        }

        // specific buildings
        if (built)
        {
            switch (building)
            {
                // fish pond
                case FishPond pond:
                    if (!CommonHelper.IsItemId(pond.fishType.Value))
                        yield return new GenericField(I18n.Building_FishPond_Population(), I18n.Building_FishPond_Population_Empty());
                    else
                    {
                        // get fish population
                        SObject fish = pond.GetFishObject();
                        fish.Stack = pond.FishCount;
                        var pondData = pond.GetFishPondData();

                        // population field
                        {
                            string populationStr = $"{fish.DisplayName} ({I18n.Generic_Ratio(pond.FishCount, pond.maxOccupants.Value)})";
                            if (pond.FishCount < pond.maxOccupants.Value)
                            {
                                SDate nextSpawn = SDate.Now().AddDays(pondData.SpawnTime - pond.daysSinceSpawn.Value);
                                populationStr += Environment.NewLine + I18n.Building_FishPond_Population_NextSpawn(relativeDate: this.GetRelativeDateStr(nextSpawn));
                            }

                            yield return new ItemIconField(this.GameHelper, I18n.Building_FishPond_Population(), fish, this.Codex, text: populationStr);
                        }

                        // output
                        yield return new ItemIconField(this.GameHelper, I18n.Building_OutputReady(), pond.output.Value, this.Codex);

                        // drops
                        float chanceOfAnyDrop = pondData.BaseMinProduceChance >= pondData.BaseMaxProduceChance
                            ? pondData.BaseMinProduceChance
                            : Utility.Lerp(pondData.BaseMinProduceChance, pondData.BaseMaxProduceChance, (float)pond.currentOccupants.Value / FishPond.MAXIMUM_OCCUPANCY);
                        yield return new FishPondDropsField(this.GameHelper, this.Codex, I18n.Building_FishPond_Drops(), pond.currentOccupants.Value, pondData, fish, preface: I18n.Building_FishPond_Drops_Preface(chance: (chanceOfAnyDrop * 100).ToString("0.##")));

                        // quests
                        if (pondData.PopulationGates?.Any(gate => gate.Key > pond.lastUnlockedPopulationGate.Value) == true)
                            yield return new CheckboxListField(I18n.Building_FishPond_Quests(), new CheckboxList(this.GetPopulationGates(pond, pondData)));
                    }
                    break;

                // Junimo hut
                case JunimoHut hut:
                    yield return new GenericField(I18n.Building_JunimoHarvestingEnabled(), I18n.Stringify(!hut.noHarvest.Value));
                    yield return new ItemIconListField(this.GameHelper, I18n.Building_OutputReady(), hut.GetOutputChest()?.GetItemsForPlayer(Game1.player.UniqueMultiplayerID), showStackSize: true);
                    break;

                // Buildings with processing rules
                default:
                    RecipeModel[] recipes =
                        this.GameHelper.GetRecipesForBuilding(building)
                        .ToArray();
                    if (recipes.Length > 0)
                    {
                        // return recipes
                        var field = new ItemRecipesField(this.GameHelper, this.Codex, I18n.Item_Recipes(), null, recipes, showUnknownRecipes: true, showInvalidRecipes: this.ShowInvalidRecipes); // building recipes don't need to be learned
                        if (this.CollapseFieldsConfig.Enabled)
                            field.CollapseIfLengthExceeds(this.CollapseFieldsConfig.BuildingRecipes, recipes.Length);
                        yield return field;

                        // return items being processed
                        if (MachineDataHelper.TryGetBuildingChestNames(data, out ISet<string> inputChestIds, out ISet<string> outputChestIds))
                        {
                            IEnumerable<Item?> inputItems = MachineDataHelper.GetBuildingChests(building, inputChestIds).SelectMany(p => p.GetItemsForPlayer());
                            IEnumerable<Item?> outputItems = MachineDataHelper.GetBuildingChests(building, outputChestIds).SelectMany(p => p.GetItemsForPlayer());

                            yield return new ItemIconListField(this.GameHelper, I18n.Building_OutputProcessing(), inputItems, showStackSize: true);
                            yield return new ItemIconListField(this.GameHelper, I18n.Building_OutputReady(), outputItems, showStackSize: true);
                        }
                    }
                    break;
            }

            // hay storage
            if (building.hayCapacity.Value > 0)
            {
                // hay summary
                Farm farm = Game1.getFarm();
                int hayCount = farm.piecesOfHay.Value;
                int maxHay = Math.Max(farm.piecesOfHay.Value, farm.GetHayCapacity());
                yield return new GenericField(
                    I18n.Building_StoredHay(),
                    I18n.Building_StoredHay_Summary(hayCount: hayCount, maxHayInLocation: maxHay, maxHayInBuilding: building.hayCapacity.Value)
                );
            }
        }

        // construction recipe
        {
            RecipeModel[] recipes = this.GameHelper
                .GetRecipes()
                .Where(recipe => recipe.Type == RecipeType.BuildingBlueprint && recipe.MachineId == building.buildingType.Value)
                .ToArray();

            if (recipes.Length > 0)
            {
                var field = new ItemRecipesField(this.GameHelper, this.Codex, I18n.Building_ConstructionCosts(), null, recipes, showUnknownRecipes: true, showLabelForSingleGroup: false, showInvalidRecipes: this.ShowInvalidRecipes, showOutputLabels: false);
                if (this.CollapseFieldsConfig.Enabled)
                    field.CollapseIfLengthExceeds(this.CollapseFieldsConfig.BuildingRecipes, recipes.Length);
                yield return field;
            }
        }

        // internal type
        yield return new GenericField(I18n.InternalId(), building.buildingType.Value);
    }

    /// <inheritdoc />
    public override IEnumerable<IDebugField> GetDebugFields()
    {
        Building target = this.Target;

        // pinned fields
        yield return new GenericDebugField("building type", target.buildingType.Value, pinned: true);
        yield return new GenericDebugField("days of construction left", target.daysOfConstructionLeft.Value, pinned: true);
        yield return new GenericDebugField("indoors name", target.GetIndoorsName(), pinned: true);
        yield return new GenericDebugField("indoors type", target.GetIndoorsType().ToString(), pinned: true);

        // raw fields
        foreach (IDebugField field in this.GetDebugFieldsFrom(target))
            yield return field;
    }

    /// <inheritdoc />
    /// <remarks>Derived from <see cref="Building.drawInMenu"/>, modified to draw within the target size.</remarks>
    public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
    {
        Building target = this.Target;
        spriteBatch.Draw(target.texture.Value, position, this.SourceRectangle, target.color, 0.0f, Vector2.Zero, size.X / this.SourceRectangle.Width, SpriteEffects.None, 0.89f);
        return true;
    }


    /*********
    ** Private fields
    *********/
    /// <summary>Get whether a building is a barn.</summary>
    /// <param name="building">The building to check.</param>
    private bool IsBarn(Building? building)
    {
        return building?.buildingType.Value is "Barn" or "Big Barn" or "Deluxe Barn";
    }

    /// <summary>Get whether a building is a coop.</summary>
    /// <param name="building">The building to check.</param>
    private bool IsCoop(Building? building)
    {
        return building?.buildingType.Value is "Coop" or "Big Coop" or "Deluxe Coop";
    }

    /// <summary>Get the building owner, if any.</summary>
    private Farmer? GetOwner()
    {
        Building target = this.Target;

        // stable
        if (target is Stable stable)
        {
            long ownerID = stable.owner.Value;
            return Game1.GetPlayer(ownerID);
        }

        // cabin
        if (this.Target.GetIndoors() is Cabin cabin)
            return cabin.owner;

        return null;
    }

    /// <summary>Get the upgrade level for a building, if applicable.</summary>
    /// <param name="building">The building to check.</param>
    private int? GetUpgradeLevel(Building building)
    {
        // barn
        if (this.IsBarn(building) && int.TryParse(building.GetIndoors()?.mapPath.Value?.Substring("Maps\\Barn".Length), out int barnUpgradeLevel))
            return barnUpgradeLevel - 1; // Barn2 is first upgrade

        // cabin
        if (building.GetIndoors() is Cabin cabin)
            return cabin.upgradeLevel;

        // coop
        if (this.IsCoop(building) && int.TryParse(building.GetIndoors()?.mapPath.Value?.Substring("Maps\\Coop".Length), out int coopUpgradeLevel))
            return coopUpgradeLevel - 1; // Coop2 is first upgrade

        return null;
    }

    /// <summary>Get the feed metrics for an animal building.</summary>
    /// <param name="building">The animal building to check.</param>
    /// <param name="total">The total number of feed trough spaces.</param>
    /// <param name="filled">The number of feed trough spaces which contain hay.</param>
    private void GetFeedMetrics(AnimalHouse building, out int total, out int filled)
    {
        Map map = building.Map;
        total = 0;
        filled = 0;

        for (int x = 0; x < map.Layers[0].LayerWidth; x++)
        {
            for (int y = 0; y < map.Layers[0].LayerHeight; y++)
            {
                if (building.doesTileHaveProperty(x, y, "Trough", "Back") != null)
                {
                    total++;
                    if (building.objects.TryGetValue(new Vector2(x, y), out SObject obj) && obj.QualifiedItemId == "(O)178")
                        filled++;
                }
            }
        }
    }

    /// <summary>Get the upgrade levels for a building, for use with a checkbox field.</summary>
    /// <param name="building">The building to check.</param>
    /// <param name="upgradeLevel">The current upgrade level, if applicable.</param>
    private IEnumerable<Checkbox> GetUpgradeLevelSummary(Building building, int? upgradeLevel)
    {
        // TODO: animal buildings were de-hardcoded in Stardew Valley 1.6, so we should generate this info from Data/Buildings instead.

        // barn
        if (this.IsBarn(building))
        {
            yield return new Checkbox(text: I18n.Building_Upgrades_Barn_0(), isChecked: true);
            yield return new Checkbox(text: I18n.Building_Upgrades_Barn_1(), isChecked: upgradeLevel >= 1);
            yield return new Checkbox(text: I18n.Building_Upgrades_Barn_2(), isChecked: upgradeLevel >= 2);
        }

        // cabin
        else if (building.GetIndoors() is Cabin)
        {
            yield return new Checkbox(text: I18n.Building_Upgrades_Cabin_0(), isChecked: true);
            yield return new Checkbox(text: I18n.Building_Upgrades_Cabin_1(), isChecked: upgradeLevel >= 1);
            yield return new Checkbox(text: I18n.Building_Upgrades_Cabin_2(), isChecked: upgradeLevel >= 2);
        }

        // coop
        else if (this.IsCoop(building))
        {
            yield return new Checkbox(text: I18n.Building_Upgrades_Coop_0(), isChecked: true);
            yield return new Checkbox(text: I18n.Building_Upgrades_Coop_1(), isChecked: upgradeLevel >= 1);
            yield return new Checkbox(text: I18n.Building_Upgrades_Coop_2(), isChecked: upgradeLevel >= 2);
        }
    }

    /// <summary>Get a fish pond's population gates for display.</summary>
    /// <param name="pond">The fish pond.</param>
    /// <param name="data">The fish pond data.</param>
    private IEnumerable<Checkbox> GetPopulationGates(FishPond pond, FishPondData data)
    {
        bool foundNextQuest = false;
        foreach (FishPondPopulationGateData gate in this.GameHelper.GetFishPondPopulationGates(data))
        {
            int newPopulation = gate.NewPopulation;

            // done
            if (pond.lastUnlockedPopulationGate.Value >= gate.RequiredPopulation)
            {
                yield return new Checkbox(text: I18n.Building_FishPond_Quests_Done(count: newPopulation), isChecked: true);
                continue;
            }

            // get required items
            string[] requiredItems = gate.RequiredItems
                .Select(drop =>
                {
                    // build display string
                    string summary = ItemRegistry.GetDataOrErrorItem(drop.ItemID).DisplayName;
                    if (drop.MinCount != drop.MaxCount)
                        summary += $" ({I18n.Generic_Range(min: drop.MinCount, max: drop.MaxCount)})";
                    else if (drop.MinCount > 1)
                        summary += $" ({drop.MinCount})";

                    // track requirement
                    return summary;
                })
                .ToArray();

            // display requirements
            string result = requiredItems.Length > 1
                ? I18n.Building_FishPond_Quests_IncompleteRandom(newPopulation, I18n.List(requiredItems))
                : I18n.Building_FishPond_Quests_IncompleteOne(newPopulation, requiredItems[0]);

            // show next quest
            if (!foundNextQuest)
            {
                foundNextQuest = true;

                int nextQuestDays = data.SpawnTime
                    + (data.SpawnTime * (pond.maxOccupants.Value - pond.currentOccupants.Value))
                    - pond.daysSinceSpawn.Value;
                result += $"; {I18n.Building_FishPond_Quests_Available(relativeDate: this.GetRelativeDateStr(nextQuestDays))}";
            }
            yield return new Checkbox(text: result, isChecked: false);
        }
    }
}
