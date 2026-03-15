using System;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.CentralStation.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForCentralStation : IGenericModConfigMenuIntegrationFor<ModConfig>
{
    /*********
    ** Fields
    *********/
    /// <summary>Get a translation provided by the content pack.</summary>
    private readonly Func<string, object[], string> GetTranslation;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="getTranslation">Get a translation provided by the content pack.</param>
    public GenericModConfigMenuIntegrationForCentralStation(Func<string, object[], string> getTranslation)
    {
        this.GetTranslation = getTranslation;
    }

    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
    {
        menu
            .Register()
            .AddCheckbox(
                name: () => this.GetTranslation("config.require-pam-bus.name", []),
                tooltip: () => this.GetTranslation("config.require-pam-bus.desc", []),
                get: config => config.RequirePamBus,
                set: (config, value) => config.RequirePamBus = value
            )
            .AddCheckbox(
                name: () => this.GetTranslation("config.require-pam.name", []),
                tooltip: () => this.GetTranslation("config.require-pam.desc", []),
                get: config => config.RequirePam,
                set: (config, value) => config.RequirePam = value
            );
    }
}
