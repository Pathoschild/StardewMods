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
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Tools;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Characters;

namespace TractorMod
{
    public class TractorMod : Mod
    {
        static Vector2 tractorSpawnLocation = new Vector2(70, 13);
        public class Tractor : Horse
        {
            public Tractor() : base() { }
            public Tractor(int tileX, int tileY) : base(tileX, tileY)
            {
                this.sprite = new AnimatedSprite(Game1.content.Load<Texture2D>("..\\Mods\\TractorMod\\TractorXNB\\tractor"), 0, 32, 32);
                this.sprite.textureUsesFlippedRightForLeft = true;
                this.sprite.loop = true;
            }
        }

        public class ToolConfig
        {
            public string name { get; set; } = "";
            public int minLevel { get; set; } = -1;
            public int effectRadius { get; set; } = -1;

            public ToolConfig(string nameInput, int minLevelInput = -1, int radiusInput = -1)
            {
                name = nameInput;
                minLevel = minLevelInput;
                effectRadius = radiusInput;
            }
        }

        public class TractorConfig
        {
            public string info1 = "Add tool with exact name you would like to use with Tractor Mode.";
            public string info2 = "Also custom minLevel and effective radius for each tool.";
            public string info3 = "Ingame tools included: Pickaxe, Axe, Hoe, Watering Can.";
            public string info4 = "I haven't tried tools like Shears or Milk Pail but you can :)";
            public string info5 = "Delete Scythe entry if you don't want to harvest stuff.";
            public ToolConfig[] tool { get; set; } = {
                                                        new ToolConfig("Scythe", 0, 2),
                                                        new ToolConfig("Hoe", 4, 1),
                                                        new ToolConfig("Watering Can", 4, 1)
                                                     };

            public int holdActivate { get; set; } = 0;
            public Keys tractorKey { get; set; } = Keys.B;
            public int tractorSpeed { get; set; } = -2;
            public Keys horseKey { get; set; } = Keys.None;
            public int globalTractor { get; set; } = 0;
        }
        
        public static TractorConfig ModConfig { get; protected set; }
        static Tractor ATractor = null;
        static bool IsNewDay = false;
        public override void Entry(IModHelper helper)
        {
            ModConfig = helper.ReadConfig<TractorConfig>();

            //delete tractor when sleep so that it doesnt get save
            StardewModdingAPI.Events.TimeEvents.OnNewDay += (p, e) =>
            {
                if (ATractor != null)
                {
                    Game1.warpCharacter((NPC)ATractor, "Farm", tractorSpawnLocation, false, true);
                    foreach (NPC character in Game1.getFarm().characters)
                    {
                        if (character is Tractor)
                        {
                            Game1.getFarm().characters.Remove(character);
                            break;
                        }
                    }
                    ATractor = null;
                }
                IsNewDay = true;
            };

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
        /*
        static int toggleDelay = 30;
        static int toggleDelayCount = 0;
        */

        static int mouseHoldDelay = 5;
        static int mouseHoldDelayCount = mouseHoldDelay;
        static int playerOrientation = -1;

        static Farm ourFarm = null;
        static void SpawnTractor()
        {
            //remove tractor if there is one already
            foreach (NPC character in Game1.getFarm().characters)
            {
                if (character is Tractor)
                {
                    Game1.getFarm().characters.Remove(character);
                    break;
                }
            }

            //spawn tractor
            ATractor = new Tractor((int)tractorSpawnLocation.X, (int)tractorSpawnLocation.Y);
            ATractor.name = "Tractor";
            Game1.getFarm().characters.Add((NPC)ATractor);
            Game1.warpCharacter((NPC)ATractor, "Farm", tractorSpawnLocation, false, true);
        }
        static void DoAction(KeyboardState currentKeyboardState, MouseState currentMouseState)
        {
            if (Game1.currentLocation == null)
                return;

            //get playerOrientation
            if(Game1.player.isMoving())
                playerOrientation = Game1.player.movementDirections[0];

            if (ourFarm == null)
                ourFarm = Game1.getFarm();

            //spawn Tractor on newday
            if (Game1.currentLocation is Farm)
            {
                if(IsNewDay == true)
                {
                    SpawnTractor();
                    IsNewDay = false;
                }
            }

            //summon Tractor
            if (currentKeyboardState.IsKeyDown(ModConfig.tractorKey))
            {
                Vector2 tile = Game1.player.getTileLocation();
                if (ATractor == null)
                    SpawnTractor();
                Game1.warpCharacter((NPC)ATractor, Game1.currentLocation.name, tile, false, true);
            }

            //summon Horse
            if (currentKeyboardState.IsKeyDown(ModConfig.horseKey))
            {
                foreach(GameLocation GL in Game1.locations)
                {
                    foreach (NPC character in GL.characters)
                    {
                        if (character is Tractor)
                        {
                            continue;
                        }
                        if (character is Horse)
                        {
                            Game1.warpCharacter((NPC)character, Game1.currentLocation.name, Game1.player.getTileLocation(), false, true);
                        }
                    }
                }
            }
            
            //check if mod can run at current location
            if (CanModRunAtCurrentLocation() == false)
                return;

            //if mod can run
            TractorOn = false;
            switch (ModConfig.holdActivate)
            {
                default: break;
                case 1:
                    if (currentMouseState.LeftButton == ButtonState.Pressed)
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
                    break;
                case 2:
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
                    break;
                case 3:
                    if (currentMouseState.MiddleButton == ButtonState.Pressed)
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
                    break;
            }
            
            if(ATractor != null)
            {
                if(ATractor.rider == Game1.player)
                {
                    TractorOn = true;
                }
            }
            else //this should be unreachable code
            {
                SpawnTractor();
            }

            bool BuffAlready = false;
            if (TractorOn == false)
                return;

            //find if tractor buff already applied
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

            //create new buff if its not already applied
            if (BuffAlready == false)
            {
                Buff TractorBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, ModConfig.tractorSpeed, 0, 0, 1, "Tractor Power");
                TractorBuff.which = buffUniqueID;
                TractorBuff.millisecondsDuration = 1000;
                Game1.buffsDisplay.addOtherBuff(TractorBuff);
                BuffAlready = true;
            }
            
            if (Game1.player.CurrentTool == null)
                ItemAction();
            else
                RunToolAction();
        }

