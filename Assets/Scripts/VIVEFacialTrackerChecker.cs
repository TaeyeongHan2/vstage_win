using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using VIVE.OpenXR.FacialTracking;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// VIVE Facial Tracker 설정 상태를 확인하는 스크립트
/// </summary>
public class VIVEFacialTrackerChecker : MonoBehaviour
{
    [Header("Check Results")]
    [TextArea(20, 30)]
    public string checkResults = "";
    
    private StringBuilder results = new StringBuilder();
    
    void Start()
    {
        PerformAllChecks();
    }
    
    void PerformAllChecks()
    {
        results.Clear();
        results.AppendLine("=== VIVE FACIAL TRACKER SETUP CHECK ===");
        results.AppendLine($"Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        results.AppendLine();
        
        // 1. 기본 Unity/XR 설정
        CheckBasicSetup();
        
        // 2. OpenXR 설정
        CheckOpenXRSetup();
        
        // 3. VIVE 관련 설정
        CheckVIVESetup();
        
        // 4. 권장사항
        AddRecommendations();
        
        checkResults = results.ToString();
        
        // 콘솔에도 출력
        Debug.Log(checkResults);
    }
    
    void CheckBasicSetup()
    {
        results.AppendLine("[ Unity/XR Basic Setup ]");
        
        // Unity 버전
        results.AppendLine($"Unity Version: {Application.unityVersion}");
        
        // XR Management
        bool hasXRManagement = UnityEngine.XR.Management.XRGeneralSettings.Instance != null;
        results.AppendLine($"XR Management: {(hasXRManagement ? "✅ Installed" : "❌ Not Found")}");
        
        if (hasXRManagement)
        {
            var xrManager = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
            bool isInitialized = xrManager != null && xrManager.isInitializationComplete;
            results.AppendLine($"XR Initialized: {(isInitialized ? "✅ Yes" : "❌ No")}");
            
            if (xrManager?.activeLoader != null)
            {
                results.AppendLine($"Active Loader: {xrManager.activeLoader.GetType().Name}");
            }
        }
        
        // VR Support
        results.AppendLine($"VR Supported: {(UnityEngine.XR.XRSettings.enabled ? "✅ Enabled" : "❌ Disabled")}");
        
        results.AppendLine();
    }
    
    void CheckOpenXRSetup()
    {
        results.AppendLine("[ OpenXR Setup ]");
        
        bool hasOpenXR = OpenXRSettings.Instance != null;
        results.AppendLine($"OpenXR Settings: {(hasOpenXR ? "✅ Available" : "❌ Not Found")}");
        
        if (hasOpenXR)
        {
            // Mock Runtime 체크
            var mockRuntime = OpenXRSettings.Instance.GetFeature<VIVE.OpenXR.Feature.ViveMockRuntime>();
            bool isMockEnabled = mockRuntime != null && mockRuntime.enabled;
            results.AppendLine($"Mock Runtime: {(isMockEnabled ? "⚠️ ENABLED (Disable for real device!)" : "✅ Disabled")}");
            
            // Enabled Features 체크
            results.AppendLine();
            results.AppendLine("[ Enabled OpenXR Features ]");
            
            // 각 VIVE feature 체크
            CheckFeature<ViveFacialTracking>("Facial Tracking");
            CheckFeature<VIVE.OpenXR.Feature.ViveAnchor>("Anchor");
            CheckFeature<VIVE.OpenXR.Passthrough.VivePassthrough>("Passthrough");
            CheckFeature<VIVE.OpenXR.Hand.ViveHandTracking>("Hand Tracking");
            CheckFeature<VIVE.OpenXR.EyeTracker.ViveEyeTracker>("Eye Tracker");
        }
        
        results.AppendLine();
    }
    
    void CheckVIVESetup()
    {
        results.AppendLine("[ VIVE Specific Setup ]");
        
        // VIVE Facial Tracking Feature
        var facialTracking = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        results.AppendLine($"Facial Tracking Feature: {(facialTracking != null ? "✅ Found" : "❌ Not Found")}");
        
        if (facialTracking != null)
        {
            results.AppendLine($"  - Enabled: {(facialTracking.enabled ? "✅ Yes" : "❌ No")}");
            results.AppendLine($"  - Feature Type: {facialTracking.GetType().Name}");
        }
        
        // Connected devices
        results.AppendLine();
        results.AppendLine("[ Connected XR Devices ]");
        
        var devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(devices);
        
        if (devices.Count == 0)
        {
            results.AppendLine("❌ No XR devices detected!");
        }
        else
        {
            foreach (var device in devices)
            {
                results.AppendLine($"- {device.name}");
                results.AppendLine($"  Manufacturer: {device.manufacturer}");
                results.AppendLine($"  Characteristics: {device.characteristics}");
                
                // VIVE Pro 2 체크
                if (device.name.Contains("VIVE") && device.name.Contains("Pro 2"))
                {
                    results.AppendLine("  ✅ VIVE Pro 2 detected!");
                }
            }
        }
        
        results.AppendLine();
    }
    
    void CheckFeature<T>(string displayName) where T : UnityEngine.XR.OpenXR.Features.OpenXRFeature
    {
        var feature = OpenXRSettings.Instance?.GetFeature<T>();
        bool isEnabled = feature != null && feature.enabled;
        results.AppendLine($"  - {displayName}: {(isEnabled ? "✅ Enabled" : "❌ Disabled/Not Found")}");
    }
    
    void AddRecommendations()
    {
        results.AppendLine("[ Recommendations ]");
        results.AppendLine();
        results.AppendLine("1. Hardware Setup:");
        results.AppendLine("   - Connect VIVE Facial Tracker via USB");
        results.AppendLine("   - Check LED status on the tracker");
        results.AppendLine("   - Ensure VIVE Console is running");
        results.AppendLine();
        results.AppendLine("2. Software Requirements:");
        results.AppendLine("   - Install VIVE Facial Tracker Driver from VIVE Console");
        results.AppendLine("   - Run SteamVR");
        results.AppendLine("   - Check SteamVR Settings > Startup/Shutdown > Choose Startup Overlay Apps");
        results.AppendLine("   - Enable 'VIVE Facial Tracker' if available");
        results.AppendLine();
        results.AppendLine("3. Unity Settings:");
        results.AppendLine("   - Project Settings > XR Plug-in Management > OpenXR");
        results.AppendLine("   - Enable 'VIVE OpenXR' features");
        results.AppendLine("   - Check 'Facial Tracking' in VIVE OpenXR section");
        results.AppendLine("   - Disable 'Mock Runtime' for real device");
        results.AppendLine();
        results.AppendLine("4. Testing:");
        results.AppendLine("   - Use VIVEFacialTrackerTest.cs for testing");
        results.AppendLine("   - Check Console for error messages");
        results.AppendLine("   - Facial expressions should be detected when moving face");
    }
    
    [ContextMenu("Refresh Check")]
    void RefreshCheck()
    {
        PerformAllChecks();
    }
} 