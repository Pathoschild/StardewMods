using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.SpaceCore
{
    /// <summary>The API provided by the SpaceCore mod.</summary>
    public interface ISpaceCoreApi
    {
        /// <summary>Get a list of all currently loaded skills' IDs.</summary>
        string[] GetCustomSkills();

        /// <summary>Get the total XP a player has for a skill.</summary>
        /// <param name="farmer">The farmer whose skills to check.</param>
        /// <param name="skill">The skill ID.</param>
        int GetExperienceForCustomSkill(Farmer farmer, string skill);

        /// <summary>Get the display name for a skill.</summary>
        /// <param name="skill">The skill ID.</param>
        string GetDisplayNameOfCustomSkill(string skill);
    }
}
