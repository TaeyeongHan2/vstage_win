using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using System.Collections.Generic;

public class FacialTrackingVisualizer : MonoBehaviour
{
    [Header("UI Setup")]
    public GameObject barPrefab; // UI Image prefab for bars
    public Transform lipBarsContainer;
    public Transform eyeBarsContainer;
    public float maxBarHeight = 200f;
    
    [Header("Settings")]
    public bool showLabels = true;
    public Color lipBarColor = Color.green;
    public Color eyeBarColor = Color.blue;
    
    private ViveFacialTracking facialTrackingFeature;
    private Dictionary<int, RectTransform> lipBars = new Dictionary<int, RectTransform>();
    private Dictionary<int, RectTransform> eyeBars = new Dictionary<int, RectTransform>();
    
    // Key lip expressions to visualize
    private readonly (XrLipExpressionHTC expression, string label)[] lipExpressions = 
    {
        (XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC, "Jaw Open"),
        (XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC, "Pout"),
        (XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_LEFT_HTC, "Smile L"),
        (XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_RIGHT_HTC, "Smile R"),
        (XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_LEFT_HTC, "Stretch L"),
        (XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_RIGHT_HTC, "Stretch R"),
    };
    
    // Key eye expressions to visualize
    private readonly (XrEyeExpressionHTC expression, string label)[] eyeExpressions = 
    {
        (XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC, "L Blink"),
        (XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC, "R Blink"),
        (XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC, "L Wide"),
        (XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_WIDE_HTC, "R Wide"),
    };
    
    void Start()
    {
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null || !facialTrackingFeature.enabled)
        {
            Debug.LogError("ViveFacialTracking feature not found!");
            enabled = false;
            return;
        }
        
        CreateBars();
    }
    
    void CreateBars()
    {
        // Create lip expression bars
        foreach (var (expression, label) in lipExpressions)
        {
            var bar = CreateBar(lipBarsContainer, label, lipBarColor);
            lipBars[(int)expression] = bar;
        }
        
        // Create eye expression bars (if supported)
        foreach (var (expression, label) in eyeExpressions)
        {
            var bar = CreateBar(eyeBarsContainer, label, eyeBarColor);
            eyeBars[(int)expression] = bar;
        }
    }
    
    RectTransform CreateBar(Transform container, string label, Color color)
    {
        var barObj = Instantiate(barPrefab, container);
        var rect = barObj.GetComponent<RectTransform>();
        
        // Set bar color
        var img = barObj.GetComponent<Image>();
        if (img) img.color = color;
        
        // Add label
        if (showLabels)
        {
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(barObj.transform);
            var text = labelObj.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 12;
            text.color = Color.white;
            
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = new Vector2(1, 0);
            labelRect.offsetMin = new Vector2(0, -20);
            labelRect.offsetMax = Vector2.zero;
        }
        
        return rect;
    }
    
    void Update()
    {
        if (facialTrackingFeature == null) return;
        
        // Update lip expressions
        float[] lipData;
        if (facialTrackingFeature.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipData))
        {
            foreach (var kvp in lipBars)
            {
                if (kvp.Key < lipData.Length)
                {
                    UpdateBar(kvp.Value, lipData[kvp.Key]);
                }
            }
        }
        
        // Update eye expressions
        float[] eyeData;
        if (facialTrackingFeature.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeData))
        {
            foreach (var kvp in eyeBars)
            {
                if (kvp.Key < eyeData.Length)
                {
                    UpdateBar(kvp.Value, eyeData[kvp.Key]);
                }
            }
        }
    }
    
    void UpdateBar(RectTransform bar, float value)
    {
        var height = Mathf.Clamp01(value) * maxBarHeight;
        bar.sizeDelta = new Vector2(bar.sizeDelta.x, height);
    }
} 