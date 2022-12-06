#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Loam
{
    /// <summary>
    /// Window for firing existing events
    /// 
    /// https://docs.unity3d.com/ScriptReference/EditorWindow.html
    /// </summary>
    public class MessageWindow : EditorWindow
    {
        // Static configurables
        private const string WINDOW_TITLE = "Message Viewer";
        private const string WINDOW_PATH = "Window/" + WINDOW_TITLE;

        // Internal
        private Vector2 scrollView;
        private List<MessageEntry> entries = new List<MessageEntry>();

        private static Messenger GetOrCreateMessenger()
        {
            if(Messenger.Instance == null)
            {
                new Messenger();
            }

            return Messenger.Instance;
        }



        /// <summary>
        /// An entry in the events window 
        /// </summary>
        public class MessageEntry
        {
            private System.Type messageType;
            private MessageMetadataAttribute messageAttirbute;
            private string messageName;
            private string messageDescription;
            private float activityValueCurrent;
            private float activityValueReset;
            private int activationCount;

            public MessageEntry(System.Type messageType, MessageMetadataAttribute messageAttirbute)
            {
                this.messageType = messageType;
                this.messageAttirbute = messageAttirbute;

                this.messageName = messageType.Name;
                this.messageDescription = messageAttirbute.Description;
                this.activityValueCurrent = 0;
                this.activityValueReset = 1f;
                this.activationCount = 0;

                GetOrCreateMessenger().Register<Internal.Demo.DemoInteraction>(OnDemo);
            }

            private void OnDemo(Message msg)
            {
                activityValueCurrent = activityValueReset;
                ++activationCount;
            }

            public void Render()
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Dispatch"))
                {
                    // Constructs our message type with a default constructor.
                    // This then dispatches the message of that type.
                    // https://learn.microsoft.com/en-us/dotnet/api/system.activator.createinstance
                    object obj = Activator.CreateInstance(messageType);
                    GetOrCreateMessenger().Dispatch(messageType, obj);
                }

                GUILayout.Label(messageName);
                GUILayout.Label(messageDescription);
                GUILayout.Label($"{activationCount}");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Must be static. This is the function that runs when you select the entry for
        /// this window in the menu. Doesn't matter the name. This is intended to get
        /// or create the message window (and not have multiple up)
        /// </summary>
        [MenuItem(WINDOW_PATH)]
#pragma warning disable IDE0051 // Suppress "Remove unused private members"
        private static void ShowWindow()
#pragma warning restore IDE0051 // Restore "Remove unused private members"
        {
            MessageWindow window = (MessageWindow)EditorWindow.GetWindow(typeof(MessageWindow));
            window.titleContent.text = WINDOW_TITLE;
            window.Initialize();
            window.Show();
        }

        /// <summary>
        /// A non-static initialization function for the window, run before the window is shown.
        /// </summary>
        public void Initialize()
        {
            CollectAllAttributes();
        }

        /// <summary>
        /// Goes through all the assemblies looking for meta message attributes.
        /// </summary>
        private void CollectAllAttributes()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i)
            {
                Assembly assembly = assemblies[i];
                CollectAttributes(assembly);
            }
        }

        /// <summary>
        /// Given an assembly, find all of the message meta attributes required
        /// and add them to our list of entries.
        /// </summary>
        private void CollectAttributes(Assembly assembly)
        {
            System.Type[] allTypesInAssembly = assembly.GetTypes();
            for (int j = 0; j < allTypesInAssembly.Length; ++j)
            {
                System.Type curType = allTypesInAssembly[j];
                System.Attribute attr = curType.GetCustomAttribute(typeof(MessageMetadataAttribute), true);
                if (attr != null)
                {
                    MessageMetadataAttribute typeAttribute = attr as MessageMetadataAttribute;
                    MessageEntry entry = new MessageEntry(curType, typeAttribute);
                    entries.Add(entry);
                }
            }
        }

        /// <summary>
        /// Drawing everything
        /// </summary>
        void OnGUI()
        {
            scrollView = EditorGUILayout.BeginScrollView(scrollView);
            for(int i = 0; i < entries.Count; ++i)
            {
                MessageEntry entry = entries[i];
                entry.Render();
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Clean up any visualized subscriptions, etc
        /// </summary>
        private void OnDestroy()
        {
            for (int i = 0; i < entries.Count; ++i)
            {
                MessageEntry entry = entries[i];

            }
        }
    }
}

#endif