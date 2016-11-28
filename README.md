# Tractor Mod - A SMAPI Mod for Stardew Valley
#### A mod for stardew valley: auto till, water, fertilize, and seed dirt tiles on your farm by simply walking over them. 

Modder: PhthaloBlue  

This is a mod that allows players to quickly till dirt, sow seeds, fertilize soil, and water crop by simple walking over them.  

It is default to only work with iridium tools equipped so that the mod doesnt ruin your early game but you can change that in the config file.

###Latest Version: [1.2.3](https://github.com/lambui/StardewValleyMod_TractorMod/releases)
####Change Log **v.1.2.3**:
+ **Add** ability to harvest animal grass (you will receive hay stored in your silo)..
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
  - Check [**Demo**](https://github.com/lambui/StardewValleyMod_TractorMod/blob/master/Changelog.md) section for demonstation of this feature.  
  - Check [**Customize**](https://github.com/lambui/StardewValleyMod_TractorMod/blob/master/Changelog.md) section for new config options of this ability.  
+ Now work in **Greenhouse**.
+ Now work in **Coops**.
+ Now work in **Barns**.
+ **Note:**
  - If you use Horse Mode then you can use [Horse Whistle Mod](https://www.google.com/#q=Horse+Whistle+stardew+valley) to bring your horse into Greenhouse, Coops, or Barns.

####Past [changelog.](https://github.com/lambui/StardewValleyMod_TractorMod/blob/master/Changelog.md)

###Demo:  
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

###Require:  
1. [Stardew Valley](http://store.steampowered.com/app/413150/)
2. [SMAPI: +1.1](https://github.com/ClxS/SMAPI/releases)

###Install:  
1. Get [Stardew Valley](http://store.steampowered.com/app/413150/) $$$
2. [Install SMAPI](http://canimod.com/guides/using-mods#installing-smapi)
3. Unzip the mod folder into Stardew Valley/Mods (just put TractorMod folder into /Mods folder)
4. Run [SMAPI](http://canimod.com/guides/using-mods#installing-smapi)


###Download [here](https://github.com/lambui/StardewValleyMod_TractorMod/releases)

###How To Use:
+ Normal Mode:  
    1. Get to your farm (this mod can only be activated on farmland, not in town, not even in greenhouse)
    2. Turn on Tractor Mode by holding down right click.  
    You will receive a +1 speed buff showing that the Tractor Mode is now on.
    3. Hold your hoe if you want to till dirt  
    Hold water can if you want to water crop  
    Hold seed bag(s) to sow seeds  
    Hold fertilizer to fertilize tilled soil
    4. After you are done with your farmwork, simply release right click to turn it off.  
    The buff will go away showing that the Mode is now off.
+ Horse Mode:  
    1. Get to your farm (this mod can only be activated on farmland, not in town, not even in greenhouse)
    2. Get on your horse.  
    3. Turn on Tractor Mode by click toggle hotkey.  
    You will receive a +1 speed buff showing that the Tractor Mode is now on.
    3. Hold your hoe if you want to till dirt  
    Hold water can if you want to water crop  
    Hold seed bag(s) to sow seeds  
    Hold fertilizer to fertilize tilled soil
    4. After you are done with your farmwork, simply click toggle hotkey again to turn it off.  
    The buff will go away showing that the Mode is now off.


###Customize:  
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

###Note:  
+ Be careful when tilling your soil because the hoe can destroy your small-medium sized fruit trees.  
+ Tractor Mode doesn't drain your stamina when using Hoe and Water can.  
+ Water can doesn't need to be refilled but needs to have some water in it to work.  
+ You can fertilize your crop AFTER sowing seeds with Tractor Mode.  
+ Be extra careful with WTFMode haha
+ If you use Horse Mode then you can use [Horse Whistle Mod](https://www.google.com/#q=Horse+Whistle+stardew+valley) to bring your horse into Greenhouse, Coops, or Barns.
+ **Have fun farming! :)**

Contact me @ [buiphuonglamvn@gmail.com](mailto:buiphuonglamvn@gmail.com) regarding whatever.
