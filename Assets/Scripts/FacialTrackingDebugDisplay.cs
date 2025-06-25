using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;

public class FacialTrackingDebugDisplay : MonoBehaviour
{
    [Header("Display Settings")]
    public bool showOnScreen = true;
    public int fontSize = 14;
    public Color textColor = Color.green;
    public Vector2 screenPosition = new Vector2(10, 10);
    
    private ViveFacialTracking facialTrackingFeature;
    private string debugText = "";
    private GUIStyle guiStyle;
    
    // Track peak values
    private float peakJawOpen = 0f;
    private float peakSmile = 0f;
    
    void Start()
    {
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null || !facialTrackingFeature.enabled)
        {
            Debug.LogError("ViveFacialTracking feature not found!");
            enabled = false;
            return;
        }
        
        // Setup GUI style
        guiStyle = new GUIStyle();
        guiStyle.fontSize = fontSize;
        guiStyle.normal.textColor = textColor;
    }
    
    void Update()
    {
        if (facialTrackingFeature == null) return;
        
        debugText = "=== FACIAL TRACKING DATA ===\n\n";
        
        // Get lip data
        float[] lipData;
        if (facialTrackingFeature.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipData))
        {
            debugText += "LIP TRACKING:\n";
            
            // Jaw Open
            float jawOpen = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC];
            peakJawOpen = Mathf.Max(peakJawOpen, jawOpen);
            debugText += $"Jaw Open: {CreateBar(jawOpen)} {jawOpen:F2} (Peak: {peakJawOpen:F2})\n";
            
            // Smile
            float smileL = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_LEFT_HTC];
            float smileR = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_RIGHT_HTC];
            float smile = (smileL + smileR) / 2f;
            peakSmile = Mathf.Max(peakSmile, smile);
            debugText += $"Smile: {CreateBar(smile)} {smile:F2} (Peak: {peakSmile:F2})\n";
            
            // Pout
            float pout = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC];
            debugText += $"Pout: {CreateBar(pout)} {pout:F2}\n";
            
            // Upper/Lower overturn
            float upperOverturn = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_OVERTURN_HTC];
            float lowerOverturn = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_OVERTURN_HTC];
            debugText += $"Upper Lip: {CreateBar(upperOverturn)} {upperOverturn:F2}\n";
            debugText += $"Lower Lip: {CreateBar(lowerOverturn)} {lowerOverturn:F2}\n";
        }
        else
        {
            debugText += "LIP TRACKING: No Data\n";
        }
        
        debugText += "\n";
        
        // Get eye data
        float[] eyeData;
        if (facialTrackingFeature.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeData))
        {
            debugText += "EYE TRACKING:\n";
            
            // Blinks
            float blinkL = eyeData[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC];
            float blinkR = eyeData[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC];
            debugText += $"Blink L: {CreateBar(blinkL)} {blinkL:F2}\n";
            debugText += $"Blink R: {CreateBar(blinkR)} {blinkR:F2}\n";
            
            // Wide
            float wideL = eyeData[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC];
            float wideR = eyeData[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_WIDE_HTC];
            debugText += $"Wide L: {CreateBar(wideL)} {wideL:F2}\n";
            debugText += $"Wide R: {CreateBar(wideR)} {wideR:F2}\n";
        }
        else
        {
            debugText += "EYE TRACKING: Not Supported\n";
        }
        
        debugText += "\n[R] Reset Peak Values";
        
        // Reset peaks
        if (Input.GetKeyDown(KeyCode.R))
        {
            peakJawOpen = 0f;
            peakSmile = 0f;
        }
    }
    
    string CreateBar(float value)
    {
        int barLength = 20;
        int filled = Mathf.RoundToInt(value * barLength);
        string bar = "[";
        for (int i = 0; i < barLength; i++)
        {
            bar += i < filled ? "█" : "-";
        }
        bar += "]";
        return bar;
    }
    
    void OnGUI()
    {
        if (!showOnScreen) return;
        
        // Background box
        float width = 400;
        float height = 300;
        GUI.Box(new Rect(screenPosition.x, screenPosition.y, width, height), "");
        
        // Text
        GUI.Label(new Rect(screenPosition.x + 10, screenPosition.y + 10, width - 20, height - 20), 
                  debugText, guiStyle);
    }
} 