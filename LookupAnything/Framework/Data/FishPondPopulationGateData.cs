namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>A fish pond population gate which unlocks new drops.</summary>
    internal class FishPondPopulationGateData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The population needed to unlock the gate.</summary>
        public int RequiredPopulation { get; }

        /// <summary>The population after the gate is unlocked.</summary>
        public int NewPopulation => this.RequiredPopulation + 1;

        /// <summary>The items required to unlock the gate. If the list has multiple entries, one will be chosen randomly.</summary>
        public FishPondPopulationGateQuestItemData[] RequiredItems { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="requiredPopulation">The population needed to unlock the gate.</param>
        /// <param name="requiredItems">The items required to unlock the gate. If the list has multiple entries, one will be chosen randomly.</param>
        public FishPondPopulationGateData(int requiredPopulation, FishPondPopulationGateQuestItemData[] requiredItems)
        {
            this.RequiredPopulation = requiredPopulation;
            this.RequiredItems = requiredItems;
        }
    }
}
