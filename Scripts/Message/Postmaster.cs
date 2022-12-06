using System.Collections.Generic;
using UnityEngine;

namespace Loam
{
    /// <summary>
    /// The manager class in charge of sending messages, coordinating
    /// registering and unregistering to recieve messages of certain types.
    /// </summary>
    public class Postmaster
    {
        /// <summary>
        /// A bundle of data pairing extra info with a raw list of message subscriptions.
        /// Can better hold meta information about the subscription list for metrics, etc.
        /// </summary>
        public class SubscriptionBundle
        {
            public System.Numerics.BigInteger SendCount;         // Number of times someone has sent a message of this type
            public System.Numerics.BigInteger ListenerCallCount; // Number of callbacks executed total (mininum 1 per message send)
            public List<MessageSubscription> Subscriptions;

            public SubscriptionBundle()
            {
                SendCount = System.Numerics.BigInteger.Zero;
                ListenerCallCount = System.Numerics.BigInteger.Zero;
                Subscriptions = new List<MessageSubscription>();
            }
        }

        /// <summary>
        /// Indicates a function that can accept a callback from the Postmaster.
        /// </summary>
        /// <param name="message">The message object, potentially containing data.</param>
        public delegate void MessageCallback(Message message);
        
        // Internal
        private Dictionary<System.Type, SubscriptionBundle> lookup = new Dictionary<System.Type, SubscriptionBundle>();
        private HashSet<MessageSubscription> toClean = new HashSet<MessageSubscription>();
        private PostmasterConfig config = PostmasterConfig.Default();


        private static Postmaster instance;
        public static Postmaster Instance
        {
            get
            {
                if (!Application.isPlaying)
                {
                    if (instance == null)
                    {
                        instance = new Postmaster();
                    }
                }

                return instance;
            }

            private set
            {
                instance = value;
            }
        }

        /// <summary>
        /// Constructs the manager and restricts to a single instance
        /// </summary>
        public Postmaster()
        {
            if (instance != null)
            {
                throw new System.Exception("You cannot have multiple Postmaster instances");
            }
            else
            {
                instance = this;
            }
        }

        /// <summary>
        /// Applies the specified config file. Reasonable defaults are used otherwise.
        /// It's suggested this should be applied before usage begins, but that's not 
        /// strictly required.
        /// </summary>
        /// <param name="config">The new configuration for the postmaster to use</param>
        public void Configure(PostmasterConfig config)
        {
            this.config = config;
        }

        /// <summary>
        /// Given a type that derives from message and a callback, bind to all messages sent
        /// with the specified type.
        /// </summary>
        /// <typeparam name="T">The specific derived message type</typeparam>
        /// <param name="callback">the callback to get if a message of type T is sent</param>
        /// <returns>A handle to manage the registered callback</returns>
        public MessageSubscription Subscribe<T>(MessageCallback callback) where T : Message
        {
            System.Type type = typeof(T);
            return Subscribe(type, callback);
        }

        /// <summary>
        /// Given a type that derives from message and a callback, bind to all messages sent
        /// with the specified type.
        /// </summary>
        /// <param name="type">The specific derived message type</typeparam>
        /// <param name="callback">the callback to get if a message of specified type is sent</param>
        /// <returns>A handle to manage the registered callback</returns>
        public MessageSubscription Subscribe(System.Type type, MessageCallback callback)
        {
            MessageSubscription handle = new MessageSubscription(callback, type, this);

            if (lookup.TryGetValue(type, out SubscriptionBundle subscriptions))
            {
                subscriptions.Subscriptions.Add(handle);
            }
            else
            {
                SubscriptionBundle subscriptionList = new SubscriptionBundle();
                subscriptionList.Subscriptions.Add(handle);
                lookup[type] = subscriptionList;
            }
            return handle;
        }

        /// <summary>
        /// Tags for disposal, avoiding duplicate tagging by using a hashset to track things we're removing.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public void Unsubscribe(MessageSubscription handle)
        {
            toClean.Add(handle);
        }

        /// <summary>
        /// Sends a message of specified type. Prefer to use this.
        /// Messages are sent to callbacks in the order the callbacks were registered.
        /// </summary>
        /// <typeparam name="T">The type of our message</typeparam>
        /// <param name="message">The instance of that message type to send</param>
        public void Send<T>(T message) where T : Message
        {
            if (lookup.TryGetValue(typeof(T), out SubscriptionBundle targets))
            {
                targets.SendCount += 1;
                targets.ListenerCallCount += targets.Subscriptions.Count;

                for (int i = 0; i < targets.Subscriptions.Count; ++i)
                {
                    MessageSubscription currentSubscription = targets.Subscriptions[i];
                    currentSubscription.Callback.Invoke(message);
                }
            }
        }

        /// <summary>
        /// Sends a message as an object, enforcing the idea that it's still a message type.
        /// If it's not, we throw an exception - otherwise, we attempt to go through and send
        /// messages in order of registration.
        /// </summary>
        /// <param name="messageType">The underlying type of our message</param>
        /// <param name="toSend">The object containing our message</param>
        public void Send(System.Type messageType, object toSend)
        {
            if (lookup.TryGetValue(messageType, out SubscriptionBundle targets))
            {
                Message message = toSend as Message;
                targets.SendCount += 1;
                targets.ListenerCallCount += targets.Subscriptions.Count;

                for (int i = 0; i < targets.Subscriptions.Count; ++i)
                {
                    MessageSubscription currentSubscription = targets.Subscriptions[i];
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

            if (config.ShowLogging)
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
        /// Allows retrieval of the SubscriptionBundle stored in the internal lookup.
        /// It's not recommended you cache this or use it long term since the Postmaster manages it.
        /// </summary>
        /// <param name="type">The type of message to look up the bundle for</param>
        /// <param name="bundle">The to-be-assigned (or null if fails) subscription bundle associated with the message type</param>
        /// <returns>Returns if the bundle for the specified type was successfuly found</returns>
        public bool TryGetInternalSubscriptionBundle(System.Type type, out SubscriptionBundle bundle)
        {
            return lookup.TryGetValue(type, out bundle);
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
            if (lookup.TryGetValue(handle.MessageType, out SubscriptionBundle targets))
            {
                bool wasRemoved = targets.Subscriptions.Remove(handle);
                if (!wasRemoved)
                {
                    if (config.ShowErrors)
                    {
                        Debug.LogError("Desync with subscriber list! Are you accidentally running this from within a message callback?");
                    }

                    return;
                }
            }
            else
            {
                if (config.ShowErrors)
                {
                    Debug.LogError("Tried to remove something that doesn't exist! Are you accidentally running this from within a message callback?");
                }

                return;
            }
        }
    }
}