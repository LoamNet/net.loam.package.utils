using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loam.Internal.Demo
{
    [MessageMetadata("Demo Message", "This is a demo message called DemoInteraction. It contains some data (a bool)", isVisible: true)]
    public class DemoInteraction : Message
    {
        public bool HasCustomData = false;
    }

    public class MessageDemo : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;
        private Messenger messages;
        
        /// <summary>
        /// Use as an opportunity for initialization
        /// </summary>
        private void Awake()
        {
            // Create the messenger at some point. 
            messages = new Messenger();

            // Configure it with a new messenger config for maximum logging
            MessengerConfig config = new MessengerConfig();
            config.ShowLogging = true;
            config.ShowWarnings = true;
            config.ShowErrors = true;
            messages.Configure(config);
        }

        /// <summary>
        /// Registration of events
        /// </summary>
        private void Start()
        {
            messages.Register<DemoInteraction>(OnDemoInteraction);
        }

        private void OnEnable()
        {
            button.onClick.AddListener(ButtonClicked);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(ButtonClicked);
        }

        private void ButtonClicked()
        {
            DemoInteraction demoInteraction = new DemoInteraction();
            demoInteraction.HasCustomData = true;
            messages.Dispatch<DemoInteraction>(demoInteraction);
        }

        /// <summary>
        /// Callback for an interaction
        /// </summary>
        /// <param name="msg"></param>
        private void OnDemoInteraction(Message msg)
        {
            DemoInteraction ineraction = msg as DemoInteraction;
        }
    }
}