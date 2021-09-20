using UnityEngine;

namespace Loam
{
    public static class CharacterControllerExtensions
    {
        /// <summary>
        /// Given a target transform, position the character controller's game object
        /// based on the speciifed location. Optionally applies rotation and scale.
        /// 
        /// Because the character controller is external code, we need to explicitly sync the 
        /// transforms after adjusting them to keep behavior consistant. Other suggestions include
        /// manually enabling and disabling the character controller before and after, but because
        /// the character controller doesn't override OnEnable and OnDisable, it appears that Collider,
        /// which it inherits from, calls Physics.SyncTransforms() or some equivalent internally in OnEnable.
        /// https://forum.unity.com/threads/character-controller-ignores-transform-position.617107/
        /// </summary>
        /// <param name="this">The Character Controller</param>
        /// <param name="target">The transform to position ourselves at (and potentially rotate to match)</param>
        /// <param name="updateRotation">Option to update rotaton as well, defaults to false</param>
        /// <param name="updateLocalScale">Option to update local scale, defaults to false</param>
        public static void Warp(this CharacterController @this, Transform target, bool updateRotation = false, bool updateLocalScale = false)
        {
            Transform toUpdate = @this.gameObject.transform;
            
            toUpdate.position = target.position;

            if (updateRotation)
            {
                toUpdate.rotation = Quaternion.Euler(target.rotation.eulerAngles);
            }

            if(updateLocalScale)
            {
                toUpdate.localScale = target.localScale;
            }

            Physics.SyncTransforms();
        }


        /// <summary>
        /// Vector3 variant of Warp that updates only the position of the object the character
        /// controller is on and ensures that the transforms are synced.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="target"></param>
        /// <param name="updateRotation"></param>
        public static void Warp(this CharacterController @this, Vector3 targetPosition)
        {
            @this.gameObject.transform.position = targetPosition;
            Physics.SyncTransforms();
        }
    }
}