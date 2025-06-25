using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using VIVE.OpenXR.Samples.FacialTracking;

/// <summary>
/// 하이브리드 얼굴 트래킹 테스트 - Mock Runtime과 실제 기기 모두 지원
/// </summary>
public class HybridFacialTrackingTest : MonoBehaviour
{
    [Header("Mode")]
    public bool useMockData = false;
    public bool autoDetectMockRuntime = true;
    
    [Header("Mock Controls (Q/W/A/S/Z/X/C/V)")]
    public MockFacialTrackingTest mockController;
    
    [Header("Real Device")]
    private ViveFacialTracking facialTrackingFeature;
    private float[] eyeBlendShapes;
    private float[] lipBlendShapes;
    private bool isInitialized = false;
    
    void Start()
    {
        // Mock Runtime 자동 감지
        if (autoDetectMockRuntime)
        {
            var mockRuntimeFeature = OpenXRSettings.Instance?.GetFeature<VIVE.OpenXR.Feature.ViveMockRuntime>();
            if (mockRuntimeFeature != null && mockRuntimeFeature.enabled)
            {
                useMockData = true;
                Debug.LogWarning("🎮 Mock Runtime detected - switching to keyboard simulation mode");
            }
        }
        
        if (useMockData)
        {
            InitializeMockMode();
        }
        else
        {
            Invoke("InitializeRealDevice", 3f);
        }
    }
    
    void InitializeMockMode()
    {
        // Mock 컨트롤러가 없으면 자동 생성
        if (mockController == null)
        {
            mockController = gameObject.AddComponent<MockFacialTrackingTest>();
        }
        
        Debug.Log("🎮 Mock Mode Active - Use keyboard:");
        Debug.Log("   Q/W: Eye Blink, A/S: Eye Wide");
        Debug.Log("   Z: Jaw Open, X: Pout, C/V: Smile");
        isInitialized = true;
    }
    
    void InitializeRealDevice()
    {
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null || !facialTrackingFeature.enabled)
        {
            Debug.LogError("❌ ViveFacialTracking not available - switching to mock mode");
            useMockData = true;
            InitializeMockMode();
            return;
        }
        
        // 트래커 생성 시도
        bool hasSupport = false;
        
        if (facialTrackingFeature.CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC))
        {
            Debug.Log("✅ Eye tracker created");
            hasSupport = true;
        }
        
        if (facialTrackingFeature.CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC))
        {
            Debug.Log("✅ Lip tracker created");
            hasSupport = true;
        }
        
        if (!hasSupport)
        {
            Debug.LogWarning("⚠️ No facial tracking support - switching to mock mode");
            useMockData = true;
            InitializeMockMode();
            return;
        }
        
        isInitialized = true;
        Debug.Log("✅ Real device facial tracking ready!");
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        if (useMockData)
        {
            // Mock 데이터 사용
            ProcessMockData();
        }
        else
        {
            // 실제 기기 데이터 사용
            ProcessRealData();
        }
    }
    
    void ProcessMockData()
    {
        if (mockController == null) return;
        
        // Mock 데이터 가져오기
        var mockEyeData = mockController.GetMockEyeBlendShapes();
        var mockLipData = mockController.GetMockLipBlendShapes();
        
        // 여기서 mockEyeData와 mockLipData를 사용해서 아바타 업데이트 등 가능
        // 예: UpdateAvatar(mockEyeData, mockLipData);
    }
    
    void ProcessRealData()
    {
        if (facialTrackingFeature == null) return;
        
        // Eye Tracking
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeBlendShapes))
        {
            // 실제 eye 데이터 처리
            DetectEyeExpressions(eyeBlendShapes);
        }
        
        // Lip Tracking
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipBlendShapes))
        {
            // 실제 lip 데이터 처리
            DetectLipExpressions(lipBlendShapes);
        }
    }
    
    void DetectEyeExpressions(float[] shapes)
    {
        float leftBlink = shapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC];
        float rightBlink = shapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC];
        
        if (leftBlink > 0.1f || rightBlink > 0.1f)
            Debug.Log($"👁️ Blink - L: {leftBlink:F2}, R: {rightBlink:F2}");
    }
    
    void DetectLipExpressions(float[] shapes)
    {
        float jawOpen = shapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC];
        
        if (jawOpen > 0.1f)
            Debug.Log($"👄 Jaw Open: {jawOpen:F2}");
    }
    
    // 외부에서 현재 블렌드셰이프 값 가져오기
    public float[] GetCurrentEyeBlendShapes()
    {
        if (useMockData && mockController != null)
            return mockController.GetMockEyeBlendShapes();
        else
            return eyeBlendShapes ?? new float[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC];
    }
    
    public float[] GetCurrentLipBlendShapes()
    {
        if (useMockData && mockController != null)
            return mockController.GetMockLipBlendShapes();
        else
            return lipBlendShapes ?? new float[(int)XrLipShapeHTC.XR_LIP_SHAPE_MAX_ENUM_HTC];
    }
    
    void OnDestroy()
    {
        if (!useMockData && facialTrackingFeature != null && isInitialized)
        {
            facialTrackingFeature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC);
            facialTrackingFeature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC);
        }
    }
} 