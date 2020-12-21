using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles
{
    /// <summary>Describes the <see cref="IslandSouthEast"/> mermaid music puzzle.</summary>
    internal class IslandMermaidPuzzleSubject : TileSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether to only show content once the player discovers it.</summary>
        private readonly bool ProgressionMode;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="location">The game location.</param>
        /// <param name="position">The tile position.</param>
        /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
        /// <param name="progressionMode">Whether to only show content once the player discovers it.</param>
        public IslandMermaidPuzzleSubject(GameHelper gameHelper, GameLocation location, Vector2 position, bool showRawTileInfo, bool progressionMode)
            : base(gameHelper, location, position, showRawTileInfo)
        {
            this.Name = I18n.Puzzle_IslandMermaid_Title();
            this.Description = null;
            this.Type = null;
            this.ProgressionMode = progressionMode;
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<ICustomField> GetData()
        {
            // mermaid puzzle
            {
                IslandSouthEast location = (IslandSouthEast)this.Location;
                bool complete = location.mermaidPuzzleFinished.Value;

                if (this.ProgressionMode && !complete)
                    yield return new GenericField(I18n.Puzzle_Solution(), I18n.Puzzle_Solution_Hidden());
                else
                {
                    int[] sequence = this.GameHelper.Metadata.PuzzleSolutions.IslandMermaidFluteBlockSequence;
                    int songIndex = location.songIndex;

                    var checkboxes = sequence
                        .Select((pitch, i) => CheckboxListField.Checkbox(text: this.Stringify(pitch), value: complete || songIndex >= i))
                        .ToArray();

                    yield return new CheckboxListField(I18n.Puzzle_Solution(), checkboxes)
                        .AddIntro(complete ? I18n.Puzzle_Solution_Solved() : I18n.Puzzle_IslandMermaid_Solution_Intro());
                }
            }

            // raw map data
            foreach (ICustomField field in base.GetData())
                yield return field;
        }
    }
}
