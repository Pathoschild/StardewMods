namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>Configures the settings for a data layer which can be shown by the 'auto' layer.</summary>
internal class LayerConfigWithAutoSupport : LayerConfig
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether this layer can be shown by the 'auto' layer.</summary>
    public bool EnabledForAutoLayer { get; set; } = true;


    /*********
    ** Public methods
    *********/
    /// <summary>Whether the data layer can be shown by the 'auto' layer.</summary>
    public bool IsEnabledForAutoLayer()
    {
        return this.EnabledForAutoLayer && this.UpdatesPerSecond > 0;
    }
}
