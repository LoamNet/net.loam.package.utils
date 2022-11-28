using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loam
{
    /// <summary>
    /// Represents a subscription to a specific type of event from the Messenger. 
    /// </summary>
    public class MessageSubscription : System.IDisposable
    {
        public System.Type MessageType { get; private set; }
        public Messenger Messenger { get; private set; }
        public Messenger.MessageCallback Callback { get; private set; }

        public MessageSubscription(Messenger.MessageCallback callback, System.Type type, Messenger messenger)
        {
            this.MessageType = type;
            this.Messenger = messenger;
            this.Callback = callback;
        }

        /// <summary>
        /// Clean up internal references and invalidate so anyone who
        /// cares can see that this handle is no longer valid.
        /// </summary>
        public void Dispose()
        {
            this.Messenger.Unregister(this);
            this.Callback = null;
            this.Messenger = null;

            return;
        }
    }
}
