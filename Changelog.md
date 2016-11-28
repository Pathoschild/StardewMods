#Change Log:  
####Change Log **v.1.2.4**:
+ **Fix** a bug when harvesting sunflower doesn't yield seeds.  
+ **Tweak** harvesting animal products now spawns little item-drops instead of adding directly to inventory.  

####Change Log **v.1.2.3**:
+ **Add** ability to harvest animal grass (you will receive hay stored in your silo).
+ **Fix** a bug that allows harvesting young non-regrowable crops.

####Change Log **v.1.2.2**:
+ **Fix** a bug that yields infinite harvest when harvesting non-regrowable crops.
    
####Change Log **v.1.2.1**:
+ **Fix** a bug that sometimes causes crash when player runs out of fertilizer/seeds while fertilizing/planting.

####Change Log **v.1.2**:
+ **Require** [SMAPI 1.1.1](https://github.com/ClxS/SMAPI/releases)
+ **Add** Harvest Ability.
  - You can harvest crops during Tractor Mode by holding **Scythe**.  
  - You can harvest fruits from fruit trees during Tractor Mode by holding **Scythe**.  
  - You can harvest dropped products (truffles, eggs, ...) during Tractor Mode by holding **Scythe**. 
  - Check **Demo** section for demonstation of this feature.  
  - Check **Customize** section for new config options of this ability.  
+ Now work in **Greenhouse**.
+ Now work in **Coops**.
+ Now work in **Barns**.
  
####Change Log **v.1.1**:
+ **Add** Horse-tractor Mode.  
  - You can either use this mode (riding your horse and toggle on/off Tractor Mode) or normal mode (hold right click).  
  - To enable this mode, change **needHorse** in config.json file to 1 instead of 0.  
  - By enabling this mode, you can no longer use holding right click to enable Tractor Mode anymore.  
  - If you use this mode then make sure to set **tractorKey** (toggle hotkey) in config.json. Default is **B**.  
  - Have to be on a horse to toggle on Tractor Mode.  
+ **Add** WTFMode.  
  - WTFMode means you can use your **pickaxe** and **axe** with Tractor Mode.  
  - To enable this, change WTFMode in config.json file to 1 instead of 0.  

####Change Log **v.1.0.2**:
+ **Remove** keyboard activation toggle.  
  - Tractor Mode will no longer turn on using keyboard.  
  - (Config tracktorKey remains in config.json, it does nothing for now)
+ **Add** holding right mouse activation.  
  - Tractor Mode will now be activated by holding down right click.  
  - Tractor Mode will be deactivated by releasing right click.
+ Tractor Mode will now automatically turn itself off outside farm.


#Customize:  
####Everything is in config.json file
1. Change needHorse to change mode.  
0 is using normal mode. Default.  
1 is using horse mode.  

2. Change tractorKey to change hotkey.  
Default is 0 (no hotkey) but you can change hotkey by changing the config.json file.  
If you don't use horse mode then this hotkey is useless. You can ignore it in that case.  
Check out KeyCode.txt file to find your desirable key code.   

3. Change WTFMode to enable/disable pickaxe and axe.  
0 is axe, pickaxe disable. Default.
1 is axe, pickaxe enable.  

3. Change harvestMode to enable/disable harvest ability.  
0 is disable harvest ability.  
1 is enable harvest ability. Default.

4. Change harvestRadius to change harvest area around player.  
Harvest stuff in a square grid around player with edge being harvestRadius tiles away from player.  
2 (5x5 grid around player) by Default.  

4. Change minToolPower to lower the minimum tool upgrade require for the Mod to work.  
0 = no upgrade required  
1 = copper upgrade  
2 = silver  
3 = gold  
4 = iridium, this is default

5. mapWidth and mapHeight is used to estimate the maximum size of your farm.  
Default is 170 for both because I don't think the farm map is bigger than 170 tiles in both directions.  
If you have mod that change your farm to be bigger than default values then make sure to change these values accordingly (don't have to be exact, just bigger is fine).

#Demo:  
####Horse Mode [+ Horse to Tractor mod by Pewtershmitz](http://community.playstarbound.com/threads/tractor-v-1-3-horse-replacement.108604/) for maximum immersion :)
![gif-horse-mode](https://github.com/lambui/StardewValleyMod_TractorMod/blob/gif/images/tractor2.gif)  

####Till Dirt  
![gif-till dirt](https://github.com/lambui/StardewValleyMod_TractorMod/blob/gif/images/TillDirt.gif)  

####Water Crop    
![gif-water crop](https://github.com/lambui/StardewValleyMod_TractorMod/blob/gif/images/water.gif)  

####Fertilize Soil    
![gif-fertilize](https://github.com/lambui/StardewValleyMod_TractorMod/blob/gif/images/fertilizing.gif)  

####Sow Seeds      
![gif-sow seed](https://github.com/lambui/StardewValleyMod_TractorMod/blob/gif/images/sowingSeed.gif)  

####Harvest Crops      
![gif-harvest_crop](https://github.com/lambui/StardewValleyMod_TractorMod/blob/gif/images/harvestCrop.gif)  

####Harvest Fruits      
![gif-harvest_fruit](https://github.com/lambui/StardewValleyMod_TractorMod/blob/gif/images/harvestFruitTree.gif)  

####Harvest Truffles      
![gif-harvest_drop](https://github.com/lambui/StardewValleyMod_TractorMod/blob/gif/images/harvestDrop.gif)  
