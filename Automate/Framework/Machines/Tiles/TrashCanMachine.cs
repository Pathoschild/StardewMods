using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Tiles;

/// <summary>A trash can that accepts input and provides output.</summary>
/// <remarks>Derived from <see cref="Town.checkAction"/>.</remarks>
internal class TrashCanMachine : BaseMachine
{
    /*********
    ** Fields
    *********/
    /// <summary>The trash can ID.</summary>
    private readonly string TrashCanId;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="location">The location to search.</param>
    /// <param name="tile">The machine's position in its location.</param>
    /// <param name="trashCanId">The trash can ID.</param>
    public TrashCanMachine(GameLocation location, Vector2 tile, string trashCanId)
        : base(location, BaseMachine.GetTileAreaFor(tile))
    {
        this.TrashCanId = this.GetActualTrashCanId(trashCanId);
    }

    /// <inheritdoc />
    public override MachineState GetState()
    {
        if (Game1.netWorldState.Value.CheckedGarbage.Contains(this.TrashCanId))
            return MachineState.Processing;

        return MachineState.Done;
    }

    /// <inheritdoc />
    public override ITrackedStack? GetOutput()
    {
        // get item
        this.Location.TryGetGarbageItem(this.TrashCanId, Game1.MasterPlayer.DailyLuck, out Item? item, out _, out _);
        if (item != null)
            return new TrackedItem(item).OnEmpty((_, _) => this.MarkChecked());

        // if nothing is returned, mark trash can checked
        this.MarkChecked();
        return null;
    }

    /// <inheritdoc />
    public override bool SetInput(IStorage input)
    {
        return false; // no input
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Reset the machine, so it starts processing the next item.</summary>
    private void MarkChecked()
    {
        if (Game1.netWorldState.Value.CheckedGarbage.Add(this.TrashCanId))
            Game1.stats.Increment("trashCansChecked");
    }

    /// <summary>Get the actual trash can ID for an <c>Action Garbage</c> tile property value.</summary>
    /// <param name="id">The trash can ID from the <c>Action Garbage</c> tile property.</param>
    /// <returns>This maps pre-1.6 trash can IDs (e.g. from a map mod which wasn't updated) to match the logic in <see cref="GameLocation.CheckGarbage"/>.</returns>
    private string GetActualTrashCanId(string id)
    {
        return id switch
        {
            "0" => "JodiAndKent",
            "1" => "EmilyAndHaley",
            "2" => "Mayor",
            "3" => "Museum",
            "4" => "Blacksmith",
            "5" => "Saloon",
            "6" => "Evelyn",
            "7" => "JojaMart",
            _ => id
        };
    }
}
