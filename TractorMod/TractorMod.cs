using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Characters;
using StardewModdingAPI;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Tools;
using StardewValley.Locations;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using PhthaloBlue;

namespace TractorMod
{
    public class TractorHouse : Building
    {
        public TractorHouse() : base()
        {
            buildingType = "Tractor House";
            humanDoor = new Point(-1, -1);
            animalDoor = new Point(-2, -1);
            indoors = null;
            nameOfIndoors = "";
            baseNameOfIndoors = "";
            nameOfIndoorsWithoutUnique = "";
            magical = false;
            tileX = 0;
            tileY = 0;
            maxOccupants = 0;
            tilesWide = 4;
            tilesHigh = 2;
            texture = Game1.content.Load<Texture2D>("..\\Mods\\TractorMod\\TractorXNB\\TractorHouse");
            daysOfConstructionLeft = 1;
        }

        public TractorHouse(BluePrint input, Vector2 tileLocation) : base(input, tileLocation)
        {
            buildingType = "Tractor House";
            humanDoor = new Point(-1, -1);
            animalDoor = new Point(-2, -1);
            indoors = null;
            nameOfIndoors = "";
            baseNameOfIndoors = "";
            nameOfIndoorsWithoutUnique = "";
            magical = true;
            tileX = (int)tileLocation.X;
            tileY = (int)tileLocation.Y;
            maxOccupants = 0;
            tilesWide = 4;
            tilesHigh = 2;
            texture = Game1.content.Load<Texture2D>("..\\Mods\\TractorMod\\TractorXNB\\Stable");
            daysOfConstructionLeft = 0;
        }

        public TractorHouse(Vector2 tileLocation) : base()
        {
            buildingType = "Tractor House";
            humanDoor = new Point(-1, -1);
            animalDoor = new Point(-2, -1);
            indoors = null;
            nameOfIndoors = "";
            baseNameOfIndoors = "";
            nameOfIndoorsWithoutUnique = "";
            magical = true;
            tileX = (int)tileLocation.X;
            tileY = (int)tileLocation.Y;
            maxOccupants = 0;
            tilesWide = 4;
            tilesHigh = 2;
            texture = Game1.content.Load<Texture2D>("..\\Mods\\TractorMod\\TractorXNB\\Stable");
            daysOfConstructionLeft = 0;
        }

        public TractorHouse SetDaysOfConstructionLeft(int input)
        {
            daysOfConstructionLeft = input;
            return this;
        }

        public override bool intersects(Rectangle boundingBox)
        {
            if (daysOfConstructionLeft > 0)
                return base.intersects(boundingBox);
            if (!base.intersects(boundingBox))
                return false;
            if (boundingBox.X >= (this.tileX + 1) * Game1.tileSize && boundingBox.Right < (this.tileX + 3) * Game1.tileSize)
                return boundingBox.Y <= (this.tileY + 1) * Game1.tileSize;
            return true;
        }

