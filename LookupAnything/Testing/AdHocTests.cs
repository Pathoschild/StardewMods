using System;
using System.Diagnostics;
using System.Linq;
using Pathoschild.LookupAnything.Framework;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Models;
using StardewValley;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything.Testing
{
    /// <summary>Contains test methods which verify that the mod correctly parses data.</summary>
    /// <remarks>These methods must be run within the context of a running game, so can't be implemented with a unit testing framework.</remarks>
    internal static class AdHocTests
    {
        /// <summary>Verify that gift tastes are correctly parsed.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public static void VerifyGiftTastes(Metadata metadata)
        {
            // get raw data
            Object[] items = 
                (
                    from entry in Game1.objectInformation
                    let item = new Object(entry.Key, 1)
                    where item.parentSheetIndex != 0 // weeds
                    orderby item.name
                    select item
                )
                .ToArray();
            NPC[] villagers = 
                (
                    from villager in Utility.getAllCharacters()
                    where villager.isVillager() && !metadata.Constants.AsocialVillagers.Contains(villager.name)
                    orderby villager.name
                    select villager
                )
                .ToArray();
            GiftTasteModel[] parsedTastes = DataParser.GetGiftTastes(DataParser.GetObjects().ToArray()).ToArray();

            // validate
            int errors = 0;
            foreach (NPC villager in villagers)
            {
                foreach (Object item in items)
                {
                    // get real taste
                    GiftTaste expected;
                    try
                    {
                        expected = (GiftTaste)villager.getGiftTasteForThisItem(item);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"skipped {villager.name}: {ex.Message}");
                        break;
                    }

                    // get parsed tastes
                    GiftTaste[] actual =
                        (
                            from entry in parsedTastes
                            where entry.ItemID == item.ParentSheetIndex && entry.Villager == villager.name
                            select entry.Taste
                        )
                        .ToArray();

                    // validate
                    if (actual.Length == 0)
                        continue; // parsing only covers objects listed in gift taste data
                    if(actual.Length > 1)
                        Debug.WriteLine($"{++errors}: {villager.name} for {item.name} ({item.parentSheetIndex} {item.category}): expected {expected}, found multiple: {string.Join(", ", actual) }");
                    else if(actual.Single() != expected)
                        Debug.WriteLine($"{++errors}: {villager.name} for {item.name} ({item.parentSheetIndex} {item.category}): expected {expected}, got {actual.Single()}");
                }
            }

            Debug.WriteLine($"{errors} errors found.");
        }
    }
}
