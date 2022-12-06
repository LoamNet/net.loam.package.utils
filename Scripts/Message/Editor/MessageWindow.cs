#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;

namespace Loam
{
    /// <summary>
    /// Window for firing existing events
    /// 
    /// https://docs.unity3d.com/ScriptReference/EditorWindow.html
    /// </summary>
    public class MessageWindow : EditorWindow
    {
        // Configurables
        public const string WINDOW_TITLE = "Message Viewer";
        public const string WINDOW_PATH = "Window/" + WINDOW_TITLE;
        public const float ACTIVITY_INDICATOR_FADE_TIME_SECONDS = 0.7f;
        public static readonly Color ACTIVITY_INDICATOR_COLOR = new Color(0f / 255f, 128f / 255f, 255f / 255f, 100f / 100f);

        // Internal
        private float lastTime;
        private bool needsRepaint;
        private static bool needsReload;
        private Vector2 scrollView;
        private List<MessageWindowEntry> entries = new List<MessageWindowEntry>();

        MultiColumnHeader columnHeader;
        MultiColumnHeaderState.Column[] columns;

        /// <summary>
        /// Callback that runs when the entry for this window is selected in the menu.
        /// Intended to get or create the message window and prevent duplicates.
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
        /// Callback that runs when the assembly reloads scripts.
        /// Intended to flag that a reload is required.
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
#pragma warning disable IDE0051 // Suppress "Remove unused private members"
        private static void OnScriptsReloaded()
#pragma warning restore IDE0051 // Restore "Remove unused private members"
        {
            needsReload = true;
        }

        /// <summary>
        /// A non-static initialization function for the window, run before the window is shown.
        /// </summary>
        public void Initialize()
        {
            Cleanup();

            if (entries == null)
            {
                entries = new List<MessageWindowEntry>();
            }

            CollectAllAttributes();
            lastTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Goes through all the assemblies looking for meta message attributes.
        /// </summary>
        private void CollectAllAttributes()
        {
            for(int i = 0; i < entries.Count; ++i)
            {
                MessageWindowEntry entry = entries[i];
                entry.Dispose();
            }
            entries.Clear();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i)
            {
                Assembly assembly = assemblies[i];
                CollectAttributes(assembly);
            }

            Alphabetize(entries);
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
                    if (typeAttribute.IsVisible)
                    {
                        MessageWindowEntry entry = new MessageWindowEntry(this, curType, typeAttribute);
                        entries.Add(entry);
                    }
                }
            }
        }

        /// <summary>
        /// Sorts the specified entry list alphabetically
        /// </summary>
        /// <param name="toSort">The list to be sorted</param>
        private void Alphabetize(List<MessageWindowEntry> toSort)
        {
            toSort.Sort((left, right) => {
                return left.MessageName.CompareTo(right.MessageName);
            });
        }

        /// <summary>
        /// Queues up a repaint.
        /// </summary>
        public void RequestRepaint()
        {
            needsRepaint = true;
        }

        /// <summary>
        /// Processes reload, repaint, and other data and visual updates as applicable.
        /// When the editor isn't playing, provides upkeep for events.
        /// </summary>
        private void Update()
        {
            if(needsReload)
            {
                Initialize();
                needsReload = false;
                needsRepaint = false;
            }

            if(needsRepaint)
            {
                Repaint();
                needsRepaint = false;
            }

            if(!Application.isPlaying)
            {
                Postmaster.Instance.Upkeep();
            }

            float curTime = Time.realtimeSinceStartup;
            float dt = curTime - lastTime;
            lastTime = curTime;
            
            for(int i = 0; i < entries.Count; ++i)
            {
                entries[i].Update(dt);
            }
        }

        /// <summary>
        /// Drawing everything
        /// </summary>
        void OnGUI()
        {
            scrollView = EditorGUILayout.BeginScrollView(scrollView);

            for (int i = 0; i < entries.Count; ++i)
            {
                MessageWindowEntry entry = entries[i];

                GUILayout.BeginHorizontal();
                WindowGUILine(entry);
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }

            // Display last session warning if applicable
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Seeing data from your last play session? You probably forgot to call Dispose() on Postmaster during shutdown. If you're calling Dispose() but seeing more than one subscription (this window), then you're not unsubscribing some events. You can intentionally avoid calling Dispose during debugging to get a snapshot of your usage.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Given a specific entry, call the required layout calls to place it.
        /// </summary>
        /// <param name="entry">The window entry we're going to create a line for</param>
        private void WindowGUILine(MessageWindowEntry entry)
        {
            // Collect data
            Postmaster postmaster = Postmaster.Instance;
            System.Type messageType = entry.MessageType;
            bool foundBundle = postmaster.TryGetInternalSubscriptionBundle(messageType, out Postmaster.SubscriptionBundle bundle);

            GUI.enabled = Application.isPlaying;
            // Button to manually activate
            if (GUILayout.Button(new GUIContent("Send", "[Create and Send Message]\n\nConstructs the message with its default constructor and sends it via the postmaster.")))
            {
                // Constructs our message type with a default constructor then sends a message of that type. 
                // https://learn.microsoft.com/en-us/dotnet/api/system.activator.createinstance
                object obj = Activator.CreateInstance(messageType);
                postmaster.Send(messageType, obj);
            }
            GUI.enabled = true;

            // Configure settings
            GUI.skin.box.wordWrap = false; 
            GUI.skin.box.padding = new RectOffset(2, 2, 0, 0);
            GUI.skin.box.alignment = TextAnchor.MiddleLeft;

            // Display the name of the message 
            GUILayout.Box(new GUIContent(entry.MessageFriendlyName, $"[Message Name]\n\nThe user-provided friendly name for the message. If a friendly name wasn't specified, the class name is used instead.\n• Friendly Name: {entry.MessageFriendlyName}\n• Class Name: {entry.MessageName}\n• Description: \"{entry.MessageDescription}\""), GUILayout.Width(120));

            if (foundBundle)
            {
                // Display sent message counter and show activity as needed. Include additional notes on how many subscribers we've had.
                GUILayout.Box(new GUIContent($"{bundle.SendCount}", $"[Sent Message Count]\n\nThe number of times this messages has been sent\n\n• Messages Sent: {bundle.SendCount}\n• Total Calls: {bundle.ListenerCallCount}"), GUILayout.Width(48));
                Rect lastRect = GUILayoutUtility.GetLastRect();
                Color color = ACTIVITY_INDICATOR_COLOR;
                color.a = entry.ActivityValueCurrent;
                EditorGUI.DrawRect(lastRect, color);

                // Add the subscriber count
                GUILayout.Box(new GUIContent($"{bundle.Subscriptions.Count}", $"[Subscriber Count]\n\nThe number of people have subscribed to this message.\n\n• Subscribers: {bundle.Subscriptions.Count}"), GUILayout.Width(24));
            }

            // Add the description
            GUILayout.Label(new GUIContent(entry.MessageDescription, $"[User Description]\n\n\"{entry.MessageDescription}\""));
        }

        /// <summary>
        /// Disposes of and cleans out window variables. You should be able to initialize after calling cleanup.
        /// </summary>
        void Cleanup()
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Count; ++i)
                {
                    MessageWindowEntry entry = entries[i];
                    entry.Dispose();
                }

                entries.Clear();
            }
        }

        /// <summary>
        /// Clean up any visualized subscriptions, etc
        /// </summary>
        private void OnDestroy()
        {
            Cleanup();
        }

    }
}

#endif