using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using System.Collections.Generic;

public class ShinanoVIVEFacialMapping : MonoBehaviour
{
    [Header("Target Settings")]
    public SkinnedMeshRenderer shinanoBodyMesh;
    
    [Header("Mapping Settings")]
    [Range(0f, 2f)] public float globalMultiplier = 1.0f;
    [Range(0f, 1f)] public float smoothingFactor = 0.15f;
    
    [Header("Debug")]
    public bool showDebugValues = false;
    public bool showDebugGUI = true;
    
    // Debug info
    private bool isSessionActive = false;
    private bool isFacialTrackingActive = false;
    private string debugStatus = "Initializing...";
    private int successfulUpdates = 0;
    private float lastSuccessfulUpdate = 0f;
    
    // Mapping structure
    [System.Serializable]
    public class ExpressionMapping
    {
        public XrLipExpressionHTC viveExpression;
        public string shinanoBlendshapeName;
        [Range(0f, 2f)] public float multiplier = 1.0f;
        [Range(-1f, 1f)] public float offset = 0f;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    }
    
    [Header("Expression Mappings")]
    public List<ExpressionMapping> mappings = new List<ExpressionMapping>();
    
    // Runtime
    private ViveFacialTracking facialTrackingFeature;
    private float[] lipExpressions = new float[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC];
    private Dictionary<string, float> smoothedValues = new Dictionary<string, float>();
    
    void Start()
    {
        // Get facial tracking feature
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null || !facialTrackingFeature.enabled)
        {
            debugStatus = "ERROR: ViveFacialTracking feature not found or not enabled!";
            Debug.LogError("[ShinanoVIVEFacialMapping] " + debugStatus);
            enabled = false;
            return;
        }
        
        // Auto-find Shinano body mesh if not assigned
        if (shinanoBodyMesh == null)
        {
            shinanoBodyMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            if (shinanoBodyMesh != null && shinanoBodyMesh.name == "Body")
            {
                Debug.Log("Found Shinano Body mesh automatically");
            }
        }
        
        // Initialize default mappings if empty
        if (mappings.Count == 0)
        {
            InitializeDefaultMappings();
        }
        
        // Initialize smoothing dictionary
        foreach (var mapping in mappings)
        {
            if (!smoothedValues.ContainsKey(mapping.shinanoBlendshapeName))
            {
                smoothedValues[mapping.shinanoBlendshapeName] = 0f;
            }
        }
        
