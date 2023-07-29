using UnityEngine;

namespace Loam
{
    public static class CanvasGroupExtensions
    {
        /// <summary>
        /// Sets alpha, interactivity, and raycast targetable.
        /// Used primarily for changing UI visibility in situations where otherwise, 
        /// you'd set the object itself to inacive but don't want to incur the higher 
        /// cost and loss of update usability. Not a replacement for SetActive
        /// use for layout groups, etc
        /// 
        /// Conceptually, this is the user percieved activity of the 
        /// object being driven by the CanvasGroup component.
        /// </summary>
        /// <param name="this">Self, the current canvas group</param>
        /// <param name="isVisible">If the canvas group is or is not going to be effectively visible</param>
        public static void SetCanvasActive(this CanvasGroup @this, bool isVisible)
        {
            if(isVisible)
            {
                @this.alpha = 1;
                @this.interactable = true;
                @this.blocksRaycasts = true;
            }
            else
            {
                @this.alpha = 0;
                @this.interactable = false;
                @this.blocksRaycasts = false;
            }
        }
    }
}
