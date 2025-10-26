using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley.GameData.Machines;

namespace Pathoschild.Stardew.Common.Integrations.ExtraMachineConfig;

/// <summary>Handles the logic for integrating with the Extra Machine Config mod.</summary>
internal class ExtraMachineConfigIntegration : BaseIntegration<IExtraMachineConfigApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public ExtraMachineConfigIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("ExtraMachineConfig", "selph.ExtraMachineConfig", "1.4.0", modRegistry, monitor) { }

    /// <inheritdoc cref="IExtraMachineConfigApi.GetExtraRequirements" />
    /// <param name="machineId">The qualified item ID for the machine type.</param>
    /// <param name="outputData">The output rule to check.</param>
    public IList<(string, int)> GetExtraRequirements(string machineId, MachineItemOutput outputData)
    {
        return
            this.SafelyCallApi(
                api => api.GetExtraRequirements(outputData),
                "Failed fetching extra requirements from {0}."
            )
            ?? [];
    }

    /// <inheritdoc cref="IExtraMachineConfigApi.GetExtraTagsRequirements" />
    /// <param name="machineId">The qualified item ID for the machine type.</param>
    /// <param name="outputData">The output rule to check.</param>
    public IList<(string, int)> GetExtraTagsRequirements(string machineId, MachineItemOutput outputData)
    {
        return
            this.SafelyCallApi(
                api => api.GetExtraTagsRequirements(outputData),
                "Failed fetching extra tag requirements from {0}."
            )
            ?? [];
    }

    /// <inheritdoc cref="IExtraMachineConfigApi.GetExtraOutputs" />
    /// <param name="machineId">The qualified item ID for the machine type.</param>
    /// <param name="outputData">The output rule to check.</param>
    /// <param name="machine">The machine data which contains the <paramref name="outputData"/>.</param>
    public IList<MachineItemOutput> GetExtraOutputs(string machineId, MachineItemOutput outputData, MachineData machine)
    {
        return
            this.SafelyCallApi(
                api => api.GetExtraOutputs(outputData, machine),
                "Failed fetching extra outputs from {0}."
            )
            ?? [];
    }
}
