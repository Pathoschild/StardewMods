using System.Collections.Generic;
using StardewValley.GameData.Machines;

namespace Pathoschild.Stardew.Common.Integrations.ExtraMachineConfig;

/// <summary>The API provided by the Extra Machine Config mod.</summary>
public interface IExtraMachineConfigApi
{
    /// <summary>Retrieves the extra fuels consumed by this recipe.</summary>
    /// <param name="outputData">The output rule to check.</param>
    IList<(string, int)> GetExtraRequirements(MachineItemOutput outputData);

    /// <summary>Retrieves the extra tag-defined fuels consumed by this recipe.</summary>
    /// <param name="outputData">The output rule to check.</param>
    IList<(string, int)> GetExtraTagsRequirements(MachineItemOutput outputData);

    /// <summary>Retrieves the extra output items produced by this recipe.</summary>
    /// <param name="outputData">The output rule to check.</param>
    /// <param name="machine">The machine data which contains the <paramref name="outputData"/>.</param>
    IList<MachineItemOutput> GetExtraOutputs(MachineItemOutput outputData, MachineData? machine);
}
