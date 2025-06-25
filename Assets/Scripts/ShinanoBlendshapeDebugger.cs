using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Shinano 모델의 블렌드셰이프를 디버깅하는 스크립트
/// 실제 사용 가능한 블렌드셰이프 이름을 확인하고 테스트
/// </summary>
public class ShinanoBlendshapeDebugger : MonoBehaviour
{
    [Header("Target Mesh")]
    public SkinnedMeshRenderer targetMesh;
    
    [Header("Debug Info")]
    public bool showAllBlendshapes = false;
    public List<string> availableBlendshapes = new List<string>();
    
    [Header("Test Settings")]
    public int testBlendshapeIndex = 0;
    [Range(0f, 100f)] public float testValue = 0f;
    public string searchFilter = "";
    
    private SkinnedMeshRenderer[] allSkinnedMeshes;
    private Dictionary<string, List<string>> meshBlendshapes = new Dictionary<string, List<string>>();
    
    void Start()
    {
        FindAllSkinnedMeshRenderers();
        
        if (targetMesh == null)
        {
            AutoFindBodyMesh();
        }
        
        if (targetMesh != null)
        {
            RefreshBlendshapeList();
        }
    }
    
    void FindAllSkinnedMeshRenderers()
    {
        allSkinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        meshBlendshapes.Clear();
        
        Debug.Log($"[BlendshapeDebugger] Found {allSkinnedMeshes.Length} SkinnedMeshRenderers:");
        
        foreach (var mesh in allSkinnedMeshes)
        {
            if (mesh.sharedMesh != null && mesh.sharedMesh.blendShapeCount > 0)
            {
                var blendshapes = new List<string>();
                for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
                {
                    blendshapes.Add(mesh.sharedMesh.GetBlendShapeName(i));
                }
                meshBlendshapes[mesh.name] = blendshapes;
                
                Debug.Log($"  - {mesh.name}: {mesh.sharedMesh.blendShapeCount} blendshapes");
            }
        }
    }
    
    void AutoFindBodyMesh()
    {
        // Shinano의 Body 메시를 찾기
        foreach (var mesh in allSkinnedMeshes)
        {
            if (mesh.name == "Body" || mesh.name.Contains("body") || mesh.name.Contains("Body"))
            {
                targetMesh = mesh;
                Debug.Log($"[BlendshapeDebugger] Auto-selected mesh: {mesh.name}");
                return;
            }
        }
        
        // Body를 못 찾으면 블렌드셰이프가 가장 많은 메시 선택
        if (allSkinnedMeshes.Length > 0)
        {
            targetMesh = allSkinnedMeshes.OrderByDescending(m => 
                m.sharedMesh != null ? m.sharedMesh.blendShapeCount : 0).First();
            Debug.Log($"[BlendshapeDebugger] Selected mesh with most blendshapes: {targetMesh.name}");
        }
    }
    
    void RefreshBlendshapeList()
    {
        if (targetMesh == null || targetMesh.sharedMesh == null) return;
        
        availableBlendshapes.Clear();
        
        for (int i = 0; i < targetMesh.sharedMesh.blendShapeCount; i++)
        {
            string name = targetMesh.sharedMesh.GetBlendShapeName(i);
            availableBlendshapes.Add($"{i}: {name}");
        }
        
        Debug.Log($"[BlendshapeDebugger] {targetMesh.name} has {availableBlendshapes.Count} blendshapes");
        
        if (showAllBlendshapes)
        {
            PrintAllBlendshapes();
        }
    }
    
    void PrintAllBlendshapes()
    {
        Debug.Log("=== All Blendshapes in Target Mesh ===");
        for (int i = 0; i < availableBlendshapes.Count; i++)
        {
            float currentValue = targetMesh.GetBlendShapeWeight(i);
            if (currentValue > 0.01f)
            {
                Debug.Log($"{availableBlendshapes[i]} = {currentValue:F2}% *");
            }
            else
            {
                Debug.Log(availableBlendshapes[i]);
            }
        }
        Debug.Log("=====================================");
    }
    
