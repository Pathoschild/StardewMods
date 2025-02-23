using Pathoschild.Stardew.DataLayers;
using Pathoschild.Stardew.TestDataLayersMod.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Pathoschild.Stardew.TestDataLayersMod;

/// <summary>The mod entry point.</summary>
public class ModEntry : Mod
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.Helper.ModRegistry
            .GetApi<IDataLayersApi>("Pathoschild.DataLayers")
            ?.RegisterLayer("checkerboard", I18n.Layer_Name, CheckerboardLayer.GetTileGroups, CheckerboardLayer.UpdateTiles);
    }
}
