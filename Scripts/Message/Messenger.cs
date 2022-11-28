using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loam
{
    public class Messenger
    {
        public delegate void MessageCallback(Message message);

        // Internal
        private Dictionary<System.Type, List<MessageSubscription>> lookup = new Dictionary<System.Type, List<MessageSubscription>>();
        private HashSet<MessageSubscription> toClean = new HashSet<MessageSubscription>();
        private MessengerConfig messengerConfig = MessengerConfig.Default();


        /// <summary>
        /// Allows construction of the messenger along with optional configuration
        /// </summary>
        /// <param name="debug"></param>
        public void Configure(MessengerConfig? config = null)
        {
            if(config.HasValue)
            {
                this.messengerConfig = config.Value;
            }
        }

        /// <summary>
        /// Given a type that derives from message and a callback, bind to all messages dispatched
        /// with the specified type.
        /// </summary>
        /// <typeparam name="T">The specific derived message type</typeparam>
        /// <param name="callback">the callback to get if dispatched</param>
        /// <returns>A handle to manage the registered callback</returns>
        public MessageSubscription Register<T>(MessageCallback callback) where T : Message
        {
            MessageSubscription handle = new MessageSubscription(callback, typeof(T), this);
            return handle;
        }

        /// <summary>
        /// Tags for disposal, avoiding duplicate tagging by using a hashset to track things we're removing.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public void Unregister(MessageSubscription handle)
        {
            toClean.Add(handle);
        }

        /// <summary>
        /// Goes through the removal list and destroys all handle subscriptions.
        /// This is intended to be called outside of any callbacks to prevent
        /// unintentionally clearing handles we're actively using/referencing.
        /// </summary>
        public void Upkeep()
        {
            if(toClean.Count <= 0)
            {
                return;
            }

            if (messengerConfig.ShowLogging)
            {
                Debug.Log($"Removing {toClean.Count} subscriptions");
            }

            foreach(MessageSubscription sub in toClean)
            {
                RemoveSubscription(sub);
            }

            toClean.Clear();
        }

        /// <summary>
        /// Given a single subscription, remove it from internal subscription list.
        /// This isn't intended to take into account any nested message calls,
        /// so should only be done when we're not in the middle of processing
        /// a bunch of messages.
        /// </summary>
        /// <param name="handle">The handle to remove from the list</param>
        private void RemoveSubscription(MessageSubscription handle)
        {
            if (lookup.TryGetValue(handle.MessageType, out List<MessageSubscription> callbackList))
            {
                bool wasRemoved = callbackList.Remove(handle);
                if (!wasRemoved)
                {
                    if (messengerConfig.ShowErrors)
                    {
                        Debug.LogError("Desync with subscriber list! Are you accidentally running this from within a message callback?");
                    }

                    return;
                }
            }
            else
            {
                if (messengerConfig.ShowErrors)
                {
                    Debug.LogError("Tried to remove something that doesn't exist! Are you accidentally running this from within a message callback?");
                }

                return;
            }
        }
    }
}