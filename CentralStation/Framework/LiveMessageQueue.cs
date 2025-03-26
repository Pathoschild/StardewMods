using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;

namespace Pathoschild.Stardew.CentralStation.Framework
{
    /// <summary>Handles iterating through a set of messages loaded from a live source (which may change between messages) which can optionally be looped or shuffled.</summary>
    internal class LiveMessageQueue
    {
        /*********
        ** Fields
        *********/
        /// <summary>The messages returned since the last loop reset.</summary>
        private readonly PerScreen<HashSet<string>> SeenMessages = new(() => []);

        /// <summary>Whether to restart once all messages have been seen.</summary>
        private readonly bool Loop;

        /// <summary>Whether to randomize the message order.</summary>
        private readonly bool Shuffle;

        /// <summary>Fetch the messages from the live source.</summary>
        private readonly Func<IEnumerable<Message>> FetchMessages;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="loop"><inheritdoc cref="Loop" path="/summary"/></param>
        /// <param name="shuffle"><inheritdoc cref="Shuffle" path="/summary"/></param>
        /// <param name="fetchMessages"><inheritdoc cref="FetchMessages" path="/summary"/></param>
        public LiveMessageQueue(bool loop, bool shuffle, Func<IEnumerable<Message>> fetchMessages)
        {
            this.Loop = loop;
            this.Shuffle = shuffle;
            this.FetchMessages = fetchMessages;
        }

        /// <summary>Get the next message in the rotation.</summary>
        /// <param name="message">The next message to display.</param>
        /// <param name="hasMoreMessages">Whether there are more messages to show after this one (including repeats).</param>
        /// <returns>Returns whether a message was found.</returns>
        public bool TryGetNext([NotNullWhen(true)] out string? message, out bool hasMoreMessages)
        {
            // fetch all messages
            HashSet<string> seen = this.SeenMessages.Value;
            List<Message> messages = new(this.FetchMessages());

            // remove seen messages (unless all of them were seen)
            bool foundAny = false;
            {
                int lastIndex = messages.Count - 1;

                for (int i = lastIndex; i >= 0; i--)
                {
                    Message next = messages[i];
                    bool canShow = !string.IsNullOrWhiteSpace(next.Text) && !seen.Contains(next.Key);

                    if (canShow)
                    {
                        if (!foundAny && i < lastIndex)
                            messages.RemoveRange(i + 1, messages.Count - i - 1); // if this is the first valid message we find, remove all the invalid ones after it

                        foundAny = true;
                    }

                    else if (foundAny)
                        messages.RemoveAt(i);
                }
            }

            // reached the end: reset or end
            if (!foundAny)
            {
                if (!this.Loop || messages.Count == 0)
                {
                    message = null;
                    hasMoreMessages = false;
                    return false;
                }

                seen.Clear();
            }

            // get next message
            {
                Message next = this.Shuffle ? Game1.random.ChooseFrom(messages) : messages[0];
                seen.Add(next.Key);

                message = next.Text;
                hasMoreMessages = messages.Count > 1 || this.Loop;
                return true;
            }
        }

        /// <summary>A message from the data source.</summary>
        /// <param name="Key">A unique key for this message within the queue, used to track whether the message has been seen.</param>
        /// <param name="Text">The message text.</param>
        public record Message(string Key, string Text);
    }
}
