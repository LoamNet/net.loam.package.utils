using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loam
{
    /// <summary>
    /// Represents a subscription to a specific type of event from the Postmaster. 
    /// </summary>
    public class MessageSubscription : System.IDisposable
    {
        public System.Type MessageType { get; private set; }
        public Postmaster Postmaster { get; private set; }
        public Postmaster.MessageCallback Callback { get; private set; }

        public MessageSubscription(Postmaster.MessageCallback callback, System.Type type, Postmaster postmaster)
        {
            this.MessageType = type;
            this.Postmaster = postmaster;
            this.Callback = callback;
        }

        /// <summary>
        /// Clean up internal references and invalidate so anyone who
        /// cares can see that this handle is no longer valid.
        /// </summary>
        public void Dispose()
        {
            this.Postmaster.Unsubscribe(this);
            this.Callback = null;
            this.Postmaster = null;

            return;
        }
    }
}
