using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

/// <summary>Describes the <see cref="IslandSouthEast"/> mermaid music puzzle.</summary>
internal class IslandMermaidPuzzleSubject : TileSubject
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="config">The mod configuration.</param>
    /// <param name="location">The game location.</param>
    /// <param name="position">The tile position.</param>
    /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
    public IslandMermaidPuzzleSubject(GameHelper gameHelper, ModConfig config, GameLocation location, Vector2 position, bool showRawTileInfo)
        : base(gameHelper, config, location, position, showRawTileInfo)
    {
        this.Name = I18n.Puzzle_IslandMermaid_Title();
        this.Description = null;
        this.Type = null;
    }

    /// <inheritdoc />
    public override IEnumerable<ICustomField> GetData()
    {
        // mermaid puzzle
        {
            IslandSouthEast location = (IslandSouthEast)this.Location;
            bool complete = location.mermaidPuzzleFinished.Value;

            if (!this.Config.ShowPuzzleSolutions && !complete)
                yield return new GenericField(I18n.Puzzle_Solution(), I18n.Puzzle_Solution_Hidden());
            else
            {
                int[] sequence = this.GameHelper.Metadata.PuzzleSolutions.IslandMermaidFluteBlockSequence;
                int songIndex = location.songIndex;

                var checkboxes = sequence
                    .Select((pitch, i) => new Checkbox(text: this.Stringify(pitch), isChecked: complete || songIndex >= i))
                    .ToArray();

                CheckboxList checkboxList = new(checkboxes);
                checkboxList.AddIntro(complete ? I18n.Puzzle_Solution_Solved() : I18n.Puzzle_IslandMermaid_Solution_Intro());

                yield return new CheckboxListField(I18n.Puzzle_Solution(), checkboxList);
            }
        }

        // raw map data
        foreach (ICustomField field in base.GetData())
            yield return field;
    }
}
