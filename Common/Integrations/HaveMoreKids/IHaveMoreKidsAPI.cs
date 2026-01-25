using StardewValley.Characters;

namespace Pathoschild.Stardew.Common.Integrations.HaveMoreKids;

public interface IHaveMoreKidsAPI
{
    /// <summary>Get the number of days until the next child growth.</summary>
    /// <param name="kid">The child to check.</param>
    public int GetDaysToNextChildGrowth(Child kid);

    /// <summary>Get the child birthday string.</summary>
    /// <param name="kid">The child to check.</param>
    public string GetChildBirthdayString(Child kid);
}
