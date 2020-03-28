This document provides technical info about the Automate mod. **For more general info, see the
[README](README.md) file instead.**

## Contents
* [FAQs](#faqs)
* [Extensibility for modders](#extensibility-for-modders)
  * [APIs](#apis)
  * [Custom chest capacity](#custom-chest-capacity)
  * [Patch Automate](#patch-automate)
* [See also](#see-also)

## FAQs
### What's the order of processed machines?
See [_machine priority_ in the README](README.md#machine-priority).

### What's the order of items taken from chests?
**Note:** this shouldn't matter in most cases and it's subject to change.

For each machine, the available chests are combined into one inventory (so items may be taken from
multiple chests simultaneously) and then scanned until Automate finds enough items to fill a recipe
for that machine. The order is difficult to predict with multiple chests, but fairly easy if there's
only one connected chest.

For example, let's say you have one chest containing these item stacks:  
`coal`  
`3× copper ore`  
`3× iron ore`  
`2× copper ore`  
`2× iron ore`

A furnace has two recipes with those ingredients: `coal` + `5× copper ore` = `copper bar`, and
`coal` + `5× iron ore` = `iron bar`. Automate will scan the items from left to right and top to bottom,
and collect items until it has a complete recipe. In this case, the furnace will start producing a
copper bar:

1. Add `coal` from first stack (two unfinished recipes):  
   ❑ `coal` + `0 of 5× copper ore` = `copper bar`  
   ❑ `coal` + `0 of 5× iron ore` = `iron bar`
2. Add `3× copper ore` from second stack (two unfinished recipes):  
   ❑ `coal` + `3 of 5× copper ore` = `copper bar`  
   ❑ `coal` + `0 of 5× iron ore` = `iron bar`
3. Add `3× iron ore` from third stack (two unfinished recipes):  
   ❑ `coal` + `3 of 5× copper ore` = `copper bar`  
   ❑ `coal` + `3 of 5× iron ore` = `iron bar`
4. Add `2× copper ore` from fourth stack (one recipe filled):  
   ✓ `coal` + `5× copper ore` = `copper bar`  
   ❑ ~~`coal` + `3 of 5× iron ore` = `iron bar`~~

</details>

### Which chest will machine output go into?
The available chests are sorted by discovery order (which isn't predictable), then prioritised in
this order:

1. chests with the "Put items in this chest first" option (see [_in-game settings_ in the README](README.md#in-game-settings));
2. chests which already contain an item of the same type;
3. any chest.

### Can I change in-game settings without Chests Anywhere?
Normally you'd change how chests are automated through [Chests Anywhere](https://www.nexusmods.com/stardewvalley/mods/518)'s
chest options UI:
> ![](screenshots/chests-anywhere-config.png)

If you don't have Chests Anywhere or want to replicate it in another mod, you can edit the chest
name a different way and add these substrings:

<table>
<tr>
  <th>tag</th>
  <th>meaning</th>
</tr>
<tr>
  <td><code>|automate:no-store|</code></td>
  <td><strong>don't</strong> store items in this chest.</td>
</tr>
<tr>
  <td><code>|automate:no-take|</code></td>
  <td><strong>don't</strong> take items from this chest.</td>
</tr>
<tr>
  <td><code>|automate:prefer-store|</code></td>
  <td>store items in this chest first.</td>
</tr>
<tr>
  <td><code>|automate:prefer-take|</code></td>
  <td>take items from this chest first.</td>
</tr>
</table>

## Extensibility for modders
### APIs
Automate has a [mod-provided API](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations#Mod-provided_APIs)
you can use to add custom machines, containers, and connectors.

#### Basic concepts
These basic concepts are core to the Automate API:

<dl>
<dt>Entity</dt>
<dd>A placed item, terrain feature, building, or other in-game thing.</dd>

<dt>Connector</dt>
<dd>Something which can be added to a machine group (thus extending its range), but otherwise has
no logic of its own. It has no state, input, or output.</dd>

<dt>Container</dt>
<dd>Something which stores and retrieves items (usually a chest).</dd>

<dt>Machine</dt>
<dd>Something which has a state (e.g. empty or processing) and accepts input, provides output, or
both. This doesn't need to be a 'machine' in the gameplay sense; Automate provides default machines
for shipping bins and fruit trees, for example.</dd>

<dt>Machine group</dt>
<dd>

A set of machines, containers, and connectors which are chained together. You can press `U` in-game
(configurable) to see machine groups highlighted in green. For example, these are two machine groups:  
![](screenshots/extensibility-machine-groups.png)

</dd>
</dl>

#### Access the API
To access the API:

1. Add a reference to the `Automate.dll` file. Make sure it's [_not_ copied to your build output](https://github.com/Pathoschild/SMAPI/blob/develop/docs/mod-build-config.md#ignore-files).
2. Hook into [SMAPI's `GameLoop.GameLaunched` event](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Events#GameLoop.GameLaunched)
   and get a copy of the API:
   ```c#
   IAutomateAPI automate = this.Helper.ModRegistry.GetApi<IAutomateAPI>("Pathoschild.Automate");
   ```
3. Use the API to extend Automate (see below).

#### Add connectors, containers, and machines
You can add automatables by implementing an `IAutomationFactory`. Automate will handle the core
logic (like finding entities, linking automatables into groups, etc); you just need to return the
automatable for a given entity. You can't change the automation for an existing automatable though;
if Automate already has an automatable for an entity, it won't call your factory.

First, let's create a basic machine that transmutes an iron bar into gold in two hours:

```c#
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace YourModName
{
    /// <summary>A machine that turns iron bars into gold bars.</summary>
    public class TransmuterMachine : IMachine
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying entity.</summary>
        private readonly SObject Entity;


        /*********
        ** Accessors
        *********/
        /// <summary>The location which contains the machine.</summary>
        public GameLocation Location { get; }

        /// <summary>The tile area covered by the machine.</summary>
        public Rectangle TileArea { get; }

        /// <summary>A unique ID for the machine type.</summary>
        /// <remarks>This value should be identical for two machines if they have the exact same behavior and input logic. For example, if one machine in a group can't process input due to missing items, Automate will skip any other empty machines of that type in the same group since it assumes they need the same inputs.</remarks>
        string MachineTypeID { get; } = "YourModId/Transmuter";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="entity">The underlying entity.</param>
        /// <param name="location">The location which contains the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public TransmuterMachine(SObject entity, GameLocation location, in Vector2 tile)
        {
            this.Entity = entity;
            this.Location = location;
            this.TileArea = new Rectangle((int)tile.X, (int)tile.Y, 1, 1);
        }

        /// <summary>Get the machine's processing state.</summary>
        public MachineState GetState()
        {
            if (this.Entity.heldObject.Value == null)
                return MachineState.Empty;

            return this.Entity.readyForHarvest.Value
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public ITrackedStack GetOutput()
        {
            return new TrackedItem(this.Entity.heldObject.Value, onEmpty: item =>
            {
                this.Entity.heldObject.Value = null;
                this.Entity.readyForHarvest.Value = false;
            });
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool SetInput(IStorage input)
        {
            if (input.TryGetIngredient(SObject.ironBar, 1, out IConsumable ingredient))
            {
                ingredient.Take();
                this.Entity.heldObject.Value = new SObject(SObject.goldBar, 1);
                this.Entity.MinutesUntilReady = 120;
                return true;
            }

            return false;
        }
    }
}
```

Next, let's create a factory which returns the new machine. This example assumes you've added an
in-game object with ID 2000 that you want to automate.

```c#
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace YourModName
{
    public class MyAutomationFactory : IAutomationFactory
    {
        /// <summary>Get a machine, container, or connector instance for a given object.</summary>
        /// <param name="obj">The in-game object.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        public IAutomatable GetFor(SObject obj, GameLocation location, in Vector2 tile)
        {
            if (obj.ParentSheetIndex == 2000)
                return new TransmuterMachine(obj, location, tile);

            return null;
        }

        /// <summary>Get a machine, container, or connector instance for a given terrain feature.</summary>
        /// <param name="feature">The terrain feature.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        public IAutomatable GetFor(TerrainFeature feature, GameLocation location, in Vector2 tile)
        {
            return null;
        }

        /// <summary>Get a machine, container, or connector instance for a given building.</summary>
        /// <param name="building">The building.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        public IAutomatable GetFor(Building building, BuildableGameLocation location, in Vector2 tile)
        {
            return null;
        }

        /// <summary>Get a machine, container, or connector instance for a given tile position.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        /// <remarks>Shipping bin logic from <see cref="Farm.leftClick"/>, garbage can logic from <see cref="Town.checkAction"/>.</remarks>
        public IAutomatable GetForTile(GameLocation location, in Vector2 tile)
        {
            return null;
        }
    }
}
```

And finally, add your factory to the automate API (see [_access the API_](#access-the-api) above):

```c#
IAutomateAPI automate = ...;
automate.AddFactory(new MyAutomationFactory());
```

That's it! When Automate scans a location for automatables, it'll call your `GetFor` method and add
your custom machine to its normal automation.

### Custom chest capacity
If a `Chest` instance has a public `Capacity` property, Automate will use that instead of the
`Chest.capacity` constant.

### Patch Automate
You can patch Automate's logic [using Harmony](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Harmony).
To simplify patching, Automate wraps all machines with a [`MachineWrapper`](Framework/MachineWrapper.cs)
instance, so you can hook one place to change any machine's input, output, or processing logic. For
example, you can patch `GetOutput` to adjust machine output, `SetInput` to add custom recipes if
none of the vanilla recipes matched, etc.

**This is strongly discouraged in most cases.** Patching Automate makes both Automate and your mod
more fragile and likely to break. If you patch Automate, **please**:

1. When your mod starts, log a clear message indicating that your mod patches Automate. This
   simplifies troubleshooting and avoids confusion. For example:

   ```cs
   if (helper.ModRegistry.IsLoaded("Pathoschild.Automate"))
      this.Monitor.Log("This mod patches Automate. If you notice issues with Automate, make sure it happens without this mod before reporting it to the Automate page.", LogLevel.Debug);
   ```

   It doesn't have to be player-visible, even a `TRACE`-level message is useful when helping a
   player troubleshoot:

   ```cs
   if (helper.ModRegistry.IsLoaded("Pathoschild.Automate"))
      this.Monitor.Log("This mod patches Automate.", LogLevel.Trace);
   ```

2. Inside your patch methods, wrap the code in a `try..catch` and log your own exception (so
   players don't report errors on the Automate page). You should also fallback to running the
   original method, so errors in your code don't break Automate.

## See also
* [README](README.md) for general info
