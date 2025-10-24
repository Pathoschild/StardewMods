using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects;

/// <summary>An auto-grabber that provides output.</summary>
/// <remarks>Derived from <see cref="SObject.DayUpdate"/> and <see cref="SObject.checkForAction"/> (search for 'case 165').</remarks>
internal class AutoGrabberMachine : GenericObjectMachine<SObject>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="machine">The underlying machine.</param>
    /// <param name="location">The in-game location.</param>
    /// <param name="tile">The tile covered by the machine.</param>
    public AutoGrabberMachine(SObject machine, GameLocation location, Vector2 tile)
        : base(machine, location, tile) { }

    /// <inheritdoc />
    public override MachineState GetState()
    {
        return this.GetNextOutput() != null
            ? MachineState.Done
            : MachineState.Processing;
    }

    /// <inheritdoc />
    public override ITrackedStack? GetOutput()
    {
        return this.GetTracked(this.GetNextOutput(), onEmpty: this.OnOutputTaken);
    }

    /// <inheritdoc />
    public override bool SetInput(IStorage input)
    {
        return false;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the output chest, if valid.</summary>
    /// <param name="chest">The chest that was found, if any.</param>
    /// <returns>Returns whether an output chest was found.</returns>
    private bool TryGetOutputChest([NotNullWhen(true)] out Chest? chest)
    {
        chest = this.Machine.heldObject.Value as Chest;
        return chest != null;
    }

    /// <summary>Remove an output item once it's been taken.</summary>
    /// <param name="trackedStack">The tracked item stack that was reduced.</param>
    /// <param name="item">The removed item.</param>
    private void OnOutputTaken(ITrackedStack trackedStack, Item item)
    {
        if (this.TryGetOutputChest(out Chest? output))
        {
            output.clearNulls();
            output.Items.Remove(item);
            this.Machine.showNextIndex.Value = !output.isEmpty();
        }
    }

    /// <summary>Get the next output item.</summary>
    private Item? GetNextOutput()
    {
        if (this.TryGetOutputChest(out Chest? output))
        {
            foreach (Item item in output.Items)
            {
                if (item == null)
                    continue;

                return item;
            }
        }

        return null;
    }
}
