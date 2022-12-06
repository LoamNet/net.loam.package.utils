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
        /// Optional user-facing friendly name. If not null or empty, this is the name that will display
        /// in the Message Viewer instead of the raw class name.
        /// </summary>
        public string FriendlyName;

        /// <summary>
        /// A user facing note that doesn't impact message behavior. This note appears in the inspector in debug windows.
        /// </summary>
        public string Description;

        /// <summary>
        /// Controls whether or not this is visible in inspector event windows. By default, all messages are visible.
        /// This does not impact your ability to listen for or send this message in code.
        /// </summary>
        public bool IsVisible;

        public MessageMetadataAttribute(string description, bool isVisible = true, string friendlyName = null)
        {
            this.FriendlyName = friendlyName;
            this.Description = description;
            this.IsVisible = isVisible;
        }
    }
}