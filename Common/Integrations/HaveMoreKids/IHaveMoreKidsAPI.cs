using StardewValley.Characters;

namespace Pathoschild.Stardew.Common.Integrations.HaveMoreKids;

public interface IHaveMoreKidsAPI
{
    /// <summary>Determine the number of days until next child growth</summary>
    /// <param name="kid"></param>
    /// <returns></returns>
    public int GetDaysToNextChildGrowth(Child kid);

    /// <summary>Get the child birthday string</summary>
    /// <param name="kid"></param>
    /// <returns></returns>
    public string GetChildBirthdayString(Child kid);
}