        static bool CanModRunAtCurrentLocation()
        {
            if (ModConfig.globalTractor != 0)
                return true;
            
            if (Game1.currentLocation.GetType() == typeof(Farm))
                return true;
            if (Game1.currentLocation.name.ToLower().Contains("greenhouse"))
                return true;
            if (Game1.currentLocation.name.ToLower().Contains("coop"))
                return true;
            if (Game1.currentLocation.name.ToLower().Contains("barn"))
                return true;

            return false;
        }

        public static void RunToolAction()
        {
            if (Game1.player.CurrentTool is MeleeWeapon && Game1.player.CurrentTool.name.ToLower().Contains("scythe"))
                HarvestAction();
            else
                ToolAction();
        }

        public static void ItemAction()
        {
            if (Game1.player.CurrentItem == null)
                return;

            if (Game1.player.CurrentItem.getCategoryName().ToLower() == "seed" || Game1.player.CurrentItem.getCategoryName().ToLower() == "fertilizer")
            {
                List<Vector2> affectedTileGrid = MakeVector2TileGrid(Game1.player.getTileLocation(), 1);

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
            //check if tool is enable from config
            ToolConfig ConfigForCurrentTool = new ToolConfig("");
            foreach (ToolConfig TC in ModConfig.tool)
            {
                if (Game1.player.CurrentTool.name.Contains("Scythe"))
                {
                    ConfigForCurrentTool = TC;
                    break;
                }
            }
            if (ConfigForCurrentTool.name == "")
                return;
            int effectRadius = ConfigForCurrentTool.effectRadius;
            List<Vector2> affectedTileGrid = MakeVector2TileGrid(Game1.player.getTileLocation(), effectRadius);
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

                    if (anObject.name.ToLower().Contains("weed"))
                    {
                        Game1.createObjectDebris(771, (int)tile.X, (int)tile.Y, -1, 0, 1f, Game1.currentLocation); //fiber
                        if(new Random().Next(0,10) < 1) //10% mixed seeds
                            Game1.createObjectDebris(770, (int)tile.X, (int)tile.Y, -1, 0, 1f, Game1.currentLocation); //fiber
                        Game1.currentLocation.removeObject(tile, false);
                        continue;
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
                        if (hoedirtTile.crop == null)
                            continue;
                        
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

        public static void ToolAction()
        {
            Tool currentTool = Game1.player.CurrentTool;

            //check if tool is enable from config
            ToolConfig ConfigForCurrentTool = new ToolConfig("");
            foreach(ToolConfig TC in ModConfig.tool)
            {
                if(currentTool.name.Contains(TC.name))
                {
                    ConfigForCurrentTool = TC;
                    break;
                }
            }
            
            if (ConfigForCurrentTool.name == "")
                return;
            else
                if (currentTool.upgradeLevel < ConfigForCurrentTool.minLevel)
                    return;

            int effectRadius = ConfigForCurrentTool.effectRadius;
            int currentWater = 0;
            if (currentTool is WateringCan)
            {
                WateringCan currentWaterCan = (WateringCan)currentTool;
                currentWater = currentWaterCan.WaterLeft;
            }
            float currentStamina = Game1.player.stamina;
            Vector2 origin = new Vector2((float)Game1.player.GetBoundingBox().Center.X, (float)Game1.player.GetBoundingBox().Center.Y);
            List<Vector2> affectedTileGrid = MakeVector2TileGrid(Game1.player.getTileLocation(), effectRadius);

            //if player on horse
            Vector2 currentMountPosition = new Vector2();
            if (Game1.player.isRidingHorse())
            {
                currentMountPosition = Game1.player.getMount().position;
                Game1.player.getMount().position = new Vector2(0, 0);
            }

            //tool use
            foreach (Vector2 tile in affectedTileGrid)
            {
                Game1.player.CurrentTool.DoFunction(Game1.currentLocation, (int)(tile.X * Game1.tileSize), (int)(tile.Y * Game1.tileSize), 1, Game1.player);
            }

            //after tool use
            if (Game1.player.isRidingHorse())
            {
                Game1.player.getMount().position = currentMountPosition;
            }
            Game1.player.stamina = currentStamina;

            if (currentTool.GetType() == typeof(WateringCan))
            {
                WateringCan currentWaterCan = (WateringCan)currentTool;
                currentWaterCan.WaterLeft = currentWater;
            }
        }

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

                    Vector2 newVec = new Vector2(origin.X - size * Game1.tileSize, origin.Y - size * Game1.tileSize);
                    newVec.X += (float)i * Game1.tileSize;
                    newVec.Y += (float)j * Game1.tileSize;
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
                    case 0: temp.Y += (numberOfTileBehindPlayer + 2) * Game1.tileSize; break; //go up
                    case 1: temp.X -= numberOfTileBehindPlayer * Game1.tileSize; break; //right
                    case 2: temp.Y -= (numberOfTileBehindPlayer) * Game1.tileSize; break; //down
                    case 3: temp.X += (numberOfTileBehindPlayer + 2) * Game1.tileSize; break; //left
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
                    Vector2 newVec = new Vector2(origin.X - size* Game1.tileSize,
                                                origin.Y - size* Game1.tileSize);

                    newVec.X += (float) i* Game1.tileSize; 
                    newVec.Y += (float) j* Game1.tileSize;

                    grid.Add(newVec);
                }
            }
            return grid;
        }

