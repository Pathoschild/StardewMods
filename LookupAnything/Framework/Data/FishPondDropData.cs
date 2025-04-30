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
    /// <param name="minPopulation">The minimum population needed for the item to drop.</param>
    /// <param name="itemID">The unqualified item ID.</param>
    /// <param name="minDrop">The minimum number to drop.</param>
    /// <param name="maxDrop">The maximum number to drop.</param>
    /// <param name="probability">The probability that the item will be dropped.</param>
    /// <param name="conditions">If set, a game state query which indicates when this entry should be applied.</param>
    public FishPondDropData(int minPopulation, int precedence, Item samppleItem, int minDrop, int maxDrop, float probability, string? conditions)
        : base(samppleItem.QualifiedItemId, minDrop, maxDrop, probability, conditions)
    {
        this.SampleItem = samppleItem;
        this.MinPopulation = Math.Max(minPopulation, 1); // rule only applies if the pond has at least one fish, so assume minimum of 1 to avoid player confusion
        this.Precedence = precedence;
    }
}
