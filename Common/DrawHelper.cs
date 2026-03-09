using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.Common;

/// <summary>Provides utility methods for drawing to the screen.</summary>
internal static class DrawHelper
{
    /*********
    ** Public methods
    *********/
    /****
    ** Fonts
    ****/
    /// <summary>Get the dimensions of a space character.</summary>
    /// <param name="font">The font to measure.</param>
    public static float GetSpaceWidth(SpriteFont font)
    {
        return CommonHelper.GetSpaceWidth(font);
    }

    /****
    ** Drawing
    ****/
    /// <summary>Draw a sprite to the screen.</summary>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    /// <param name="sheet">The sprite sheet containing the sprite.</param>
    /// <param name="sprite">The sprite coordinates and dimensions in the sprite sheet.</param>
    /// <param name="x">The X-position at which to draw the sprite.</param>
    /// <param name="y">The X-position at which to draw the sprite.</param>
    /// <param name="errorSize">The size of the error icon to draw if the sprite is invalid.</param>
    /// <param name="color">The color to tint the sprite.</param>
    /// <param name="scale">The scale to draw.</param>
    public static void DrawSprite(this SpriteBatch spriteBatch, Texture2D sheet, Rectangle sprite, float x, float y, Vector2 errorSize, Color? color = null, float scale = 1)
    {
        try
        {
            spriteBatch.Draw(sheet, new Vector2(x, y), sprite, color ?? Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
        catch
        {
            Utility.DrawErrorTexture(spriteBatch, new Rectangle((int)x, (int)y, (int)errorSize.X, (int)errorSize.Y), 0);
        }
    }

    /// <inheritdoc cref="DrawSprite(SpriteBatch, Texture2D, Rectangle, float, float, Vector2, Color?, float)" />
    public static void DrawSprite(this SpriteBatch spriteBatch, Texture2D sheet, Rectangle sprite, float x, float y, Point errorSize, Color? color = null, float scale = 1)
    {
        try
        {
            spriteBatch.Draw(sheet, new Vector2(x, y), sprite, color ?? Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
        catch
        {
            Utility.DrawErrorTexture(spriteBatch, new Rectangle((int)x, (int)y, errorSize.X, errorSize.Y), 0);
        }
    }

    /// <summary>Draw a sprite to the screen scaled and centered to fit the given dimensions.</summary>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    /// <param name="sprite">The sprite to draw.</param>
    /// <param name="x">The X-position at which to draw the sprite.</param>
    /// <param name="y">The X-position at which to draw the sprite.</param>
    /// <param name="size">The size to draw.</param>
    /// <param name="color">The color to tint the sprite.</param>
    public static void DrawSpriteWithin(this SpriteBatch spriteBatch, SpriteInfo? sprite, float x, float y, Vector2 size, Color? color = null)
    {
        try
        {
            sprite?.Draw(spriteBatch, (int)x, (int)y, size, color);
        }
        catch
        {
            Utility.DrawErrorTexture(spriteBatch, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), 0);
        }
    }

    /// <summary>Draw a sprite to the screen scaled and centered to fit the given dimensions.</summary>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    /// <param name="sheet">The sprite sheet containing the sprite.</param>
    /// <param name="sprite">The sprite coordinates and dimensions in the sprite sheet.</param>
    /// <param name="x">The X-position at which to draw the sprite.</param>
    /// <param name="y">The X-position at which to draw the sprite.</param>
    /// <param name="size">The size to draw.</param>
    /// <param name="color">The color to tint the sprite.</param>
    public static void DrawSpriteWithin(this SpriteBatch spriteBatch, Texture2D sheet, Rectangle sprite, float x, float y, Vector2 size, Color? color = null)
    {
        // calculate dimensions
        float largestDimension = Math.Max(sprite.Width, sprite.Height);
        float scale = size.X / largestDimension;
        float leftOffset = Math.Max((size.X - (sprite.Width * scale)) / 2, 0);
        float topOffset = Math.Max((size.Y - (sprite.Height * scale)) / 2, 0);

        // draw
        spriteBatch.DrawSprite(sheet, sprite, x + leftOffset, y + topOffset, size, color ?? Color.White, scale);
    }

    /// <summary>Draw a sprite to the screen.</summary>
    /// <param name="batch">The sprite batch.</param>
    /// <param name="x">The X-position at which to start the line.</param>
    /// <param name="y">The X-position at which to start the line.</param>
    /// <param name="size">The line dimensions.</param>
    /// <param name="color">The color to tint the sprite.</param>
    public static void DrawLine(this SpriteBatch batch, float x, float y, Vector2 size, Color? color = null)
    {
        batch.Draw(CommonHelper.Pixel, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), color ?? Color.White);
    }

    /// <summary>Capture arbitrary drawn content into a texture.</summary>
    /// <param name="draw">Draw the content.</param>
    /// <param name="pixelWidth">The pixel width of the texture to capture.</param>
    /// <param name="pixelHeight">The pixel height of the texture to capture.</param>
    /// <param name="useSpriteBatch">If set, draw using this sprite batch instead of creating a new one. This sprite batch must be in a non-begun state.</param>
    public static Texture2D RenderToTexture(Action<SpriteBatch> draw, int pixelWidth, int pixelHeight, SpriteBatch? useSpriteBatch = null)
    {
        // back up render target
        RenderTarget2D? wasRenderTarget;
        {
            RenderTargetBinding[] wasRenderTargets = Game1.graphics.GraphicsDevice.GetRenderTargets();
            wasRenderTarget = wasRenderTargets.Length > 0
                ? wasRenderTargets[0].RenderTarget as RenderTarget2D
                : null;
        }

        // capture output
        SpriteBatch? disposeSpriteBatch = null;
        RenderTarget2D renderTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, pixelWidth, pixelHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        Game1.SetRenderTarget(renderTarget);
        try
        {
            SpriteBatch renderBatch;
            if (useSpriteBatch != null)
                renderBatch = useSpriteBatch;
            else
                renderBatch = disposeSpriteBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);

            renderBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            draw(renderBatch);

            renderBatch.End();

            return renderTarget;
        }

        // restore original state
        finally
        {
            Game1.SetRenderTarget(wasRenderTarget);
            disposeSpriteBatch?.Dispose();
        }
    }
}
