using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;

public class SimpleLipTracker : MonoBehaviour
{
    private ViveFacialTracking facialTrackingFeature;
    private float lastLogTime = 0f;
    
    void Start()
    {
        Debug.Log("[SimpleLipTracker] Starting...");
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null || !facialTrackingFeature.enabled)
        {
            Debug.LogError("[SimpleLipTracker] ViveFacialTracking feature not found or not enabled!");
            return;
        }
        
        Debug.Log("[SimpleLipTracker] ✅ Ready to track lip movements!");
    }
    
    void Update()
    {
        if (facialTrackingFeature == null) return;
        
        // Log every 0.5 seconds
        if (Time.time - lastLogTime < 0.5f) return;
        lastLogTime = Time.time;
        
        float[] lipExpressions;
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipExpressions))
        {
            if (lipExpressions != null && lipExpressions.Length > 0)
            {
                float jawOpen = lipExpressions[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC];
                if (jawOpen > 0.1f)
                {
                    Debug.Log($"[SimpleLipTracker] 👄 Jaw Open: {jawOpen:F2}");
                }
            }
        }
    }
} 