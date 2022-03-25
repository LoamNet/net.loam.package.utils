#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Loam.Editor
{
    /// <summary>
    /// Menu items that appear in right-click in 'Project' view and in Unity's Assets menu
    /// </summary>
    public static class AssetMenuItems
    {
        /// <summary>
        /// Attempt to find the GUID of the active object. If multiple are selected,
        /// then the first one selected is considered active. If something in-scene is
        /// selected, it will show up as active but won't have a valid path.
        /// </summary>
        [MenuItem("Assets/Loam/Copy .meta GUID to clipboard")]
        public static void MetaGUIDToClipboard()
        {
            Object active = Selection.activeObject;

            if (active != null)
            {
                string path = AssetDatabase.GetAssetPath(active);
                string guid = AssetDatabase.AssetPathToGUID(path);

                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(guid))
                {
                    GUIUtility.systemCopyBuffer = guid;
                    Debug.Log($"Copied GUID '{guid}' to clipboard from '{path}'");
                }
                else
                {
                    Debug.LogWarning($"Unable to copy GUID to clipboard for object '{active.name}' - Object likely not located in the assets directory.");
                }
            }
            else
            {
                Debug.LogWarning("Unable to copy GUID to clipboard - No item selected!");
            }
        }
    }
}
#endif