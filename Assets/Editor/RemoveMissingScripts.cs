using UnityEngine;
using UnityEditor;

public class RemoveMissingScripts : MonoBehaviour
{
    [MenuItem("Tools/Remove Missing Scripts in Scene")]
    static void RemoveMissingScriptsInScene()
    {
        int count = 0;
        foreach (GameObject go in FindObjectsOfType<GameObject>())
        {
            int before = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (before > 0)
            {
                Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                count += before;
            }
        }
        Debug.Log($"Removed {count} missing scripts.");
    }
}
