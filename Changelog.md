#Change Log:  
####Change Log **v.2.1.3**:
+ **Fix** a bug that freezes the game if player uses under power tools with Tractor on objects that require more powerful tools to break.  

####Change Log **v.2.1.2**:
+ **Fix** a bug that still prevents game from saving.
+ **Add** ability to custom active frequency of each tool to maintain game fps
  - activeEveryTickAmount entry is added for each tool in config.json
  - activeEveryTickAmount default is 1, meaning 1 action per 1 gametick.
  - activeEveryTickAmount is 8, meaning 1 action per 8 gametick. Increase this value to improve game fps.
+ **Add** ItemRadius in config.json, it's radius for seeding and fertilizing.
+ **Add** ability to update config.json ingame.
  - updateConfig sets hotkey to make current config.json taking effect ingame.
  - Make changes to config.json while playing, save it.
  - Return to game and press updateConfig hotkey. Now your config.json changes are live ingame.

####Change Log **v.2.1.1**:
+ **Fix** a bug that prevents game from saving when sleeping if player left Tractor outside Farm.
+ **Fix** a bug that prevents player from summon horse if horse is outside Farm.

####Change Log **v.2.1**:
+ **Change** config.json:
  - Remove WTFMode
  - Remove harvestMode, harvestRadius  
  - Remove minToolPower
  - Remove mapWidth, mapHeight. You no longer have to worry about your map size
  - Add **tool** list and **info** regarding how to use it (do not delete info).
+ **Improve** algorithm, improve performance.

####Change Log **v.2.0**:
+ **Remove** horseMode.
  - needHorse in config.json is removed.
  - Ability to toggle TractorMode on horse is removed.
+ **Add** Tractor:
  - Now you have a brand spanking new **Tractor** seperated from your horse.
  - Tractor will return to the spot right behind your selling box every norming.
  - Riding the Tractor automatically turn on TractorMode.
  - tractorKey in config.json now sets hotkey to summon Tractor to your location. Default to B.
  - tractor sprite and animation by [Horse to Tractor mod by Pewtershmitz](http://community.playstarbound.com/threads/tractor-v-1-3-horse-replacement.108604/)
  - You can change tractor sprite and animation by modding tractor.xnb in TractorXNB folder.
+ **Add** option to change mouse-activation hotkey (activating TractorMod while not on Tractor).
  - holdActivate in config.json file.
  - 0: no mouse hotkey (can't activate TractorMode while not on Tractor). Default.
  - 1: hold left mouse button to activate.
  - 2: hold right mouse button to activate (this is the old one).
  - 3: hold mouse wheel down to activate.
+ **Change** TractorMod buff.
  - TractorMod no longer provides +1 speed buff.
  - TractorMod now gives -2 speed buff (for balance).
  - You can change it by changing tractorSpeed in config.json because I'm a good person.
+ **Add** horse summon hotkey.
  - horseKey in config.json now sets hotkey to summon your horse (if you have one) to your location.
  - Default to None (deactivated).
  
####Change Log **v.1.3**:
+ **Add** Global option:
  - You can use Tractor Mode everywhere, no longer limited in the farm.  
  - Enable this by setting globalTractor equal to 1 in config.json. It is 0 (disable) by default.  
+ **Add** ability to harvest fiber-weeds.  

####Change Log **v.1.2.4**:
+ **Fix** a bug when harvesting sunflower doesn't yield seeds.  
+ **Tweak** harvesting animal products now spawns little item-drops instead of adding them directly to inventory.  

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
+ Tool (explaint by info in config.json):  
To add a tool, add a block of code at the end of the last block of code that look like this (separate these blocks with comma)  
As long as you have the *name* variable, the config.json will auto generate other variables with default for you  
```
{
  "name": "Tool Name Here",
  "minLevel": numerical value (default value 0),
  "effectRadius": another numberical value (default value 1),
  "actionEveryTickAmount": yet another numberical value (default value 1)
}
```

+ Change ItemRadius to change radius for seeding and fertilizing  

+ Change holdActivate to change mouse-activation
0 is disable mouse-activation. Default.  
1 is holding left mouse button to activate TractorMode.  
2 is holding right mouse button to activate TractorMode.  
3 is holding mouse wheel down to activate TractorMode.  

+ Change tractorKey to change summon Tractor key.  
Default is B but you can change hotkey by changing the config.json file.  
Check out KeyCode.txt file to find your desirable key code.   

+ Change tractorSpeed to change speed buff of the Tractor buff.  
Default is -2.  

+ Change horseKey to change summon horse key.  
Default is 0 (no key assigned) but you can change hotkey by changing the config.json file.  
Check out KeyCode.txt file to find your desirable key code.   

+ globalTractor is use to limit areas the mod can function.  
0 = can only use Tractor Mode within your farm land (and areas within it like barns, coops, greenhouse), this is default.  
1 = can use Tractor Mode everywhere you want.  

+ updateConfig sets hotkey for updating config.json ingame  
Default is P but you can change hotkey by changing the config.json file.  
Check out KeyCode.txt file to find your desirable key code.   
<details><summary>*Click here for more details*</summary>
  updateConfig sets hotkey to make current config.json taking effect ingame.
  1. Make changes to config.json while playing, save it.
  2. Get ingame and press updateConfig hotkey. Now your config.json changes are live ingame.
</details>


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
