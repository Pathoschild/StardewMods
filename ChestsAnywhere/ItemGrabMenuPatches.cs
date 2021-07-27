using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using StardewModdingAPI;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere
{
    [HarmonyPatch(typeof(ItemGrabMenu), nameof(ItemGrabMenu.FillOutStacks))]
    internal class ItemGrabMenuPatches
    {
        private static ChestFactory ChestFactory;
        private static IMonitor Monitor;
        private static Func<RangeHandler> GetRangeHandler;
        private static bool IsProcessing = false;

        internal static void Initialize(IMonitor monitor, ChestFactory chestFactory, Func<RangeHandler> getRangeHandler)
        {
            Monitor = monitor;
            ChestFactory = chestFactory;
            GetRangeHandler = getRangeHandler;
        }

        [HarmonyPostfix]
        internal static void Postfix(ItemGrabMenu __instance)
        {
            if (IsProcessing)
                return;

            try
            {
                IsProcessing = true;

                var chests = ChestFactory.GetChests(GetRangeHandler())
                    .Select(c => c.Container)
                    .Select(c => c.OpenMenu())
                    .OfType<ItemGrabMenu>()
                    .Where(m => m != __instance);


                foreach(ItemGrabMenu chest in chests)
                    chest.FillOutStacks();
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(ItemGrabMenuPatches)}.{nameof(Postfix)}:\n{ex}", LogLevel.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
