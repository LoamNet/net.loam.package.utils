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

        /// <summary>
        /// Use as an opportunity for initialization
        /// </summary>
        private void Awake()
        {
            // Configure it with a new config for maximum logging
            PostmasterConfig config = new PostmasterConfig();
            config.ShowLogging = true;
            config.ShowWarnings = true;
            config.ShowErrors = true;
            Postmaster.Instance.Configure(config);
        }

        /// <summary>
        /// Registration of events
        /// </summary>
        private void Start()
        {
            Postmaster.Instance.Subscribe<DemoInteraction>(OnDemoInteraction);
        }

        private void Update()
        {
            Postmaster.Instance.Upkeep();
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
            // postmaster.Cleanup();
        }

        /// <summary>
        /// Demo of sending a message with additional code-specified data.
        /// </summary>
        private void ButtonClicked()
        {
            DemoInteraction demoInteraction = new DemoInteraction();
            demoInteraction.HasCustomData = true;
            Postmaster.Instance.Send<DemoInteraction>(demoInteraction);
        }

        /// <summary>
        /// Callback for an interaction
        /// </summary>
        /// <param name="msg"></param>
        private void OnDemoInteraction(Message msg)
        {
            DemoInteraction demo = msg as DemoInteraction;

            string adjective = demo.HasCustomData ? "customized" : "default";
            Debug.Log($"Callback recieved with {adjective} data");
        }
    }
}