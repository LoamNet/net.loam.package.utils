using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loam
{
    /// <summary>
    /// Provides configuration for the Messenger class.
    /// </summary>
    public struct MessengerConfig
    {
        public bool ShowLogging;
        public bool ShowWarnings;
        public bool ShowErrors;

        /// <summary>
        /// Constructs the MessengerConfig with resonable defaults and returns it.
        /// These defaults are what's used when nothing else is specified by a user.
        /// </summary>
        /// <returns>A config of default behaviors and values</returns>
        public static MessengerConfig Default()
        {
            // Note: Initializer list is syntatic sugar only produces different IL during debug compilation, release
            // generates the same code and therefore the same performance. Vanilla assignment used because it's the most
            // clear. Check out the IL perf at https://stackoverflow.com/questions/1509983/object-initializer-performance.

            MessengerConfig config = new MessengerConfig();
            config.ShowLogging = false;
            config.ShowWarnings = true;
            config.ShowErrors = true;

            return config;
        }
    }
}