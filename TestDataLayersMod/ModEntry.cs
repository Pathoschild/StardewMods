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
    [EventPriority(EventPriority.Normal - 1)]
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var dataLayers = this.Helper.ModRegistry.GetApi<IDataLayersApi>("Pathoschild.DataLayers")!;
        dataLayers.RegisterLayer(this.ModManifest, "checkerboard", new CheckerboardLayer());
    }
}
