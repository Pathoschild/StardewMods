using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewModdingAPI;
using System.IO;
using Microsoft.Xna.Framework.Input;
using StardewValley.Tools;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace TractorMod
{
    public class TractorMod : Mod
    {
        public static TractorConfig ModConfig { get; protected set; }
        public override void Entry(params object[] objects)
        {
            ModConfig = new TractorConfig().InitializeConfig(BaseConfigPath);
            StardewModdingAPI.Events.GameEvents.EighthUpdateTick += UpdateTickEvent;
        }
        //PhthaloBlue: these are my codes
        static void UpdateTickEvent(object sender, EventArgs e)
        {
            if (ModConfig == null)
                return;

            if (StardewValley.Game1.currentLocation == null)
                return;

            KeyboardState currentKeyboardState = Keyboard.GetState();
            DoAction(currentKeyboardState);
        }

        const int buffUniqueID = 58012397;
        static bool TractorOn = false;
        static int toggleDelay = 30;
        static int toggleDelayCount = 0;

        static void DoAction(KeyboardState currentKeyboardState)
        {
            if (Game1.currentLocation == null)
                return;

            if (Game1.currentLocation.isFarm == false || Game1.currentLocation.isOutdoors == false)
                return;

            if (tileSize == 0)
                tileSize = (float)Game1.player.GetBoundingBox().Width;

            if (toggleDelayCount > 0)
                toggleDelayCount -= 1;

            if (currentKeyboardState.IsKeyDown(ModConfig.tractorKey))
            {
                if (toggleDelayCount <= 0)
                {
                    TractorOn = !TractorOn;
                    toggleDelayCount = toggleDelay;
                }
            }

            bool BuffAlready = false;
            if (TractorOn == false)
                return;

            foreach (Buff buff in Game1.buffsDisplay.otherBuffs)
            {
                if (buff.which == buffUniqueID)
                {
                    if (buff.millisecondsDuration <= 35)
                    {
                        buff.millisecondsDuration = 1000;
                    }
                    BuffAlready = true;
                    break;
                }
            }

            if (BuffAlready == false)
            {
                Buff TractorBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, "Tractor Power");
                TractorBuff.which = buffUniqueID;
                TractorBuff.millisecondsDuration = 1000;
                Game1.buffsDisplay.addOtherBuff(TractorBuff);
                BuffAlready = true;
            }

            if (Game1.player.CurrentTool == null)
                ItemAction();
            else
                ToolAction();
        }

        public static void ItemAction()
        {
            if (Game1.player.CurrentItem == null)
                return;

            if (Game1.player.CurrentItem.getCategoryName().ToLower() == "seed" || Game1.player.CurrentItem.getCategoryName().ToLower() == "fertilizer")
            {
                Vector2 origin = new Vector2((float)Game1.player.GetBoundingBox().Center.X, (float)Game1.player.GetBoundingBox().Center.Y);
                List<Vector2> affectedTileGrid = new List<Vector2>();
                //isTileHoeDirt

                foreach (var terrainfeature in Game1.currentLocation.terrainFeatures)
                {
                    if (terrainfeature.Value is HoeDirt)
                    {
                        HoeDirt hoedirtTile = (HoeDirt)terrainfeature.Value;
                        Vector2 location = terrainfeature.Key;
                        if (Game1.player == Game1.currentLocation.isTileOccupiedByFarmer(location))
                        {
                            for (int i = 0; i < 2 * 1 + 1; i++)
                            {
                                for (int j = 0; j < 2 * 1 + 1; j++)
                                {
                                    Vector2 newVec = new Vector2(location.X - 1,
                                                                location.Y - 1);

                                    newVec.X += (float)i;
                                    newVec.Y += (float)j;

                                    affectedTileGrid.Add(newVec);
                                }
                            }
                            break;
                        }
                    }
                }

                foreach (Vector2 tile in affectedTileGrid)
                {
                    TerrainFeature terrainTile;
                    if (Game1.currentLocation.terrainFeatures.TryGetValue(tile, out terrainTile))
                    {
                        if (Game1.currentLocation.terrainFeatures[tile] is HoeDirt)
                        {
                            HoeDirt hoedirtTile = (HoeDirt)Game1.currentLocation.terrainFeatures[tile];

                            if (Game1.player.CurrentItem.getCategoryName().ToLower() == "seed")
                            {
                                if (hoedirtTile.crop != null)
                                    continue;
                                if (hoedirtTile.plant(Game1.player.CurrentItem.parentSheetIndex, (int)tile.X, (int)tile.Y, Game1.player))
                                {
                                    Game1.player.CurrentItem.Stack -= 1;
                                    if (Game1.player.CurrentItem.Stack <= 0)
                                    {
                                        Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                                    }
                                }
                            }
                            if (Game1.player.CurrentItem.getCategoryName().ToLower() == "fertilizer")
                            {
                                if (hoedirtTile.fertilizer != 0)
                                    continue;
                                hoedirtTile.fertilizer = Game1.player.CurrentItem.parentSheetIndex;
                                Game1.player.CurrentItem.Stack -= 1;
                                if (Game1.player.CurrentItem.Stack <= 0)
                                {
                                    Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void ToolAction()
        {
            if (Game1.player.CurrentTool.GetType() == typeof(Hoe) || Game1.player.CurrentTool.GetType() == typeof(WateringCan))
            {
                Vector2 playerTile = new Vector2(0,0);
                bool foundplayerTile = false;
                for(int i = 0; i < ModConfig.mapWidth; i++)
                {
                    for (int j = 0; j < ModConfig.mapHeight; j++)
                    {
                        Vector2 mapTile = new Vector2((int)i, (int)j);
                        if (Game1.player == Game1.currentLocation.isTileOccupiedByFarmer(mapTile))
                        {
                            playerTile = mapTile;
                            foundplayerTile = true;
                            break;
                        }
                    }
                    if (foundplayerTile)
                        break;
                }
                if (foundplayerTile == false)
                    return;
                List<Vector2> tileGrid = new List<Vector2>();
                for (int i = 0; i < 2 * 1 + 1; i++)
                {
                    for (int j = 0; j < 2 * 1 + 1; j++)
                    {
                        Vector2 newVec = new Vector2(playerTile.X - 1, playerTile.Y - 1);

                        newVec.X += (float)i;
                        newVec.Y += (float)j;

                        tileGrid.Add(newVec);
                    }
                }


                Tool currentTool = Game1.player.CurrentTool;
                if (currentTool.upgradeLevel < ModConfig.minToolPower)
                    return;
                int currentWater = 0;
                if (currentTool.GetType() == typeof(WateringCan))
                {
                    WateringCan currentWaterCan = (WateringCan)currentTool;
                    currentWater = currentWaterCan.WaterLeft;
                }

                float currentStamina = Game1.player.stamina;
                Vector2 origin = new Vector2((float)Game1.player.GetBoundingBox().Center.X, (float)Game1.player.GetBoundingBox().Center.Y);
                List<Vector2> affectedTileGrid = MakeVector2Grid(origin, 1);
                int index = 0;
                foreach (Vector2 tile in affectedTileGrid)
                {
                    TerrainFeature terrainTile;
                    if (Game1.currentLocation.terrainFeatures.TryGetValue(tileGrid[index], out terrainTile))
                    {
                        index++;
                        if(Game1.player.CurrentTool.GetType() == typeof(WateringCan))
                            Game1.player.CurrentTool.DoFunction(Game1.currentLocation, (int)Math.Round(tile.X, MidpointRounding.AwayFromZero), (int)Math.Round(tile.Y, MidpointRounding.AwayFromZero), 1, Game1.player);
                        continue;
                    }
                    index++;
                    Game1.player.CurrentTool.DoFunction(Game1.currentLocation, (int)Math.Round(tile.X, MidpointRounding.AwayFromZero), (int)Math.Round(tile.Y, MidpointRounding.AwayFromZero), 1, Game1.player);
                }

                Game1.player.stamina = currentStamina;

                if (currentTool.GetType() == typeof(WateringCan))
                {
                    WateringCan currentWaterCan = (WateringCan)currentTool;
                    currentWaterCan.WaterLeft = currentWater;
                }
            }
        }

        public static float tileSize = 0;
        //this will make a list of all the vector2 around origin with size radius (ex: size = 3 => 7x7 grid)
        static List<Vector2> MakeVector2Grid(Vector2 origin, int size)
        {
            List<Vector2> grid = new List<Vector2>();
            for (int i = 0; i < 2 * size + 1; i++)
            {
                for (int j = 0; j < 2 * size + 1; j++)
                {
                    Vector2 newVec = new Vector2(origin.X - size * tileSize,
                                                origin.Y - size * tileSize);

                    newVec.X += (float)i * tileSize;
                    newVec.Y += (float)j * tileSize;

                    grid.Add(newVec);
                }
            }
            return grid;
        }
    }

    public class TractorConfig : Config
    {
        public Keys tractorKey;
        public int minToolPower;
        public int mapWidth;
        public int mapHeight;

        public override T GenerateDefaultConfig<T>()
        {
            tractorKey = Keys.B;
            minToolPower = 4;
            mapWidth = 170; //i dont think any farm maps exceed 170 tiles any direction
            mapHeight = 170;
            return this as T;
        }
    }
}
