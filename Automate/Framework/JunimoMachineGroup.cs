using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>An aggregate collection of machine groups linked by Junimo chests.</summary>
    internal class JunimoMachineGroup : MachineGroup
    {
        /*********
        ** Fields
        *********/
        /// <summary>Sort machines by priority.</summary>
        private readonly Func<IEnumerable<IMachine>, IEnumerable<IMachine>> SortMachines;

        /// <summary>The underlying machine groups.</summary>
        private readonly List<IMachineGroup> MachineGroups = new();


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public override bool HasInternalAutomation => this.Machines.Length > 0;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="sortMachines">Sort machines by priority.</param>
        public JunimoMachineGroup(Func<IEnumerable<IMachine>, IEnumerable<IMachine>> sortMachines)
            : base(
                locationKey: null,
                machines: new IMachine[0],
                containers: new IContainer[0],
                tiles: new Vector2[0]
            )
        {
            this.IsJunimoGroup = true;
            this.SortMachines = sortMachines;
        }

        /// <summary>Get the underlying machine groups.</summary>
        public IEnumerable<IMachineGroup> GetAll()
        {
            return this.MachineGroups;
        }

        /// <summary>Add machine groups to the collection.</summary>
        /// <param name="groups">The groups to add.</param>
        /// <remarks>Make sure to call <see cref="Rebuild"/> after making changes.</remarks>
        public void Add(params IMachineGroup[] groups)
        {
            this.MachineGroups.AddRange(groups);
        }

        /// <summary>Remove machine groups from the collection.</summary>
        /// <param name="match">A predicate which returns true for locations that should be removed.</param>
        /// <returns>Returns whether any machine groups were removed.</returns>
        /// <remarks>Make sure to call <see cref="Rebuild"/> after making changes.</remarks>
        public bool RemoveAll(Predicate<IMachineGroup> match)
        {
            return this.MachineGroups.RemoveAll(match) > 0;
        }

        /// <summary>Rebuild the aggregate group for changes to the underlying machine groups.</summary>
        public void Rebuild()
        {
            this.StorageManager.SetContainers(this.GetUniqueContainers());

            int junimoChests = 0;
            this.Containers = this.MachineGroups.SelectMany(p => p.Containers).Where(p => !p.IsJunimoChest || ++junimoChests == 1).ToArray();
            this.Machines = this.SortMachines(this.MachineGroups.SelectMany(p => p.Machines)).ToArray();
            this.Tiles = this.MachineGroups.SelectMany(p => p.Tiles).ToArray();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the unique containers from the underlying machine groups.</summary>
        private IEnumerable<IContainer> GetUniqueContainers()
        {
            int junimoChests = 0;
            foreach (IContainer container in this.MachineGroups.SelectMany(p => p.Containers))
            {
                if (!container.IsJunimoChest || ++junimoChests == 1)
                    yield return container;
            }
        }
    }
}
