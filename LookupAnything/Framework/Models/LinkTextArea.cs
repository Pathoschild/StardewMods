using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models;

/// <summary>Record for info about a linked text area within a field.</summary>
/// <param name="Subject">The subject to open when the link is clicked.</param>
/// <param name="PixelArea">The pixel area on screen containing the link.</param>
internal record LinkTextArea(ISubject Subject, Rectangle PixelArea);
