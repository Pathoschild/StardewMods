using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
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
        /// <summary>The number of milliseconds to pause output for a given item ID when the connected chests can't accept it.</summary>
        private readonly int OutputPauseMilliseconds = 5000;

        /// <summary>The number of milliseconds to pause machines when they crash.</summary>
        private readonly int MachinePauseMilliseconds = 30000;

        /// <summary>Machines which are temporarily paused, with the game time in milliseconds when their pause expires.</summary>
        private readonly Dictionary<IMachine, double> MachinePauseExpiries = new(new ObjectReferenceComparer<IMachine>());

        /// <summary>The output items which are temporarily paused, with the game time in milliseconds when their pause expires.</summary>
        private readonly Dictionary<string, double> OutputPauseExpiries = new();

        /// <summary>The storage manager for the group.</summary>
        protected readonly StorageManager StorageManager;

        /// <summary>The tiles covered by this machine group.</summary>
        private readonly HashSet<Vector2> Tiles;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string? LocationKey { get; }

        /// <inheritdoc />
        public IMachine[] Machines { get; protected set; }

        /// <inheritdoc />
        public IContainer[] Containers { get; protected set; }

        /// <inheritdoc />
        [MemberNotNullWhen(false, nameof(IMachineGroup.LocationKey))]
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
        /// <param name="buildStorage">Build a storage manager for the given containers.</param>
        public MachineGroup(string? locationKey, IEnumerable<IMachine> machines, IEnumerable<IContainer> containers, IEnumerable<Vector2> tiles, Func<IContainer[], StorageManager> buildStorage)
        {
            this.LocationKey = locationKey;
            this.Machines = machines.ToArray();
            this.Containers = containers.ToArray();
            this.Tiles = new HashSet<Vector2>(tiles);

            this.IsJunimoGroup = this.Containers.Any(p => p.IsJunimoChest);
            this.StorageManager = buildStorage(this.GetUniqueContainers(this.Containers));
        }

        /// <inheritdoc />
        public virtual IReadOnlySet<Vector2> GetTiles(string locationKey)
        {
            return this.LocationKey == locationKey
                ? this.Tiles
                : ImmutableHashSet<Vector2>.Empty;
        }

        /// <inheritdoc />
        public void Automate()
        {
            IStorage storage = this.StorageManager;
            double curTime = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;

            // clear expired timers
            if (this.MachinePauseExpiries.Count > 0)
            {
                IMachine[] expired = this.MachinePauseExpiries.Where(p => curTime >= p.Value).Select(p => p.Key).ToArray();
                foreach (IMachine machine in expired)
                    this.MachinePauseExpiries.Remove(machine);
            }
            if (this.OutputPauseExpiries.Count > 0)
            {
                string[] expired = this.OutputPauseExpiries.Where(p => curTime >= p.Value).Select(p => p.Key).ToArray();
                foreach (string itemId in expired)
                    this.OutputPauseExpiries.Remove(itemId);
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
                ITrackedStack? output = null;
                try
                {
                    // get output
                    output = machine.GetOutput();
                    if (output is null)
                    {
                        if (machine.GetState() is MachineState.Empty)
                            inputReady.Add(machine);
                        continue;
                    }

                    // check if ignored
                    string outputKey = $"{output.Type}:{output.Sample.ParentSheetIndex}";
                    if (this.OutputPauseExpiries.ContainsKey(outputKey))
                        continue;

                    // try to push output
                    if (storage.TryPush(output))
                    {
                        if (machine.GetState() is MachineState.Empty)
                            inputReady.Add(machine);
                        continue;
                    }

                    // ignore output that can't be stored in chest
                    this.OutputPauseExpiries[outputKey] = curTime + this.OutputPauseMilliseconds;
                }
                catch (Exception ex)
                {
                    string error = $"Failed to automate machine '{machine.MachineTypeID}' at {machine.Location?.Name} (tile: {machine.TileArea.X}, {machine.TileArea.Y}). An error occurred while ";
                    if (output == null)
                        error += "retrieving its output.";
                    else
                    {
                        error += $"storing its output item {output.Sample.QualifiedItemId} ('{output.Sample.Name}'";
                        if (output.Sample is SObject outputObj && CommonHelper.IsItemId(outputObj.preservedParentSheetIndex.Value))
                            error += $", preserved item {outputObj.preservedParentSheetIndex.Value}";
                        error += ").";
                    }
                    error += $" Machine paused for {this.MachinePauseMilliseconds / 1000}s.";

                    this.MachinePauseExpiries[machine] = curTime + this.MachinePauseMilliseconds;
                    throw new InvalidOperationException(error, ex);
                }
            }

            // process input
            HashSet<string> ignoreMachines = new();
            foreach (IMachine machine in inputReady)
            {
                if (ignoreMachines.Contains(machine.MachineTypeID))
                    continue;

                if (!machine.SetInput(storage))
                    ignoreMachines.Add(machine.MachineTypeID); // if the machine can't process available input, no need to ask every instance of its type
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get container instances, ensuring that only one container instance is returned for each shared inventory.</summary>
        /// <param name="containers">The containers to filter.</param>
        protected IContainer[] GetUniqueContainers(IEnumerable<IContainer> containers)
        {
            HashSet<object> seenInventories = new(new ObjectReferenceComparer<object>());

            return containers
                .Where(container => seenInventories.Add(container.InventoryReferenceId))
                .ToArray();
        }
    }
}
