using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using VIVE.OpenXR.Samples.FacialTracking;
using System.Collections;

public class SafeFacialTrackingTest : MonoBehaviour
{
    private ViveFacialTracking facialTrackingFeature;
    private float[] eyeBlendShapes;
    private float[] lipBlendShapes;
    private bool isInitialized = false;
    
    [Header("Status")]
    public bool isXRSessionReady = false;
    public bool isFacialTrackingReady = false;
    
    void Start()
    {
        // XR 세션이 준비될 때까지 대기
        StartCoroutine(WaitForXRSession());
    }
    
    IEnumerator WaitForXRSession()
    {
        Debug.Log("⏳ Waiting for initialization...");
        
        // OpenXRSettings가 null이 아닐 때까지 대기
        while (OpenXRSettings.Instance == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("✅ OpenXR settings available!");
        isXRSessionReady = true;
        
        // 추가로 2초 대기 (안정화를 위해)
        yield return new WaitForSeconds(2f);
        
        // 얼굴 트래킹 초기화
        InitializeFacialTracking();
    }
    
    void InitializeFacialTracking()
    {
        facialTrackingFeature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null)
        {
            Debug.LogError("❌ ViveFacialTracking feature is not enabled in OpenXR Settings!");
            return;
        }
        
        if (!facialTrackingFeature.enabled)
        {
            Debug.LogError("❌ ViveFacialTracking feature is disabled!");
            return;
        }
        
        // 얼굴 트래킹 트래커 생성 시도
        if (!facialTrackingFeature.CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC))
        {
            Debug.LogWarning("⚠️ Failed to create eye tracker - device may not support eye tracking");
        }
        else
        {
            Debug.Log("✅ Eye tracker created successfully");
        }
        
        if (!facialTrackingFeature.CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC))
        {
            Debug.LogWarning("⚠️ Failed to create lip tracker - device may not support lip tracking");
        }
        else
        {
            Debug.Log("✅ Lip tracker created successfully");
        }
        
        isInitialized = true;
        isFacialTrackingReady = true;
        Debug.Log("✅ Facial tracking initialization complete!");
    }
    
    void Update()
    {
        if (!isInitialized || facialTrackingFeature == null) return;
        
        // Eye Tracking 테스트
        TestEyeTracking();
        
        // Lip Tracking 테스트
        TestLipTracking();
    }
    
    void TestEyeTracking()
    {
        // GetFacialExpressions가 true를 반환하는지 확인
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeBlendShapes))
        {
            float leftBlink = eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC];
            float rightBlink = eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC];
            
            if (leftBlink > 0.1f || rightBlink > 0.1f)
            {
                Debug.Log($"👁️ Eye Blink - Left: {leftBlink:F2}, Right: {rightBlink:F2}");
            }
        }
    }
    
    void TestLipTracking()
    {
        // GetFacialExpressions가 true를 반환하는지 확인
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipBlendShapes))
        {
            float jawOpen = lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC];
            
            if (jawOpen > 0.1f)
            {
                Debug.Log($"👄 Jaw Open: {jawOpen:F2}");
            }
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && isInitialized)
        {
            Debug.Log("🔄 Application resumed - reinitializing facial tracking...");
            StartCoroutine(WaitForXRSession());
        }
    }
    
    void OnDestroy()
    {
        if (facialTrackingFeature != null && isInitialized)
        {
            // 트래커 정리
            facialTrackingFeature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC);
            facialTrackingFeature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC);
            Debug.Log("🧹 Facial trackers destroyed");
        }
    }
} 