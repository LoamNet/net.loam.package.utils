using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loam
{
    public class Messenger
    {
        public static Messenger Instance { get; private set; } // Restrict to one messenger at this time
        public delegate void MessageCallback(Message message);
        
        // Internal
        private Dictionary<System.Type, List<MessageSubscription>> lookup = new Dictionary<System.Type, List<MessageSubscription>>();
        private HashSet<MessageSubscription> toClean = new HashSet<MessageSubscription>();
        private MessengerConfig messengerConfig = MessengerConfig.Default();


        /// <summary>
        /// Configure 
        /// </summary>
        public Messenger()
        {
            if (Instance != null)
            {
                throw new System.Exception("You cannot have multiple Messenger instances");
            }
            else
            {
                Instance = this;
            }
        }

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
            System.Type type = typeof(T);
            MessageSubscription handle = new MessageSubscription(callback, type, this);

            if(lookup.TryGetValue(type, out List<MessageSubscription> objs))
            {
                objs.Add(handle);
            }
            else
            {
                List<MessageSubscription> subscriptionList = new List<MessageSubscription>();
                subscriptionList.Add(handle);
                lookup[type] = subscriptionList;
            }
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
        /// Dispatches a message of specified type. Prefer to use this.
        /// Events are dispatched to handles in the order the handles were registered.
        /// </summary>
        /// <typeparam name="T">The type of our message</typeparam>
        /// <param name="message">The instance of that message type to dispatch</param>
        public void Dispatch<T>(T message) where T : Message
        {
            if (lookup.TryGetValue(typeof(T), out List<MessageSubscription> subscriptions))
            {
                for (int i = 0; i < subscriptions.Count; ++i)
                {
                    MessageSubscription currentSubscription = subscriptions[i];
                    currentSubscription.Callback.Invoke(message);
                }
            }
        }

        /// <summary>
        /// Dispatches a message as an object, enforcing the idea that it's still a mesasge type.
        /// If it's not, we throw an exception - otherwise, we attempt to go through and dispatch
        /// events in order of registration.
        /// </summary>
        /// <param name="messageType">The underlying type of our message</param>
        /// <param name="toDispatch">The object containing our message</param>
        public void Dispatch(System.Type messageType, object toDispatch)
        {
            if(!messageType.IsAssignableFrom(toDispatch.GetType()))
            {
                throw new System.Exception("Provided type is not a Message or drived type");
            }

            if (lookup.TryGetValue(messageType, out List<MessageSubscription> subscriptions))
            {
                Message message = toDispatch as Message;
                for (int i = 0; i < subscriptions.Count; ++i)
                {
                    MessageSubscription currentSubscription = subscriptions[i];
                    currentSubscription.Callback.Invoke(message);
                }
            }
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