using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using VIVE.OpenXR.Samples.FacialTracking;
using System.Collections;
using UnityEngine.XR.Management;

/// <summary>
/// 개선된 하이브리드 얼굴 트래킹 - XR 세션 상태를 확인하고 초기화
/// </summary>
public class ImprovedHybridFacialTracking : MonoBehaviour
{
    [Header("Mode")]
    public bool useMockData = false;
    public bool autoDetectMockRuntime = true;
    
    [Header("Initialization")]
    public float maxWaitTime = 10f; // 최대 대기 시간
    
    [Header("Status")]
    public bool isXRSessionReady = false;
    public bool isFacialTrackingReady = false;
    public string currentStatus = "Not Started";
    
    [Header("Mock Controls")]
    private MockFacialTrackingTest mockController;
    
    [Header("Real Device")]
    private ViveFacialTracking facialTrackingFeature;
    private float[] eyeBlendShapes;
    private float[] lipBlendShapes;
    private bool isInitialized = false;
    
    void Start()
    {
        currentStatus = "Starting...";
        StartCoroutine(InitializeSequence());
    }
    
    IEnumerator InitializeSequence()
    {
        // 1. Mock Runtime 체크
        currentStatus = "Checking Mock Runtime...";
        yield return null;
        
        var mockRuntimeFeature = OpenXRSettings.Instance?.GetFeature<VIVE.OpenXR.Feature.ViveMockRuntime>();
        if (mockRuntimeFeature != null && mockRuntimeFeature.enabled)
        {
            Debug.LogWarning("🎮 Mock Runtime detected - switching to keyboard simulation");
            useMockData = true;
            InitializeMockMode();
            yield break;
        }
        
        // 2. XR System 초기화 대기
        currentStatus = "Waiting for XR System...";
        float waitStartTime = Time.time;
        
        while (!XRGeneralSettings.Instance || 
               XRGeneralSettings.Instance.Manager == null || 
               !XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            if (Time.time - waitStartTime > maxWaitTime)
            {
                Debug.LogError("❌ XR System initialization timeout - switching to mock mode");
                useMockData = true;
                InitializeMockMode();
                yield break;
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("✅ XR System initialized");
        
        // 3. OpenXR Session 대기
        currentStatus = "Waiting for OpenXR Session...";
        while (OpenXRSettings.Instance == null)
        {
            if (Time.time - waitStartTime > maxWaitTime)
            {
                Debug.LogError("❌ OpenXR Session timeout - switching to mock mode");
                useMockData = true;
                InitializeMockMode();
                yield break;
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("✅ OpenXR Session ready");
        isXRSessionReady = true;
        
        // 4. 추가 안정화 대기
        currentStatus = "Stabilizing...";
        yield return new WaitForSeconds(2f);
        
        // 5. 얼굴 트래킹 초기화
        currentStatus = "Initializing Facial Tracking...";
        InitializeRealDevice();
    }
    
    void InitializeMockMode()
    {
        currentStatus = "Mock Mode Active";
        
        if (mockController == null)
        {
            mockController = gameObject.AddComponent<MockFacialTrackingTest>();
        }
        
        Debug.Log("🎮 Mock Mode Active - Keyboard Controls:");
        Debug.Log("   Q/W: Eye Blink | A/S: Eye Wide");
        Debug.Log("   Z: Jaw Open | X: Pout | C/V: Smile");
        Debug.Log("   Space: Reset | Enter: Auto Animation");
        
        isInitialized = true;
    }
    
    void InitializeRealDevice()
    {
        try
        {
            facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
            
            if (facialTrackingFeature == null || !facialTrackingFeature.enabled)
            {
                Debug.LogError("❌ ViveFacialTracking feature not available/enabled");
                currentStatus = "Feature Not Available - Using Mock";
                useMockData = true;
                InitializeMockMode();
                return;
            }
            
            // 기능 지원 체크
            bool eyeSupported = CheckAndCreateTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, "Eye");
            bool lipSupported = CheckAndCreateTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, "Lip");
            
            if (!eyeSupported && !lipSupported)
            {
                Debug.LogWarning("⚠️ No facial tracking support detected");
                currentStatus = "No Tracking Support - Using Mock";
                useMockData = true;
                InitializeMockMode();
                return;
            }
            
            currentStatus = "Real Device Ready";
            isFacialTrackingReady = true;
            isInitialized = true;
            
            Debug.Log("✅ Real device facial tracking initialized!");
            Debug.Log($"   Eye Tracking: {(eyeSupported ? "✅" : "❌")}");
            Debug.Log($"   Lip Tracking: {(lipSupported ? "✅" : "❌")}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Exception during initialization: {e.Message}");
            currentStatus = "Initialization Failed - Using Mock";
            useMockData = true;
            InitializeMockMode();
        }
    }
    
    bool CheckAndCreateTracker(XrFacialTrackingTypeHTC trackingType, string typeName)
    {
        try
        {
            // 먼저 지원 여부 체크 (세션이 준비되었는지 확인)
            if (!isXRSessionReady)
            {
                Debug.LogWarning($"XR Session not ready for {typeName} tracking check");
                return false;
            }
            
            // 트래커 생성 시도
            bool created = facialTrackingFeature.CreateFacialTracker(trackingType);
            if (created)
            {
                Debug.Log($"✅ {typeName} tracker created successfully");
                return true;
            }
            else
            {
                Debug.LogWarning($"❌ Failed to create {typeName} tracker");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception checking {typeName} tracker: {e.Message}");
            return false;
        }
    }
    
    // Update와 나머지 메서드들은 기존과 동일...
    void Update()
    {
        if (!isInitialized) return;
        
        if (useMockData && mockController != null)
        {
            // Mock 데이터는 MockFacialTrackingTest가 자체적으로 로그 출력
        }
        else if (facialTrackingFeature != null)
        {
            ProcessRealData();
        }
    }
    
    void ProcessRealData()
    {
        // Eye Tracking
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeBlendShapes))
        {
            DetectEyeExpressions(eyeBlendShapes);
        }
        
        // Lip Tracking
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipBlendShapes))
        {
            DetectLipExpressions(lipBlendShapes);
        }
    }
    
    void DetectEyeExpressions(float[] shapes)
    {
        if (shapes == null || shapes.Length == 0) return;
        
        float leftBlink = shapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC];
        float rightBlink = shapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC];
        
        if (leftBlink > 0.1f || rightBlink > 0.1f)
            Debug.Log($"👁️ Blink - L: {leftBlink:F2}, R: {rightBlink:F2}");
    }
    
    void DetectLipExpressions(float[] shapes)
    {
        if (shapes == null || shapes.Length == 0) return;
        
        float jawOpen = shapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC];
        
        if (jawOpen > 0.1f)
            Debug.Log($"👄 Jaw Open: {jawOpen:F2}");
    }
    
    void OnDestroy()
    {
        if (!useMockData && facialTrackingFeature != null && isInitialized)
        {
            try
            {
                facialTrackingFeature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC);
                facialTrackingFeature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC);
            }
            catch { }
        }
    }
} 