using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework;

/// <summary>The model for the <c>data.json</c> file.</summary>
internal class ModData
{
    /*********
    ** Accessors
    *********/
    /// <summary>The predefined world areas for <see cref="ChestRange.CurrentWorldArea"/>.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
    public Dictionary<string, HashSet<string>> WorldAreas { get; } = [];
}
