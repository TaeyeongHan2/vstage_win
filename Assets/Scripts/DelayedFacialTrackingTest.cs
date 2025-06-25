using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using VIVE.OpenXR.Samples.FacialTracking;

public class DelayedFacialTrackingTest : MonoBehaviour
{
    private ViveFacialTracking facialTrackingFeature;
    private float[] eyeBlendShapes;
    private float[] lipBlendShapes;
    private float initializationTime = 5f; // 5초 후에 시작 (XR 세션 완전 초기화 대기)
    private bool isInitialized = false;
    
    void Start()
    {
        Debug.Log("🚀 Facial tracking test will start in " + initializationTime + " seconds...");
        Invoke("InitializeFacialTracking", initializationTime);
    }
    
    void InitializeFacialTracking()
    {
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null)
        {
            Debug.LogError("❌ ViveFacialTracking feature is not available!");
            Debug.LogError("Please check: Edit > Project Settings > XR Plug-in Management > OpenXR > VIVE Facial Tracking");
            return;
        }
        
        if (!facialTrackingFeature.enabled)
        {
            Debug.LogError("❌ ViveFacialTracking feature is not enabled!");
            return;
        }
        
        // 지원되는 기능 확인
        Debug.Log("📋 Checking supported features...");
        
        // Eye Tracking 확인
        bool eyeSupported = facialTrackingFeature.CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC);
        Debug.Log($"   Eye Tracking: {(eyeSupported ? "✅ Supported" : "❌ Not Supported")}");
        
        // Lip Tracking 확인
        bool lipSupported = facialTrackingFeature.CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC);
        Debug.Log($"   Lip Tracking: {(lipSupported ? "✅ Supported" : "❌ Not Supported")}");
        
        if (!eyeSupported && !lipSupported)
        {
            Debug.LogWarning("⚠️ No facial tracking features are supported on this device!");
            Debug.LogWarning("   - VIVE Pro 2 requires VIVE Full Face Tracker accessory");
            Debug.LogWarning("   - VIVE Pro Eye only supports eye tracking");
            Debug.LogWarning("   - VIVE Focus 3 requires Eye & Facial Tracker accessory");
            return;
        }
        
        isInitialized = true;
        Debug.Log("✅ Facial tracking initialized successfully!");
    }
    
    void Update()
    {
        if (!isInitialized || facialTrackingFeature == null) return;
        
        // Eye Tracking 데이터 읽기
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeBlendShapes))
        {
            // 깜빡임 확인
            float leftBlink = eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC];
            float rightBlink = eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC];
            
            if (leftBlink > 0.1f)
                Debug.Log($"👁️ Left Eye Blink: {leftBlink:F2}");
            if (rightBlink > 0.1f)
                Debug.Log($"👁️ Right Eye Blink: {rightBlink:F2}");
                
            // 눈 크게 뜨기 확인
            float leftWide = eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC];
            float rightWide = eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_WIDE_HTC];
            
            if (leftWide > 0.1f || rightWide > 0.1f)
                Debug.Log($"👁️ Eyes Wide - Left: {leftWide:F2}, Right: {rightWide:F2}");
        }
        
        // Lip Tracking 데이터 읽기
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipBlendShapes))
        {
            // 입 열기
            float jawOpen = lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC];
            if (jawOpen > 0.1f)
                Debug.Log($"👄 Jaw Open: {jawOpen:F2}");
                
            // 입술 내밀기
            float pout = lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_POUT_HTC];
            if (pout > 0.1f)
                Debug.Log($"👄 Mouth Pout: {pout:F2}");
                
            // 웃기 (입꼬리 올리기)
            float raiserLeft = lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_RAISER_LEFT_HTC];
            float raiserRight = lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_RAISER_RIGHT_HTC];
            if (raiserLeft > 0.1f || raiserRight > 0.1f)
                Debug.Log($"😊 Smile - Left: {raiserLeft:F2}, Right: {raiserRight:F2}");
        }
    }
    
    void OnDestroy()
    {
        if (facialTrackingFeature != null && isInitialized)
        {
            facialTrackingFeature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC);
            facialTrackingFeature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC);
            Debug.Log("🧹 Facial trackers cleaned up");
        }
    }
} 