using UnityEngine;
using UnityEditor;

namespace OPZ.EditorTools
{
    public static class MissingScriptCleaner
    {
        [MenuItem("OPZ/Tools/Remove All Missing Scripts In Scene")]
        public static void CleanScene()
        {
            int removed = 0;
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (count > 0)
                {
                    Debug.Log("Removed " + count + " missing script(s) from: " + go.name);
                    removed += count;
                }
            }
            Debug.Log("[MissingScriptCleaner] Total removed: " + removed);
        }
    }
}
