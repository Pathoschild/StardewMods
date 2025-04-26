using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.ConfigModels;

/// <summary>
/// Config preview image model
/// </summary>
/// <param name="Target"></param>
/// <param name="SourceRect"></param>
public record ConfigPreviewImage(string Target, Rectangle? SourceRect, int Scale = Game1.pixelZoom)
{
    private IAssetName? AssetName = null;

    public Texture2D GetPreviewTexture(IGameContentHelper gameContentHelper)
    {
        this.AssetName ??= gameContentHelper.ParseAssetName(this.Target);
        if (gameContentHelper.DoesAssetExist<Texture2D>(this.AssetName))
            return gameContentHelper.Load<Texture2D>(this.AssetName);
        return Game1.staminaRect;
    }
}
