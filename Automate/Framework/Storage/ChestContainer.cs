using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
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

        /// <summary>Get the number of items that can be stored in the chest.</summary>
        private readonly Func<int> Capacity;


        /*********
        ** Accessors
        *********/
        /// <summary>The container name (if any).</summary>
        public string Name
        {
            get => this.Chest.Name;
            private set => this.Chest.Name = value;
        }

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
        /// <param name="reflection">An API for accessing inaccessible code.</param>
        public ChestContainer(Chest chest, GameLocation location, Vector2 tile, IReflectionHelper reflection)
        {
            // save metadata
            this.Chest = chest;
            this.Location = location;
            this.TileArea = new Rectangle((int)tile.X, (int)tile.Y, 1, 1);
            this.Name = this.MigrateLegacyOptions(this.Name);

            // get capacity
            IReflectedProperty<int> capacity = reflection.GetProperty<int>(chest, "Capacity", required: false); // let mods like MegaStorage override capacity
            if (capacity != null)
                this.Capacity = capacity.GetValue;
            else
                this.Capacity = () => Chest.capacity;
        }

        /// <summary>Store an item stack.</summary>
        /// <param name="stack">The item stack to store.</param>
        /// <remarks>If the storage can't hold the entire stack, it should reduce the tracked stack accordingly.</remarks>
        public void Store(ITrackedStack stack)
        {
            if (stack.Count <= 0)
                return;

            IList<Item> inventory = this.Chest.items;

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
            int capacity = this.Capacity();
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
            foreach (Item item in this.Chest.items.ToArray())
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
        /// <summary>Find items in the pipe matching a predicate.</summary>
        /// <param name="predicate">Matches items that should be returned.</param>
        /// <param name="count">The number of items to find.</param>
        /// <remarks>If there aren't enough items in the pipe, it should return those it has.</remarks>
        private IEnumerable<ITrackedStack> GetImpl(Func<Item, bool> predicate, int count)
        {
            int countFound = 0;
            foreach (Item item in this.Chest.items)
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
                return new TrackedItem(item, onEmpty: i => this.Chest.items.Remove(i));
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
        /// <param name="name">The chest name to migrate.</param>
        private string MigrateLegacyOptions(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !Regex.IsMatch(name, @"\|automate:(?:ignore|input|output|noinput|nooutput)\|"))
                return name;

            // migrate renamed tags
            name = name
                .Replace("|automate:noinput|", "|automate:no-store|")
                .Replace("|automate:output|", "|automate:prefer-store|")
                .Replace("|automate:nooutput|", "|automate:no-take|")
                .Replace("|automate:input|", "|automate:prefer-take|");

            // migrate removed tags
            if (name.Contains("|automate:ignore|"))
            {
                string newTag = "";
                foreach (string tag in new[] { "|automate:no-store|", "|automate:no-take|" })
                {
                    if (!name.Contains(tag))
                        newTag = $"{newTag} {tag}".Trim();
                }

                name = name.Replace("|automate:ignore|", newTag);
            }

            // normalize
            name = Regex.Replace(name, @"\| +\|", "| |");
            return name.Trim();
        }
    }
}
