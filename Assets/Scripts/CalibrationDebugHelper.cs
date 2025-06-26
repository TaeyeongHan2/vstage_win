using UnityEngine;
using RootMotion.Demos;

public class CalibrationDebugHelper : MonoBehaviour
{
    [ContextMenu("Check All Components")]
    void CheckComponents()
    {
        var vrikController = FindObjectOfType<VRIKCalibrationController>();
        var simpleCalib = FindObjectOfType<SimpleVRIKCalibration>();
        
        Debug.Log("=== Component Check ===");
        Debug.Log($"VRIKCalibrationController: {(vrikController != null ? "Found" : "Not Found")}");
        Debug.Log($"SimpleVRIKCalibration: {(simpleCalib != null ? "Found" : "Not Found")}");
        
        if (simpleCalib != null)
        {
            // 리플렉션으로 메서드 확인
            var type = simpleCalib.GetType();
            var methods = type.GetMethods();
            
            Debug.Log("Available methods in SimpleVRIKCalibration:");
            foreach (var method in methods)
            {
                if (method.DeclaringType == type)
                {
                    Debug.Log($"- {method.Name}()");
                }
            }
        }
    }
    
    [ContextMenu("Force Recompile Scripts")]
    void ForceRecompile()
    {
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.EditorUtility.RequestScriptReload();
        Debug.Log("Script reload requested!");
        #endif
    }
}