using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loam
{
    /// <summary>
    /// Broad, one-off functions live here.
    /// </summary>
    public static class LoamUtils
    {
        /// <summary>
        /// Clears out the developer console window present in the unity editor. If this is called 
        /// outside the editor, nothing happens. This is accomplished by sifting through the editor
        /// assembly to collect the `Clear()` function off the LogEntries class and calling it.
        /// </summary>
        public static void EditorClearConsole()
        {
#if UNITY_EDITOR
            // Fully qualify everything just in case to avoid namespace issues on more obscure platforms
            System.Type assemblyType = typeof(UnityEditor.Editor);
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(assemblyType);
            System.Type logEntriesType = assembly.GetType("UnityEditor.LogEntries");
            System.Reflection.MethodInfo clearMethod = logEntriesType.GetMethod("Clear");

            clearMethod.Invoke(new object(), null);
#endif
        }
    }
}
