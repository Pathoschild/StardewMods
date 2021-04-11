using System.Collections.Generic;

namespace ContentPatcher
{
    /// <summary>A set of parsed conditions linked to the Content Patcher context. These conditions are <strong>per-screen</strong>, so the result depends on the screen that's active when calling the members.</summary>
    public interface IManagedConditions
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the conditions were parsed successfully (regardless of whether they're in scope currently).</summary>
        bool IsValid { get; }

        /// <summary>If <see cref="IsValid"/> is false, an error phrase indicating why the conditions failed to parse, formatted like this: <c>'seasonz' isn't a valid token name; must be one of &lt;token list&gt;</c>. If the conditions are valid, this is <c>null</c>.</summary>
        string ValidationError { get; }

        /// <summary>Whether the conditions' tokens are all valid in the current context. For example, this would be false if the conditions use <c>Season</c> and a save isn't loaded yet.</summary>
        bool IsReady { get; }

        /// <summary>Whether <see cref="IsReady"/> is true, and the conditions all match in the current context.</summary>
        bool IsMatch { get; }

        /// <summary>Whether <see cref="IsMatch"/> may change depending on the context. For example, <c>Season</c> is mutable since it depends on the in-game season. <c>HasMod</c> is not mutable, since it can't change after the game is launched.</summary>
        bool IsMutable { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Update the conditions based on Content Patcher's current context for every active screen. It's safe to call this as often as you want, but it has no effect if the Content Patcher context hasn't changed since you last called it.</summary>
        /// <returns>Returns the screens for which <see cref="IsMatch"/> changed value. To check if the current screen changed, you can check <c>UpdateContext()</c></returns>
        IEnumerable<int> UpdateContext();

        /// <summary>If <see cref="IsMatch"/> is false, analyze the conditions/context and get a human-readable reason phrase explaining why the conditions don't match the context. For example: <c>conditions don't match: season</c>. If the conditions do match, this returns <c>null</c>.</summary>
        string GetReasonNotMatched();
    }
}
