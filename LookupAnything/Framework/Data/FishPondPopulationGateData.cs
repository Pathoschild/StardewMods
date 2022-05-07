namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>A fish pond population gate which unlocks new drops.</summary>
    /// <param name="RequiredPopulation">The population needed to unlock the gate.</param>
    /// <param name="RequiredItems">The items required to unlock the gate. If the list has multiple entries, one will be chosen randomly.</param>
    internal record FishPondPopulationGateData(int RequiredPopulation, FishPondPopulationGateQuestItemData[] RequiredItems)
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The population after the gate is unlocked.</summary>
        public int NewPopulation => this.RequiredPopulation + 1;
    }
}
