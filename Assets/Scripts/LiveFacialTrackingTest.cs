using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;

public class LiveFacialTrackingTest : MonoBehaviour
{
    private float logInterval = 1.0f;
    private float lastLogTime = 0f;
    private ViveFacialTracking facialTrackingFeature;
    private bool isInitialized = false;
    
    void Start()
    {
        Debug.Log("=== Live Facial Tracking Test Started ===");
        
        // Get facial tracking feature
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null || !facialTrackingFeature.enabled)
        {
            Debug.LogError("ViveFacialTracking feature not found or not enabled!");
            enabled = false;
            return;
        }
        
        Debug.Log("✅ ViveFacialTracking feature found and enabled");
        isInitialized = true;
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        if (Time.time - lastLogTime >= logInterval)
        {
            lastLogTime = Time.time;
            LogFacialData();
        }
    }
    
    void LogFacialData()
    {
        Debug.Log($"[{Time.time:F1}s] === Facial Tracking Update ===");
        
        // Try to get lip expressions
        float[] lipExpressions;
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipExpressions))
        {
            if (lipExpressions != null && lipExpressions.Length > 0)
            {
                Debug.Log("Lip Expressions:");
                
                // Check specific expressions
                float jawOpen = lipExpressions[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC];
                float mouthPout = lipExpressions[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC];
                float mouthRaiserLeft = lipExpressions[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_LEFT_HTC];
                float mouthRaiserRight = lipExpressions[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_RIGHT_HTC];
                
                if (jawOpen > 0.1f)
                    Debug.Log($"  Jaw Open: {jawOpen:F2}");
                if (mouthPout > 0.1f)
                    Debug.Log($"  Mouth Pout: {mouthPout:F2}");
                if (mouthRaiserLeft > 0.1f || mouthRaiserRight > 0.1f)
                    Debug.Log($"  Smile - L: {mouthRaiserLeft:F2}, R: {mouthRaiserRight:F2}");
            }
        }
        else
        {
            Debug.Log("Lip tracking: No data");
        }
        
        // Try to get eye expressions
        float[] eyeExpressions;
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeExpressions))
        {
            if (eyeExpressions != null && eyeExpressions.Length > 0)
            {
                Debug.Log("Eye Expressions:");
                
                // Check specific expressions
                float leftBlink = eyeExpressions[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC];
                float rightBlink = eyeExpressions[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC];
                float leftWide = eyeExpressions[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC];
                float rightWide = eyeExpressions[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_WIDE_HTC];
                
                if (leftBlink > 0.1f || rightBlink > 0.1f)
                    Debug.Log($"  Blink - L: {leftBlink:F2}, R: {rightBlink:F2}");
                if (leftWide > 0.1f || rightWide > 0.1f)
                    Debug.Log($"  Eyes Wide - L: {leftWide:F2}, R: {rightWide:F2}");
            }
        }
        else
        {
            Debug.Log("Eye tracking: No data (likely not supported on this device)");
        }
    }
} 