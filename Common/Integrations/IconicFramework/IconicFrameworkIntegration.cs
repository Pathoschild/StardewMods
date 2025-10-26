using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.IconicFramework;

/// <summary>Handles the logic for integrating with the Iconic Framework mod.</summary>
internal class IconicFrameworkIntegration : BaseIntegration<IIconicFrameworkApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public IconicFrameworkIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("IconicFramework", "furyx639.ToolbarIcons", "3.1.0-beta.1", modRegistry, monitor) { }

    /// <summary>Add a single icon with a default identifier and onClick action.</summary>
    /// <param name="texturePath">The path to the texture icon.</param>
    /// <param name="sourceRect">The source rectangle of the icon.</param>
    /// <param name="getTitle">Text to appear as the title in the Radial Menu.</param>
    /// <param name="getDescription">Text to appear when hovering over the icon.</param>
    /// <param name="onClick">An action to perform when the icon is pressed.</param>
    /// <param name="onRightClick">An optional secondary action to perform when the icon is pressed.</param>
    public void AddToolbarIcon(string texturePath, Rectangle? sourceRect, Func<string>? getTitle, Func<string>? getDescription, Action onClick, Action? onRightClick = null)
    {
        this.AssertLoaded();

        this.SafelyCallApi(
            api => api.AddToolbarIcon(texturePath, sourceRect, getTitle, getDescription, onClick, onRightClick),
            "Failed adding toolbar icon from {0}."
        );
    }
}
