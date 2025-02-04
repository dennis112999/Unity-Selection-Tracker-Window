using System.Linq;
using UnityEngine;

namespace Dennis.Tools
{
#if UNITY_EDITOR

    using UnityEditor;

    public static class EditorWindowUtils
    {
        /// <summary>
        /// Finds an existing EditorWindow of the specified type that is not currently focused
        /// </summary>
        public static T GetWindowWithoutFocus<T>() where T : EditorWindow
        {
            return Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
        }
    }

#endif
}
