using UnityEngine;

namespace Loam
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Recurisively sets the layer of an object along with all of its children.
        /// </summary>
        /// <param name="this">The current game object and target of this extension method</param>
        /// <param name="layer">The layer to set this object and all its children to</param>
        public static void SetLayerRecursive(this GameObject @this, int layer)
        {
            @this.layer = layer;

            // NOTE: GetComponentsInChildren is a recursive call.
            // https://docs.unity3d.com/ScriptReference/GameObject.GetComponentsInChildren.html
            Transform[] children = @this.GetComponentsInChildren<Transform>();

            foreach (Transform childTransform in children)
            {
                childTransform.gameObject.layer = layer;
            }
        }
    }
}