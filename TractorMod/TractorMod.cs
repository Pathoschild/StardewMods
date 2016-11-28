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
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace TractorMod
{
    public class TractorMod : Mod
    {

        public class TractorConfig
        {
            public int needHorse { get; set; } = 0;
            public Keys tractorKey { get; set; } = Keys.B;
            public int WTFMode { get; set; } = 0;
            public int harvestMode { get; set; } = 1;
            public int harvestRadius { get; set; } = 2;
            public int minToolPower { get; set; } = 4; //iridium level
            public int mapWidth { get; set; } = 170;
            public int mapHeight { get; set; } = 170;
        }

        public static TractorConfig ModConfig { get; protected set; }
        public override void Entry(IModHelper helper)
        {
            ModConfig = Helper.ReadConfig<TractorConfig>();

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

        static Farm ourFarm = null;

        static void DoAction(KeyboardState currentKeyboardState, MouseState currentMouseState)
        {
            if (Game1.currentLocation == null)
                return;

            //if (Game1.currentLocation.isFarm == false || Game1.currentLocation.isOutdoors == false )
            bool canRun = false;
            if (Game1.currentLocation.GetType() == typeof(Farm))
            {
                if (ourFarm == null)
                    ourFarm = (Farm)Game1.currentLocation;
                canRun = true;
            }
            if (Game1.currentLocation.name.ToLower().Contains("greenhouse"))
                canRun = true;
            if (Game1.currentLocation.name.ToLower().Contains("coop"))
                canRun = true;
            if (Game1.currentLocation.name.ToLower().Contains("barn"))
                canRun = true;

            if(canRun == false)
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
                Point tileOrigin = Utility.Vector2ToPoint(origin);
                Vector2 playerTile = GetTileOccupiedByFarmer(Game1.player);
                if (playerTile.X == -1)
                    return;
                List<Vector2> affectedTileGrid = MakeVector2TileGrid(playerTile, 1);

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
                                        return;
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
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void HarvestAction()
        {
            if (ModConfig.harvestMode == 0)
                return;

            Vector2 origin = new Vector2((float)Game1.player.GetBoundingBox().Center.X, (float)Game1.player.GetBoundingBox().Center.Y);
            Point tileOrigin = Utility.Vector2ToPoint(origin);
            Vector2 playerTile = GetTileOccupiedByFarmer(Game1.player);
            if (playerTile.X == -1)
                return;

            List<Vector2> affectedTileGrid = MakeVector2TileGrid(playerTile, ModConfig.harvestRadius);
            foreach (Vector2 tile in affectedTileGrid)
            {
                StardewValley.Object anObject;
                if (Game1.currentLocation.objects.TryGetValue(tile, out anObject))
                {
                    if (anObject.isSpawnedObject)
                    {
                        if (anObject.isForage(Game1.currentLocation))
                        {
                            bool gatherer = CheckFarmerProfession(Game1.player, Farmer.gatherer);
                            bool botanist = CheckFarmerProfession(Game1.player, Farmer.botanist);
                            if (botanist)
                                anObject.quality = 4;
                            if (gatherer)
                            {
                                int num = new Random().Next(0, 100);
                                if (num < 20)
                                {
                                    anObject.stack *= 2;
                                }
                            }
                        }

                        /* this for putting item directly in inventory, but its boring and not juicy enough
                        int slot = FindSlotForInputItemInFarmerInventory(Game1.player, anObject);
                        if (slot == -1)
                            continue;
                        Game1.player.addItemToInventory(anObject, slot);
                        */

                        for (int i = 0; i < anObject.stack; i++)
                            Game1.currentLocation.debris.Add(new Debris(anObject, new Vector2(tile.X * Game1.tileSize, tile.Y * Game1.tileSize)));
                        Game1.currentLocation.removeObject(tile, false);
                        continue;
                    }

                    /*
                    if (anObject.name.ToLower().Contains("weed"))
                    {
                        int slot = FindSlotForInputItemInFarmerInventory(Game1.player, anObject);
                        if (slot == -1)
                            continue;
                        Game1.player.addItemToInventory(anObject, slot);
                        Game1.currentLocation.removeObject(tile, false);
                        continue;
                    }
                    */
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
                        if (hoedirtTile.crop == null)
                            continue;

                        Log.Debug(hoedirtTile.crop.indexOfHarvest);
                        if (hoedirtTile.crop.harvest((int)tile.X, (int)tile.Y, hoedirtTile))
                        {
                            if (hoedirtTile.crop.indexOfHarvest == 421) //sun flower
                            {
                                int seedDrop = new Random().Next(1, 4);
                                for (int i = 0; i < seedDrop; i++)
                                    Game1.createObjectDebris(431, (int)tile.X, (int)tile.Y, -1, 0, 1f, Game1.currentLocation); //spawn sunflower seeds
                            }

                            if (hoedirtTile.crop.regrowAfterHarvest == -1)
                            {
                                hoedirtTile.destroyCrop(tile, true);
                            }
                        }
                        continue;
                    }

                    if (Game1.currentLocation.terrainFeatures[tile] is FruitTree)
                    {
                        FruitTree tree = (FruitTree)Game1.currentLocation.terrainFeatures[tile];
                        tree.shake(tile, false);
                        continue;
                    }
                    
                    //will test once I have giantcrop
                    /*
                    if(Game1.currentLocation.terrainFeatures[tile] is GiantCrop)
                    {
                        GiantCrop bigCrop = (GiantCrop)Game1.currentLocation.terrainFeatures[tile];
                        bigCrop.performToolAction((Tool)new Axe(), 100, tile);
                        continue;
                    }
                    */
                    
                    if(Game1.currentLocation.terrainFeatures[tile] is Grass)
                    {
                        Grass grass = (Grass)Game1.currentLocation.terrainFeatures[tile];
                        grass = null;
                        Game1.currentLocation.terrainFeatures.Remove(tile);
                        ourFarm.tryToAddHay(2);
                        continue;
                    }


                    if (Game1.currentLocation.terrainFeatures[tile] is Tree)
                    {
                        continue;
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
                else
                {
                    if (Game1.player.CurrentTool.GetType() == typeof(MeleeWeapon) && Game1.player.CurrentTool.name.ToLower().Contains("scythe"))
                    {
                        HarvestAction();
                    }
                }
            }
            else
            {
                if (Game1.player.CurrentTool.GetType() == typeof(MeleeWeapon) && Game1.player.CurrentTool.name.ToLower().Contains("scythe"))
                {
                    HarvestAction();
                }
                else
                {
                    if (ModConfig.needHorse == 0)
                        ToolAction();
                    else
                        HorseToolAction();
                }
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

            for (int i = 0; i < 2*size+1; i++)
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

        static List<Vector2> MakeVector2TileGrid(Vector2 origin, int size)
        {
            List<Vector2> grid = new List<Vector2>();
            for (int i = 0; i < 2 * size + 1; i++)
            {
                for (int j = 0; j < 2 * size + 1; j++)
                {
                    Vector2 newVec = new Vector2(origin.X - size,
                                                origin.Y - size);

                    newVec.X += (float)i;
                    newVec.Y += (float)j;

                    grid.Add(newVec);
                }
            }

            return grid;
        }

        static Vector2 GetTileOccupiedByFarmer(Farmer input)
        {
            for(int i = 0; i <= ModConfig.mapWidth; i++)
            {
                for(int j = 0; j <= ModConfig.mapHeight; j++)
                {
                    if(input == Game1.currentLocation.isTileOccupiedByFarmer(new Vector2(i, j)))
                    {
                        return new Vector2(i, j);
                    }
                }
            }
            return new Vector2(-1, -1);
        }

        static int FindEmptySlotInFarmerInventory(Farmer input)
        {
            for(int i = 0; i < input.items.Count; i++)
            {
                if (input.items[i] == null)
                    return i;
            }
            return -1;
        }

        static int FindSlotWithSameItemInFarmerInventory(Farmer input, Item inputItem)
        {
            for (int i = 0; i < input.items.Count; i++)
            {
                if (input.items[i] == null)
                    continue;
                if (input.items[i].getRemainingStackSpace() <= 0)
                    continue;
                if(input.items[i].canStackWith(inputItem))
                {
                    return i;
                }
            }
            return -1;
        }

        static int FindSlotForInputItemInFarmerInventory(Farmer input, Item inputItem)
        {
            int slot = FindSlotWithSameItemInFarmerInventory(input, inputItem);
            if (slot == -1)
            {
                slot = FindEmptySlotInFarmerInventory(input);
            }
            return slot;
        }

        static bool CheckFarmerProfession(Farmer farmerInput, int professionIndex)
        {
            foreach(int i in farmerInput.professions)
            {
                if (i == professionIndex)
                    return true; 
            }
            return false;
        }
    }
}
