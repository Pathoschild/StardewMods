using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.FishPond;
using StardewValley.Locations;
using StardewValley.Monsters;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="building">The lookup target.</param>
        /// <param name="sourceRectangle">The building's source rectangle in its spritesheet.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        public BuildingSubject(SubjectFactory codex, GameHelper gameHelper, Building building, Rectangle sourceRectangle, ITranslationHelper translations)
            : base(codex, gameHelper, building.buildingType.Value, null, L10n.Types.Building(), translations)
        {
            // init
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
            var text = this.Text;

            // get info
            Building building = this.Target;
            bool built = !building.isUnderConstruction();
            int? upgradeLevel = this.GetUpgradeLevel(building);

            // construction / upgrade
            if (!built || building.daysUntilUpgrade.Value > 0)
            {
                int daysLeft = building.isUnderConstruction() ? building.daysOfConstructionLeft.Value : building.daysUntilUpgrade.Value;
                SDate readyDate = SDate.Now().AddDays(daysLeft);
                yield return new GenericField(this.GameHelper, L10n.Building.Construction(), L10n.Building.ConstructionSummary(date: readyDate));
            }

            // owner
            Farmer owner = this.GetOwner();
            if (owner != null)
                yield return new LinkField(this.GameHelper, L10n.Building.Owner(), owner.Name, () => this.Codex.GetPlayer(owner));
            else if (building.indoors.Value is Cabin)
                yield return new GenericField(this.GameHelper, L10n.Building.Owner(), L10n.Building.OwnerNone());

            // stable horse
            if (built && building is Stable stable)
            {
                Horse horse = Utility.findHorse(stable.HorseId);
                if (horse != null)
                {
                    yield return new LinkField(this.GameHelper, L10n.Building.Horse(), horse.Name, () => this.Codex.GetCharacter(horse));
                    yield return new GenericField(this.GameHelper, L10n.Building.HorseLocation(), L10n.Building.HorseLocationSummary(location: horse.currentLocation.Name, x: horse.getTileX(), y: horse.getTileY()));
                }
            }

            // animals
            if (built && building.indoors.Value is AnimalHouse animalHouse)
            {
                // animal counts
                yield return new GenericField(this.GameHelper, L10n.Building.Animals(), L10n.Building.AnimalsSummary(count: animalHouse.animalsThatLiveHere.Count, max: animalHouse.animalLimit.Value));

                // feed trough
                if ((building is Barn || building is Coop) && upgradeLevel >= 2)
                    yield return new GenericField(this.GameHelper, L10n.Building.FeedTrough(), L10n.Building.FeedTroughAutomated());
                else
                {
                    this.GetFeedMetrics(animalHouse, out int totalFeedSpaces, out int filledFeedSpaces);
                    yield return new GenericField(this.GameHelper, L10n.Building.FeedTrough(), L10n.Building.FeedTroughSummary(filled: filledFeedSpaces, max: totalFeedSpaces));
                }
            }

            // slimes
            if (built && building.indoors.Value is SlimeHutch slimeHutch)
            {
                // slime count
                int slimeCount = slimeHutch.characters.OfType<GreenSlime>().Count();
                yield return new GenericField(this.GameHelper, L10n.Building.Slimes(), L10n.Building.SlimesSummary(count: slimeCount, max: 20));

                // water trough
                yield return new GenericField(this.GameHelper, L10n.Building.WaterTrough(), L10n.Building.WaterTroughSummary(filled: slimeHutch.waterSpots.Count(p => p), max: slimeHutch.waterSpots.Count));
            }

            // upgrade level
            if (built)
            {
                var upgradeLevelSummary = this.GetUpgradeLevelSummary(building, upgradeLevel).ToArray();
                if (upgradeLevelSummary.Any())
                    yield return new CheckboxListField(this.GameHelper, L10n.Building.Upgrades(), upgradeLevelSummary);
            }

            // specific buildings
            if (built)
            {
                switch (building)
                {
                    // fish pond
                    case FishPond pond:
                        if (pond.fishType.Value <= -1)
                            yield return new GenericField(this.GameHelper, L10n.Building.FishPondPopulation(), L10n.Building.FishPondPopulationEmpty());
                        else
                        {
                            // get fish population
                            SObject fish = pond.GetFishObject();
                            fish.Stack = pond.FishCount;
                            var pondData = pond.GetFishPondData();

                            // population field
                            {
                                string populationStr = $"{fish.DisplayName} ({L10n.Generic.Ratio(pond.FishCount, pond.maxOccupants.Value)})";
                                if (pond.FishCount < pond.maxOccupants.Value)
                                {
                                    SDate nextSpawn = SDate.Now().AddDays(pondData.SpawnTime - pond.daysSinceSpawn.Value);
                                    populationStr += Environment.NewLine + L10n.Building.FishPondPopulationNextSpawn(relativeDate: this.GetRelativeDateStr(nextSpawn));
                                }

                                yield return new ItemIconField(this.GameHelper, L10n.Building.FishPondPopulation(), fish, text: populationStr);
                            }

                            // output
                            yield return new ItemIconField(this.GameHelper, L10n.Building.OutputReady(), pond.output.Value);

                            // drops
                            int chanceOfAnyDrop = (int)Math.Round(Utility.Lerp(0.15f, 0.95f, pond.currentOccupants.Value / 10f) * 100);
                            yield return new FishPondDropsField(this.GameHelper, L10n.Building.FishPondDrops(), pond.currentOccupants.Value, pondData, preface: L10n.Building.FishPondDropsPreface(chance: chanceOfAnyDrop.ToString()));

                            // quests
                            if (pondData.PopulationGates?.Any(gate => gate.Key > pond.lastUnlockedPopulationGate.Value) == true)
                                yield return new CheckboxListField(this.GameHelper, L10n.Building.FishPondQuests(), this.GetPopulationGates(pond, pondData));
                        }
                        break;

                    // Junimo hut
                    case JunimoHut hut:
                        yield return new GenericField(this.GameHelper, L10n.Building.JunimoHarvestingEnabled(), text.Stringify(!hut.noHarvest.Value));
                        yield return new ItemIconListField(this.GameHelper, L10n.Building.OutputReady(), hut.output.Value?.items, showStackSize: true);
                        break;

                    // mill
                    case Mill mill:
                        yield return new ItemIconListField(this.GameHelper, L10n.Building.OutputProcessing(), mill.input.Value?.items, showStackSize: true);
                        yield return new ItemIconListField(this.GameHelper, L10n.Building.OutputReady(), mill.output.Value?.items, showStackSize: true);
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
                                this.GameHelper,
                                L10n.Building.StoredHay(),
                                siloCount == 1
                                    ? L10n.Building.StoredHaySummaryOneSilo(hayCount: hayCount, maxHay: maxHay)
                                    : L10n.Building.StoredHaySummaryMultipleSilos(hayCount: hayCount, maxHay: maxHay, siloCount: siloCount)
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
                yield return new KeyValuePair<IFormattedText[], bool>(
                    key: new IFormattedText[] { new FormattedText(L10n.Building.UpgradesBarn0()) },
                    value: true
                );
                yield return new KeyValuePair<IFormattedText[], bool>(
                    key: new IFormattedText[] { new FormattedText(L10n.Building.UpgradesBarn1()) },
                    value: upgradeLevel >= 1
                );
                yield return new KeyValuePair<IFormattedText[], bool>(
                    key: new IFormattedText[] { new FormattedText(L10n.Building.UpgradesBarn2()) },
                    value: upgradeLevel >= 2
                );
            }

            // cabin
            else if (building.indoors.Value is Cabin)
            {
                yield return new KeyValuePair<IFormattedText[], bool>(
                    key: new IFormattedText[] { new FormattedText(L10n.Building.UpgradesCabin0()) },
                    value: true
                );
                yield return new KeyValuePair<IFormattedText[], bool>(
                    key: new IFormattedText[] { new FormattedText(L10n.Building.UpgradesCabin1()) },
                    value: upgradeLevel >= 1
                );
                yield return new KeyValuePair<IFormattedText[], bool>(
                    key: new IFormattedText[] { new FormattedText(L10n.Building.UpgradesCabin2()) },
                    value: upgradeLevel >= 2
                );
            }

            // coop
            else if (building is Coop)
            {
                yield return new KeyValuePair<IFormattedText[], bool>(
                    key: new IFormattedText[] { new FormattedText(L10n.Building.UpgradesCoop0()) },
                    value: true
                );
                yield return new KeyValuePair<IFormattedText[], bool>(
                    key: new IFormattedText[] { new FormattedText(L10n.Building.UpgradesCoop1()) },
                    value: upgradeLevel >= 1
                );
                yield return new KeyValuePair<IFormattedText[], bool>(
                    key: new IFormattedText[] { new FormattedText(L10n.Building.UpgradesCoop2()) },
                    value: upgradeLevel >= 2
                );
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
                    yield return new KeyValuePair<IFormattedText[], bool>(
                        key: new IFormattedText[] { new FormattedText(L10n.Building.FishPondQuestsDone(count: newPopulation)) },
                        value: true
                    );
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
                            summary += $" ({L10n.Generic.Range(min: drop.MinCount, max: drop.MaxCount)})";
                        else if (drop.MinCount > 1)
                            summary += $" ({drop.MinCount})";

                        // track requirement
                        return summary;
                    })
                    .ToArray();

                // display requirements
                string itemList = string.Join(", ", requiredItems);
                string result = requiredItems.Length > 1
                    ? L10n.Building.FishPondQuestsIncompleteRandom(newPopulation, itemList)
                    : L10n.Building.FishPondQuestsIncompleteOne(newPopulation, requiredItems[0]);

                // show next quest
                if (!foundNextQuest)
                {
                    foundNextQuest = true;

                    int nextQuestDays = data.SpawnTime
                        + (data.SpawnTime * (pond.maxOccupants.Value - pond.currentOccupants.Value))
                        - pond.daysSinceSpawn.Value;
                    result += $"; {L10n.Building.FishPondQuestsAvailable(relativeDate: this.GetRelativeDateStr(nextQuestDays))}";
                }
                yield return new KeyValuePair<IFormattedText[], bool>(key: new IFormattedText[] { new FormattedText(result) }, value: false);
            }
        }
    }
}
