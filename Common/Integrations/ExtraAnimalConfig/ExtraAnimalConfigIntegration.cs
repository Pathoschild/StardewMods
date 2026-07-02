using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley.GameData;

namespace Pathoschild.Stardew.Common.Integrations.ExtraAnimalConfig;

/// <summary>Handles the logic for integrating with the Extra Machine Config mod.</summary>
internal class ExtraAnimalConfigIntegration : BaseIntegration<IExtraAnimalConfigApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public ExtraAnimalConfigIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("ExtraMachineConfig", "selph.ExtraAnimalConfig", "1.9.7", modRegistry, monitor) { }

    /// <inheritdoc cref="IExtraAnimalConfigApi.GetItemQueryOverrides(string, string)" />
    /// <param name="animalType">the animal type (ie. the key in Data/FarmAnimals).</param>
    /// <param name="produceId">the qualified or unqualified ID of the base produce (ie. the value in (Deluxe)ProduceItemIds)</param>
    public List<GenericSpawnItemDataWithCondition> GetItemQueryOverrides(string animalType, string produceId)
    {
        return
            this.SafelyCallApi(
                api => api.GetItemQueryOverrides(animalType, produceId),
                "Failed to get animal item query overrides for {0} {0}."
            )
            ?? [];
    }

    /// <inheritdoc cref="IExtraAnimalConfigApi.GetExtraDrops(string)" />
    /// <param name="animalType">the animal type (ie. the key in Data/FarmAnimals).</param>
    public Dictionary<string, List<string>> GetExtraDrops(string animalType)
    {
        return
            this.SafelyCallApi(
                api => api.GetExtraDrops(animalType),
                "Failed to get animal extra drops for {0}."
            )
            ?? [];
    }
}