        static List<Vector2> MakeVector2TileGridForHorse(Vector2 origin, int size)
        {
            List<Vector2> grid = new List<Vector2>();
            if (Game1.player.isMoving() == false)
                return new List<Vector2>();
            
            for (int i = 0; i < 2 * size + 1; i++)
            {
                for (int j = 0; j < 2 * size + 1; j++)
                {
                    bool NoAdd = false;
                    switch (playerOrientation)
                    {
                        default: break;
                        case 0: if (j != 0) NoAdd = true; break;
                        case 2: if (j != 0) NoAdd = true; break;
                        case 1: if (i != 0) NoAdd = true; break;
                        case 3: if (i != 0) NoAdd = true; break;
                    }
                    if (NoAdd)
                        continue;

                    Vector2 newVec = new Vector2(origin.X - size, origin.Y - size);
                    newVec.X += (float)i;
                    newVec.Y += (float)j;
                    grid.Add(newVec);
                }
            }

            //adjust depending on facing
            for (int i = 0; i < grid.Count; i++)
            {
                Vector2 temp = grid[i];
                int numberOfTileBehindPlayer = 1;
                switch (playerOrientation)
                {
                    default: break;
                    case 0: temp.Y += (numberOfTileBehindPlayer + 2); break; //go up
                    case 1: temp.X -= numberOfTileBehindPlayer; break; //right
                    case 2: temp.Y -= (numberOfTileBehindPlayer); break; //down
                    case 3: temp.X += (numberOfTileBehindPlayer + 2); break; //left
                }
                grid[i] = temp;
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
