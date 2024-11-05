using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models;

/// <summary>Record for info about a linked text area within a field.</summary>
/// <param name="Subject">Subject to open</param>
internal record LinkTextArea(ISubject Subject)
{
    internal Rectangle Rect { get; set; } = Rectangle.Empty;
};