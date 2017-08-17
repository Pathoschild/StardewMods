# Customize:  
#### Everything is in config.json file
+ Tool (explaint by info in config.json):  
To add a tool, add a block of code at the end of the last block of code that look like this (separate these blocks with comma)  
As long as you have the *name* variable, the config.json will auto generate other variables with default value for you  
```
> example:
{
  "name": "Tool Name Here",
  "minLevel": numerical value (default value 0),
  "effectRadius": another numberical value (default value 1),
  "actionEveryTickAmount": yet another numberical value (default value 1)
}

> explanation:
name: tool name, be sure it exactly matches ingame tool name, case sensitive.
minLevel: the minimum tool upgrade required to use it with Tractor Mode. Ex: 0 = basic tool, 1 = copper tool...
effectRadius: the radius of effect when use in Tractor Mode.             Ex: 1 = 3x3 grid, 2 = 5x5 grid...
actionEveryTickAmount: number of game tick cool down after each tool action in Tractor Mode.
                       this is for optimization. If you notice any drop in fps while using tool with Tractor Mode,
                       consider increasing this value until fps drop is gone.
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

+ Change PhoneKey to change hotkey for your cellphone.  
Default is N but you can change hotkey by changing the config.json file.  
Check out KeyCode.txt file to find your desirable key code.   

+ Change TractorHousePrice to change Garage building price.  
Default is 150000 (150.000 g) 

+ updateConfig sets hotkey for applying changes in config.json live.  
Default is P but you can change hotkey by changing the config.json file.  
Check out KeyCode.txt file to find your desirable key code.

  1. Make changes to config.json while playing, save it.  
  2. Get ingame and press updateConfig hotkey. Now your config.json changes are live ingame.
