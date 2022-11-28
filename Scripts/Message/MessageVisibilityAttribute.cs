using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loam
{
    /// <summary>
    /// An attribute for tagging messages to send through the Messenger. 
    /// It's intended to contain meta information about a message like user-only notes/details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageVisibilityAttribute : System.Attribute
    {
        // For more information on custom attributes:
        // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/creating-custom-attributes

        /// <summary>
        /// A user facing note that doesn't impact message behavior. This note appears in the inspector in debug windows.
        /// </summary>
        public string Description;

        /// <summary>
        /// Controls whether or not this is visible in inspector event windows.
        /// </summary>
        public bool IsVisible;

        public MessageVisibilityAttribute(string description, bool isVisible)
        {
            this.Description = description;
            this.IsVisible = isVisible;
        }
    }
}