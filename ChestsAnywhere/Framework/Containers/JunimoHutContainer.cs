using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;

/// <summary>A storage container for an in-game Junimo huts.</summary>
internal class JunimoHutContainer : ChestContainer
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying Junimo hut.</summary>
    private readonly JunimoHut JunimoHut;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="junimoHut">The in-game junimo hut.</param>
    public JunimoHutContainer(JunimoHut junimoHut)
        : base(junimoHut.GetOutputChest(), context: junimoHut, showColorPicker: false)
    {
        this.JunimoHut = junimoHut;
    }

    /// <inheritdoc />
    public override bool TryGetIcon([NotNullWhen(true)] out Texture2D? texture, out Rectangle sourceRect, out float scale)
    {
        texture = this.JunimoHut.texture.Value;
        sourceRect = this.JunimoHut.getSourceRectForMenu() ?? this.JunimoHut.getSourceRect();
        scale = Game1.pixelZoom * (32f / sourceRect.Height);
        return true;
    }
}
