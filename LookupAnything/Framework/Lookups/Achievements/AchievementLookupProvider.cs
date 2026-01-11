using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Achievements;

/// <summary>Provides lookup data for in-game achievements.</summary>
internal class AchievementLookupProvider : BaseLookupProvider
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="reflection">Simplifies access to private game code.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    public AchievementLookupProvider(IReflectionHelper reflection, GameHelper gameHelper)
        : base(reflection, gameHelper) { }

    /// <inheritdoc />
    public override ISubject? GetSubject(IClickableMenu menu, int cursorX, int cursorY)
    {
        IClickableMenu targetMenu = this.GameHelper.GetGameMenuPage(menu) ?? menu;
        switch (targetMenu)
        {
            /****
            ** GameMenu
            ****/
            // achievements tab
            // derived from CollectionsPage::performHoverAction
            case CollectionsPage collectionsTab:
                {
                    int currentTab = collectionsTab.currentTab;
                    if (currentTab is not CollectionsPage.achievementsTab)
                        break;

                    // get selected achievement index
                    int selectedIndex = 0;
                    {
                        int currentPage = collectionsTab.currentPage;
                        bool found = false;

                        for (int page = 0; page < currentPage; page++)
                            selectedIndex += collectionsTab.collections[currentTab][page].Count;

                        foreach (ClickableTextureComponent component in collectionsTab.collections[currentTab][currentPage])
                        {
                            if (component.containsPoint(cursorX, cursorY))
                            {
                                found = true;
                                break;
                            }

                            selectedIndex++;
                        }

                        if (!found)
                            break;
                    }

                    // get achievement
                    int achievementId = 0;
                    string? achievementData = null;
                    {
                        int curIndex = 0;
                        foreach ((int id, string? rawData) in Game1.achievements)
                        {
                            if (rawData is null)
                                continue;

                            if (curIndex == selectedIndex)
                            {
                                achievementId = id;
                                achievementData = rawData;
                                break;
                            }

                            curIndex++;
                        }

                        if (achievementData is null)
                            break;
                    }

                    // yield subject
                    string[] fields = achievementData.Split('^');
                    return this.BuildSubject(achievementId, fields);
                }
        }

        return null;
    }

    /// <inheritdoc />
    public override IEnumerable<ISubject> GetSearchSubjects()
    {
        foreach ((int id, string? rawData) in Game1.achievements)
        {
            if (rawData is null)
                continue;

            string[] fields = rawData.Split('^');
            yield return this.BuildSubject(id, fields);
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Build a subject.</summary>
    /// <param name="achievementId">The achievement ID.</param>
    /// <param name="fields">The raw achievement data fields.</param>
    private ISubject BuildSubject(int achievementId, string[] fields)
    {
        return new AchievementSubject(this.GameHelper, achievementId, fields);
    }
}
