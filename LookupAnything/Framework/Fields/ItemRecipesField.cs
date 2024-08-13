using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a list of recipes.</summary>
    internal class ItemRecipesField : GenericField
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to list by type.</summary>
        private readonly RecipeByTypeGroup[] RecipesByType;

        /// <summary>Provides utility methods for interacting with the game code.</summary>
        private readonly GameHelper GameHelper;

        /// <summary>Whether to show recipes the player hasn't learned in-game yet.</summary>
        private readonly bool ShowUnknownRecipes;

        /// <summary>Whether to show recipes involving error items.</summary>
        private readonly bool ShowInvalidRecipes;

        /// <summary>Whether to show the recipe group labels even if there's only one group.</summary>
        private readonly bool ShowLabelForSingleGroup;

        /// <summary>Whether to show the output item for recipes.</summary>
        private readonly bool ShowOutputLabels;

        /// <summary>The number of pixels between an item's icon and text.</summary>
        private readonly int IconMargin = 5;

        /// <summary>The height of a recipe line.</summary>
        private readonly float LineHeight = Game1.smallFont.MeasureString("ABC").Y;

        /// <summary>The width and height of an item icon.</summary>
        private float IconSize => this.LineHeight;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="ingredient">The ingredient item.</param>
        /// <param name="recipes">The recipes to list.</param>
        /// <param name="showUnknownRecipes">Whether to show recipes the player hasn't learned in-game yet.</param>
        /// <param name="showInvalidRecipes">Whether to show recipes involving error items.</param>
        /// <param name="showLabelForSingleGroup">Whether to show the recipe group labels even if there's only one group.</param>
        /// <param name="showOutputLabels">Whether to show the output item for recipes.</param>
        public ItemRecipesField(GameHelper gameHelper, string label, Item? ingredient, RecipeModel[] recipes, bool showUnknownRecipes, bool showInvalidRecipes, bool showLabelForSingleGroup = true, bool showOutputLabels = true)
            : base(label, true)
        {
            this.GameHelper = gameHelper;
            this.RecipesByType = this.BuildRecipeGroups(ingredient, recipes).ToArray();
            this.ShowUnknownRecipes = showUnknownRecipes;
            this.ShowInvalidRecipes = showInvalidRecipes;
            this.ShowLabelForSingleGroup = showLabelForSingleGroup;
            this.ShowOutputLabels = showOutputLabels;
        }

        /// <summary> Get number of recipes that will be drawn</summary>
        /// <returns>Number of shown recipes</returns>
        public int ShownRecipesCount()
        {
            return this.RecipesByType.Sum(
                (group) => group.Recipes.Count(
                    (entry) => (this.ShowInvalidRecipes || entry.IsValid) && (this.ShowUnknownRecipes || entry.IsKnown)));
        }

        /// <inheritdoc />
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            // get margins
            const int groupVerticalMargin = 6;
            const int groupLeftMargin = 0;
            const int firstRecipeTopMargin = 5;
            const int firstRecipeLeftMargin = 14;
            const int otherRecipeTopMargin = 2;
            float inputDividerWidth = font.MeasureString("+").X;
            float itemSpacer = inputDividerWidth;

            // current drawing position
            Vector2 curPos = position;
            float absoluteWrapWidth = position.X + wrapWidth;

            // icon size and line height
            float lineHeight = this.LineHeight;
            var iconSize = new Vector2(this.IconSize);
            float joinerWidth = inputDividerWidth + (itemSpacer * 2);

            // draw recipes
            curPos.Y += groupVerticalMargin;
            foreach (RecipeByTypeGroup group in this.RecipesByType)
            {
                // check if we can align columns
                bool alignColumns = wrapWidth >= (group.TotalColumnWidth + itemSpacer + ((group.ColumnWidths.Length - 1) * joinerWidth)); // columns + space between output/input + space between each input

                // draw group label
                if (this.ShowLabelForSingleGroup || this.RecipesByType.Length > 1)
                {
                    curPos.X = position.X + groupLeftMargin;
                    curPos += this.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, $"{group.Type}:", Color.Black);
                }

                int hiddenUnknownRecipesCount = 0;

                // draw recipe lines
                foreach (RecipeEntry entry in group.Recipes)
                {
                    if (!this.ShowInvalidRecipes && !entry.IsValid)
                    {
                        continue;
                    }
                    if (!this.ShowUnknownRecipes && !entry.IsKnown)
                    {
                        hiddenUnknownRecipesCount++;
                        continue;
                    }

                    // fade recipes which aren't known
                    Color iconColor = entry.IsKnown ? Color.White : Color.White * .5f;
                    Color textColor = entry.IsKnown ? Color.Black : Color.Gray;

                    // reset position for recipe output
                    float recipeLeftMargin = position.X + firstRecipeLeftMargin;
                    curPos = new Vector2(
                        recipeLeftMargin,
                        curPos.Y + firstRecipeTopMargin
                    );

                    // draw output item (icon + name + count + chance)
                    float inputLeft = 0;
                    if (this.ShowOutputLabels)
                    {
                        Vector2 outputSize = this.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, entry.Output.DisplayText, textColor, entry.Output.Sprite, iconSize, iconColor, qualityIcon: entry.Output.Quality);
                        float outputWidth = alignColumns
                            ? group.ColumnWidths[0]
                            : outputSize.X;

                        inputLeft = curPos.X + outputWidth + itemSpacer;
                        curPos.X = inputLeft;
                    }

                    // draw input items
                    for (int i = 0, last = entry.Inputs.Length - 1; i <= last; i++)
                    {
                        RecipeItemEntry input = entry.Inputs[i];

                        // get icon size
                        Vector2 curIconSize = iconSize;
                        if (input is { IsGoldPrice: true, Sprite: not null })
                            curIconSize = Utility.PointToVector2(input.Sprite.SourceRectangle.Size) * Game1.pixelZoom; // gold icon doesn't resize well, draw it at the intended size

                        // move the draw position down to a new line if the next item would be drawn off the right edge
                        Vector2 inputSize = this.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, input.DisplayText, textColor, input.Sprite, curIconSize, iconColor, input.Quality, probe: true);
                        if (alignColumns)
                            inputSize.X = group.ColumnWidths[i + 1];

                        if (curPos.X + inputSize.X > absoluteWrapWidth)
                        {
                            curPos = new Vector2(
                                x: inputLeft,
                                y: curPos.Y + lineHeight + otherRecipeTopMargin
                            );
                        }

                        // draw input item (icon + name + count)
                        this.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, input.DisplayText, textColor, input.Sprite, curIconSize, iconColor, input.Quality);
                        curPos = new Vector2(
                            x: curPos.X + inputSize.X,
                            y: curPos.Y
                        );

                        // draw input item joiner
                        if (i != last)
                        {
                            // move draw position to next line if needed
                            if (curPos.X + joinerWidth > absoluteWrapWidth)
                            {
                                curPos = new Vector2(
                                    x: inputLeft,
                                    y: curPos.Y + lineHeight + otherRecipeTopMargin
                                );
                            }
                            else
                                curPos.X += itemSpacer;

                            // draw the input item joiner
                            var joinerSize = this.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, "+", textColor);
                            curPos.X += joinerSize.X + itemSpacer;
                        }
                    }
                    curPos.X = recipeLeftMargin;
                    curPos.Y += lineHeight;

                    // draw condition
                    if (entry.Conditions != null)
                        curPos.Y += this.DrawIconText(spriteBatch, font, curPos with { X = curPos.X + this.IconSize + this.IconMargin }, absoluteWrapWidth, I18n.Item_RecipesForMachine_Conditions(conditions: entry.Conditions), textColor).Y;
                }

                // draw number of unknown recipes
                if (hiddenUnknownRecipesCount > 0)
                {
                    // reset position for unknown recipe count (aligned horizontally with other recipes)
                    curPos = new Vector2(
                        position.X + firstRecipeLeftMargin + this.IconMargin + this.IconSize,
                        curPos.Y + firstRecipeTopMargin
                    );

                    this.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, I18n.Item_UnknownRecipes(hiddenUnknownRecipesCount), Color.Gray);
                    curPos.Y += lineHeight;
                }

                curPos.Y += lineHeight; // blank line between groups
            }

            // vertical spacer at the bottom of the recipes
            curPos.Y += groupVerticalMargin;

            // get drawn dimensions
            return new Vector2(wrapWidth, curPos.Y - position.Y - lineHeight);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Build an optimized representation of the recipes to display.</summary>
        /// <param name="ingredient">The ingredient item.</param>
        /// <param name="rawRecipes">The raw recipes to list.</param>
        private IEnumerable<RecipeByTypeGroup> BuildRecipeGroups(Item? ingredient, RecipeModel[] rawRecipes)
        {
            /****
            ** build models for matching recipes
            ****/
            Dictionary<string, RecipeEntry[]> rawGroups = rawRecipes
                // split into specific recipes that match the item
                // (e.g. a recipe with several possible inputs => several recipes with one possible input)
                .Select(recipe =>
                {
                    // get output item
                    Item? outputItem = ingredient is not null && recipe.IsForMachine(ingredient)
                        ? recipe.TryCreateItem(null)
                        : recipe.TryCreateItem(ingredient);

                    // handle error recipe
                    if (recipe.OutputQualifiedItemId == DataParser.ComplexRecipeId)
                    {
                        return new RecipeEntry(
                            name: recipe.Key,
                            type: recipe.DisplayType,
                            isKnown: recipe.IsKnown(),
                            inputs: [],
                            output: this.CreateItemEntry(
                                name: I18n.Item_RecipesForMachine_TooComplex(),
                                item: outputItem,
                                sprite: recipe.SpecialOutput?.Sprite,
                                hasInputAndOutput: false,
                                isError: false
                            ),
                            conditions: recipe.Conditions.Length > 0
                                ? I18n.List(recipe.Conditions.Select(HumanReadableConditionParser.Format))
                                : null
                        );
                    }

                    // get output model
                    RecipeItemEntry output;
                    if (ItemRegistry.GetDataOrErrorItem(recipe.OutputQualifiedItemId)?.ItemId == "DROP_IN")
                    {
                        output = this.CreateItemEntry(
                            name: I18n.Item_RecipesForMachine_SameAsInput(),
                            item: null,
                            sprite: null,
                            minCount: recipe.MinOutput,
                            maxCount: recipe.MaxOutput,
                            chance: recipe.OutputChance,
                            quality: recipe.Quality,
                            hasInputAndOutput: true,
                            isError: false
                        );
                    }
                    else
                    {
                        output = this.CreateItemEntry(
                            name: recipe.SpecialOutput?.DisplayText ?? outputItem?.DisplayName ?? string.Empty,
                            item: outputItem,
                            sprite: recipe.SpecialOutput?.Sprite,
                            minCount: recipe.MinOutput,
                            maxCount: recipe.MaxOutput,
                            chance: recipe.OutputChance,
                            quality: recipe.Quality,
                            hasInputAndOutput: true,
                            isError: recipe.SpecialOutput?.IsError
                        );
                    }

                    // get ingredient models
                    IEnumerable<RecipeItemEntry> inputs = recipe.Ingredients
                        .Select(this.TryCreateItemEntry)
                        .WhereNotNull();
                    if (recipe.Type != RecipeType.TailorInput) // tailoring is always two ingredients with cloth first
                        inputs = inputs.OrderBy(entry => entry.DisplayText);

                    if (recipe.GoldPrice > 0)
                    {
                        inputs = inputs.Concat(new[]
                        {
                            new RecipeItemEntry(
                                new SpriteInfo(Game1.debrisSpriteSheet, new Rectangle(5, 69, 6, 6)),
                                Utility.getNumberWithCommas(recipe.GoldPrice),
                                null,
                                IsGoldPrice: true
                            )
                        });
                    }

                    // build recipe
                    return new RecipeEntry(
                        name: recipe.Key,
                        type: recipe.DisplayType,
                        isKnown: recipe.IsKnown(),
                        inputs: inputs.ToArray(),
                        output: output,
                        conditions: recipe.Conditions.Length > 0
                            ? I18n.List(recipe.Conditions.Select(HumanReadableConditionParser.Format))
                            : null
                    );
                })

                // filter to unique recipe
                // (e.g. two recipe matches => one recipe)
                .GroupBy(recipe => recipe.UniqueKey)
                .Select(item => item.First())

                // sort
                .OrderBy(recipe => recipe.Type)
                .ThenBy(recipe => recipe.Output.DisplayText)

                // group by type
                .GroupBy(p => p.Type)
                .ToDictionary(p => p.Key, p => p.ToArray());

            /****
            ** build recipe groups with column widths
            ****/
            foreach ((string type, RecipeEntry[] recipes) in rawGroups)
            {
                // build column width list
                var columnWidths = new List<float>();
                void TrackWidth(int index, string text, SpriteInfo? icon)
                {
                    while (columnWidths.Count < index + 1)
                        columnWidths.Add(0);

                    float width = Game1.smallFont.MeasureString(text).X;
                    if (icon != null)
                        width += this.IconSize + this.IconMargin;

                    columnWidths[index] = Math.Max(columnWidths[index], width);
                }

                // get max width of each column in the group
                foreach (RecipeEntry recipe in recipes)
                {
                    TrackWidth(0, $"{recipe.Output.DisplayText}:", recipe.Output.Sprite);

                    for (int i = 0; i < recipe.Inputs.Length; i++)
                        TrackWidth(i + 1, recipe.Inputs[i].DisplayText, recipe.Inputs[i].Sprite);
                }

                // save widths
                yield return new RecipeByTypeGroup(
                    Type: type,
                    Recipes: recipes,
                    ColumnWidths: columnWidths.ToArray()
                );
            }
        }

        /// <summary>Draw text with an icon.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="font">The sprite font.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="absoluteWrapWidth">The width at which to wrap the text.</param>
        /// <param name="text">The block of text to write.</param>
        /// <param name="textColor">The text color.</param>
        /// <param name="icon">The sprite to draw.</param>
        /// <param name="iconSize">The size to draw.</param>
        /// <param name="iconColor">The color to tint the sprite.</param>
        /// <param name="qualityIcon">The quality for which to draw an icon over the sprite.</param>
        /// <param name="probe">Whether to calculate the positions without actually drawing anything to the screen.</param>
        /// <returns>Returns the drawn size.</returns>
        private Vector2 DrawIconText(SpriteBatch batch, SpriteFont font, Vector2 position, float absoluteWrapWidth, string text, Color textColor, SpriteInfo? icon = null, Vector2? iconSize = null, Color? iconColor = null, int? qualityIcon = null, bool probe = false)
        {
            // draw icon
            int textOffset = 0;
            if (icon != null && iconSize.HasValue)
            {
                if (!probe)
                    batch.DrawSpriteWithin(icon, position.X, position.Y, iconSize.Value, iconColor);
                textOffset = this.IconMargin;
            }
            else
                iconSize = Vector2.Zero;

            // draw quality icon overlay
            if (qualityIcon > 0 && iconSize is { X: > 0, Y: > 0 })
            {
                Rectangle qualityRect = qualityIcon < SObject.bestQuality ? new(338 + (qualityIcon.Value - 1) * 8, 400, 8, 8) : new(346, 392, 8, 8); // from Item.DrawMenuIcons
                Texture2D qualitySprite = Game1.mouseCursors;

                Vector2 qualitySize = iconSize.Value / 2;
                Vector2 qualityPos = new Vector2(
                    position.X + iconSize.Value.X - qualitySize.X,
                    position.Y + iconSize.Value.Y - qualitySize.Y
                );

                batch.DrawSpriteWithin(qualitySprite, qualityRect, qualityPos.X, qualityPos.Y, qualitySize, iconColor);
            }


            // draw text
            Vector2 textSize = probe
                ? font.MeasureString(text)
                : batch.DrawTextBlock(font, text, position + new Vector2(iconSize.Value.X + textOffset, 0), absoluteWrapWidth - position.X, textColor);

            // get drawn size
            return new Vector2(
                x: iconSize.Value.X + textSize.X,
                y: Math.Max(iconSize.Value.Y, textSize.Y)
            );
        }

        /// <summary>Create a recipe item model.</summary>
        /// <param name="ingredient">The recipe ingredient model for the item.</param>
        /// <returns>The equivalent item entry model, or <c>null</c> for a category with no matching items.</returns>
        private RecipeItemEntry TryCreateItemEntry(RecipeIngredientModel ingredient)
        {
            // special cases
            switch (ingredient.InputId)
            {
                case "-777":
                    return this.CreateItemEntry(
                        name: I18n.Item_WildSeeds(),
                        minCount: ingredient.Count,
                        maxCount: ingredient.Count,
                        isError: false
                    );
            }

            // from category
            if (int.TryParse(ingredient.InputId, out int category) && category < 0)
            {
                Item? input = this.GameHelper.GetObjectsByCategory(category).FirstOrDefault();
                if (input != null)
                {
                    string displayName;
                    switch (input.Category)
                    {
                        case SObject.EggCategory:
                            displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572"); // Egg (Any)
                            break;

                        case SObject.MilkCategory:
                            displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573"); // Milk (Any)
                            break;

                        default:
                            displayName = input.getCategoryName();
                            break;
                    }

                    return this.CreateItemEntry(
                        name: displayName,
                        minCount: ingredient.Count,
                        maxCount: ingredient.Count,
                        isError: false
                    );
                }
            }

            // from item
            if (ingredient.InputId != null)
            {
                Item input = ItemRegistry.Create(ingredient.InputId, allowNull: true);

                if (input is SObject obj)
                {
                    if (ingredient.PreservedItemId != null)
                        obj.preservedParentSheetIndex.Value = ingredient.PreservedItemId;
                    if (ingredient.PreserveType != null)
                        obj.preserve.Value = ingredient.PreserveType.Value;
                }

                if (input is not null)
                {
                    string name = input.DisplayName ?? input.ItemId;
                    if (ingredient.InputContextTags.Length > 0) // if the item has both item ID and context tags, show tags to disambiguate
                        name += $" ({I18n.List(ingredient.InputContextTags.Select(HumanReadableContextTagParser.Format))})";

                    return this.CreateItemEntry(
                        name: name,
                        item: input,
                        minCount: ingredient.Count,
                        maxCount: ingredient.Count
                    );
                }
            }

            // from context tags
            if (ingredient.InputContextTags.Length > 0)
            {
                return this.CreateItemEntry(
                    name: I18n.List(ingredient.InputContextTags.Select(HumanReadableContextTagParser.Format)),
                    minCount: ingredient.Count,
                    maxCount: ingredient.Count,
                    isError: false
                );
            }

            // unsupported type, show placeholder with error sprite
            {
                ObjectDataDefinition objectTypeDef = ItemRegistry.GetObjectTypeDefinition();

                string? displayName = ingredient.InputId;
                if (ingredient.InputContextTags.Length > 0)
                {
                    displayName = !string.IsNullOrWhiteSpace(displayName)
                        ? I18n.List([displayName, .. ingredient.InputContextTags])
                        : I18n.List(ingredient.InputContextTags);
                }
                displayName ??= "???";

                return this.CreateItemEntry(
                    name: displayName,
                    sprite: new SpriteInfo(
                        objectTypeDef.GetErrorTexture(),
                        objectTypeDef.GetErrorSourceRect()
                    ),
                    isError: true
                );
            }
        }

        /// <summary>Create a recipe item model.</summary>
        /// <param name="name">The display name for the item.</param>
        /// <param name="item">The instance of the item.</param>
        /// <param name="sprite">The item sprite, or <c>null</c> to generate it automatically.</param>
        /// <param name="minCount">The minimum number of items needed or created.</param>
        /// <param name="maxCount">The maximum number of items needed or created.</param>
        /// <param name="chance">The chance of creating an output item.</param>
        /// <param name="quality">The item quality that will be produced, if applicable.</param>
        /// <param name="hasInputAndOutput">Whether the item has both input and output ingredients.</param>
        /// <param name="isError">Mark item entry as error or not error explicitly, otherwise derive based on item.</param>
        private RecipeItemEntry CreateItemEntry(string name, Item? item = null, SpriteInfo? sprite = null, int minCount = 1, int maxCount = 1, decimal chance = 100, int? quality = null, bool hasInputAndOutput = false, bool? isError = null)
        {
            // get display text
            string text;
            {
                // name + count
                if (minCount != maxCount)
                    text = I18n.Item_RecipesForMachine_MultipleItems(name: name, count: I18n.Generic_Range(min: minCount, max: maxCount));
                else if (minCount > 1)
                    text = I18n.Item_RecipesForMachine_MultipleItems(name: name, count: minCount);
                else
                    text = name;

                // chance
                if (chance is > 0 and < 100)
                    text += $" ({I18n.Generic_Percent(chance)})";

                // output suffix
                if (hasInputAndOutput)
                    text += ":";
            }

            return new RecipeItemEntry(
                Sprite: sprite ?? this.GameHelper.GetSprite(item),
                DisplayText: text,
                Quality: quality,
                IsGoldPrice: false,
                IsError: isError ?? (item != null && ItemRegistry.GetData(item?.QualifiedItemId) == null)
            );
        }
    }
}
