using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.WildTrees;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;

/// <summary>Describes a non-fruit tree.</summary>
internal class TreeSubject : BaseSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying target.</summary>
    private readonly Tree Target;

    /// <summary>The tree's tile position.</summary>
    private readonly Vector2 Tile;

    /// <summary>Provides subject entries.</summary>
    private readonly ISubjectRegistry Codex;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="codex">Provides subject entries.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="tree">The lookup target.</param>
    /// <param name="tile">The tree's tile position.</param>
    public TreeSubject(ISubjectRegistry codex, GameHelper gameHelper, Tree tree, Vector2 tile)
        : base(gameHelper, TreeSubject.GetName(tree), null, I18n.Type_Tree())
    {
        this.Codex = codex;
        this.Target = tree;
        this.Tile = tile;
    }

    /// <inheritdoc />
    /// <remarks>Tree growth algorithm reverse engineered from <see cref="Tree.dayUpdate"/>.</remarks>
    public override IEnumerable<ICustomField> GetData()
    {
        Tree tree = this.Target;
        WildTreeData data = tree.GetData();
        GameLocation location = tree.Location;
        bool isFertilized = tree.fertilized.Value;

        // added by mod
        {
            IModInfo? fromMod = this.GameHelper.TryGetModFromStringId(tree.treeType.Value);
            if (fromMod != null)
                yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(modName: fromMod.Manifest.Name));
        }

        // get growth stage
        WildTreeGrowthStage stage = (WildTreeGrowthStage)Math.Min(tree.growthStage.Value, (int)WildTreeGrowthStage.Tree);
        bool isFullyGrown = stage == WildTreeGrowthStage.Tree;
        yield return new GenericField(I18n.Tree_Stage(), isFullyGrown
            ? I18n.Tree_Stage_Done()
            : I18n.Tree_Stage_Partial(stageName: I18n.For(stage), step: (int)stage, max: (int)WildTreeGrowthStage.Tree)
        );

        // get growth schedule
        if (!isFullyGrown)
        {
            string label = I18n.Tree_NextGrowth();
            if (!data.GrowsInWinter && location.GetSeason() == Season.Winter && !location.SeedsIgnoreSeasonsHere() && !isFertilized)
                yield return new GenericField(label, I18n.Tree_NextGrowth_Winter());
            else if (stage == WildTreeGrowthStage.Tree - 1 && this.HasAdjacentTrees(this.Tile))
                yield return new GenericField(label, I18n.Tree_NextGrowth_AdjacentTrees());
            else
            {
                double chance = Math.Round((isFertilized ? data.FertilizedGrowthChance : data.GrowthChance) * 100, 2);
                yield return new GenericField(label, I18n.Tree_NextGrowth_Chance(stage: I18n.For(stage + 1), chance: chance));
            }
        }

        // get fertilizer
        if (!isFullyGrown)
        {
            if (!isFertilized)
                yield return new GenericField(I18n.Tree_IsFertilized(), this.Stringify(false));
            else
            {
                Item fertilizer = ItemRegistry.Create("(O)805");
                yield return new ItemIconField(this.GameHelper, I18n.Tree_IsFertilized(), fertilizer, this.Codex);
            }
        }

        // get seed
        if (isFullyGrown && !string.IsNullOrWhiteSpace(data.SeedItemId))
        {
            string seedName = GameI18n.GetObjectName(data.SeedItemId);

            if (tree.hasSeed.Value)
                yield return new ItemIconField(this.GameHelper, I18n.Tree_Seed(), ItemRegistry.Create(data.SeedItemId), this.Codex);
            else
            {
                List<string> lines = new(2);

                if (data.SeedOnShakeChance > 0)
                    lines.Add(I18n.Tree_Seed_ProbabilityDaily(chance: data.SeedOnShakeChance * 100, itemName: seedName));
                if (data.SeedOnChopChance > 0)
                    lines.Add(I18n.Tree_Seed_ProbabilityOnChop(chance: data.SeedOnChopChance * 100, itemName: seedName));

                if (lines.Any())
                    yield return new GenericField(I18n.Tree_Seed(), I18n.Tree_Seed_NotReady() + Environment.NewLine + string.Join(Environment.NewLine, lines));
            }
        }

        // internal type
        yield return new GenericField(I18n.InternalId(), tree.treeType.Value);
    }

    /// <inheritdoc />
    public override IEnumerable<IDebugField> GetDebugFields()
    {
        Tree target = this.Target;

        // pinned fields
        yield return new GenericDebugField("has seed", this.Stringify(target.hasSeed.Value), pinned: true);
        yield return new GenericDebugField("growth stage", target.growthStage.Value, pinned: true);
        yield return new GenericDebugField("health", target.health.Value, pinned: true);

        // raw fields
        foreach (IDebugField field in this.GetDebugFieldsFrom(target))
            yield return field;
    }

    /// <inheritdoc />
    public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
    {
        this.Target.drawInMenu(spriteBatch, position, Vector2.Zero, 1, 1);
        return true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get a display name for the tree.</summary>
    /// <param name="tree">The tree object.</param>
    private static string GetName(Tree tree)
    {
        string type = tree.treeType.Value;

        return type switch
        {
            Tree.mushroomTree => I18n.Tree_Name_BigMushroom(),
            Tree.mahoganyTree => I18n.Tree_Name_Mahogany(),
            Tree.leafyTree => I18n.Tree_Name_Maple(),
            Tree.bushyTree => I18n.Tree_Name_Oak(),
            Tree.palmTree => I18n.Tree_Name_Palm(),
            Tree.palmTree2 => I18n.Tree_Name_Palm(),
            Tree.pineTree => I18n.Tree_Name_Pine(),
            Tree.greenRainTreeBushy => I18n.Tree_Name_Mossy(),
            Tree.greenRainTreeLeafy => I18n.Tree_Name_Mossy(),
            Tree.greenRainTreeFern => I18n.Tree_Name_Mossy(),
            Tree.mysticTree => I18n.Tree_Name_Mystic(),
            _ => I18n.Tree_Name_Unknown()
        };
    }

    /// <summary>Whether there are adjacent trees that prevent growth.</summary>
    /// <param name="position">The tree's position in the current location.</param>
    private bool HasAdjacentTrees(Vector2 position)
    {
        GameLocation location = Game1.currentLocation;
        return (
            from adjacentTile in Utility.getSurroundingTileLocationsArray(position)
            let otherTree = location.terrainFeatures.ContainsKey(adjacentTile)
                ? location.terrainFeatures[adjacentTile] as Tree
                : null
            select otherTree != null && otherTree.growthStage.Value >= (int)(WildTreeGrowthStage.Tree - 1)
        ).Any(p => p);
    }
}
