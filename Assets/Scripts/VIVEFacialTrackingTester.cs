using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Test script to diagnose VIVE facial tracking and manually test Shinano blendshapes
/// </summary>
public class VIVEFacialTrackingTester : MonoBehaviour
{
    [Header("Target")]
    public SkinnedMeshRenderer targetMesh;
    
    [Header("Manual Testing")]
    public bool enableManualMode = false;
    [Range(0f, 100f)] public float manualMouthOpen = 0f;
    [Range(0f, 100f)] public float manualSmile = 0f;
    [Range(0f, 100f)] public float manualMouthWide = 0f;
    [Range(0f, 100f)] public float manualMouthO = 0f;
    [Range(0f, 100f)] public float manualMouthSad = 0f;
    
    [Header("Debug Info")]
    public bool showRawVIVEValues = true;
    
    // Runtime
    private ViveFacialTracking facialTrackingFeature;
    private float[] lipExpressions = new float[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC];
    private bool isTracking = false;
    private Dictionary<XrLipExpressionHTC, float> activeExpressions = new Dictionary<XrLipExpressionHTC, float>();
    
    // Blendshape indices cache
    private Dictionary<string, int> blendshapeIndices = new Dictionary<string, int>();
    
    void Start()
    {
        // Find target mesh
        if (targetMesh == null)
        {
            var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
            targetMesh = meshes.FirstOrDefault(m => m.name == "Body") ?? meshes.FirstOrDefault();
        }
        
        if (targetMesh == null)
        {
            Debug.LogError("[VIVEFacialTrackingTester] No SkinnedMeshRenderer found!");
            enabled = false;
            return;
        }
        
        // Cache blendshape indices
        CacheBlendshapeIndices();
        
        // Get facial tracking feature
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null)
        {
            Debug.LogWarning("[VIVEFacialTrackingTester] ViveFacialTracking feature not found. Manual mode only.");
        }
        else
        {
            Debug.Log($"[VIVEFacialTrackingTester] ViveFacialTracking feature found. Enabled: {facialTrackingFeature.enabled}");
        }
    }
    
    void CacheBlendshapeIndices()
    {
        blendshapeIndices.Clear();
        
        if (targetMesh == null || targetMesh.sharedMesh == null) return;
        
        for (int i = 0; i < targetMesh.sharedMesh.blendShapeCount; i++)
        {
            string name = targetMesh.sharedMesh.GetBlendShapeName(i);
            blendshapeIndices[name] = i;
        }
        
        Debug.Log($"[VIVEFacialTrackingTester] Cached {blendshapeIndices.Count} blendshapes");
    }
    
    void Update()
    {
        if (targetMesh == null) return;
        
        if (enableManualMode)
        {
            // Manual control mode
            ApplyManualBlendshapes();
        }
        else if (facialTrackingFeature != null)
        {
            // VIVE tracking mode
            UpdateVIVETracking();
        }
        
        // Keyboard shortcuts for quick testing
        HandleKeyboardShortcuts();
    }
    
    void ApplyManualBlendshapes()
    {
        SetBlendshapeWeight("mouth_a1", manualMouthOpen);
        SetBlendshapeWeight("mouth_smile", manualSmile);
        SetBlendshapeWeight("mouth_wide", manualMouthWide);
        SetBlendshapeWeight("mouth_o1", manualMouthO);
        SetBlendshapeWeight("mouth_sad", manualMouthSad);
    }
    
    void UpdateVIVETracking()
    {
        // Removed IsSessionRunning() check as it's not available
        // Just try to get facial expressions
        
        bool success = facialTrackingFeature.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, 
            out lipExpressions
        );
        
        if (!success || lipExpressions == null)
        {
            isTracking = false;
            return;
        }
        
        isTracking = true;
        
        // Update active expressions for debug display
        activeExpressions.Clear();
        for (int i = 0; i < lipExpressions.Length; i++)
        {
            if (lipExpressions[i] > 0.01f)
            {
                activeExpressions[(XrLipExpressionHTC)i] = lipExpressions[i];
            }
        }
    }
    
    void HandleKeyboardShortcuts()
    {
        // Quick expression tests
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestExpression("Happy");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestExpression("Sad");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestExpression("Surprise");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestExpression("Angry");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ResetAllBlendshapes();
        }
        
        // Toggle manual mode
        if (Input.GetKeyDown(KeyCode.M))
        {
            enableManualMode = !enableManualMode;
            Debug.Log($"[VIVEFacialTrackingTester] Manual mode: {enableManualMode}");
        }
    }
    
    void TestExpression(string expression)
    {
        ResetAllBlendshapes();
        
        switch (expression)
        {
            case "Happy":
                SetBlendshapeWeight("mouth_smile", 80f);
                SetBlendshapeWeight("eye_joy", 60f);
                SetBlendshapeWeight("other_cheek_1", 50f);
                break;
                
            case "Sad":
                SetBlendshapeWeight("mouth_sad", 70f);
                SetBlendshapeWeight("eye_sad", 80f);
                SetBlendshapeWeight("eyebrow_sad1", 90f);
                break;
                
            case "Surprise":
                SetBlendshapeWeight("mouth_o1", 70f);
                SetBlendshapeWeight("eye_surprise", 100f);
                SetBlendshapeWeight("eyebrow_surprised", 100f);
                break;
                
            case "Angry":
                SetBlendshapeWeight("mouth_angry", 60f);
                SetBlendshapeWeight("eye_angry", 80f);
                SetBlendshapeWeight("eyebrow_angry", 90f);
                break;
        }
        
        Debug.Log($"[VIVEFacialTrackingTester] Applied expression: {expression}");
    }
    
    void ResetAllBlendshapes()
    {
        for (int i = 0; i < targetMesh.sharedMesh.blendShapeCount; i++)
        {
            targetMesh.SetBlendShapeWeight(i, 0);
        }
    }
    
    void SetBlendshapeWeight(string name, float weight)
    {
        if (blendshapeIndices.TryGetValue(name, out int index))
        {
            targetMesh.SetBlendShapeWeight(index, weight);
        }
    }
    
    void OnGUI()
    {
        int y = 410; // Start below other debug UIs
        
        // Test controls
        GUI.Box(new Rect(10, y, 300, 100), "");
        y += 5;
        GUI.Label(new Rect(15, y, 290, 20), "=== Facial Test Controls ===");
        y += 25;
        
        GUI.Label(new Rect(15, y, 290, 20), "Keys: 1=Happy 2=Sad 3=Surprise 4=Angry");
        y += 20;
        GUI.Label(new Rect(15, y, 290, 20), "      5=Reset M=Toggle Manual Mode");
        y += 20;
        GUI.Label(new Rect(15, y, 290, 20), $"Mode: {(enableManualMode ? "MANUAL" : "VIVE TRACKING")}");
        
        // VIVE tracking info
        if (!enableManualMode && showRawVIVEValues && isTracking)
        {
            y = 520;
            GUI.Box(new Rect(10, y, 400, 200), "");
            y += 5;
            GUI.Label(new Rect(15, y, 390, 20), "=== Active VIVE Expressions ===");
            y += 25;
            
            if (activeExpressions.Count == 0)
            {
                GUI.Label(new Rect(15, y, 390, 20), "No active expressions detected");
            }
            else
            {
                foreach (var kvp in activeExpressions.Take(8)) // Show top 8
                {
                    GUI.Label(new Rect(15, y, 390, 20), $"{kvp.Key}: {kvp.Value:F2}");
                    y += 20;
                }
            }
        }
    }
} 