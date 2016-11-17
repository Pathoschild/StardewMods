# Tractor Mod - A SMAPI Mod for Stardew Valley
#### A mod for stardew valley: auto till, water, fertilize, and seed dirt tiles on your farm by simply walking over them. 

Modder: PhthaloBlue  

This is a mod that allows players to quickly till dirt, sow seeds, fertilize soil, and water crop by simple walking over them.  

It is default to only work with iridium tools equipped so that the mod doesnt ruin your early game but you can change that in the config file.

###Latest Version: [1.1](https://github.com/lambui/StardewValleyMod_TractorMod/releases)
####Change Log **v.1.1**:
+ **Add** Horse-tractor Mode.  
  - You can either use this mode (riding your horse and toggle on/off Tractor Mode) or normal mode (hold right click).  
  - To enable this mode, change **needHorse** in config.json file to 1 instead of 0.  
  - By enabling this mode, you can no longer use holding right click to enable Tractor Mode anymore.  
  - If you use this mode then make sure to set **tractorKey** (toggle hotkey) in config.json. Default is **B**.  
  - Have to be on a horse to toggle on/off Tractor Mode.  
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

###Demo:  
####Horse Mode [+ Horse to Tractor mod by Pewtershmitz](http://community.playstarbound.com/threads/tractor-v-1-3-horse-replacement.108604/) for maximum immersion :)
![gif-horse-mode](https://github.com/lambui/StardewValleyMod_TractorMod/blob/master/tractor2.gif)  

####Till Dirt  
![gif-till dirt](https://github.com/lambui/StardewValleyMod_TractorMod/blob/master/TillDirt.gif)  

####Water Crop    
![gif-water crop](https://github.com/lambui/StardewValleyMod_TractorMod/blob/master/water.gif)  

####Fertilize Soil    
![gif-fertilize](https://github.com/lambui/StardewValleyMod_TractorMod/blob/master/fertilizing.gif)  

####Sow Seeds      
![gif-sow seed](https://github.com/lambui/StardewValleyMod_TractorMod/blob/master/sowingSeed.gif)  


###Require:  
1. [Stardew Valley](http://store.steampowered.com/app/413150/)
2. [SMAPI: +0.40.1](https://github.com/ClxS/SMAPI/releases)

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
0 is using normal mode.  
1 is using horse mode.  

2. Change tractorKey to change hotkey.
Default is 0 (no hotkey) but you can change hotkey by changing the config.json file.  
If you don't use horse mode then this hotkey is useless. You can ignore it in that case.  
Check out KeyCode.txt file to find your desirable key code.   

3. Change WTFMode to enable pickaxe and axe.  
0 is axe, pickaxe disable.  
1 is axe, pickaxe enable.  

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
+ **Have fun farming! :)**

Contact me @ [buiphuonglamvn@gmail.com](mailto:buiphuonglamvn@gmail.com) regarding whatever.
