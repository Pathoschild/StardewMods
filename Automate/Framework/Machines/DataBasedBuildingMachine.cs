using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Inventories;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Framework.Machines;

/// <summary>A building that accepts input and provides output based on the rules in <see cref="Game1.buildingData"/>.</summary>
internal class DataBasedBuildingMachine : BaseMachineForBuilding<Building>
{
    /*********
    ** Fields
    *********/
    /// <summary>The cached building data from <see cref="Building.GetData"/>.</summary>
    private BuildingData Data;

    /// <summary>The output chest IDs.</summary>
    private readonly HashSet<string> OutputChests = [];

    /// <summary>The input chest IDs.</summary>
    private readonly HashSet<string> InputChests = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="building">The underlying building.</param>
    /// <param name="location">The location which contains the building.</param>
    public DataBasedBuildingMachine(Building building, GameLocation location)
        : base(building, location, GetTileAreaFor(building), GetDefaultMachineId(building.buildingType.Value))
    {
        this.UpdateData();
    }

    /// <summary>Get whether a building can be automated by this implementation.</summary>
    /// <param name="building">The building instance.</param>
    public static bool CanAutomate(Building building)
    {
        return building.GetData()?.ItemConversions?.Count > 0;
    }

    /// <inheritdoc />
    public override MachineState GetState()
    {
        Building building = this.Machine;

        // disabled
        if (building.isUnderConstruction())
            return MachineState.Disabled;

        // output ready
        foreach (string chestId in this.OutputChests)
        {
            if (this.GetBuildingChest(chestId)?.Items?.Count > 0)
                return MachineState.Done;
        }

        // can accept input
        foreach (string chestId in this.InputChests)
        {
            Chest? chest = this.GetBuildingChest(chestId);
            if (chest is null)
                continue;

            if (!this.InputFull(chest))
                return MachineState.Empty;
        }

        return MachineState.Processing;
    }

    /// <inheritdoc />
    public override ITrackedStack? GetOutput()
    {
        foreach (string outputId in this.OutputChests)
        {
            Chest? output = this.GetBuildingChest(outputId);
            if (output is null)
                continue;

            foreach (Item item in output.Items)
            {
                if (item is not null)
                    return this.GetTracked(item, onEmpty: (_, taken) => this.OnOutputTaken(output, taken));
            }
        }

        return null;
    }

    /// <inheritdoc />
    public override bool SetInput(IStorage input)
    {
        return
            this.TryGetItemConversion(input, out Chest? inputChest, out ITrackedStack? fromStack, out BuildingItemConversion? rule)
            && this.TryAddInput(fromStack, inputChest, rule.RequiredCount);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Try to add an item to the input queue, and adjust its stack size accordingly.</summary>
    /// <param name="item">The item stack to add.</param>
    /// <param name="chest">The chest to which to add the item.</param>
    /// <param name="count">The number of the <paramref name="item"/> to add.</param>
    /// <returns>Returns whether any items were taken from the stack.</returns>
    private bool TryAddInput(ITrackedStack item, Chest chest, int count)
    {
        // nothing to add
        if (item.Count <= 0)
            return false;

        // clean up input bin
        chest.clearNulls();

        // try adding to input
        int originalCount = item.Count;
        IInventory slots = chest.GetItemsForPlayer();
        int maxStackSize = item.Sample.maximumStackSize();
        for (int i = 0; i < chest.GetActualCapacity() && count > 0; i++)
        {
            // add new slot
            if (slots.Count <= i)
            {
                slots.Add(item.Take(count)!);
                count = 0;
            }

            // else add to existing slot
            else
            {
                Item slot = slots[i];
                if (item.Sample.canStackWith(slot) && slot.Stack < maxStackSize)
                {
                    Item sample = item.Sample.getOne();
                    sample.Stack = Math.Min(count, maxStackSize - slot.Stack); // the most items we can add to the stack (in theory)
                    int actualAdded = sample.Stack - slot.addToStack(sample); // how many items were actually added to the stack

                    item.Reduce(actualAdded);
                    count -= actualAdded;
                }
            }
        }

        return count < originalCount;
    }

    /// <summary>Get whether an input chest is full.</summary>
    /// <param name="chest">The input chest to check.</param>
    private bool InputFull(Chest chest)
    {
        Inventory slots = chest.Items;

        // free slots
        if (slots.Count < Chest.capacity)
            return false;

        // free space in stacks
        foreach (Item? slot in slots)
        {
            if (slot is null || slot.Stack < slot.maximumStackSize())
                return false;
        }

        return true;
    }

    /// <summary>Remove an output item once it's been taken.</summary>
    /// <param name="outputChest">The chest from which the item was taken.</param>
    /// <param name="item">The removed item.</param>
    private void OnOutputTaken(Chest outputChest, Item item)
    {
        outputChest.clearNulls();
        outputChest.Items.Remove(item);
    }

    /// <summary>Get an input or output chest that's part of the building (not a chest connected to it through Automate).</summary>
    /// <param name="id">The chest ID.</param>
    private Chest? GetBuildingChest(string id)
    {
        return this.Machine.GetBuildingChest(id); // TODO: cache building chests to avoid iterating the list each time?
    }

    /// <summary>Update the cached data.</summary>
    [MemberNotNull(nameof(Data))]
    private void UpdateData()
    {
        this.Data = this.Machine.GetData();
        this.InputChests.Clear();
        this.OutputChests.Clear();

        MachineDataHelper.GetBuildingChestNames(this.Data, this.InputChests, this.OutputChests);
    }

    /// <summary>Get an item and conversion rule which can be applied for the given input.</summary>
    /// <param name="input">The available items.</param>
    /// <param name="forInputChest">The building chest to which to add the input.</param>
    /// <param name="item">The item to add to the chest.</param>
    /// <param name="conversionRule">The conversion rule that will be applied.</param>
    private bool TryGetItemConversion(IStorage input, [NotNullWhen(true)] out Chest? forInputChest, [NotNullWhen(true)] out ITrackedStack? item, [NotNullWhen(true)] out BuildingItemConversion? conversionRule)
    {
        // get matching conversion rule
        Building building = this.Machine;
        foreach (string inputChestId in this.InputChests)
        {
            Chest? curChest = this.GetBuildingChest(inputChestId);
            if (curChest is null || this.InputFull(curChest))
                continue;

            foreach (ITrackedStack curItem in input.GetItems())
            {
                BuildingItemConversion? curRule = building.GetItemConversionForItem(curItem.Sample, curChest);
                if (curRule != null && curRule.RequiredCount <= curItem.Count)
                {
                    forInputChest = curChest;
                    item = curItem;
                    conversionRule = curRule;
                    return true;
                }
            }
        }

        // none found
        forInputChest = null;
        item = null;
        conversionRule = null;
        return false;
    }
}
