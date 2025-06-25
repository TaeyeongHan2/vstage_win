using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using VIVE.OpenXR.Samples.FacialTracking;
using System.Collections;
using UnityEngine.XR.Management;
using System.Linq;

/// <summary>
/// VIVE Facial Tracker 전용 테스트 스크립트
/// VIVE Pro 2 + VIVE Facial Tracker 조합에 최적화
/// </summary>
public class VIVEFacialTrackerTest : MonoBehaviour
{
    [Header("Status")]
    public string currentStatus = "Not Started";
    public bool isSessionCreated = false;
    public bool isEyeTrackingSupported = false;
    public bool isLipTrackingSupported = false;
    
    [Header("Debug Options")]
    public bool verboseLogging = true;
    public bool autoRetry = true;
    public int maxRetryAttempts = 3;
    
    private ViveFacialTracking facialTrackingFeature;
    private float[] eyeBlendShapes;
    private float[] lipBlendShapes;
    private int retryCount = 0;
    
    void Start()
    {
        StartCoroutine(InitializationSequence());
    }
    
    IEnumerator InitializationSequence()
    {
        LogInfo("🚀 Starting VIVE Facial Tracker initialization...");
        currentStatus = "Initializing...";
        
        // 1. 기본 시스템 체크
        yield return CheckBasicRequirements();
        
        // 2. OpenXR 초기화 대기
        yield return WaitForOpenXRInitialization();
        
        // 3. 세션 생성 대기
        yield return WaitForSessionCreation();
        
        // 4. Facial Tracking 초기화
        yield return InitializeFacialTracking();
    }
    
    IEnumerator CheckBasicRequirements()
    {
        currentStatus = "Checking requirements...";
        
        // OpenXR 로더 확인
        if (XRGeneralSettings.Instance == null || 
            XRGeneralSettings.Instance.Manager == null ||
            XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            LogError("❌ XR Loader not active! Check XR Plug-in Management settings.");
            yield break;
        }
        
        // Mock Runtime 체크
        var mockRuntime = OpenXRSettings.Instance?.GetFeature<VIVE.OpenXR.Feature.ViveMockRuntime>();
        if (mockRuntime != null && mockRuntime.enabled)
        {
            LogWarning("⚠️ Mock Runtime is enabled! Disable it for real device testing.");
            LogInfo("Project Settings > XR Plug-in Management > OpenXR > VIVE OpenXR > Mock Runtime");
        }
        
        yield return null;
    }
    
    IEnumerator WaitForOpenXRInitialization()
    {
        currentStatus = "Waiting for OpenXR...";
        float timeout = 10f;
        float elapsed = 0f;
        
        while (OpenXRSettings.Instance == null)
        {
            if (elapsed > timeout)
            {
                LogError("❌ OpenXR initialization timeout!");
                yield break;
            }
            
            elapsed += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
        
        LogInfo("✅ OpenXR initialized");
        
        // Feature 목록 출력
        if (verboseLogging)
        {
            LogInfo("Checking enabled OpenXR features...");
            
            // ViveFacialTracking feature 확인
            var facialFeature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
            if (facialFeature != null && facialFeature.enabled)
            {
                LogInfo("✅ ViveFacialTracking feature is enabled!");
            }
            else
            {
                LogWarning("⚠️ ViveFacialTracking feature not found or disabled");
            }
            
            // 다른 VIVE 기능들 확인
            var mockRuntime = OpenXRSettings.Instance.GetFeature<VIVE.OpenXR.Feature.ViveMockRuntime>();
            if (mockRuntime != null && mockRuntime.enabled)
            {
                LogWarning("⚠️ Mock Runtime is still enabled!");
            }
        }
    }
    
    IEnumerator WaitForSessionCreation()
    {
        currentStatus = "Waiting for XR Session...";
        
        // OpenXR의 세션 상태를 확인하는 더 나은 방법
        // XR 디바이스가 연결될 때까지 대기
        float timeout = 15f;
        float elapsed = 0f;
        
        while (!IsXRDevicePresent())
        {
            if (elapsed > timeout)
            {
                LogError("❌ XR Device connection timeout!");
                yield break;
            }
            
            LogInfo($"Waiting for XR device... ({elapsed:F1}s)");
            elapsed += 1f;
            yield return new WaitForSeconds(1f);
        }
        
        // 추가 안정화 시간
        LogInfo("XR Device detected, stabilizing...");
        yield return new WaitForSeconds(3f);
        
        isSessionCreated = true;
        LogInfo("✅ XR Session ready");
    }
    
    bool IsXRDevicePresent()
    {
        var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
            UnityEngine.XR.InputDeviceCharacteristics.HeadMounted, devices);
        return devices.Count > 0;
    }
    
    IEnumerator InitializeFacialTracking()
    {
        currentStatus = "Initializing Facial Tracking...";
        
        // Feature 가져오기
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null)
        {
            LogError("❌ ViveFacialTracking feature not found!");
            LogInfo("Check: Project Settings > XR Plug-in Management > OpenXR > VIVE OpenXR > Facial Tracking");
            yield break;
        }
        
