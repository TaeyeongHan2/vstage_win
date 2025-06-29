using UnityEngine;
using UnityEditor;
using RootMotion.FinalIK;

public class GrounderIKFixerEditor
{
    [MenuItem("Tools/FinalIK/Fix GrounderIK Issues")]
    public static void FixGrounderIKIssues()
    {
        Debug.Log("=== Starting GrounderIK Issue Analysis ===");
        
        // 1. 모든 GrounderIK 컴포넌트 찾기
        GrounderIK[] grounderIKs = Object.FindObjectsByType<GrounderIK>(FindObjectsSortMode.None);
        Debug.Log($"Found {grounderIKs.Length} GrounderIK components");
        
        if (grounderIKs.Length == 0)
        {
            Debug.Log("No GrounderIK components found in the scene");
            return;
        }
        
        foreach (var grounderIK in grounderIKs)
        {
            Debug.Log($"\n--- Analyzing GrounderIK on: {grounderIK.gameObject.name} ---");
            
            // 2. VRIK 컴포넌트 확인
            VRIK vrik = grounderIK.GetComponent<VRIK>();
            if (vrik != null)
            {
                Debug.LogWarning($"❌ INCOMPATIBILITY: GrounderIK is used with VRIK on {grounderIK.gameObject.name}");
                Debug.LogWarning("GrounderIK is NOT compatible with VRIK (IKSolverVR)");
                Debug.Log("Solutions:");
                Debug.Log("  1. Replace GrounderIK with GrounderFBBIK (recommended for VRIK)");
                Debug.Log("  2. Replace VRIK with FBBIK and keep GrounderIK");
                Debug.Log("  3. Remove GrounderIK if ground adaptation is not needed");
            }
            
            // 3. FBBIK 컴포넌트 확인
            FullBodyBipedIK fbbik = grounderIK.GetComponent<FullBodyBipedIK>();
            if (fbbik != null)
            {
                Debug.Log($"✅ COMPATIBLE: GrounderIK is properly used with FBBIK on {grounderIK.gameObject.name}");
            }
            
            // 4. solver 확인
            if (grounderIK.solver == null)
            {
                Debug.LogError($"❌ ERROR: GrounderIK solver is NULL on {grounderIK.gameObject.name}");
            }
            else
            {
                Debug.Log($"Solver type: {grounderIK.solver.GetType().Name}");
            }
            
            // 5. 컴포넌트 상태 확인
            Debug.Log($"Enabled: {grounderIK.enabled}");
            Debug.Log($"GameObject active: {grounderIK.gameObject.activeInHierarchy}");
        }
        
        Debug.Log("\n=== Analysis Complete ===");
    }
    
    [MenuItem("Tools/FinalIK/Replace GrounderIK with GrounderFBBIK")]
    public static void ReplaceGrounderIKWithGrounderFBBIK()
    {
        GrounderIK[] grounderIKs = Object.FindObjectsByType<GrounderIK>(FindObjectsSortMode.None);
        int replacedCount = 0;
        
        foreach (var grounderIK in grounderIKs)
        {
            VRIK vrik = grounderIK.GetComponent<VRIK>();
            if (vrik != null)
            {
                Debug.Log($"Replacing GrounderIK with GrounderFBBIK on {grounderIK.gameObject.name}");
                
                // 기존 설정 저장
                float weight = grounderIK.weight;
                bool enabled = grounderIK.enabled;
                
                // GrounderFBBIK 추가
                GrounderFBBIK grounderFBBIK = grounderIK.gameObject.AddComponent<GrounderFBBIK>();
                grounderFBBIK.weight = weight;
                grounderFBBIK.enabled = enabled;
                
                // 기존 GrounderIK 제거
                Object.DestroyImmediate(grounderIK);
                
                replacedCount++;
                Debug.Log($"✅ Successfully replaced GrounderIK with GrounderFBBIK on {grounderFBBIK.gameObject.name}");
            }
        }
        
        Debug.Log($"Replaced {replacedCount} GrounderIK components with GrounderFBBIK");
        
        if (replacedCount > 0)
        {
            EditorUtility.SetDirty(EditorUtility.InstanceIDToObject(0)); // Mark scene as dirty
        }
    }
    
    [MenuItem("Tools/FinalIK/Remove All GrounderIK Components")]
    public static void RemoveAllGrounderIKComponents()
    {
        if (!EditorUtility.DisplayDialog("Remove GrounderIK Components", 
            "Are you sure you want to remove ALL GrounderIK components from the scene?", 
            "Yes", "Cancel"))
        {
            return;
        }
        
        GrounderIK[] grounderIKs = Object.FindObjectsByType<GrounderIK>(FindObjectsSortMode.None);
        
        foreach (var grounderIK in grounderIKs)
        {
            Debug.Log($"Removing GrounderIK from {grounderIK.gameObject.name}");
            Object.DestroyImmediate(grounderIK);
        }
        
        Debug.Log($"Removed {grounderIKs.Length} GrounderIK components");
        
        if (grounderIKs.Length > 0)
        {
            EditorUtility.SetDirty(EditorUtility.InstanceIDToObject(0)); // Mark scene as dirty
        }
    }
}