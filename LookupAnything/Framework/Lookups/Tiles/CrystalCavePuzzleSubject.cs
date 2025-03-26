using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

/// <summary>Describes the <see cref="IslandWestCave1"/> crystal cave puzzle.</summary>
internal class CrystalCavePuzzleSubject : TileSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The ID of the crystal being looked up, if any.</summary>
    private readonly int? CrystalId;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="config">The mod configuration.</param>
    /// <param name="location">The game location.</param>
    /// <param name="position">The tile position.</param>
    /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
    /// <param name="crystalId">The ID of the crystal being looked up, if any.</param>
    public CrystalCavePuzzleSubject(GameHelper gameHelper, ModConfig config, GameLocation location, Vector2 position, bool showRawTileInfo, int? crystalId)
        : base(gameHelper, config, location, position, showRawTileInfo)
    {
        this.Name = I18n.Puzzle_IslandCrystalCave_Title();
        this.Description = null;
        this.Type = null;
        this.CrystalId = crystalId;
    }

    /// <inheritdoc />
    public override IEnumerable<ICustomField> GetData()
    {
        // island crystal puzzle
        {
            var cave = (IslandWestCave1)this.Location;

            // crystal ID
            if (this.CrystalId.HasValue && this.Config.ShowPuzzleSolutions)
                yield return new GenericField(I18n.Puzzle_IslandCrystalCave_CrystalId(), this.Stringify(this.CrystalId.Value));

            // sequence
            {
                string label = I18n.Puzzle_Solution();
                if (cave.completed.Value)
                    yield return new GenericField(label, I18n.Puzzle_Solution_Solved());
                else if (!this.Config.ShowPuzzleSolutions)
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
                            new Checkbox(
                                text: this.Stringify(id + 1),
                                isChecked: cave.currentCrystalSequenceIndex.Value > index
                            )
                        )
                        .ToArray();

                    CheckboxList checkboxList = new(checkboxes);
                    checkboxList.AddIntro(I18n.Puzzle_IslandCrystalCave_Solution_Activated());

                    yield return new CheckboxListField(label, checkboxList);
                }
            }
        }

        // raw map data
        foreach (ICustomField field in base.GetData())
            yield return field;
    }
}