        public override void draw(SpriteBatch b)
        {
            if (this.daysOfConstructionLeft > 0)
            {
                this.drawInConstruction(b);
            }
            else
            {
                this.drawShadow(b, -1, -1);
                b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize))), new Rectangle?(this.texture.Bounds), this.color * this.alpha, 0.0f, new Vector2(0.0f, (float)this.texture.Bounds.Height), 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh - 1) * Game1.tileSize) / 10000f);
            }
        }

        public override void dayUpdate(int dayOfMonth)
        {
            base.dayUpdate(dayOfMonth);
            if (this.daysOfConstructionLeft > 0)
                return;
            //this.grabHorse();
            //do special action for this building here
            /*
             */
        }

        public override Rectangle getSourceRectForMenu()
        {
            return new Rectangle(0, 0, this.texture.Bounds.Width, this.texture.Bounds.Height);
        }
    }

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

    public class SaveCollection
    {
        public class Save
        {
            public string FarmerName { get; set; } = "";
            public ulong SaveSeed { get; set; }
            public List<Vector2> TractorHouse = new List<Vector2>();
            public Save() { SaveSeed = ulong.MaxValue; }
            public Save(string nameInput, ulong input)
            {
                SaveSeed = input;
                FarmerName = nameInput;
            }

            public Save AddTractorHouse(int inputX, int inputY)
            {
                foreach (Vector2 THS in TractorHouse)
                {
                    if (THS.X == inputX && THS.Y == inputY)
                        return this;
                }
                TractorHouse.Add(new Vector2(inputX, inputY));
                return this;
            }
        }
        public List<Save> saves = new List<Save>();
        public SaveCollection() { }
        public SaveCollection Add(Save input)
        {
            saves.Add(input);
            return this;
        }
        public Save FindSave(string nameInput, ulong input)
        {
            foreach (SaveCollection.Save ASave in saves)
            {
                if (ASave.SaveSeed == input && ASave.FarmerName == nameInput)
                {
                    return ASave;
                }
            }
            return new SaveCollection.Save();
        }

        public Save FindCurrentSave()
        {
            return FindSave(Game1.player.name, Game1.uniqueIDForThisGame);
        }
    }
    
    public class TractorConfig
    {
        public class ToolConfig
        {
            public string name { get; set; } = "";
            public int minLevel { get; set; } = 0;
            public int effectRadius { get; set; } = 1;
            public int activeEveryTickAmount { get; set; } = 1;
            private int activeCount = 0;

            public ToolConfig() { }

            public ToolConfig(string nameInput)
            {
                name = nameInput;
            }

            public ToolConfig(string nameInput, int minLevelInput, int radiusInput, int activeEveryTickAmountInput)
            {
                name = nameInput;
                minLevel = minLevelInput;
                effectRadius = radiusInput;
                if (activeEveryTickAmountInput <= 0)
                    activeEveryTickAmount = 1;
                else
                    activeEveryTickAmount = activeEveryTickAmountInput;
            }

            public void incrementActiveCount()
            {
                activeCount++;
                activeCount %= activeEveryTickAmount;
            }

            public bool canToolBeActive()
            {
                return (activeCount == 0);
            }
        }
        public string info1 = "Add tool with exact name you would like to use with Tractor Mode.";
        public string info2 = "Also custom minLevel and effective radius for each tool.";
        public string info3 = "Ingame tools included: Pickaxe, Axe, Hoe, Watering Can.";
        public string info4 = "I haven't tried tools like Shears or Milk Pail but you can :)";
        public string info5 = "Delete Scythe entry if you don't want to harvest stuff.";
        public ToolConfig[] tool { get; set; } = {
                                                        new ToolConfig("Scythe", 0, 2, 1),
                                                        new ToolConfig("Hoe"),
                                                        new ToolConfig("Watering Can")
                                                     };

        public int ItemRadius { get; set; } = 1;
        public int holdActivate { get; set; } = 0;
        public Keys tractorKey { get; set; } = Keys.B;
        public int tractorSpeed { get; set; } = -2;
        public Keys horseKey { get; set; } = Keys.None;
        public Keys PhoneKey { get; set; } = Keys.N;
        public int TractorHousePrice { get; set; } = 150000;
        public Keys updateConfig { get; set; } = Keys.P;
    }

    public class TractorMod : Mod
    {
        static Vector2 tractorSpawnLocation = new Vector2(70, 13);

        public static TractorConfig ModConfig { get; set; }
        
        static Tractor ATractor = null;
        static IModHelper TheHelper = null;
        static SaveCollection AllSaves;

        //use to write AllSaves info to some .json file to store save
        static void SaveModInfo()
        {
            if (AllSaves == null)
                AllSaves = new SaveCollection().Add(new SaveCollection.Save(Game1.player.name, Game1.uniqueIDForThisGame));

            SaveCollection.Save currentSave = AllSaves.FindSave(Game1.player.name, Game1.uniqueIDForThisGame);

            if (currentSave.SaveSeed != ulong.MaxValue)
            {
                currentSave.TractorHouse.Clear();
                foreach (Building b in Game1.getFarm().buildings)
                {
                    if (b is TractorHouse)
                        currentSave.AddTractorHouse(b.tileX, b.tileY);
                }
            }
            else
            {
                AllSaves.saves.Add(new SaveCollection.Save(Game1.player.name, Game1.uniqueIDForThisGame));
                SaveModInfo();
                return;
            }
            TheHelper.WriteJsonFile<SaveCollection>("TractorModSave.json", AllSaves);
        }

        //use to load save info from some .json file to AllSaves
        static void LoadModInfo()
        {
            AllSaves = TheHelper.ReadJsonFile<SaveCollection>("TractorModSave.json");
            if (AllSaves == null)
                return;

            SaveCollection.Save currentSave = AllSaves.FindSave(Game1.player.name, Game1.uniqueIDForThisGame);

            if (currentSave.SaveSeed != ulong.MaxValue)
            {
                foreach (Vector2 THS in currentSave.TractorHouse)
                {
                    Game1.getFarm().buildStructure(new TractorHouse().SetDaysOfConstructionLeft(0), THS, false, Game1.player);
                }
            }
        }

        static bool IsNewDay = false;
        public override void Entry(IModHelper helper)
        {
            if (TheHelper == null)
                TheHelper = helper;

            ModConfig = helper.ReadConfig<TractorConfig>();
            AllSaves = helper.ReadJsonFile<SaveCollection>("TractorModSave.json");

            //delete additional objects when sleep so that they dont get save to the vanilla save file
            StardewModdingAPI.Events.TimeEvents.OnNewDay += (p, e) =>
            {
                //save before destroying
                if (IsNewDay == false)
                    SaveModInfo();

                //destroying TractorHouse building
                for (int i = Game1.getFarm().buildings.Count - 1; i >= 0; i--)
                    if (Game1.getFarm().buildings[i] is TractorHouse)
                        Game1.getFarm().destroyStructure(ourFarm.buildings[i]);

                //destroying Tractor
                foreach (GameLocation GL in Game1.locations)
                    RemoveEveryCharactersOfType<Tractor>(GL);
                IsNewDay = true;
            };
            StardewModdingAPI.Events.TimeEvents.DayOfMonthChanged += (p, e) =>
            {
                IsNewDay = true;
            };

            //so that weird shit wouldnt happen
            StardewModdingAPI.Events.MenuEvents.MenuChanged += (p, e) =>
            {
                if (e.NewMenu is PhthaloBlueCarpenterMenu)
                    PhthaloBlueCarpenterMenu.IsOpen = true;
                else
                    PhthaloBlueCarpenterMenu.IsOpen = false;
            };
            StardewModdingAPI.Events.MenuEvents.MenuClosed += (p, e) =>
            {
                if (e.PriorMenu is PhthaloBlueCarpenterMenu)
                    PhthaloBlueCarpenterMenu.IsOpen = false;
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
            if (Keyboard.GetState().IsKeyDown(ModConfig.updateConfig))
                ModConfig = TheHelper.ReadConfig<TractorConfig>();
            DoAction(currentKeyboardState, currentMouseState);
        }

        const int buffUniqueID = 58012397;
        static bool TractorOn = false;
        static int mouseHoldDelay = 5;
        static int mouseHoldDelayCount = mouseHoldDelay;

        static Farm ourFarm = null;
        static void SpawnTractor(bool SpawnAtFirstTractorHouse = true)
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

            if(SpawnAtFirstTractorHouse == false)
            {
                ATractor = new Tractor((int)tractorSpawnLocation.X, (int)tractorSpawnLocation.Y);
                ATractor.name = "Tractor";
                Game1.getFarm().characters.Add((NPC)ATractor);
                Game1.warpCharacter((NPC)ATractor, "Farm", tractorSpawnLocation, false, true);
                return;
            }

            //spawn tractor
            foreach(Building building in Game1.getFarm().buildings)
            {
                if(building is TractorHouse)
                {
                    ATractor = new Tractor((int)building.tileX + 1, (int)building.tileY + 1);
                    ATractor.name = "Tractor";
                    Game1.getFarm().characters.Add((NPC)ATractor);
                    Game1.warpCharacter((NPC)ATractor, "Farm", new Vector2((int)building.tileX + 1, (int)building.tileY + 1), false, true);
                    break;
                }
            }
            /*
            ATractor = new Tractor((int)tractorSpawnLocation.X, (int)tractorSpawnLocation.Y);
            ATractor.name = "Tractor";
            */
        }
        static void RemoveEveryCharactersOfType<T>(GameLocation GL)
        {
            bool found = false;
            foreach (NPC character in GL.characters)
            {
                if (character is T)
                {
                    found = true;
                    GL.characters.Remove(character);
                    break;
                }
            }
            if (found)
                RemoveEveryCharactersOfType<T>(GL);
        }
        static void DoAction(KeyboardState currentKeyboardState, MouseState currentMouseState)
        {
            if (Game1.currentLocation == null)
                return;

            if (ourFarm == null)
                ourFarm = Game1.getFarm();

            if (Game1.currentLocation is Farm && IsNewDay)
            {
                LoadModInfo();
                SpawnTractor();
                IsNewDay = false;
            }
            

            //use cellphone
            if (currentKeyboardState.IsKeyDown(ModConfig.PhoneKey))
            {
                if (PhthaloBlueCarpenterMenu.IsOpen)
                    return;
                Response[] answerChoices = new Response[2]
                {
                    new Response("Construct", "Browse PhthaloBlue Corp.'s building catalog"),
                    new Response("Leave", "Hang up")
                };

                Game1.currentLocation.createQuestionDialogue("Hello, this is PhthaloBlue Corporation. How can I help you?", answerChoices, OpenPhthaloBlueCarpenterMenu);
            }
            
            //summon Tractor
            if (currentKeyboardState.IsKeyDown(ModConfig.tractorKey))
            {
                Vector2 tile = Game1.player.getTileLocation();
                if (ATractor == null) //check if you already own TractorHouse, if so then spawn tractor if its null
                {
                    SaveCollection.Save currentSave = AllSaves.FindSave(Game1.player.name, Game1.uniqueIDForThisGame);
                    if (currentSave.TractorHouse.Count > 0)
                        SpawnTractor(false);
                }
                if(ATractor != null)
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

            //disable Tractor Mode if player doesn't have TractorHouse built
            /*
            if (AllSaves.FindCurrentSave().TractorHouse.Count <= 0) 
                return;
            */

            //staring tractorMod
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
                List<Vector2> affectedTileGrid = MakeVector2TileGrid(Game1.player.getTileLocation(), ModConfig.ItemRadius);

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
            TractorConfig.ToolConfig ConfigForCurrentTool = new TractorConfig.ToolConfig("");
            foreach (TractorConfig.ToolConfig TC in ModConfig.tool)
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
                        
                        if (hoedirtTile.crop.whichForageCrop == 1) //spring onion
                        {
                            StardewValley.Object anObject = new StardewValley.Object(399, 1);
                            bool gatherer = CheckFarmerProfession(Game1.player, Farmer.gatherer);
                            bool botanist = CheckFarmerProfession(Game1.player, Farmer.botanist);
                            if (botanist)
                            {
                                anObject.quality = 4;
                            }
                            if (gatherer)
                            {
                                int num = new Random().Next(0, 100);
                                if (num < 20)
                                {
                                    anObject.stack *= 2;
                                }
                            }
                            for (int i = 0; i < anObject.stack; i++)
                                Game1.currentLocation.debris.Add(new Debris(anObject, new Vector2(tile.X * Game1.tileSize, tile.Y * Game1.tileSize)));

                            hoedirtTile.destroyCrop(tile, true);
                            continue;
                        }

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
            TractorConfig.ToolConfig ConfigForCurrentTool = new TractorConfig.ToolConfig("");
            foreach (TractorConfig.ToolConfig TC in ModConfig.tool)
            {
                if (currentTool.name.Contains(TC.name))
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

            if (ConfigForCurrentTool.activeEveryTickAmount > 1)
            {
                ConfigForCurrentTool.incrementActiveCount();
                if (ConfigForCurrentTool.canToolBeActive() == false)
                    return;
            }

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

            //Tool 
            /*
            if(currentTool is Axe && currentTool.upgradeLevel < 2) //steel level
                affectedTileGrid = RemoveTileWithResourceClumpType(ResourceClump.hollowLogIndex, affectedTileGrid);
            if (currentTool is Axe && currentTool.upgradeLevel < 1)
                affectedTileGrid = RemoveTileWithResourceClumpType(ResourceClump.stumpIndex, affectedTileGrid);
            if (currentTool is Pickaxe && currentTool.upgradeLevel < 3)
                affectedTileGrid = RemoveTileWithResourceClumpType(ResourceClump.meteoriteIndex, affectedTileGrid);
            if (currentTool is Pickaxe && currentTool.upgradeLevel < 2)
                affectedTileGrid = RemoveTileWithResourceClumpType(ResourceClump.boulderIndex, affectedTileGrid);
            if (currentTool is Pickaxe && currentTool.upgradeLevel < 1)
            {
                affectedTileGrid = RemoveTileWithResourceClumpType(ResourceClump.mineRock1Index, affectedTileGrid);
                affectedTileGrid = RemoveTileWithResourceClumpType(ResourceClump.mineRock2Index, affectedTileGrid);
                affectedTileGrid = RemoveTileWithResourceClumpType(ResourceClump.mineRock3Index, affectedTileGrid);
                affectedTileGrid = RemoveTileWithResourceClumpType(ResourceClump.mineRock4Index, affectedTileGrid);
            }
            */
            
            //tool use
            Tool newTool = currentTool;
            newTool.upgradeLevel = 4;
            Game1.player.toolPower = 0;
            foreach (Vector2 tile in affectedTileGrid)
            {
                newTool.DoFunction(Game1.currentLocation, (int)(tile.X * Game1.tileSize), (int)(tile.Y * Game1.tileSize), 0, Game1.player);
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

        /*
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
        */

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

        static List<Vector2> RemoveTileWithResourceClumpType(int ResourceClumpIndex, List<Vector2> input)
        {
            List<Vector2> output = new List<Vector2>();
            foreach(Vector2 tile in input)
            {
                if (CheckIfTileBelongToResourceClump(ResourceClumpIndex, tile))
                    continue;
                else
                    output.Add(new Vector2(tile.X, tile.Y));
            }

            return output;
        }

        static bool CheckIfTileBelongToResourceClump(int ResourceClumpIndex, Vector2 tile)
        {
            TerrainFeature check;
            if (Game1.currentLocation.terrainFeatures.TryGetValue(tile, out check))
            {
                if (check is ResourceClump)
                {
                    ResourceClump RC = (ResourceClump)check;
                    if (RC.parentSheetIndex == ResourceClumpIndex)
                        return true;
                }
            }

            Vector2 tileToLeft = new Vector2(tile.X - 1, tile.Y);
            if (Game1.currentLocation.terrainFeatures.TryGetValue(tileToLeft, out check))
            {
                if (check is ResourceClump)
                {
                    ResourceClump RC = (ResourceClump)check;
                    if (RC.parentSheetIndex == ResourceClumpIndex)
                        return true;
                }
            }

            Vector2 tileToTopLeft = new Vector2(tile.X - 1, tile.Y - 1);
            if (Game1.currentLocation.terrainFeatures.TryGetValue(tileToTopLeft, out check))
            {
                if (check is ResourceClump)
                {
                    ResourceClump RC = (ResourceClump)check;
                    if (RC.parentSheetIndex == ResourceClumpIndex)
                        return true;
                }
            }

            Vector2 tileToTop = new Vector2(tile.X, tile.Y - 1);
            if (Game1.currentLocation.terrainFeatures.TryGetValue(tileToTop, out check))
            {
                if (check is ResourceClump)
                {
                    ResourceClump RC = (ResourceClump)check;
                    if (RC.parentSheetIndex == ResourceClumpIndex)
                        return true;
                }
            }

            return false;
        }

        static void OpenPhthaloBlueCarpenterMenu(Farmer who, string whichAnswer)
        {
            switch (whichAnswer)
            {
                case "Construct":
                    BluePrint TractorBP = new BluePrint("Garage");
                    TractorBP.itemsRequired.Clear();
                    TractorBP.texture = Game1.content.Load<Texture2D>("..\\Mods\\TractorMod\\TractorXNB\\TractorHouse");
                    TractorBP.humanDoor = new Point(-1, -1);
                    TractorBP.animalDoor = new Point(-2, -1);
                    TractorBP.mapToWarpTo = "null";
                    TractorBP.description = "A structure to store PhthaloBlue Corp.'s tractor.\nTractor included!";
                    TractorBP.blueprintType = "Buildings";
                    TractorBP.nameOfBuildingToUpgrade = "";
                    TractorBP.actionBehavior = "null";
                    TractorBP.maxOccupants = -1;
                    TractorBP.moneyRequired = ModConfig.TractorHousePrice;
                    TractorBP.tilesWidth = 4;
                    TractorBP.tilesHeight = 2;
                    TractorBP.sourceRectForMenuView = new Rectangle(0, 0, 64, 96);
                    TractorBP.namesOfOkayBuildingLocations.Clear();
                    TractorBP.namesOfOkayBuildingLocations.Add("Farm");
                    TractorBP.magical = true;
                    Game1.activeClickableMenu = new PhthaloBlueCarpenterMenu().AddBluePrint<TractorHouse>(TractorBP);
                    ((PhthaloBlueCarpenterMenu)Game1.activeClickableMenu).WherePlayerOpenThisMenu = Game1.currentLocation;
                    break;
                case "Leave":
                    new PhthaloBlueCarpenterMenu().Hangup();
                    break;
            }
        }
    }
}
