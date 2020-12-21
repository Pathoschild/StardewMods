using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Storage
{
    /// <summary>A in-game chest which can provide or store items.</summary>
    internal class ChestContainer : IContainer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying chest.</summary>
        private readonly Chest Chest;


        /*********
        ** Accessors
        *********/
        /// <summary>The container name (if any).</summary>
        public string Name => this.Chest.Name;

        /// <summary>The raw mod data for the container.</summary>
        public ModDataDictionary ModData => this.Chest.modData;

        /// <summary>The location which contains the container.</summary>
        public GameLocation Location { get; }

        /// <summary>The tile area covered by the container.</summary>
        public Rectangle TileArea { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The underlying chest.</param>
        /// <param name="location">The location which contains the container.</param>
        /// <param name="tile">The tile area covered by the container.</param>
        /// <param name="migrateLegacyOptions">Whether to migrate legacy chest options, if applicable.</param>
        public ChestContainer(Chest chest, GameLocation location, Vector2 tile, bool migrateLegacyOptions = true)
        {
            this.Chest = chest;
            this.Location = location;
            this.TileArea = new Rectangle((int)tile.X, (int)tile.Y, 1, 1);

            if (migrateLegacyOptions)
                this.MigrateLegacyOptions();
        }

        /// <summary>Store an item stack.</summary>
        /// <param name="stack">The item stack to store.</param>
        /// <remarks>If the storage can't hold the entire stack, it should reduce the tracked stack accordingly.</remarks>
        public void Store(ITrackedStack stack)
        {
            if (stack.Count <= 0 || this.Chest.SpecialChestType == Chest.SpecialChestTypes.AutoLoader)
                return;

            IList<Item> inventory = this.GetInventory();

            // try stack into existing slot
            foreach (Item slot in inventory)
            {
                if (slot != null && stack.Sample.canStackWith(slot))
                {
                    Item sample = stack.Sample.getOne();
                    sample.Stack = stack.Count;
                    int added = stack.Count - slot.addToStack(sample);
                    stack.Reduce(added);
                    if (stack.Count <= 0)
                        return;
                }
            }

            // try add to empty slot
            int capacity = this.Chest.GetActualCapacity();
            for (int i = 0; i < capacity && i < inventory.Count; i++)
            {
                if (inventory[i] == null)
                {
                    inventory[i] = stack.Take(stack.Count);
                    return;
                }
            }

            // try add new slot
            if (inventory.Count < capacity)
                inventory.Add(stack.Take(stack.Count));
        }

        /// <summary>Find items in the pipe matching a predicate.</summary>
        /// <param name="predicate">Matches items that should be returned.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>If the pipe has no matching item, returns <c>null</c>. Otherwise returns a tracked item stack, which may have less items than requested if no more were found.</returns>
        public ITrackedStack Get(Func<Item, bool> predicate, int count)
        {
            ITrackedStack[] stacks = this.GetImpl(predicate, count).ToArray();
            if (!stacks.Any())
                return null;
            return new TrackedItemCollection(stacks);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ITrackedStack> GetEnumerator()
        {
            foreach (Item item in this.GetInventory().ToArray())
            {
                ITrackedStack stack = this.GetTrackedItem(item);
                if (stack != null)
                    yield return stack;
            }
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the chest inventory.</summary>
        private IList<Item> GetInventory()
        {
            return this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
        }

        /// <summary>Find items in the pipe matching a predicate.</summary>
        /// <param name="predicate">Matches items that should be returned.</param>
        /// <param name="count">The number of items to find.</param>
        /// <remarks>If there aren't enough items in the pipe, it should return those it has.</remarks>
        private IEnumerable<ITrackedStack> GetImpl(Func<Item, bool> predicate, int count)
        {
            int countFound = 0;
            foreach (Item item in this.GetInventory())
            {
                if (item != null && predicate(item))
                {
                    ITrackedStack stack = this.GetTrackedItem(item);
                    if (stack == null)
                        continue;

                    countFound += item.Stack;
                    yield return stack;
                    if (countFound >= count)
                        yield break;
                }
            }
        }

        /// <summary>Get a tracked item synced with the chest inventory.</summary>
        /// <param name="item">The item to track.</param>
        private ITrackedStack GetTrackedItem(Item item)
        {
            if (item == null)
                return null;

            try
            {
                return new TrackedItem(item, onEmpty: i => this.GetInventory().Remove(i));
            }
            catch (KeyNotFoundException)
            {
                return null; // invalid/broken item, silently ignore it
            }
            catch (Exception ex)
            {
                string error = $"Failed to retrieve item #{item.ParentSheetIndex} ('{item.Name}'";
                if (item is SObject obj && obj.preservedParentSheetIndex.Value >= 0)
                    error += $", preserved item #{obj.preservedParentSheetIndex.Value}";
                error += $") from container '{this.Chest.Name}' at {this.Location.Name} (tile: {this.TileArea.X}, {this.TileArea.Y}).";

                throw new InvalidOperationException(error, ex);
            }
        }

        /// <summary>Migrate legacy options stored in a chest name.</summary>
        private void MigrateLegacyOptions()
        {
            // get chest name
            string name = this.Chest.Name;
            if (name == null || name == "Chest" || !name.Contains("|"))
                return;

            // get tags
            MatchCollection matches = Regex.Matches(name, @"\|automate:[a-z\-]*\|");
            if (matches.Count == 0)
                return;

            // migrate to mod data
            void Set(string key, AutomateContainerPreference value) => this.Chest.modData[key] = value.ToString();
            foreach (Match match in matches)
            {
                switch (match.Groups["tag"].Value.ToLower())
                {
                    case "noinput":
                    case "no-store":
                        Set(AutomateContainerHelper.StoreItemsKey, AutomateContainerPreference.Disable);
                        break;

                    case "output":
                    case "prefer-store":
                        Set(AutomateContainerHelper.StoreItemsKey, AutomateContainerPreference.Prefer);
                        break;

                    case "nooutput":
                    case "no-take":
                        Set(AutomateContainerHelper.TakeItemsKey, AutomateContainerPreference.Disable);
                        break;

                    case "input":
                    case "prefer-take":
                        Set(AutomateContainerHelper.TakeItemsKey, AutomateContainerPreference.Prefer);
                        break;

                    case "ignore":
                        Set(AutomateContainerHelper.StoreItemsKey, AutomateContainerPreference.Disable);
                        Set(AutomateContainerHelper.TakeItemsKey, AutomateContainerPreference.Disable);
                        break;
                }

                name = name.Replace(match.Value, "");
            }
            this.Chest.Name = name.Trim();
        }
    }
}
