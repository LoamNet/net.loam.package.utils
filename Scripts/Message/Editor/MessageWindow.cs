#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

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
            
            if(entries == null)
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
                    MessageWindowEntry entry = new MessageWindowEntry(this, curType, typeAttribute);
                    entries.Add(entry);
                }
            }
        }

        /// <summary>
        /// Queues up a repaint.
        /// </summary>
        public void RequestRepaint()
        {
            needsRepaint = true;
        }

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
            for(int i = 0; i < entries.Count; ++i)
            {
                MessageWindowEntry entry = entries[i];
                entry.Render();
            }
            EditorGUILayout.EndScrollView();
        }

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