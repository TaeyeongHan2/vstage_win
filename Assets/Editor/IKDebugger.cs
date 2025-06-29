using UnityEngine;
using UnityEditor;

public class IKDebugger
{
    [MenuItem("Debug/Find All FinalIK Components")]
    public static void FindAllFinalIKComponents()
    {
        Debug.Log("Starting FinalIK component search...");
        
        // Find the debugger component
        var debugger = Object.FindFirstObjectByType<FinalIKDebug.FindFinalIKComponents>();
        if (debugger != null)
        {
            debugger.FindAllFinalIKComponents();
            debugger.CheckForGrounderIKErrors();
        }
        else
        {
            // Manual search if debugger not found
            MonoBehaviour[] allComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var component in allComponents)
            {
                string typeName = component.GetType().FullName;
                if (typeName.Contains("RootMotion.FinalIK") || typeName.Contains("GrounderIK") || typeName.Contains("VRIK"))
                {
                    Debug.Log($"Found FinalIK component: {typeName} on GameObject: {component.gameObject.name} at path: {GetGameObjectPath(component.transform)}");
                    Debug.Log($"Component enabled: {component.enabled}, GameObject active: {component.gameObject.activeInHierarchy}");
                }
            }
        }
        
        Debug.Log("FinalIK component search completed.");
    }
    
    private static string GetGameObjectPath(Transform transform)
    {
        if (transform.parent == null)
            return transform.name;
        return GetGameObjectPath(transform.parent) + "/" + transform.name;
    }
}