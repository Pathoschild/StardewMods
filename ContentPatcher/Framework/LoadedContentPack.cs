using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework;

/// <summary>A loaded content pack.</summary>
internal class LoadedContentPack : RawContentPack
{
    /*********
    ** Accessors
    *********/
    /// <summary>Handles reading, normalizing, and saving the configuration for the content pack.</summary>
    public ConfigFileHandler ConfigFileHandler { get; }

    /// <summary>The content pack's configuration.</summary>
    public InvariantDictionary<ConfigField> Config { get; private set; }
    public GenericModConfigMenuIntegrationForContentPack GMCMConfigMenu { get; internal set; }
    public GenericModConfigMenuIntegration<InvariantDictionary<ConfigField>> GMCMAPI { get; internal set; }
    private IMonitor Monitor { get; }

    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="contentPack">The raw content pack instance.</param>
    /// <param name="configFileHandler">Handles reading, normalizing, and saving the configuration for the content pack.</param>
    /// <param name="config">The content pack's configuration.</param>
    public LoadedContentPack(RawContentPack contentPack, ConfigFileHandler configFileHandler, InvariantDictionary<ConfigField> config, IMonitor monitor)
        : base(contentPack)
    {
        this.ConfigFileHandler = configFileHandler;
        this.Config = config;
        this.Monitor = monitor;
    }

    public void ReloadConfig()
    {
        var config = this.ConfigFileHandler.Read(this.ContentPack, this.Content.ConfigSchema, this.Content.Format!);
        this.ConfigFileHandler.Save(this.ContentPack, config);
        this.Config = config;

        this.GMCMConfigMenu.Config = config;
        this.GMCMConfigMenu.Register(this.GMCMAPI, this.Monitor);
    }
}
