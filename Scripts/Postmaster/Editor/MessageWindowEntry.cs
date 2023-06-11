#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Loam
{
    /// <summary>
    /// An entry in the events window 
    /// </summary>
    public class MessageWindowEntry : IDisposable
    {
        // Collected info
        public MessageWindow Window { get; private set; }
        public System.Type MessageType { get; private set; }
        public string MessageName { get; private set; }
        public string MessageFriendlyName { get; private set; }
        public string MessageDescription { get; private set; }
        public float ActivityValueCurrent { get; private set; }

        // Internal
        private MessageSubscription handle;

        /// <summary>
        /// Constructs the object, collecting and caching relevant info for drawing it in the UI
        /// </summary>
        /// <param name="messageType">The underlying type of the message</param>
        /// <param name="messageAttirbute">The name of the message</param>
        public MessageWindowEntry(MessageWindow window, System.Type messageType, MessageMetadataAttribute messageAttirbute)
        {
            this.Window = window;
            this.MessageType = messageType;

            this.MessageName = messageType.Name;
            this.MessageFriendlyName = string.IsNullOrWhiteSpace(messageAttirbute.FriendlyName) ? MessageName : messageAttirbute.FriendlyName;
            this.MessageDescription = messageAttirbute.Description;
            this.ActivityValueCurrent = 0;

            if (Application.isPlaying && Postmaster.Instance != null)
            { 
                this.handle = Postmaster.Instance.Subscribe(messageType, OnEventCallback);
            }
        }

        /// <summary>
        /// The subscription to in the message viewer so that we can monitor activity.
        /// If the message isn't subscribed 
        /// </summary>
        /// <param name="msg">The message instance</param>
        private void OnEventCallback(Message msg)
        {
            Window.RequestRepaint();
            ActivityValueCurrent = MessageWindow.ACTIVITY_INDICATOR_FADE_TIME_SECONDS;
        }

        public void Update(float dt)
        {
            // Decrement color as needed with a lower bound of 0.
            if (ActivityValueCurrent > 0)
            {
                ActivityValueCurrent -= dt;
                ActivityValueCurrent = Mathf.Max(ActivityValueCurrent, 0);
                Window.RequestRepaint();
            }
        }

        public void Dispose()
        {
            handle?.Dispose();
        }
    }
}
#endif