        debugStatus = $"Initialized with {mappings.Count} mappings";
        Debug.Log($"[ShinanoVIVEFacialMapping] ✅ {debugStatus}");
    }
    
    void InitializeDefaultMappings()
    {
        // Basic mouth opening
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC,
            shinanoBlendshapeName = "mouth_a1",
            multiplier = 1.2f
        });
        
        // Smile right
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_RIGHT_HTC,
            shinanoBlendshapeName = "mouth_smile",
            multiplier = 1.0f
        });
        
        // Smile left
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_LEFT_HTC,
            shinanoBlendshapeName = "mouth_smile",
            multiplier = 1.0f
        });
        
        // Wide mouth right
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_RIGHT_HTC,
            shinanoBlendshapeName = "mouth_wide",
            multiplier = 0.8f
        });
        
        // Wide mouth left
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_LEFT_HTC,
            shinanoBlendshapeName = "mouth_wide",
            multiplier = 0.8f
        });
        
        // Cheek suck (combined left/right)
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_SUCK_HTC,
            shinanoBlendshapeName = "other_pout",
            multiplier = 1.0f
        });
        
        // O shape (Pout)
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC,
            shinanoBlendshapeName = "mouth_o1",
            multiplier = 0.9f
        });
        
        // Sad expression (using lower mouth movements)
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNRIGHT_HTC,
            shinanoBlendshapeName = "mouth_sad",
            multiplier = 0.8f
        });
        
        // Sad expression (left side)
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNLEFT_HTC,
            shinanoBlendshapeName = "mouth_sad",
            multiplier = 0.8f
        });
        
        // I sound (using mouth stretcher as approximation)
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_LEFT_HTC,
            shinanoBlendshapeName = "mouth_i1",
            multiplier = 0.5f,
            offset = -0.2f
        });
        
        // E sound - Upper lip up
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC,
            shinanoBlendshapeName = "mouth_e1",
            multiplier = 0.8f,
            curve = AnimationCurve.EaseInOut(0, 0, 1, 1)
        });
        
        // U sound (using pout as approximation)
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC,
            shinanoBlendshapeName = "mouth_u1",
            multiplier = 0.7f,
            offset = 0.1f
        });
        
        // Tongue out
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_LONGSTEP1_HTC,
            shinanoBlendshapeName = "tongue_pero",
            multiplier = 1.0f
        });
        
        // Additional mappings for cheek puff
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_PUFF_RIGHT_HTC,
            shinanoBlendshapeName = "other_cheek_2",
            multiplier = 1.0f
        });
        
        mappings.Add(new ExpressionMapping 
        { 
            viveExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_PUFF_LEFT_HTC,
            shinanoBlendshapeName = "other_cheek_2",
            multiplier = 1.0f
        });
    }
    
    void Update()
    {
        if (shinanoBodyMesh == null || facialTrackingFeature == null) return;
        
        // Check session status - removed IsSessionRunning() as it's not available
        // Instead, we'll just try to get facial expressions and handle failures
        
        // Get facial expressions
        bool success = facialTrackingFeature.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, 
            out lipExpressions
        );
        
        if (!success || lipExpressions == null)
        {
            debugStatus = "WAITING: No facial data. Check headset connection.";
            isFacialTrackingActive = false;
            isSessionActive = false;
            return;
        }
        
        // If we got here, session is active
        isSessionActive = true;
        isFacialTrackingActive = true;
        successfulUpdates++;
        lastSuccessfulUpdate = Time.time;
        debugStatus = "ACTIVE: Facial tracking working";
        
        // Reset all mapped blendshapes to 0
        foreach (var mapping in mappings)
        {
            int index = GetBlendshapeIndex(mapping.shinanoBlendshapeName);
            if (index >= 0)
            {
                shinanoBodyMesh.SetBlendShapeWeight(index, 0);
            }
        }
        
        // Apply VIVE expressions
        foreach (var mapping in mappings)
        {
            int viveIndex = (int)mapping.viveExpression;
            if (viveIndex < lipExpressions.Length)
            {
                float viveValue = lipExpressions[viveIndex];
                
                // Apply curve, offset, and multipliers
                float mappedValue = mapping.curve.Evaluate(viveValue);
                mappedValue = (mappedValue + mapping.offset) * mapping.multiplier * globalMultiplier;
                mappedValue = Mathf.Clamp01(mappedValue);
                
                // Smooth the value
                string key = mapping.shinanoBlendshapeName;
                if (smoothedValues.ContainsKey(key))
                {
                    smoothedValues[key] = Mathf.Lerp(smoothedValues[key], mappedValue, smoothingFactor);
                    mappedValue = smoothedValues[key];
                }
                
                // Apply to blendshape (combine if multiple VIVE expressions map to same blendshape)
                int blendshapeIndex = GetBlendshapeIndex(mapping.shinanoBlendshapeName);
                if (blendshapeIndex >= 0)
                {
                    float currentWeight = shinanoBodyMesh.GetBlendShapeWeight(blendshapeIndex) / 100f;
                    float newWeight = Mathf.Max(currentWeight, mappedValue);
                    shinanoBodyMesh.SetBlendShapeWeight(blendshapeIndex, newWeight * 100f);
                    
                    if (showDebugValues && mappedValue > 0.01f)
                    {
                        Debug.Log($"{mapping.viveExpression} -> {mapping.shinanoBlendshapeName}: {mappedValue:F2}");
                    }
                }
            }
        }
    }
    
    int GetBlendshapeIndex(string blendshapeName)
    {
        if (shinanoBodyMesh == null || shinanoBodyMesh.sharedMesh == null) return -1;
        
        for (int i = 0; i < shinanoBodyMesh.sharedMesh.blendShapeCount; i++)
        {
            if (shinanoBodyMesh.sharedMesh.GetBlendShapeName(i) == blendshapeName)
            {
                return i;
            }
        }
        return -1;
    }
    
    // Special combined expressions
    public void ApplySpecialExpression(string expressionName)
    {
        switch (expressionName)
        {
            case "Surprise":
                SetBlendshapeWeight("mouth_o1", 0.7f);
                SetBlendshapeWeight("eye_surprise", 1.0f);
                SetBlendshapeWeight("eyebrow_surprised", 1.0f);
                break;
                
            case "Happy":
                SetBlendshapeWeight("mouth_smile", 0.8f);
                SetBlendshapeWeight("eye_joy", 0.6f);
                SetBlendshapeWeight("other_cheek_1", 0.5f);
                break;
                
            case "Sad":
                SetBlendshapeWeight("mouth_sad", 0.7f);
                SetBlendshapeWeight("eye_sad", 0.8f);
                SetBlendshapeWeight("eyebrow_sad1", 0.9f);
                break;
        }
    }
    
    void SetBlendshapeWeight(string name, float weight)
    {
        int index = GetBlendshapeIndex(name);
        if (index >= 0)
        {
            shinanoBodyMesh.SetBlendShapeWeight(index, weight * 100f);
        }
    }
    
    void OnGUI()
    {
        if (!showDebugGUI) return;
        
        int y = 250; // Start lower to avoid overlap with BlendshapeDebugger
        GUI.Box(new Rect(10, y, 400, 140), "");
        
        y += 5;
        GUI.Label(new Rect(15, y, 390, 20), "=== VIVE Facial Tracking Status ===");
        y += 25;
        
        // Status with color
        Color oldColor = GUI.color;
        if (isFacialTrackingActive)
            GUI.color = Color.green;
        else if (isSessionActive)
            GUI.color = Color.yellow;
        else
            GUI.color = Color.red;
            
        GUI.Label(new Rect(15, y, 390, 20), $"Status: {debugStatus}");
        GUI.color = oldColor;
        y += 20;
        
        GUI.Label(new Rect(15, y, 390, 20), $"Session Active: {(isSessionActive ? "YES" : "NO")}");
        y += 20;
        
        GUI.Label(new Rect(15, y, 390, 20), $"Facial Tracking: {(isFacialTrackingActive ? "ACTIVE" : "INACTIVE")}");
        y += 20;
        
        GUI.Label(new Rect(15, y, 390, 20), $"Successful Updates: {successfulUpdates}");
        y += 20;
        
        if (lastSuccessfulUpdate > 0)
        {
            float timeSinceUpdate = Time.time - lastSuccessfulUpdate;
            GUI.Label(new Rect(15, y, 390, 20), $"Last Update: {timeSinceUpdate:F1}s ago");
        }
    }
} 