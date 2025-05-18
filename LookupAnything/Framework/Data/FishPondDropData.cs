using System;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data;

/// <summary>An item that can be produced by a fish pond.</summary>
internal record FishPondDropData : ItemDropData
{
    /*********
    ** Accessors
    *********/
    /// <summary>An instance of the produced item.</summary>
    public Item SampleItem { get; }

    /// <summary>The minimum population needed for the item to drop.</summary>
    public int MinPopulation { get; }

    /// <summary>Order by which drops are checked, lower is earlier.</summary>
    public int Precedence { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="minPopulation"><inheritdoc cref="MinPopulation" path="/summary"/></param>
    /// <param name="precedence"><inheritdoc cref="Precedence" path="/summary"/></param>
    /// <param name="sampleItem"><inheritdoc cref="SampleItem" path="/summary"/></param>
    /// <param name="minDrop"><inheritdoc cref="ItemDropData.MinDrop" path="/summary"/></param>
    /// <param name="maxDrop"><inheritdoc cref="ItemDropData.MaxDrop" path="/summary"/></param>
    /// <param name="probability"><inheritdoc cref="ItemDropData.Probability" path="/summary"/></param>
    /// <param name="conditions"><inheritdoc cref="ItemDropData.Conditions" path="/summary"/></param>
    public FishPondDropData(int minPopulation, int precedence, Item sampleItem, int minDrop, int maxDrop, float probability, string? conditions)
        : base(sampleItem.QualifiedItemId, minDrop, maxDrop, probability, conditions)
    {
        this.SampleItem = sampleItem;
        this.MinPopulation = Math.Max(minPopulation, 1); // rule only applies if the pond has at least one fish, so assume minimum of 1 to avoid player confusion
        this.Precedence = precedence;
    }
}
