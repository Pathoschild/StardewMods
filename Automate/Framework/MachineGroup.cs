using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A collection of machines and storage which work as one unit.</summary>
    internal class MachineGroup : IMachineGroup
    {
        /*********
        ** Fields
        *********/
        /// <summary>The number of milliseconds to pause machines when they crash.</summary>
        private readonly int ErrorPauseMilliseconds = 30000;

        /// <summary>Machines which are temporarily paused, with the game time in milliseconds when their pause expires.</summary>
        private readonly IDictionary<IMachine, double> MachinePauseExpiries = new Dictionary<IMachine, double>(new ObjectReferenceComparer<IMachine>());

        /// <summary>The storage manager for the group.</summary>
        protected readonly StorageManager StorageManager;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string LocationKey { get; }

        /// <inheritdoc />
        public IMachine[] Machines { get; protected set; }

        /// <inheritdoc />
        public IContainer[] Containers { get; protected set; }

        /// <inheritdoc />
        public Vector2[] Tiles { get; protected set; }

        /// <inheritdoc />
        public bool IsJunimoGroup { get; protected set; }

        /// <inheritdoc />
        public virtual bool HasInternalAutomation => this.IsJunimoGroup || (this.Machines.Length > 0 && this.Containers.Any(p => !p.IsJunimoChest));


        /*********
        ** Public methods
        *********/
        /// <summary>Create an instance.</summary>
        /// <param name="locationKey">The main location containing the group (as formatted by <see cref="MachineGroupFactory.GetLocationKey"/>).</param>
        /// <param name="machines">The machines in the group.</param>
        /// <param name="containers">The containers in the group.</param>
        /// <param name="tiles">The tiles comprising the group.</param>
        public MachineGroup(string locationKey, IEnumerable<IMachine> machines, IEnumerable<IContainer> containers, IEnumerable<Vector2> tiles)
        {
            this.LocationKey = locationKey;
            this.Machines = machines.ToArray();
            this.Containers = containers.ToArray();
            this.Tiles = tiles.ToArray();

            this.IsJunimoGroup = this.Containers.Any(p => p.IsJunimoChest);
            this.StorageManager = new StorageManager(this.Containers);
        }

        /// <inheritdoc />
        public void Automate()
        {
            IStorage storage = this.StorageManager;
            double curTime = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;

            // clear expired timers
            if (this.MachinePauseExpiries.Count > 0)
            {
                foreach (var entry in this.MachinePauseExpiries.ToArray())
                {
                    if (curTime >= entry.Value)
                        this.MachinePauseExpiries.Remove(entry.Key);
                }
            }

            // get machines ready for input/output
            IList<IMachine> outputReady = new List<IMachine>();
            IList<IMachine> inputReady = new List<IMachine>();
            foreach (IMachine machine in this.Machines)
            {
                if (this.MachinePauseExpiries.ContainsKey(machine))
                    continue;

                switch (machine.GetState())
                {
                    case MachineState.Done:
                        outputReady.Add(machine);
                        break;

                    case MachineState.Empty:
                        inputReady.Add(machine);
                        break;
                }
            }
            if (!outputReady.Any() && !inputReady.Any())
                return;

            // process output
            foreach (IMachine machine in outputReady)
            {
                ITrackedStack output = null;
                try
                {
                    output = machine.GetOutput();
                    if (storage.TryPush(output) && machine.GetState() == MachineState.Empty)
                        inputReady.Add(machine);
                }
                catch (Exception ex)
                {
                    string error = $"Failed to automate machine '{machine.MachineTypeID}' at {machine.Location?.Name} (tile: {machine.TileArea.X}, {machine.TileArea.Y}). An error occurred while ";
                    if (output == null)
                        error += "retrieving its output.";
                    else
                    {
                        error += $"storing its output item #{output.Sample.ParentSheetIndex} ('{output.Sample.Name}'";
                        if (output.Sample is SObject outputObj && outputObj.preservedParentSheetIndex.Value >= 0)
                            error += $", preserved item #{outputObj.preservedParentSheetIndex.Value}";
                        error += ").";
                    }
                    error += $" Machine paused for {this.ErrorPauseMilliseconds / 1000}s.";

                    this.MachinePauseExpiries[machine] = curTime + this.ErrorPauseMilliseconds;
                    throw new InvalidOperationException(error, ex);
                }
            }

            // process input
            HashSet<string> ignoreMachines = new HashSet<string>();
            foreach (IMachine machine in inputReady)
            {
                if (ignoreMachines.Contains(machine.MachineTypeID))
                    continue;

                if (!machine.SetInput(storage))
                    ignoreMachines.Add(machine.MachineTypeID); // if the machine can't process available input, no need to ask every instance of its type
            }
        }
    }
}