        if (!facialTrackingFeature.enabled)
        {
            LogError("❌ ViveFacialTracking feature is not enabled!");
            yield break;
        }
        
        LogInfo("✅ ViveFacialTracking feature found and enabled");
        
        // 재시도 로직으로 트래커 생성
        bool eyeCreated = false;
        bool lipCreated = false;
        
        for (int i = 0; i < maxRetryAttempts; i++)
        {
            if (!eyeCreated)
            {
                LogInfo($"Attempting to create Eye tracker... (attempt {i + 1}/{maxRetryAttempts})");
                eyeCreated = TryCreateTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, "Eye");
            }
            
            if (!lipCreated)
            {
                LogInfo($"Attempting to create Lip tracker... (attempt {i + 1}/{maxRetryAttempts})");
                lipCreated = TryCreateTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, "Lip");
            }
            
            if (eyeCreated && lipCreated) break;
            
            if (i < maxRetryAttempts - 1)
            {
                LogInfo("Retrying in 2 seconds...");
                yield return new WaitForSeconds(2f);
            }
        }
        
        isEyeTrackingSupported = eyeCreated;
        isLipTrackingSupported = lipCreated;
        
        if (eyeCreated || lipCreated)
        {
            currentStatus = "Facial Tracking Ready";
            LogInfo("🎉 Facial tracking initialized successfully!");
            LogInfo($"   Eye Tracking: {(eyeCreated ? "✅" : "❌")}");
            LogInfo($"   Lip Tracking: {(lipCreated ? "✅" : "❌")}");
            
            // 데이터 읽기 시작
            StartCoroutine(ReadFacialData());
        }
        else
        {
            currentStatus = "Facial Tracking Failed";
            LogError("❌ Failed to initialize any facial tracking!");
            LogInfo("Possible reasons:");
            LogInfo("1. VIVE Facial Tracker not connected properly");
            LogInfo("2. Driver not installed");
            LogInfo("3. SteamVR not recognizing the tracker");
            LogInfo("4. OpenXR runtime not supporting the extension");
        }
    }
    
    bool TryCreateTracker(XrFacialTrackingTypeHTC trackingType, string typeName)
    {
        try
        {
            bool created = facialTrackingFeature.CreateFacialTracker(trackingType);
            if (created)
            {
                LogInfo($"✅ {typeName} tracker created successfully!");
                return true;
            }
            else
            {
                LogWarning($"❌ Failed to create {typeName} tracker");
                return false;
            }
        }
        catch (System.Exception e)
        {
            LogError($"Exception creating {typeName} tracker: {e.Message}");
            return false;
        }
    }
    
    IEnumerator ReadFacialData()
    {
        while (isEyeTrackingSupported || isLipTrackingSupported)
        {
            // Eye data
            if (isEyeTrackingSupported)
            {
                if (facialTrackingFeature.GetFacialExpressions(
                    XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, 
                    out eyeBlendShapes))
                {
                    ProcessEyeData(eyeBlendShapes);
                }
            }
            
            // Lip data
            if (isLipTrackingSupported)
            {
                if (facialTrackingFeature.GetFacialExpressions(
                    XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, 
                    out lipBlendShapes))
                {
                    ProcessLipData(lipBlendShapes);
                }
            }
            
            yield return new WaitForSeconds(0.1f); // 10Hz update
        }
    }
    
    void ProcessEyeData(float[] shapes)
    {
        if (shapes == null || shapes.Length == 0) return;
        
        float leftBlink = shapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC];
        float rightBlink = shapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC];
        
        if (leftBlink > 0.1f || rightBlink > 0.1f)
        {
            LogInfo($"👁️ Eye: Blink L:{leftBlink:F2} R:{rightBlink:F2}");
        }
    }
    
    void ProcessLipData(float[] shapes)
    {
        if (shapes == null || shapes.Length == 0) return;
        
        float jawOpen = shapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC];
        
        if (jawOpen > 0.1f)
        {
            LogInfo($"👄 Lip: Jaw Open {jawOpen:F2}");
        }
    }
    
    void LogInfo(string message)
    {
        Debug.Log($"[VIVEFacialTracker] {message}");
    }
    
    void LogWarning(string message)
    {
        Debug.LogWarning($"[VIVEFacialTracker] {message}");
    }
    
    void LogError(string message)
    {
        Debug.LogError($"[VIVEFacialTracker] {message}");
    }
    
    void OnDestroy()
    {
        if (facialTrackingFeature != null)
        {
            if (isEyeTrackingSupported)
            {
                facialTrackingFeature.DestroyFacialTracker(
                    XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC);
            }
            
            if (isLipTrackingSupported)
            {
                facialTrackingFeature.DestroyFacialTracker(
                    XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC);
            }
        }
    }
} 