    void Update()
    {
        if (targetMesh == null || targetMesh.sharedMesh == null) return;
        
        // Apply test value to selected blendshape
        if (testBlendshapeIndex >= 0 && testBlendshapeIndex < targetMesh.sharedMesh.blendShapeCount)
        {
            targetMesh.SetBlendShapeWeight(testBlendshapeIndex, testValue);
        }
        
        // Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintAllBlendshapes();
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            LogCurrentNonZeroBlendshapes();
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            FindBlendshapesByKeyword();
        }
        
        // Quick test specific blendshapes
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TestSpecificBlendshape("mouth_a1", 80f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            TestSpecificBlendshape("mouth_smile", 80f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            TestSpecificBlendshape("mouth_o1", 80f);
        }
    }
    
    void LogCurrentNonZeroBlendshapes()
    {
        Debug.Log("=== Currently Active Blendshapes ===");
        bool foundAny = false;
        
        for (int i = 0; i < targetMesh.sharedMesh.blendShapeCount; i++)
        {
            float value = targetMesh.GetBlendShapeWeight(i);
            if (value > 0.01f)
            {
                string name = targetMesh.sharedMesh.GetBlendShapeName(i);
                Debug.Log($"{i}: {name} = {value:F2}%");
                foundAny = true;
            }
        }
        
        if (!foundAny)
        {
            Debug.Log("No active blendshapes found.");
        }
        Debug.Log("===================================");
    }
    
    void FindBlendshapesByKeyword()
    {
        if (string.IsNullOrEmpty(searchFilter))
        {
            Debug.Log("Set searchFilter in Inspector to find blendshapes");
            return;
        }
        
        Debug.Log($"=== Blendshapes containing '{searchFilter}' ===");
        
        for (int i = 0; i < targetMesh.sharedMesh.blendShapeCount; i++)
        {
            string name = targetMesh.sharedMesh.GetBlendShapeName(i);
            if (name.ToLower().Contains(searchFilter.ToLower()))
            {
                Debug.Log($"{i}: {name}");
            }
        }
        Debug.Log("=========================================");
    }
    
    void TestSpecificBlendshape(string blendshapeName, float value)
    {
        for (int i = 0; i < targetMesh.sharedMesh.blendShapeCount; i++)
        {
            if (targetMesh.sharedMesh.GetBlendShapeName(i) == blendshapeName)
            {
                targetMesh.SetBlendShapeWeight(i, value);
                Debug.Log($"[Test] Set {blendshapeName} to {value}%");
                return;
            }
        }
        Debug.LogWarning($"[Test] Blendshape '{blendshapeName}' not found!");
    }
    
    void OnGUI()
    {
        if (targetMesh == null) return;
        
        int y = 10;
        GUI.Label(new Rect(10, y, 500, 20), $"=== Blendshape Debugger - {targetMesh.name} ===");
        y += 25;
        
        GUI.Label(new Rect(10, y, 500, 20), $"Total Blendshapes: {targetMesh.sharedMesh.blendShapeCount}");
        y += 20;
        
        GUI.Label(new Rect(10, y, 500, 20), $"Test Index: {testBlendshapeIndex} = {testValue:F1}%");
        y += 20;
        
        if (testBlendshapeIndex >= 0 && testBlendshapeIndex < availableBlendshapes.Count)
        {
            GUI.Label(new Rect(10, y, 500, 20), $"Testing: {availableBlendshapes[testBlendshapeIndex]}");
            y += 20;
        }
        
        y += 10;
        GUI.Label(new Rect(10, y, 500, 20), "Keys: P=Print All, L=List Active, F=Find by Filter");
        y += 20;
        GUI.Label(new Rect(10, y, 500, 20), "Test: 6=mouth_a1, 7=mouth_smile, 8=mouth_o1");
        
        // Show all meshes
        if (meshBlendshapes.Count > 1)
        {
            y += 30;
            GUI.Label(new Rect(10, y, 500, 20), "=== All Meshes ===");
            y += 20;
            foreach (var kvp in meshBlendshapes)
            {
                GUI.Label(new Rect(10, y, 500, 20), $"{kvp.Key}: {kvp.Value.Count} blendshapes");
                y += 20;
            }
        }
    }
} 