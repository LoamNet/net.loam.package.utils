using System;

namespace Loam
{
    /// <summary>
    /// An attribute for tagging messages to send through the Postmaster. 
    /// It's intended to contain meta information about a message like user-only notes/details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageMetadataAttribute : System.Attribute
    {
        // For more information on custom attributes:
        // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/creating-custom-attributes

        /// <summary>
        /// The user-facing name that shows in places the message dispatcher UI
        /// </summary>
        public string FriendlyName;

        /// <summary>
        /// A user facing note that doesn't impact message behavior. This note appears in the inspector in debug windows.
        /// </summary>
        public string Description;

        /// <summary>
        /// Controls whether or not this is visible in inspector event windows.
        /// </summary>
        public bool IsVisible;

        public MessageMetadataAttribute(string friendlyName, string description, bool isVisible)
        {
            this.FriendlyName = friendlyName;
            this.Description = description;
            this.IsVisible = isVisible;
        }
    }
}