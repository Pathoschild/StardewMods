using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.BushBloomMod;

/// <summary>Model used for custom bushes.</summary>
/// <remarks>Derived from <a href="https://github.com/ncarigon/StardewValleyMods/blob/main/BushBloomMod/BushBloomMod/Api.cs">Bush Bloom Mod's <c>Api</c> class</a>.</remarks>
[SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Parameter names match the original Custom Bush interface.")]
public interface IBushBloomModApi
{
    /// <summary>Get an array of (item_id, first_day, last_day) for all possible active blooming schedules on the given season and day, optionally within the given year and/or location.</summary>
    (string, WorldDate, WorldDate)[] GetActiveSchedules(string season, int dayofMonth, int? year = null, GameLocation? location = null, Vector2? tile = null);

    /// <summary>Get whether Bush Bloom Mod successfully parsed all schedules.</summary>
    bool IsReady();
}
