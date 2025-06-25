using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using System.Collections.Generic;

public class FacialTrackingAvatar : MonoBehaviour
{
    [Header("Avatar Setup")]
    public SkinnedMeshRenderer faceMesh;
    
    [System.Serializable]
    public class BlendshapeMapping
    {
        public string blendshapeName;
        public XrLipExpressionHTC lipExpression = (XrLipExpressionHTC)(-1);
        public XrEyeExpressionHTC eyeExpression = (XrEyeExpressionHTC)(-1);
        [Range(0, 2)] public float multiplier = 1f;
    }
    
    [Header("Blendshape Mapping")]
    public List<BlendshapeMapping> blendshapeMappings = new List<BlendshapeMapping>();
    
    private ViveFacialTracking facialTrackingFeature;
    private Dictionary<string, int> blendshapeIndices = new Dictionary<string, int>();
    
    void Start()
    {
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null || !facialTrackingFeature.enabled)
        {
            Debug.LogError("ViveFacialTracking feature not found!");
            enabled = false;
            return;
        }
        
        if (faceMesh == null)
        {
            Debug.LogError("Face mesh not assigned!");
            enabled = false;
            return;
        }
        
        // Cache blendshape indices
        for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
        {
            string name = faceMesh.sharedMesh.GetBlendShapeName(i);
            blendshapeIndices[name] = i;
        }
        
        // Auto-generate common mappings if empty
        if (blendshapeMappings.Count == 0)
        {
            GenerateDefaultMappings();
        }
    }
    
    void GenerateDefaultMappings()
    {
        // Common blendshape mappings
        blendshapeMappings.Add(new BlendshapeMapping 
        { 
            blendshapeName = "JawOpen",
            lipExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC,
            multiplier = 1f
        });
        
        blendshapeMappings.Add(new BlendshapeMapping 
        { 
            blendshapeName = "MouthSmileLeft",
            lipExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_LEFT_HTC,
            multiplier = 1f
        });
        
        blendshapeMappings.Add(new BlendshapeMapping 
        { 
            blendshapeName = "MouthSmileRight",
            lipExpression = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_RIGHT_HTC,
            multiplier = 1f
        });
        
        blendshapeMappings.Add(new BlendshapeMapping 
        { 
            blendshapeName = "EyeBlinkLeft",
            eyeExpression = XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC,
            multiplier = 1f
        });
        
        blendshapeMappings.Add(new BlendshapeMapping 
        { 
            blendshapeName = "EyeBlinkRight",
            eyeExpression = XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC,
            multiplier = 1f
        });
    }
    
    void Update()
    {
        if (facialTrackingFeature == null || faceMesh == null) return;
        
        // Get facial expressions
        float[] lipData;
        bool hasLipData = facialTrackingFeature.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipData);
            
        float[] eyeData;
        bool hasEyeData = facialTrackingFeature.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeData);
        
        // Apply to blendshapes
        foreach (var mapping in blendshapeMappings)
        {
            if (!blendshapeIndices.TryGetValue(mapping.blendshapeName, out int index))
                continue;
                
            float value = 0f;
            
            // Get value from lip or eye data
            if (mapping.lipExpression != (XrLipExpressionHTC)(-1) && hasLipData)
            {
                int expr = (int)mapping.lipExpression;
                if (expr < lipData.Length)
                    value = lipData[expr];
            }
            else if (mapping.eyeExpression != (XrEyeExpressionHTC)(-1) && hasEyeData)
            {
                int expr = (int)mapping.eyeExpression;
                if (expr < eyeData.Length)
                    value = eyeData[expr];
            }
            
            // Apply with multiplier
            faceMesh.SetBlendShapeWeight(index, value * mapping.multiplier * 100f);
        }
    }
    
    // Helper to list all blendshapes
    [ContextMenu("List All Blendshapes")]
    void ListBlendshapes()
    {
        if (faceMesh == null || faceMesh.sharedMesh == null) return;
        
        Debug.Log($"=== Blendshapes in {faceMesh.name} ===");
        for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
        {
            Debug.Log($"[{i}] {faceMesh.sharedMesh.GetBlendShapeName(i)}");
        }
    }
} 