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
        // Collected
        private MessageWindow window;
        private System.Type messageType;

        private string messageName;
        private string messageDescription;
        private float activityValueCurrent;

        // Generated 
        private MessageSubscription handle;

        /// <summary>
        /// Constructs the object, collecting and caching relevant info for drawing it in the UI
        /// </summary>
        /// <param name="messageType">The underlying </param>
        /// <param name="messageAttirbute"></param>
        public MessageWindowEntry(MessageWindow window, System.Type messageType, MessageMetadataAttribute messageAttirbute)
        {
            this.window = window;
            this.messageType = messageType;

            this.messageName = messageType.Name;
            this.messageDescription = messageAttirbute.Description;
            this.activityValueCurrent = 0;

            handle = Postmaster.Instance.Register(messageType, OnEventCallback);
        }

        private void OnEventCallback(Message msg)
        {
            window.RequestRepaint();
            activityValueCurrent = MessageWindow.ACTIVITY_INDICATOR_FADE_TIME_SECONDS;
        }

        public void Update(float dt)
        {
            // Decrement color as needed with a lower bound of 0.
            if (activityValueCurrent > 0)
            {
                activityValueCurrent -= dt;
                activityValueCurrent = Mathf.Max(activityValueCurrent, 0);
                window.RequestRepaint();
            }
        }

        public void Render()
        {
            Postmaster postmaster = Postmaster.Instance;
            GUILayout.BeginHorizontal();

            // Activity light/box
            GUILayout.Box(" ", GUILayout.Height(15), GUILayout.Width(15));
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Color color = MessageWindow.ACTIVITY_INDICATOR_COLOR;
            color.a = activityValueCurrent;
            EditorGUI.DrawRect(lastRect, color);

            // Button to manually activate
            if (GUILayout.Button("Dispatch"))
            {
                // Constructs our message type with a default constructor.
                // This then dispatches the message of that type. 
                // https://learn.microsoft.com/en-us/dotnet/api/system.activator.createinstance
                object obj = Activator.CreateInstance(messageType);
                postmaster.Dispatch(messageType, obj);
            }

            bool foundBundle = postmaster.TryGetInternalSubscriptionBundle(messageType, out Postmaster.SubscriptionBundle bundle);
            if (foundBundle)
            {
                GUILayout.Label($"{bundle.DispatchCount}");
                GUILayout.Label($"{bundle.ListenerCallCount}");
                GUILayout.Label($"{bundle.Subscriptions.Count}");
            }

            GUILayout.Label(messageName);
            GUILayout.Label(messageDescription);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public void Dispose()
        {
            handle.Dispose();
        }
    }
}
#endif