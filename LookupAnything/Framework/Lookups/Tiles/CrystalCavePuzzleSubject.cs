using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles
{
    /// <summary>Describes the <see cref="IslandWestCave1"/> crystal cave puzzle.</summary>
    internal class CrystalCavePuzzleSubject : TileSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether to only show content once the player discovers it.</summary>
        private readonly bool ProgressionMode;

        /// <summary>The ID of the crystal being looked up, if any.</summary>
        private readonly int? CrystalId;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="location">The game location.</param>
        /// <param name="position">The tile position.</param>
        /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
        /// <param name="progressionMode">Whether to only show content once the player discovers it.</param>
        /// <param name="crystalId">The ID of the crystal being looked up, if any.</param>
        public CrystalCavePuzzleSubject(GameHelper gameHelper, GameLocation location, Vector2 position, bool showRawTileInfo, bool progressionMode, int? crystalId)
            : base(gameHelper, location, position, showRawTileInfo)
        {
            this.Name = I18n.Puzzle_IslandCrystalCave_Title();
            this.Description = null;
            this.Type = null;
            this.ProgressionMode = progressionMode;
            this.CrystalId = crystalId;
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<ICustomField> GetData()
        {
            // island crystal puzzle
            {
                var cave = (IslandWestCave1)this.Location;

                // crystal ID
                if (this.CrystalId.HasValue && !this.ProgressionMode)
                    yield return new GenericField(I18n.Puzzle_IslandCrystalCave_CrystalId(), this.Stringify(this.CrystalId.Value));

                // sequence
                {
                    string label = I18n.Puzzle_Solution();
                    if (cave.completed.Value)
                        yield return new GenericField(label, I18n.Puzzle_Solution_Solved());
                    else if (this.ProgressionMode)
                        yield return new GenericField(label, new FormattedText(I18n.Puzzle_Solution_Hidden(), Color.Gray));
                    else if (!cave.isActivated.Value)
                        yield return new GenericField(label, I18n.Puzzle_IslandCrystalCave_Solution_NotActivated());
                    else if (!cave.currentCrystalSequence.Any())
                        yield return new GenericField(label, I18n.Puzzle_IslandCrystalCave_Solution_Waiting());
                    else
                    {
                        var checkboxes = cave
                            .currentCrystalSequence
                            .Select((id, index) =>
                                CheckboxListField.Checkbox(
                                    text: this.Stringify(id + 1),
                                    value: cave.currentCrystalSequenceIndex.Value > index
                                )
                            )
                            .ToArray();

                        yield return new CheckboxListField(label, checkboxes)
                            .AddIntro(I18n.Puzzle_IslandCrystalCave_Solution_Activated());
                    }
                }
            }

            // raw map data
            foreach (ICustomField field in base.GetData())
                yield return field;
        }
    }
}
