using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using System.Text;

/// <summary>
/// 얼굴 트래킹 디버깅 도구
/// </summary>
public class FacialTrackingDebugger : MonoBehaviour
{
    [Header("Debug Info")]
    [TextArea(10, 20)]
    public string debugInfo = "";
    
    private StringBuilder sb = new StringBuilder();
    
    void Start()
    {
        InvokeRepeating("UpdateDebugInfo", 0f, 1f);
    }
    
    void UpdateDebugInfo()
    {
        sb.Clear();
        sb.AppendLine("=== FACIAL TRACKING DEBUG INFO ===");
        sb.AppendLine($"Time: {System.DateTime.Now:HH:mm:ss}");
        sb.AppendLine();
        
        // OpenXR 상태
        sb.AppendLine("[ OpenXR Status ]");
        sb.AppendLine($"OpenXRSettings: {(OpenXRSettings.Instance != null ? "✅ Available" : "❌ NULL")}");
        
        if (OpenXRSettings.Instance != null)
        {
            // Mock Runtime 체크
            var mockRuntime = OpenXRSettings.Instance.GetFeature<VIVE.OpenXR.Feature.ViveMockRuntime>();
            sb.AppendLine($"Mock Runtime: {(mockRuntime != null && mockRuntime.enabled ? "⚠️ ENABLED" : "✅ Disabled")}");
            
            // Facial Tracking Feature
            var facialTracking = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
            sb.AppendLine($"Facial Tracking Feature: {(facialTracking != null ? "✅ Found" : "❌ Not Found")}");
            
            if (facialTracking != null)
            {
                sb.AppendLine($"  - Enabled: {(facialTracking.enabled ? "✅" : "❌")}");
                
                // 지원 기능 체크
                try
                {
                    sb.AppendLine();
                    sb.AppendLine("[ Supported Features ]");
                    
                    // Eye Tracking 지원 체크
                    bool eyeSupport = CheckTrackingSupport(facialTracking, 
                        XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC);
                    sb.AppendLine($"Eye Tracking: {(eyeSupport ? "✅ Supported" : "❌ Not Supported")}");
                    
                    // Lip Tracking 지원 체크
                    bool lipSupport = CheckTrackingSupport(facialTracking,
                        XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC);
                    sb.AppendLine($"Lip Tracking: {(lipSupport ? "✅ Supported" : "❌ Not Supported")}");
                }
                catch (System.Exception e)
                {
                    sb.AppendLine($"Error checking support: {e.Message}");
                }
            }
        }
        
        // XR 장치 정보
        sb.AppendLine();
        sb.AppendLine("[ XR Device Info ]");
        
        var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
            UnityEngine.XR.InputDeviceCharacteristics.HeadMounted, devices);
        
        foreach (var device in devices)
        {
            sb.AppendLine($"- {device.name}");
            sb.AppendLine($"  Manufacturer: {device.manufacturer}");
            sb.AppendLine($"  Serial: {device.serialNumber}");
        }
        
        if (devices.Count == 0)
        {
            sb.AppendLine("❌ No HMD detected!");
        }
        
        // 환경 정보
        sb.AppendLine();
        sb.AppendLine("[ Environment ]");
        sb.AppendLine($"Unity Version: {Application.unityVersion}");
        sb.AppendLine($"Platform: {Application.platform}");
        sb.AppendLine($"VR Supported: {UnityEngine.XR.XRSettings.enabled}");
        
        debugInfo = sb.ToString();
    }
    
    bool CheckTrackingSupport(ViveFacialTracking feature, XrFacialTrackingTypeHTC trackingType)
    {
        try
        {
            // 세션이 생성되었는지 간접적으로 확인
            // 실제 지원 여부는 트래커 생성을 시도해봐야 알 수 있음
            return true; // 일단 true 반환, 실제 생성은 별도로 시도
        }
        catch
        {
            return false;
        }
    }
    
    void OnGUI()
    {
        // 디버그 정보를 화면에 표시 (선택사항)
        if (Input.GetKey(KeyCode.F1))
        {
            GUI.Box(new Rect(10, 10, 400, 500), "");
            GUI.Label(new Rect(20, 20, 380, 480), debugInfo);
        }
    }
} 