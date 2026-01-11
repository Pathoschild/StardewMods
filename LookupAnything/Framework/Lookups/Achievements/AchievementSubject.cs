using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.DataMinedValues;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Shops;
using ShopData = StardewValley.GameData.Shops.ShopData;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Achievements;

/// <summary>Describes an achievement.</summary>
internal class AchievementSubject : BaseSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The achievement ID.</summary>
    private readonly int AchievementId;

    /// <summary>The raw achievement data fields.</summary>
    private readonly string[] DataFields;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="id">The achievement ID.</param>
    /// <param name="dataFields">The raw achievement data fields.</param>
    public AchievementSubject(GameHelper gameHelper, int id, string[] dataFields)
        : base(gameHelper)
    {
        this.AchievementId = id;
        this.DataFields = dataFields;
        this.Description = ArgUtility.Get(dataFields, 1);

        // initialize
        this.Initialize(
            name: ArgUtility.Get(dataFields, 0),
            description: ArgUtility.Get(dataFields, 1),
            type: I18n.Type_Achievement()
        );
    }

    /// <inheritdoc />
    public override IEnumerable<ICustomField> GetData()
    {
        // unlocked
        yield return new GenericField(I18n.Achievement_Unlocked(), I18n.Stringify(Game1.player.achievements.Contains(this.AchievementId)));

        // hat shop
        {
            Item[] hatsUnlocked = this.GetHatsUnlocked().ToArray();
            if (hatsUnlocked.Length > 0)
                yield return new ItemIconListField(this.GameHelper, I18n.Achievement_HatShop(), hatsUnlocked, introText: I18n.Achievement_HatShop_HatsAdded(), showStackSize: false, iconIndent: 20);
            else
                yield return new GenericField(I18n.Achievement_HatShop(), I18n.Achievement_HatShop_NoHatsAdded());
        }

        // internal ID
        yield return new GenericField(I18n.InternalId(), this.AchievementId.ToString());
    }

    /// <inheritdoc />
    public override IEnumerable<IDataMinedValue> GetDataMinedValues()
    {
        yield return new GenericDataMinedValue(null, "Data", I18n.Stringify(this.DataFields));
    }

    /// <inheritdoc />
    public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
    {
        spriteBatch.Draw(Game1.mouseCursors, position, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25), Color.White);
        return true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the hat items unlocked in the Hat Mouse shop when this achievement is unlocked.</summary>
    private IEnumerable<Item> GetHatsUnlocked()
    {
        if (!DataLoader.Shops(Game1.content).TryGetValue(Game1.shop_hatMouse, out ShopData? shopData) || shopData?.Items?.Count is not > 0)
            yield break;

        foreach (ShopItemData? shopItem in shopData.Items)
        {
            if (this.IsHatUnlockCondition(shopItem.Condition))
            {
                Item? hat = ItemRegistry.Create(shopItem.ItemId, allowNull: true);
                if (hat is { TypeDefinitionId: ItemRegistry.type_hat })
                    yield return hat;
            }
        }
    }

    /// <summary>Get whether the given shop item conditions represent a simple achievement requirement matching the <see cref="AchievementId"/>.</summary>
    /// <param name="conditions">The shop item conditions to check.</param>
    private bool IsHatUnlockCondition(string conditions)
    {
        if (GameStateQuery.IsImmutablyTrue(conditions))
            return false;

        GameStateQuery.ParsedGameStateQuery[] parsedConditions = GameStateQuery.Parse(conditions);

        bool foundUnlock = false;
        foreach (GameStateQuery.ParsedGameStateQuery query in parsedConditions)
        {
            if (query.Error != null || query.Negated || !query.Query[0].EqualsIgnoreCase(nameof(GameStateQuery.DefaultResolvers.PLAYER_HAS_ACHIEVEMENT)))
                return false;

            string player = ArgUtility.Get(query.Query, 1);
            if (!player.Equals("Current") && !player.EqualsIgnoreCase("Any"))
                return false;

            string achievementId = ArgUtility.Get(query.Query, 2);
            if (!achievementId.EqualsIgnoreCase(this.AchievementId.ToString()))
                return false;

            foundUnlock = true;
        }

        return foundUnlock;
    }
}
