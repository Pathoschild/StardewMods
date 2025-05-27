using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Constants;
using StardewValley.Extensions;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects;

/// <summary>A crab pot that accepts input and provides output.</summary>
/// <remarks>Derived from <see cref="CrabPot.DayUpdate"/> and <see cref="CrabPot.performObjectDropInAction"/>.</remarks>
internal class CrabPotMachine : GenericObjectMachine<CrabPot>
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>The qualified fish IDs for which any crab pot has logged an 'invalid fish data' error.</summary>
    private static readonly HashSet<string> LoggedInvalidDataErrors = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="machine">The underlying machine.</param>
    /// <param name="location">The location containing the machine.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="tile">The tile covered by the machine.</param>
    public CrabPotMachine(CrabPot machine, GameLocation location, Vector2 tile, IMonitor monitor)
        : base(machine, location, tile)
    {
        this.Monitor = monitor;
    }

    /// <inheritdoc />
    public override MachineState GetState()
    {
        MachineState state = this.GetGenericState();

        if (state == MachineState.Empty && !this.Machine.NeedsBait(null))
            state = MachineState.Processing;

        return state;
    }

    /// <inheritdoc />
    /// <remarks>Derived from <see cref="CrabPot.checkForAction"/>.</remarks>
    public override ITrackedStack? GetOutput()
    {
        CrabPot pot = this.Machine;
        Item? output = pot.heldObject.Value;

        // The Art o' Crabbing book adds a 25% chance at double output when you check the crab pot.
        //
        // It's possible that there's only room for some of the output, which means a player could collect part of
        // the output multiple times. That's hard to solve, it's technically an issue in the base game too, and it
        // only works until the 25% chance fails, so it's probably fine as a known limitation here.
        if (output != null && Game1.player.stats.Get(StatKeys.Book_Crabbing) > 0 && Utility.CreateDaySaveRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed * 77, pot.TileLocation.X * 777 + pot.TileLocation.Y).NextBool(.25))
        {
            output = output.getOne();
            output.Stack = pot.heldObject.Value.Stack * 2;
        }

        return this.GetTracked(output, onEmpty: this.Reset);
    }

    /// <inheritdoc />
    public override bool SetInput(IStorage input)
    {
        // get bait
        if (input.TryGetIngredient(p => p.Sample is { TypeDefinitionId: ItemRegistry.type_object, Category: SObject.baitCategory }, 1, out IConsumable? bait))
        {
            if (!Game1.netWorldState.Value.farmhandData.ContainsKey(this.Machine.owner.Value))
                this.Machine.owner.Value = Game1.player.UniqueMultiplayerID; // default owner to main player if not owned by a farmhand

            this.Machine.bait.Value = (SObject)bait.Take()!;
            this.Machine.lidFlapping = true;
            this.Machine.lidFlapTimer = CrabPot.lidFlapTimerInterval;
            return true;
        }

        return false;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Reset the machine, so it's ready to accept a new input.</summary>
    /// <param name="item">The output item that was taken.</param>
    /// <remarks>XP and achievement logic based on <see cref="CrabPot.checkForAction"/>.</remarks>
    private void Reset(Item item)
    {
        CrabPot pot = this.Machine;
        Farmer owner = this.GetOwner();

        // add fishing XP
        owner.gainExperience(Farmer.fishingSkill, 5);

        // mark fish caught for achievements and stats
        IDictionary<string, string> fishData = DataLoader.Fish(Game1.content);
        if (fishData.TryGetValue(item.ItemId, out string? fishRow))
        {
            int size = 0;
            try
            {
                string[] fields = fishRow.Split('/');
                int lowerSize = ArgUtility.GetInt(fields, 5, 1);
                int upperSize = ArgUtility.GetInt(fields, 6, 10);
                size = Game1.random.Next(lowerSize, upperSize + 1);
            }
            catch (Exception ex)
            {
                // The fish length stats don't affect anything, so it's not worth notifying the
                // user; just log one trace message per affected fish for troubleshooting.
                if (CrabPotMachine.LoggedInvalidDataErrors.Add(item.QualifiedItemId))
                    this.Monitor.Log($"The game's fish data has an invalid entry ({item.ItemId}: {fishData[item.ItemId]}). Automated crab pots won't track fish length stats for that fish.\n{ex}");
            }

            owner.caughtFish(item.ItemId, size);
        }

        // reset pot
        this.GenericReset(item);
        pot.tileIndexToShow = 710;
        pot.bait.Value = null;
        pot.lidFlapping = true;
        pot.lidFlapTimer = 60f;
        pot.shake = Vector2.Zero;
        pot.shakeTimer = 0f;
    }
}
