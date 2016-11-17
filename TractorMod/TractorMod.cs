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

            if(ModConfig.needHorse == 0)
                StardewModdingAPI.Events.GameEvents.EighthUpdateTick += UpdateTickEvent;
            else
                StardewModdingAPI.Events.GameEvents.UpdateTick += UpdateTickEvent;
        }

        static void UpdateTickEvent(object sender, EventArgs e)
        {
            if (ModConfig == null)
                return;

            if (StardewValley.Game1.currentLocation == null)
                return;

            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();
            DoAction(currentKeyboardState, currentMouseState);
        }

        const int buffUniqueID = 58012397;
        static bool TractorOn = false;
        static int toggleDelay = 30;
        static int toggleDelayCount = 0;

        static int mouseHoldDelay = 5;
        static int mouseHoldDelayCount = mouseHoldDelay;

        static void DoAction(KeyboardState currentKeyboardState, MouseState currentMouseState)
        {
            if (Game1.currentLocation == null)
                return;

            if (Game1.currentLocation.isFarm == false || Game1.currentLocation.isOutdoors == false)
            {
            	TractorOn = false;
                return;
            }

            if (tileSize == 0)
                tileSize = (float)Game1.player.GetBoundingBox().Width;

            if (ModConfig.needHorse == 0)
            {
                //if doesnt need horse then hold right click to activate
                if (currentMouseState.RightButton == ButtonState.Pressed)
                {
                    if (mouseHoldDelayCount > 0)
                    {
                        mouseHoldDelayCount -= 1;
                    }
                    if (mouseHoldDelayCount <= 0)
                    {
                        TractorOn = true;
                        mouseHoldDelayCount = mouseHoldDelay;
                    }
                }
                else
                {
                    TractorOn = false;
                }
            }
            else
            {
                if (toggleDelayCount > 0)
                    toggleDelayCount -= 1;
                if (Game1.player.isRidingHorse())
                {
                    //or use keyboard to toggle on/off
                    if (currentKeyboardState.IsKeyDown(ModConfig.tractorKey))
                    {
                        if (toggleDelayCount <= 0)
                        {
                            TractorOn = !TractorOn;
                            toggleDelayCount = toggleDelay;
                        }
                    }
                }
                else
                {
                    TractorOn = false;
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
                        if (ModConfig.needHorse == 0)
                            buff.millisecondsDuration = 1000;
                        else
                            buff.millisecondsDuration = 2000;
                    }
                    BuffAlready = true;
                    break;
                }
            }

            if (BuffAlready == false)
            {
                Buff TractorBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, "Tractor Power");
                TractorBuff.which = buffUniqueID;
                if (ModConfig.needHorse == 0)
                    TractorBuff.millisecondsDuration = 1000;
                else
                    TractorBuff.millisecondsDuration = 2000;
                Game1.buffsDisplay.addOtherBuff(TractorBuff);
                BuffAlready = true;
            }

            if (Game1.player.CurrentTool == null)
                ItemAction();
            else
            {
                RunToolAction();
            }
                
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

        public static void RunToolAction()
        {
            if (ModConfig.WTFMode == 0)
            {
                if (Game1.player.CurrentTool.GetType() == typeof(Hoe) || Game1.player.CurrentTool.GetType() == typeof(WateringCan))
                {
                    if (ModConfig.needHorse == 0)
                        ToolAction();
                    else
                        HorseToolAction();
                }
            }
            else
            {
                if (ModConfig.needHorse == 0)
                    ToolAction();
                else
                    HorseToolAction();
            }
        }

        public static void ToolAction()
        {
            Vector2 playerTile = new Vector2(0, 0);
            bool foundplayerTile = false;
            for (int i = 0; i < ModConfig.mapWidth; i++)
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
                if(ModConfig.WTFMode == 0) //if WTFMode == 1 then it bypass all safety TerrainFeature check
                {
                    TerrainFeature terrainTile;
                    if (Game1.currentLocation.terrainFeatures.TryGetValue(tileGrid[index], out terrainTile))
                    {
                        index++;
                        if (Game1.player.CurrentTool.GetType() == typeof(WateringCan))
                            Game1.player.CurrentTool.DoFunction(Game1.currentLocation, (int)Math.Round(tile.X, MidpointRounding.AwayFromZero), (int)Math.Round(tile.Y, MidpointRounding.AwayFromZero), 1, Game1.player);
                        continue;
                    }
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

        public static void HorseToolAction()
        {
            if (Game1.player.CurrentTool.GetType() == typeof(Hoe) || Game1.player.CurrentTool.GetType() == typeof(WateringCan))
            {
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
                List<Vector2> affectedTileGrid = MakeVector2GridForHorse(origin, 1);

                foreach (Vector2 tile in affectedTileGrid)
                {
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
        static List<Vector2> MakeVector2GridForHorse(Vector2 origin, int size)
        {
            List<Vector2> grid = new List<Vector2>();
            if (Game1.player.movementDirections.Count <= 0)
                return new List<Vector2>();

            for (int i = 0; i < 2 * size + 1; i++)
            {
                for (int j = 0; j < 2 * size + 1; j++)
                {
                    bool NoAdd = false;
                    switch (Game1.player.movementDirections[0])
                    {
                        case 0: if (j != 0) NoAdd = true; break;
                        case 2: if (j != 0) NoAdd = true; break;

                        case 1: if (i != 0) NoAdd = true; break;
                        case 3: if (i != 0) NoAdd = true; break;
                    }
                    if (NoAdd)
                        continue;

                    Vector2 newVec = new Vector2(origin.X - size * tileSize, origin.Y - size * tileSize);
                    newVec.X += (float)i * tileSize;
                    newVec.Y += (float)j * tileSize;
                    grid.Add(newVec);
                }
            }
            
            //adjust depending on facing
            for (int i = 0; i < grid.Count; i++)
            {
                Vector2 temp = grid[i];
                int numberOfTileBehindPlayer = 1;
                switch (Game1.player.movementDirections[0])
                {
                    case 0: temp.Y += (numberOfTileBehindPlayer + 2) * tileSize; break; //go up
                    case 1: temp.X -= numberOfTileBehindPlayer * tileSize; break; //right
                    case 2: temp.Y -= (numberOfTileBehindPlayer) * tileSize; break; //down
                    case 3: temp.X += (numberOfTileBehindPlayer + 2) * tileSize; break; //left
                }
                grid[i] = temp;
            }
            return grid;
        }

        static List<Vector2> MakeVector2Grid(Vector2 origin, int size)
        {
            List<Vector2> grid = new List<Vector2>();
            for(int i = 0; i < 2*size+1; i++)
            {
                for (int j = 0; j < 2 * size + 1; j++)
                {
                    Vector2 newVec = new Vector2(origin.X - size*tileSize,
                                                origin.Y - size*tileSize);

                    newVec.X += (float) i*tileSize; 
                    newVec.Y += (float) j*tileSize;

                    grid.Add(newVec);
                }
            }
            return grid;
        }
    }

    public class TractorConfig : Config
    {
        public int needHorse;
        public Keys tractorKey;
        public int WTFMode;
        public int minToolPower;
        public int mapWidth;
        public int mapHeight;

        public override T GenerateDefaultConfig<T>()
        {
            needHorse = 0;
            WTFMode = 0;
            tractorKey = Keys.B;
            minToolPower = 4;
            mapWidth = 170; //i dont think any farm maps exceed 170 tiles any direction
            mapHeight = 170;
            return this as T;
        }
    }
}
