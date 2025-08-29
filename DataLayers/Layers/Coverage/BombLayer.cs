using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.Extensions;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.DataLayers.Layers.Coverage;

/// <summary>A data layer which shows bomb radius when held.</summary>
internal class BombLayer : BaseLayer, IAutoItemLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend entry for inner tiles covered by a bomb explosion where dirt will be dug.</summary>
    private readonly LegendEntry Dig;

    /// <summary>The legend entry for outer tiles covered by a bomb explosion where objects will be destroyed.</summary>
    private readonly LegendEntry Explosion;

    /// <summary>The legend entry for tiles outside a bomb explosion where monsters and players still receive damage.</summary>
    private readonly LegendEntry Shockwave;

    /// <summary>The bomb radius by qualified item ID.</summary>
    /// <remarks>Derived from <see cref="TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(int,float,int,int,Vector2,bool,bool,GameLocation,Farmer)"/>.</remarks>
    private readonly Dictionary<string, int> BombRadius = new()
    {
        ["(O)286"] = 3,
        ["(O)287"] = 5,
        ["(O)288"] = 7
    };


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    public BombLayer(LayerConfig config, ColorScheme colors)
        : base(I18n.Bombs_Name(), config)
    {
        const string layerId = "BombCoverage";

        this.Legend = [
            this.Dig = new LegendEntry(I18n.Keys.Bombs_Dig, colors.Get(layerId, "Dig", Color.Red)),
            this.Explosion = new LegendEntry(I18n.Keys.Bombs_Explosion, colors.Get(layerId, "Explosion", Color.Magenta)),
            this.Shockwave = new LegendEntry(I18n.Keys.Bombs_Shockwave, colors.Get(layerId, "Shockwave", Color.Black))
        ];
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        HashSet<Vector2>? digTiles = null;
        HashSet<Vector2>? explosionTiles = null;
        HashSet<Vector2>? damageTiles = null;

        // get placed bombs
        foreach (var sprite in location.TemporarySprites)
        {
            if (sprite.bombRadius <= 0)
                continue;

            Vector2 tile = new Vector2(sprite.Position.X / Game1.tileSize, sprite.Position.Y / Game1.tileSize);
            if (this.TryGetCoverage(sprite.bombRadius, tile, visibleArea, out HashSet<Vector2>? curDigTiles, out HashSet<Vector2>? curExplosionTiles, out HashSet<Vector2>? curDamageTiles))
            {
                digTiles ??= [];
                explosionTiles ??= [];
                damageTiles ??= [];

                digTiles.AddRange(curDigTiles);
                explosionTiles.AddRange(curExplosionTiles);
                damageTiles.AddRange(curDamageTiles);
            }
        }

        // get held bomb
        SObject heldObj = Game1.player.ActiveObject;
        if (heldObj != null && this.BombRadius.TryGetValue(heldObj.QualifiedItemId, out int radius))
        {
            if (this.TryGetCoverage(radius, cursorTile, visibleArea, out HashSet<Vector2>? curDigTiles, out HashSet<Vector2>? curExplosionTiles, out HashSet<Vector2>? curDamageTiles))
            {
                digTiles ??= [];
                explosionTiles ??= [];
                damageTiles ??= [];

                digTiles.AddRange(curDigTiles);
                explosionTiles.AddRange(curExplosionTiles);
                damageTiles.AddRange(curDamageTiles);
            }
        }

        // build groups
        if (explosionTiles != null)
        {
            damageTiles!.ExceptWith(explosionTiles);
            explosionTiles.ExceptWith(digTiles!);

            return
            [
                new TileGroup(digTiles!.Select(p => new TileData(p, this.Dig)), outerBorderColor: this.Dig.Color),
                new TileGroup(explosionTiles.Select(p => new TileData(p, this.Explosion)), outerBorderColor: this.Explosion.Color),
                new TileGroup(damageTiles.Select(p => new TileData(p, this.Shockwave)), outerBorderColor: this.Shockwave.Color)
            ];
        }
        return [];
    }

    /// <inheritdoc />
    public bool AppliesTo(Item? item)
    {
        return
            item is not null
            && this.BombRadius.ContainsKey(item.QualifiedItemId);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the tiles covered by a bomb radius.</summary>
    /// <param name="radius">The bomb radius.</param>
    /// <param name="origin">The bomb's tile.</param>
    /// <param name="visibleArea">The tile area currently visible on the screen.</param>
    /// <param name="digTiles">The inner tiles covered by a bomb explosion where dirt will be dug.</param>
    /// <param name="explosionTiles">The outer tiles covered by a bomb explosion where objects will be destroyed.</param>
    /// <param name="shockwaveTiles">The tiles outside a bomb explosion where monsters and players still receive damage.</param>
    /// <remarks>Derived from <see cref="GameLocation.explode"/>.</remarks>
    private bool TryGetCoverage(int radius, Vector2 origin, Rectangle visibleArea, [NotNullWhen(true)] out HashSet<Vector2>? digTiles, [NotNullWhen(true)] out HashSet<Vector2>? explosionTiles, [NotNullWhen(true)] out HashSet<Vector2>? shockwaveTiles)
    {
        // get area of effect
        Rectangle areaOfEffect = new Rectangle((int)origin.X - radius, (int)origin.Y - radius, radius * 2 + 1, radius * 2 + 1);
        if (!areaOfEffect.Intersects(visibleArea))
        {
            digTiles = null;
            explosionTiles = null;
            shockwaveTiles = null;
            return false;
        }
        shockwaveTiles = new HashSet<Vector2>(areaOfEffect.GetTiles());

        // get main explosion radius
        explosionTiles = [];
        {
            bool[,] circleOutline = Game1.getCircleOutlineGrid(radius);
            int top = (int)origin.Y - radius;
            int x = (int)origin.X - radius;
            int y = top;

            int insideCircle = 0;
            for (int i = 0; i < radius * 2 + 1; i++)
            {
                for (int j = 0; j < radius * 2 + 1; j++)
                {
                    if (i == 0 || j == 0 || i == radius * 2 || j == radius * 2)
                    {
                        insideCircle = circleOutline[i, j] ? 1 : 0;
                    }
                    else if (circleOutline[i, j])
                    {
                        insideCircle += j <= radius ? 1 : -1;

                        if (insideCircle <= 0)
                            explosionTiles.Add(new Vector2(x, y));
                    }

                    if (insideCircle >= 1)
                        explosionTiles.Add(new Vector2(x, y));
                    y++;
                }

                x++;
                y = top;
            }
        }

        // get dig radius
        digTiles = [];
        {
            radius /= 2;
            bool[,] circleOutline = Game1.getCircleOutlineGrid(radius);
            int top = (int)origin.Y - radius;
            int x = (int)origin.X - radius;
            int y = top;

            int insideCircle = 0;
            for (int i = 0; i < radius * 2 + 1; i++)
            {
                for (int j = 0; j < radius * 2 + 1; j++)
                {
                    if (i == 0 || j == 0 || i == radius * 2 || j == radius * 2)
                    {
                        insideCircle = circleOutline[i, j] ? 1 : 0;
                    }
                    else if (circleOutline[i, j])
                    {
                        insideCircle += j <= radius ? 1 : -1;
                        digTiles.Add(new Vector2(x, y));
                    }

                    if (insideCircle >= 1)
                        digTiles.Add(new Vector2(x, y));

                    y++;
                }

                x++;
                y = top;
            }
        }

        return true;
    }
}
