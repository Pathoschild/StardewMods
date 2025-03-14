using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

/// <summary>Describes a fishable area.</summary>
internal class FishingAreaSubject : TileSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The fish area ID which applies, if any.</summary>
    private readonly string FishAreaId;

    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="location">The game location.</param>
    /// <param name="position">The tile position.</param>
    /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
    public FishingAreaSubject(GameHelper gameHelper, GameLocation location, Vector2 position, bool showRawTileInfo)
        : base(gameHelper, location, position, showRawTileInfo)
    {
        location.TryGetFishAreaForTile(position, out this.FishAreaId, out _);

        this.Name = this.GameHelper.GetLocationDisplayName(location, this.FishAreaId);
        this.Description = null;
        this.Type = I18n.Type_FishingArea();
    }

    /// <inheritdoc/>
    public override IEnumerable<ICustomField> GetData()
    {
        yield return new FishSpawnRulesField(this.GameHelper, I18n.Item_FishSpawnRules(), this.Location, this.Position, this.FishAreaId);

        // raw map data
        foreach (ICustomField field in base.GetData())
            yield return field;
    }
}
