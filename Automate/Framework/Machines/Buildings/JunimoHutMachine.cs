using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework.Models;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Buildings;

/// <summary>A Junimo hut machine that accepts input and provides output.</summary>
internal class JunimoHutMachine : BaseMachineForBuilding<JunimoHut>
{
    /*********
    ** Fields
    *********/
    /// <summary>How to handle gems in the hut or connected chests.</summary>
    /// <remarks>This is never <see cref="JunimoHutBehavior.AutoDetect"/>, since that gets replaced before the machine is constructed.</remarks>
    private readonly JunimoHutBehavior GemBehavior;

    /// <summary>How to handle fertilizer in the hut or connected chests.</summary>
    /// <inheritdoc cref="GemBehavior" path="/remarks" />
    private readonly JunimoHutBehavior FertilizerBehavior;

    /// <summary>How to handle seeds in the hut or connected chests.</summary>
    /// <inheritdoc cref="GemBehavior" path="/remarks" />
    private readonly JunimoHutBehavior SeedBehavior;

    /// <inheritdoc cref="ModConfig.JunimoHutBehaviors" />
    private readonly Dictionary<string, JunimoHutBehavior> ItemBehavior;

    /// <summary>Whether the mod settings specify any items which should be moved into the Junimo hut.</summary>
    private readonly bool HasInputRules;

    /// <summary>Whether the mod settings specify any items which should <strong>not</strong> be moved into a connected chest.</summary>
    private readonly bool HasIgnoreOutputRules;

    /// <summary>The Junimo hut's output chest.</summary>
    private Chest Output => this.Machine.GetOutputChest();


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="hut">The underlying Junimo hut.</param>
    /// <param name="location"><inheritdoc cref="BaseMachine.Location" path="/summary" /></param>
    /// <param name="gemBehavior"><inheritdoc cref="GemBehavior" path="/summary" /></param>
    /// <param name="fertilizerBehavior"><inheritdoc cref="FertilizerBehavior" path="/summary" /></param>
    /// <param name="seedBehavior"><inheritdoc cref="SeedBehavior" path="/summary" /></param>
    /// <param name="itemBehavior"><inheritdoc cref="ItemBehavior" path="/summary" /></param>
    public JunimoHutMachine(JunimoHut hut, GameLocation location, JunimoHutBehavior gemBehavior, JunimoHutBehavior fertilizerBehavior, JunimoHutBehavior seedBehavior, Dictionary<string, JunimoHutBehavior> itemBehavior)
        : base(hut, location, BaseMachine.GetTileAreaFor(hut))
    {
        this.GemBehavior = gemBehavior;
        this.FertilizerBehavior = fertilizerBehavior;
        this.SeedBehavior = seedBehavior;
        this.ItemBehavior = itemBehavior;

        this.HasInputRules =
            gemBehavior is JunimoHutBehavior.MoveIntoHut
            || fertilizerBehavior is JunimoHutBehavior.MoveIntoHut
            || seedBehavior is JunimoHutBehavior.MoveIntoHut
            || itemBehavior.Any(p => p.Value is JunimoHutBehavior.MoveIntoHut);

        this.HasIgnoreOutputRules =
            gemBehavior is not JunimoHutBehavior.MoveIntoChests
            || fertilizerBehavior is not JunimoHutBehavior.MoveIntoChests
            || seedBehavior is not JunimoHutBehavior.MoveIntoChests
            || itemBehavior.Any(p => p.Value is not (JunimoHutBehavior.AutoDetect or JunimoHutBehavior.MoveIntoChests));
    }

    /// <inheritdoc />
    public override MachineState GetState()
    {
        if (this.Machine.isUnderConstruction())
            return MachineState.Disabled;

        if (this.GetNextOutput() != null)
            return MachineState.Done;

        return this.HasInputRules
            ? MachineState.Empty
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
        // get next item
        if (this.HasInputRules)
        {
            foreach (ITrackedStack stack in input.GetItems())
            {
                if (this.ShouldMoveIntoHut(stack.Sample))
                {
                    Item item = stack.Take(1)!;
                    this.Output.addItem(item);
                    return true;
                }
            }
        }

        return false;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Remove an output item once it's been taken.</summary>
    /// <param name="trackedStack">The tracked item stack that was reduced.</param>
    /// <param name="item">The removed item.</param>
    private void OnOutputTaken(ITrackedStack trackedStack, Item item)
    {
        this.Output.clearNulls();
        this.Output.Items.Remove(item);
    }

    /// <summary>Get the next output item.</summary>
    private Item? GetNextOutput()
    {
        foreach (Item? item in this.Output.Items)
        {
            if (item is not null && this.ShouldMoveIntoChest(item))
                return item;
        }

        return null;
    }

    /// <summary>Get whether an item should be moved into a connected chest.</summary>
    /// <param name="item">The item to check.</param>
    private bool ShouldMoveIntoChest(Item item)
    {
        bool hasCustomBehavior = this.HasIgnoreOutputRules;

        // check item ID
        if (hasCustomBehavior && this.ItemBehavior.TryGetValue(item.QualifiedItemId, out JunimoHutBehavior behavior))
        {
            switch (behavior)
            {
                case JunimoHutBehavior.Ignore:
                case JunimoHutBehavior.MoveIntoHut:
                    return false;

                case JunimoHutBehavior.MoveIntoChests:
                    return true;
            }
        }
        if (item.QualifiedItemId == "(O)Raisins")
            return false; // raisins change the vanilla Junimo hut behavior

        // check item category
        if (hasCustomBehavior)
        {
            switch (item.Category)
            {
                case SObject.SeedsCategory when this.SeedBehavior is not JunimoHutBehavior.MoveIntoChests:
                case SObject.fertilizerCategory when this.FertilizerBehavior is not JunimoHutBehavior.MoveIntoChests:
                case (SObject.GemCategory or SObject.mineralsCategory) when this.GemBehavior is not JunimoHutBehavior.MoveIntoChests:
                    return false;
            }
        }

        // all items are moved into connected chests by default
        return true;
    }

    /// <summary>Get whether an item should be moved into the Junimo hut.</summary>
    /// <param name="item">The item to check.</param>
    private bool ShouldMoveIntoHut(Item item)
    {
        // check item ID
        if (this.ItemBehavior.TryGetValue(item.QualifiedItemId, out JunimoHutBehavior behavior))
        {
            switch (behavior)
            {
                case JunimoHutBehavior.Ignore:
                case JunimoHutBehavior.MoveIntoChests:
                    return false;

                case JunimoHutBehavior.MoveIntoHut:
                    return true;
            }
        }

        // check item category
        switch (item.Category)
        {
            case SObject.SeedsCategory when this.SeedBehavior is JunimoHutBehavior.MoveIntoHut:
            case SObject.fertilizerCategory when this.FertilizerBehavior is JunimoHutBehavior.MoveIntoHut:
            case (SObject.GemCategory or SObject.mineralsCategory) when this.GemBehavior is JunimoHutBehavior.MoveIntoHut:
                return true;
        }

        // no items are moved into Junimo huts by default
        return false;
    }
}
