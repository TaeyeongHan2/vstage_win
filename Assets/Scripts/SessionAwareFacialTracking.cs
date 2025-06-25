using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using VIVE.OpenXR.Samples.FacialTracking;
using System.Collections;
using UnityEngine.XR;

public class SessionAwareFacialTracking : MonoBehaviour
{
    private ViveFacialTracking facialTrackingFeature;
    private float[] eyeBlendShapes;
    private float[] lipBlendShapes;
    private bool isInitialized = false;
    
    [Header("Status")]
    public bool isXRDeviceConnected = false;
    public bool isOpenXRReady = false;
    public bool isMockRuntime = false;
    
    void Start()
    {
        StartCoroutine(InitializeWhenReady());
    }
    
    IEnumerator InitializeWhenReady()
    {
        Debug.Log("🔍 Checking XR environment...");
        
        // OpenXR이 초기화될 때까지 대기
        while (OpenXRSettings.Instance == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        // Mock Runtime 확인
        var mockRuntimeFeature = OpenXRSettings.Instance.GetFeature<VIVE.OpenXR.Feature.ViveMockRuntime>();
        if (mockRuntimeFeature != null && mockRuntimeFeature.enabled)
        {
            isMockRuntime = true;
            Debug.LogWarning("⚠️ Mock Runtime is enabled! Facial tracking won't work in simulation mode.");
            Debug.LogWarning("   To use real device: Project Settings > XR Plug-in Management > OpenXR > Disable Mock Runtime");
            Debug.LogWarning("   To test without hardware: Use MockFacialTrackingTest.cs instead");
            yield break;
        }
        
        // XR 장치가 연결될 때까지 대기
        Debug.Log("⏳ Waiting for XR device...");
        float timeout = 10f;
        float elapsed = 0f;
        
        while (!XRSettings.isDeviceActive && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
            
            // 장치 정보 출력
            if (elapsed % 2f == 0)
            {
                var devices = new System.Collections.Generic.List<InputDevice>();
                InputDevices.GetDevices(devices);
                Debug.Log($"   Found {devices.Count} XR devices");
                foreach (var device in devices)
                {
                    Debug.Log($"   - {device.name}");
                }
            }
        }
        
        if (!XRSettings.isDeviceActive)
        {
            Debug.LogError("❌ No XR device detected after " + timeout + " seconds!");
            Debug.LogError("   Please make sure your VR headset is connected and SteamVR is running");
            yield break;
        }
        
        isXRDeviceConnected = true;
        Debug.Log("✅ XR device connected: " + XRSettings.loadedDeviceName);
        
        // 추가로 2초 대기 (세션 안정화)
        yield return new WaitForSeconds(2f);
        
        // 얼굴 트래킹 초기화
        InitializeFacialTracking();
    }
    
    void InitializeFacialTracking()
    {
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null)
        {
            Debug.LogError("❌ ViveFacialTracking feature not found!");
            return;
        }
        
        if (!facialTrackingFeature.enabled)
        {
            Debug.LogError("❌ ViveFacialTracking feature is disabled!");
            return;
        }
        
        Debug.Log("📋 Initializing Facial Tracking...");
        
        // Eye Tracking 시도
        bool eyeSupported = false;
        try
        {
            eyeSupported = facialTrackingFeature.CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC);
            Debug.Log($"   Eye Tracking: {(eyeSupported ? "✅ Created" : "❌ Failed")}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Eye Tracking Error: {e.Message}");
        }
        
        // Lip Tracking 시도
        bool lipSupported = false;
        try
        {
            lipSupported = facialTrackingFeature.CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC);
            Debug.Log($"   Lip Tracking: {(lipSupported ? "✅ Created" : "❌ Failed")}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Lip Tracking Error: {e.Message}");
        }
        
        if (!eyeSupported && !lipSupported)
        {
            Debug.LogWarning("⚠️ No facial tracking features available!");
            Debug.LogWarning("   Possible reasons:");
            Debug.LogWarning("   1. Device doesn't support facial tracking (VIVE Pro 2 needs Face Tracker accessory)");
            Debug.LogWarning("   2. Facial tracking drivers not installed");
            Debug.LogWarning("   3. SteamVR not properly configured");
            return;
        }
        
        isInitialized = true;
        isOpenXRReady = true;
        Debug.Log("✅ Facial tracking ready!");
    }
    
    void Update()
    {
        if (!isInitialized || facialTrackingFeature == null) return;
        
        // 여기서 실제 얼굴 트래킹 데이터 읽기 (이전 코드와 동일)
        TestFacialTracking();
    }
    
    void TestFacialTracking()
    {
        // Eye Tracking
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeBlendShapes))
        {
            float leftBlink = eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC];
            float rightBlink = eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC];
            
            if (leftBlink > 0.1f || rightBlink > 0.1f)
                Debug.Log($"👁️ Blink - L: {leftBlink:F2}, R: {rightBlink:F2}");
        }
        
        // Lip Tracking
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipBlendShapes))
        {
            float jawOpen = lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC];
            
            if (jawOpen > 0.1f)
                Debug.Log($"👄 Jaw Open: {jawOpen:F2}");
        }
    }
    
    void OnDestroy()
    {
        if (facialTrackingFeature != null && isInitialized)
        {
            facialTrackingFeature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC);
            facialTrackingFeature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC);
        }
    }
} 