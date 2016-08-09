// Decompiled with JetBrains decompiler
// Type: AccessChestAnywhere.AccessChestAnywhere
// Assembly: AccessChestAnywhere, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A5EF4C5A-AE47-40FE-981A-E2469D9B9502
// Assembly location: C:\Program Files (x86)\GalaxyClient\Games\Stardew Valley\Mods\AccessChestAnywhere\AccessChestAnywhere.dll

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace AccessChestAnywhere
{
    public class AccessChestAnywhere : Mod
    {
        public AccessChestAnywhere()
        {
            base.\u002Ector();
        }

        public virtual void Entry(params object[] objects)
        {
            ControlEvents.add_KeyPressed(new EventHandler<EventArgsKeyPressed>(this.ControlEvents_KeyPressed));
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (!e.get_KeyPressed().ToString().Equals("B") || Game1.activeClickableMenu != null)
                return;
            Dictionary<string, List<Vector2>> chestList = new Dictionary<string, List<Vector2>>();
            using (List<GameLocation>.Enumerator enumerator1 = ((List<GameLocation>)Game1.locations).GetEnumerator())
            {
                while (enumerator1.MoveNext())
                {
                    GameLocation current1 = enumerator1.Current;
                    List<Vector2> vector2List = new List<Vector2>();
                    using (Dictionary<Vector2, Object>.Enumerator enumerator2 = ((Dictionary<Vector2, Object>)current1.objects).GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            KeyValuePair<Vector2, Object> current2 = enumerator2.Current;
                            if (current2.Value is Chest)
                            {
                                Chest chest = (Chest)current2.Value;
                                if (((Item)chest).get_Name() == "Chest")
                                    ((Item)chest).set_Name(string.Format("Chest({0},{1})", (object)current2.Key.X, (object)current2.Key.Y));
                                if (!((string)((Object)chest).name).Contains("ignore"))
                                    vector2List.Add(current2.Key);
                            }
                        }
                    }
                    if (vector2List.Count > 0)
                        chestList.Add((string)current1.name, vector2List);
                }
            }
            if (chestList.Count > 0)
                Game1.activeClickableMenu = (__Null)new ACAMenu(chestList);
        }
    }
}
