using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.FishPond;
using StardewValley.Locations;
using StardewValley.Monsters;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Buildings
{
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

        /// <summary>The central registry for subject lookups.</summary>
        private readonly ISubjectRegistry Codex;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="building">The lookup target.</param>
        /// <param name="sourceRectangle">The building's source rectangle in its spritesheet.</param>
        public BuildingSubject(ISubjectRegistry codex, GameHelper gameHelper, Building building, Rectangle sourceRectangle)
            : base(gameHelper, building.buildingType.Value, null, I18n.Type_Building())
        {
            // init
            this.Codex = codex;
            this.Target = building;
            this.SourceRectangle = sourceRectangle;

            // get name/description from blueprint if available
            try
            {
                BluePrint blueprint = new BluePrint(building.buildingType.Value);
                this.Name = blueprint.displayName;
                this.Description = blueprint.description;
            }
            catch (ContentLoadException)
            {
                // use default values
            }
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<ICustomField> GetData()
        {
            // get info
            Building building = this.Target;
            bool built = !building.isUnderConstruction();
            int? upgradeLevel = this.GetUpgradeLevel(building);

            // construction / upgrade
            if (!built || building.daysUntilUpgrade.Value > 0)
            {
                int daysLeft = building.isUnderConstruction() ? building.daysOfConstructionLeft.Value : building.daysUntilUpgrade.Value;
                SDate readyDate = SDate.Now().AddDays(daysLeft);
                yield return new GenericField(I18n.Building_Construction(), I18n.Building_Construction_Summary(date: this.Stringify(readyDate)));
            }

            // owner
            Farmer owner = this.GetOwner();
            if (owner != null)
                yield return new LinkField(I18n.Building_Owner(), owner.Name, () => this.Codex.GetByEntity(owner));
            else if (building.indoors.Value is Cabin)
                yield return new GenericField(I18n.Building_Owner(), I18n.Building_Owner_None());

            // stable horse
            if (built && building is Stable stable)
            {
                Horse horse = Utility.findHorse(stable.HorseId);
                if (horse != null)
                {
                    yield return new LinkField(I18n.Building_Horse(), horse.Name, () => this.Codex.GetByEntity(horse));
                    yield return new GenericField(I18n.Building_HorseLocation(), I18n.Building_HorseLocation_Summary(location: horse.currentLocation.Name, x: horse.getTileX(), y: horse.getTileY()));
                }
            }

            // animals
            if (built && building.indoors.Value is AnimalHouse animalHouse)
            {
                // animal counts
                yield return new GenericField(I18n.Building_Animals(), I18n.Building_Animals_Summary(count: animalHouse.animalsThatLiveHere.Count, max: animalHouse.animalLimit.Value));

                // feed trough
                if ((building is Barn || building is Coop) && upgradeLevel >= 2)
                    yield return new GenericField(I18n.Building_FeedTrough(), I18n.Building_FeedTrough_Automated());
                else
                {
                    this.GetFeedMetrics(animalHouse, out int totalFeedSpaces, out int filledFeedSpaces);
                    yield return new GenericField(I18n.Building_FeedTrough(), I18n.Building_FeedTrough_Summary(filled: filledFeedSpaces, max: totalFeedSpaces));
                }
            }

            // slimes
            if (built && building.indoors.Value is SlimeHutch slimeHutch)
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
                    yield return new CheckboxListField(I18n.Building_Upgrades(), upgradeLevelSummary);
            }

            // specific buildings
            if (built)
            {
                switch (building)
                {
                    // fish pond
                    case FishPond pond:
                        if (pond.fishType.Value <= -1)
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

                                yield return new ItemIconField(this.GameHelper, I18n.Building_FishPond_Population(), fish, text: populationStr);
                            }

                            // output
                            yield return new ItemIconField(this.GameHelper, I18n.Building_OutputReady(), pond.output.Value);

                            // drops
                            int chanceOfAnyDrop = (int)Math.Round(Utility.Lerp(0.15f, 0.95f, pond.currentOccupants.Value / 10f) * 100);
                            yield return new FishPondDropsField(this.GameHelper, I18n.Building_FishPond_Drops(), pond.currentOccupants.Value, pondData, preface: I18n.Building_FishPond_Drops_Preface(chance: chanceOfAnyDrop.ToString()));

                            // quests
                            if (pondData.PopulationGates?.Any(gate => gate.Key > pond.lastUnlockedPopulationGate.Value) == true)
                                yield return new CheckboxListField(I18n.Building_FishPond_Quests(), this.GetPopulationGates(pond, pondData));
                        }
                        break;

                    // Junimo hut
                    case JunimoHut hut:
                        yield return new GenericField(I18n.Building_JunimoHarvestingEnabled(), I18n.Stringify(!hut.noHarvest.Value));
                        yield return new ItemIconListField(this.GameHelper, I18n.Building_OutputReady(), hut.output.Value?.GetItemsForPlayer(Game1.player.UniqueMultiplayerID), showStackSize: true);
                        break;

                    // mill
                    case Mill mill:
                        yield return new ItemIconListField(this.GameHelper, I18n.Building_OutputProcessing(), mill.input.Value?.GetItemsForPlayer(Game1.player.UniqueMultiplayerID), showStackSize: true);
                        yield return new ItemIconListField(this.GameHelper, I18n.Building_OutputReady(), mill.output.Value?.GetItemsForPlayer(Game1.player.UniqueMultiplayerID), showStackSize: true);
                        break;

                    // silo
                    case Building _ when building.buildingType.Value == "Silo":
                        {
                            // hay summary
                            Farm farm = Game1.getFarm();
                            int siloCount = Utility.numSilos();
                            int hayCount = farm.piecesOfHay.Value;
                            int maxHay = Math.Max(farm.piecesOfHay.Value, siloCount * 240);
                            yield return new GenericField(
                                I18n.Building_StoredHay(),
                                siloCount == 1
                                    ? I18n.Building_StoredHay_SummaryOneSilo(hayCount: hayCount, maxHay: maxHay)
                                    : I18n.Building_StoredHay_SummaryMultipleSilos(hayCount: hayCount, maxHay: maxHay, siloCount: siloCount)
                            );
                        }
                        break;
                }
            }
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
        public override IEnumerable<IDebugField> GetDebugFields()
        {
            Building target = this.Target;

            // pinned fields
            yield return new GenericDebugField("building type", target.buildingType.Value, pinned: true);
            yield return new GenericDebugField("days of construction left", target.daysOfConstructionLeft.Value, pinned: true);
            yield return new GenericDebugField("name of indoors", target.nameOfIndoors, pinned: true);

            // raw fields
            foreach (IDebugField field in this.GetDebugFieldsFrom(target))
                yield return field;
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        /// <remarks>Derived from <see cref="Building.drawInMenu"/>, modified to draw within the target size.</remarks>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            Building target = this.Target;
            spriteBatch.Draw(target.texture.Value, position, this.SourceRectangle, target.color.Value, 0.0f, Vector2.Zero, size.X / this.SourceRectangle.Width, SpriteEffects.None, 0.89f);
            return true;
        }


        /*********
        ** Private fields
        *********/
        /// <summary>Get the building owner, if any.</summary>
        private Farmer GetOwner()
        {
            Building target = this.Target;

            // stable
            if (target is Stable stable)
            {
                long ownerID = stable.owner.Value;
                return Game1.getFarmerMaybeOffline(ownerID);
            }

            // cabin
            if (this.Target.indoors.Value is Cabin cabin)
                return cabin.owner;

            return null;
        }

        /// <summary>Get the upgrade level for a building, if applicable.</summary>
        /// <param name="building">The building to check.</param>
        private int? GetUpgradeLevel(Building building)
        {
            // barn
            if (building is Barn barn && int.TryParse(barn.nameOfIndoorsWithoutUnique.Substring("Barn".Length), out int barnUpgradeLevel))
                return barnUpgradeLevel - 1; // Barn2 is first upgrade

            // cabin
            if (building.indoors.Value is Cabin cabin)
                return cabin.upgradeLevel;

            // coop
            if (building is Coop coop && int.TryParse(coop.nameOfIndoorsWithoutUnique.Substring("Coop".Length), out int coopUpgradeLevel))
                return coopUpgradeLevel - 1; // Coop2 is first upgrade

            return null;
        }

        /// <summary>Get the feed metrics for an animal building.</summary>
        /// <param name="building">The animal building to check.</param>
        /// <param name="total">The total number of feed trough spaces.</param>
        /// <param name="filled">The number of feed trough spaces which contain hay.</param>
        private void GetFeedMetrics(AnimalHouse building, out int total, out int filled)
        {
            var map = building.Map;
            total = 0;
            filled = 0;

            for (int x = 0; x < map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < map.Layers[0].LayerHeight; y++)
                {
                    if (building.doesTileHaveProperty(x, y, "Trough", "Back") != null)
                    {
                        total++;
                        if (building.objects.TryGetValue(new Vector2(x, y), out SObject obj) && obj.ParentSheetIndex == 178)
                            filled++;
                    }
                }
            }
        }

        /// <summary>Get the upgrade levels for a building, for use with a checkbox field.</summary>
        /// <param name="building">The building to check.</param>
        /// <param name="upgradeLevel">The current upgrade level, if applicable.</param>
        private IEnumerable<KeyValuePair<IFormattedText[], bool>> GetUpgradeLevelSummary(Building building, int? upgradeLevel)
        {
            // barn
            if (building is Barn)
            {
                yield return CheckboxListField.Checkbox(text: I18n.Building_Upgrades_Barn_0(), value: true);
                yield return CheckboxListField.Checkbox(text: I18n.Building_Upgrades_Barn_1(), value: upgradeLevel >= 1);
                yield return CheckboxListField.Checkbox(text: I18n.Building_Upgrades_Barn_2(), value: upgradeLevel >= 2);
            }

            // cabin
            else if (building.indoors.Value is Cabin)
            {
                yield return CheckboxListField.Checkbox(text: I18n.Building_Upgrades_Cabin_0(), value: true);
                yield return CheckboxListField.Checkbox(text: I18n.Building_Upgrades_Cabin_1(), value: upgradeLevel >= 1);
                yield return CheckboxListField.Checkbox(text: I18n.Building_Upgrades_Cabin_2(), value: upgradeLevel >= 2);
            }

            // coop
            else if (building is Coop)
            {
                yield return CheckboxListField.Checkbox(text: I18n.Building_Upgrades_Coop_0(), value: true);
                yield return CheckboxListField.Checkbox(text: I18n.Building_Upgrades_Coop_1(), value: upgradeLevel >= 1);
                yield return CheckboxListField.Checkbox(text: I18n.Building_Upgrades_Coop_2(), value: upgradeLevel >= 2);
            }
        }

        /// <summary>Get a fish pond's population gates for display.</summary>
        /// <param name="pond">The fish pond.</param>
        /// <param name="data">The fish pond data.</param>
        private IEnumerable<KeyValuePair<IFormattedText[], bool>> GetPopulationGates(FishPond pond, FishPondData data)
        {
            bool foundNextQuest = false;
            foreach (FishPondPopulationGateData gate in this.GameHelper.GetFishPondPopulationGates(data))
            {
                int newPopulation = gate.NewPopulation;

                // done
                if (pond.lastUnlockedPopulationGate.Value >= gate.RequiredPopulation)
                {
                    yield return CheckboxListField.Checkbox(text: I18n.Building_FishPond_Quests_Done(count: newPopulation), value: true);
                    continue;
                }

                // get required items
                string[] requiredItems = gate.RequiredItems
                    .Select(drop =>
                    {
                        // build display string
                        SObject obj = this.GameHelper.GetObjectBySpriteIndex(drop.ItemID);
                        string summary = obj.DisplayName;
                        if (drop.MinCount != drop.MaxCount)
                            summary += $" ({I18n.Generic_Range(min: drop.MinCount, max: drop.MaxCount)})";
                        else if (drop.MinCount > 1)
                            summary += $" ({drop.MinCount})";

                        // track requirement
                        return summary;
                    })
                    .ToArray();

                // display requirements
                string itemList = string.Join(", ", requiredItems);
                string result = requiredItems.Length > 1
                    ? I18n.Building_FishPond_Quests_IncompleteRandom(newPopulation, itemList)
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
                yield return CheckboxListField.Checkbox(text: result, value: false);
            }
        }
    }
}
