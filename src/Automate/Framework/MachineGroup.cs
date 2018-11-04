using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A collection of machines and storage which work as one unit.</summary>
    internal class MachineGroup
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The location containing the group.</summary>
        public GameLocation Location { get; }

        /// <summary>The machines in the group.</summary>
        public IMachine[] Machines { get; }

        /// <summary>The containers in the group.</summary>
        public IContainer[] Containers { get; }

        /// <summary>The storage manager for the group.</summary>
        public IStorage StorageManager { get; }

        /// <summary>The tiles comprising the group.</summary>
        public Vector2[] Tiles { get; }

        /// <summary>Whether the group has the minimum requirements to enable internal automation (i.e., at least one chest and one machine).</summary>
        public bool HasInternalAutomation => this.Machines.Length > 0 && this.Containers.Length > 0;


        /*********
        ** Public methods
        *********/
        /// <summary>Create an instance.</summary>
        /// <param name="location">The location containing the group.</param>
        /// <param name="machines">The machines in the group.</param>
        /// <param name="containers">The containers in the group.</param>
        /// <param name="tiles">The tiles comprising the group.</param>
        public MachineGroup(GameLocation location, IMachine[] machines, IContainer[] containers, Vector2[] tiles)
        {
            this.Location = location;
            this.Machines = machines;
            this.Containers = containers;
            this.Tiles = tiles;
            this.StorageManager = new StorageManager(containers);
        }

        /// <summary>Automate the machines inside the group.</summary>
        public void Automate()
        {
            foreach (IMachine machine in this.Machines)
            {
                MachineState state = machine.GetState();
                switch (state)
                {
                    case MachineState.Empty:
                        machine.SetInput(this.StorageManager);
                        break;

                    case MachineState.Done:
                        this.StorageManager.TryPush(machine.GetOutput());
                        break;
                }
            }
        }
    }
}
