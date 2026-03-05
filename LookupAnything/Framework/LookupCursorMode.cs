namespace Pathoschild.Stardew.LookupAnything.Framework;

/// <summary>How to handle the cursor position when searching for a match.</summary>
internal enum LookupCursorMode
{
    /// <summary>Check the cursor position if the cursor is available.</summary>
    AutoDetect,

    /// <summary>Check the cursor position even if the game doesn't consider it applicable (e.g. on Android).</summary>
    ForceCheck,

    /// <summary>Ignore the cursor position.</summary>
    Ignore
}
