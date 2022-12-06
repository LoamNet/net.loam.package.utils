using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loam.Internal.Demo
{
    [MessageMetadata(
        friendlyName: "Demo Message",
        description: "This is a demo message called DemoInteraction. It contains some data.",
        isVisible: true)]
    public class DemoInteraction : Message
    {
        public bool HasCustomData = false;
    }

    public class MessageDemo : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;
        private Postmaster postmaster;
        
        /// <summary>
        /// Use as an opportunity for initialization
        /// </summary>
        private void Awake()
        {
            // Create the postmaster at some point. 
            postmaster = new Postmaster();

            // Configure it with a new config for maximum logging
            PostmasterConfig config = new PostmasterConfig();
            config.ShowLogging = true;
            config.ShowWarnings = true;
            config.ShowErrors = true;
            postmaster.Configure(config);
        }

        /// <summary>
        /// Registration of events
        /// </summary>
        private void Start()
        {
            postmaster.Subscribe<DemoInteraction>(OnDemoInteraction);
        }

        private void Update()
        {
            postmaster.Upkeep();
        }

        private void OnEnable()
        {
            button.onClick.AddListener(ButtonClicked);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(ButtonClicked);
        }

        private void OnDestroy()
        {
            postmaster.Dispose();
        }

        private void ButtonClicked()
        {
            DemoInteraction demoInteraction = new DemoInteraction();
            demoInteraction.HasCustomData = true;
            postmaster.Send<DemoInteraction>(demoInteraction);
        }

        /// <summary>
        /// Callback for an interaction
        /// </summary>
        /// <param name="msg"></param>
        private void OnDemoInteraction(Message msg)
        {
            DemoInteraction demo = msg as DemoInteraction;

            string adjective = demo.HasCustomData ? "custom " : "default";
            Debug.Log($"Callback recieved with {adjective} data");
        }
    }
}