using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;

/// <summary>A storage container for an in-game Junimo huts.</summary>
internal class JunimoHutContainer : ChestContainer
{
    private readonly JunimoHut junimoHut;

    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="junimoHut">The in-game junimo hut.</param>
    public JunimoHutContainer(JunimoHut junimoHut)
        : base(junimoHut.GetOutputChest(), context: junimoHut, showColorPicker: false)
    {
        this.junimoHut = junimoHut;
    }

    /// <inheritdoc />
    public override bool TryGetIcon([NotNullWhen(true)] out Texture2D? texture, out Rectangle sourceRect, out float scale)
    {
        texture = this.junimoHut.texture.Value;
        sourceRect = this.junimoHut.getSourceRectForMenu() ?? this.junimoHut.getSourceRect();
        scale = 4f * (32f / sourceRect.Height);
        return true;
    }
}
