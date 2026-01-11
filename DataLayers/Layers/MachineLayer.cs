using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using Pathoschild.Stardew.DataLayers.Framework.ConfigModels;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.DataLayers.Layers;

/// <summary>A data layer which shows which machines are currently processing.</summary>
internal class MachineLayer : BaseLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend entry for machines with no input.</summary>
    private readonly LegendEntry Empty;

    /// <summary>The legend entry for machines that are currently processing input.</summary>
    private readonly LegendEntry Processing;

    /// <summary>The legend entry for machines whose output is ready to collect.</summary>
    private readonly LegendEntry Finished;

    /// <summary>Handles access to the supported mod integrations.</summary>
    private readonly ModIntegrations Mods;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    /// <param name="mods">Handles access to the supported mod integrations.</param>
    public MachineLayer(LayerConfig config, ColorScheme colors, ModIntegrations mods)
        : base(I18n.Machines_Name(), config)
    {
        const string layerId = "MachineProcessing";

        this.Legend = [
            this.Empty = new LegendEntry(I18n.Keys.Machines_Empty, colors.Get(layerId, "Empty", Color.Red)),
            this.Processing = new LegendEntry(I18n.Keys.Machines_Processing, colors.Get(layerId, "Processing", Color.Orange)),
            this.Finished = new LegendEntry(I18n.Keys.Machines_Finished, colors.Get(layerId, "Finished", Color.Green))
        ];
        this.Mods = mods;
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        var tileGroups = this
            .GetTiles(location, visibleArea, visibleTiles)
            .ToLookup(p => p.Type.Id);

        return [
            new TileGroup(tileGroups[this.Empty.Id], this.Empty.Color),
            new TileGroup(tileGroups[this.Processing.Id], this.Processing.Color),
            new TileGroup(tileGroups[this.Finished.Id], this.Finished.Color)
        ];
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the updated data layer tiles.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleArea">The tile area currently visible on the screen.</param>
    /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
    private IEnumerable<TileData> GetTiles(GameLocation location, Rectangle visibleArea, IReadOnlySet<Vector2> visibleTiles)
    {
        // get from Automate if installed
        if (this.Mods.Automate.IsLoaded)
        {
            IDictionary<Vector2, int> machineStates = this.Mods.Automate.GetMachineStates(location, visibleArea);
            foreach (Vector2 tile in visibleTiles)
            {
                LegendEntry? type = null;
                if (machineStates.TryGetValue(tile, out int state))
                {
                    type = state switch
                    {
                        1 => this.Empty,
                        2 => this.Processing,
                        3 => this.Finished,
                        _ => null
                    };
                }

                if (type != null)
                    yield return new TileData(tile, type);
            }
        }

        // else get standard machines from Data/Machines
        else
        {
            foreach (Vector2 tile in visibleTiles)
            {
                // get machine
                if (!location.objects.TryGetValue(tile, out Object? machine))
                    continue;
                if (machine is not CrabPot && !machine.HasContextTag("is_machine"))
                    continue;

                // get status
                if (machine.heldObject.Value == null && (machine is not CrabPot crabPot || crabPot.NeedsBait(null)))
                    yield return new TileData(tile, this.Empty);
                else if (machine.readyForHarvest.Value)
                    yield return new TileData(tile, this.Finished);
                else
                    yield return new TileData(tile, this.Processing);
            }
        }
    }
}
