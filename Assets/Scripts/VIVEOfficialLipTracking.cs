using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using System.Collections.Generic;

public class VIVEOfficialLipTracking : MonoBehaviour
{
    [Header("Avatar Setup")]
    public SkinnedMeshRenderer headSkinnedMeshRenderer;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool logSignificantValues = true;
    public float significantThreshold = 0.1f;
    
    private ViveFacialTracking facialTrackingFeature;
    private float[] blendshapes = new float[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC];
    private Dictionary<XrLipExpressionHTC, int> shapeMap = new Dictionary<XrLipExpressionHTC, int>();
    
    // For debug display
    private float lastLogTime = 0f;
    private float logInterval = 0.5f;
    
    void Start()
    {
        Debug.Log("[VIVEOfficialLipTracking] Starting...");
        
        // Get facial tracking feature
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null || !facialTrackingFeature.enabled)
        {
            Debug.LogError("[VIVEOfficialLipTracking] ViveFacialTracking feature not found or not enabled!");
            enabled = false;
            return;
        }
        
        // Initialize shape mapping (you'll need to map these to your avatar's blendshapes)
        InitializeShapeMapping();
        
        Debug.Log($"[VIVEOfficialLipTracking] ✅ Initialized with {shapeMap.Count} shape mappings");
    }
    
    void InitializeShapeMapping()
    {
        // Basic mapping - you'll need to adjust these based on your avatar's blendshape names
        // For now, we'll use the index directly
        for (int i = 0; i < (int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC; i++)
        {
            shapeMap[(XrLipExpressionHTC)i] = i;
        }
        
        // If you have specific blendshape names, map them like this:
        // shapeMap[XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_RIGHT_HTC] = GetBlendShapeIndex("Jaw_Right");
        // shapeMap[XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_LEFT_HTC] = GetBlendShapeIndex("Jaw_Left");
        // etc...
    }
    
    void Update()
    {
        if (facialTrackingFeature == null) return;
        
        // Get facial expressions
        bool success = facialTrackingFeature.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, 
            out blendshapes
        );
        
        if (success && blendshapes != null)
        {
            // Update avatar if we have one
            if (headSkinnedMeshRenderer != null)
            {
                UpdateAvatarBlendshapes();
            }
            
            // Debug logging
            if (showDebugInfo && Time.time - lastLogTime >= logInterval)
            {
                LogSignificantValues();
                lastLogTime = Time.time;
            }
        }
    }
    
    void UpdateAvatarBlendshapes()
    {
        // Update all lip expressions
        for (int i = 0; i < (int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC; i++)
        {
            XrLipExpressionHTC expression = (XrLipExpressionHTC)i;
            if (shapeMap.ContainsKey(expression))
            {
                int blendshapeIndex = shapeMap[expression];
                float value = blendshapes[i] * 100f; // Convert to percentage
                
                // Only update if index is valid
                if (blendshapeIndex >= 0 && blendshapeIndex < headSkinnedMeshRenderer.sharedMesh.blendShapeCount)
                {
                    headSkinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndex, value);
                }
            }
        }
    }
    
    void LogSignificantValues()
    {
        if (!logSignificantValues) return;
        
        Debug.Log($"[{Time.time:F1}s] === Lip Tracking Update ===");
        
        // Log only significant values
        for (int i = 0; i < blendshapes.Length; i++)
        {
            if (blendshapes[i] > significantThreshold)
            {
                XrLipExpressionHTC expression = (XrLipExpressionHTC)i;
                Debug.Log($"  {expression}: {blendshapes[i]:F2}");
            }
        }
        
        // Always log jaw open as it's most important
        float jawOpen = blendshapes[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC];
        if (jawOpen > 0.01f)
        {
            Debug.Log($"  👄 JAW OPEN: {jawOpen:F2}");
        }
    }
    
    // Helper method to get blendshape index by name
    int GetBlendShapeIndex(string blendshapeName)
    {
        if (headSkinnedMeshRenderer == null || headSkinnedMeshRenderer.sharedMesh == null)
            return -1;
            
        for (int i = 0; i < headSkinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
        {
            if (headSkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i) == blendshapeName)
            {
                return i;
            }
        }
        
        Debug.LogWarning($"Blendshape '{blendshapeName}' not found!");
        return -1;
    }
    
    void OnGUI()
    {
        if (!showDebugInfo || blendshapes == null) return;
        
        // Display key values on screen
        GUI.color = Color.green;
        int y = 10;
        
        GUI.Label(new Rect(10, y, 400, 20), "=== VIVE Lip Tracking ===");
        y += 25;
        
        // Show most important expressions
        string[] importantExpressions = {
            "JAW_OPEN", "MOUTH_RAISER_RIGHT", "MOUTH_RAISER_LEFT", 
            "MOUTH_FROWN_RIGHT", "MOUTH_FROWN_LEFT"
        };
        
        foreach (string expressionName in importantExpressions)
        {
            try
            {
                var expression = (XrLipExpressionHTC)System.Enum.Parse(typeof(XrLipExpressionHTC), $"XR_LIP_EXPRESSION_{expressionName}_HTC");
                float value = blendshapes[(int)expression];
                
                if (value > 0.01f)
                {
                    GUI.Label(new Rect(10, y, 400, 20), $"{expressionName}: {value:F2}");
                    
                    // Draw bar
                    GUI.color = Color.green;
                    GUI.DrawTexture(new Rect(200, y, value * 100, 15), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    
                    y += 20;
                }
            }
            catch { }
        }
    }
} 