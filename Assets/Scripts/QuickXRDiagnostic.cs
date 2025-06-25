using UnityEngine;
using UnityEngine.XR.Management;

/// <summary>
/// 빠른 XR 진단 도구 - 현재 설정 상태를 즉시 확인
/// </summary>
public class QuickXRDiagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 🔍 Quick XR Diagnostic ===");
        Debug.Log($"Unity Version: {Application.unityVersion}");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log("");
        
        // 1. XR Plugin Management 체크
        Debug.Log("[ XR Plugin Management ]");
        if (XRGeneralSettings.Instance == null)
        {
            Debug.LogError("❌ NOT INSTALLED - Install via Package Manager!");
            return;
        }
        Debug.Log("✅ Installed");
        
        // 2. XR Manager 체크
        Debug.Log("");
        Debug.Log("[ XR Manager ]");
        if (XRGeneralSettings.Instance.Manager == null)
        {
            Debug.LogError("❌ NOT CONFIGURED - Enable OpenXR in XR Plug-in Management!");
            Debug.Log("👉 Edit > Project Settings > XR Plug-in Management > PC tab > Check 'OpenXR'");
            return;
        }
        Debug.Log("✅ Configured");
        
        // 3. Active Loaders 체크
        Debug.Log("");
        Debug.Log("[ XR Loaders ]");
        var loaders = XRGeneralSettings.Instance.Manager.activeLoaders;
        Debug.Log($"Active Loader Count: {loaders.Count}");
        
        if (loaders.Count == 0)
        {
            Debug.LogError("❌ NO LOADERS - OpenXR not enabled!");
            Debug.Log("👉 Project Settings > XR Plug-in Management > PC > Enable 'OpenXR'");
            Debug.Log("👉 Then RESTART Unity!");
        }
        else
        {
            foreach (var loader in loaders)
            {
                Debug.Log($"  ✅ {loader.GetType().Name}");
            }
        }
        
        // 4. 초기화 상태
        Debug.Log("");
        Debug.Log("[ Initialization Status ]");
        bool isInit = XRGeneralSettings.Instance.Manager.isInitializationComplete;
        Debug.Log($"Is Initialized: {(isInit ? "✅ Yes" : "❌ No")}");
        
        if (!isInit)
        {
            Debug.Log("👉 Initialization will happen when entering Play Mode");
            Debug.Log("👉 Make sure:");
            Debug.Log("   1. SteamVR is running");
            Debug.Log("   2. VIVE headset is connected");
            Debug.Log("   3. OpenXR runtime is set to SteamVR");
        }
        
        // 5. XR Settings
        Debug.Log("");
        Debug.Log("[ XR Settings ]");
        Debug.Log($"XR Enabled: {UnityEngine.XR.XRSettings.enabled}");
        Debug.Log($"Loaded Device: {UnityEngine.XR.XRSettings.loadedDeviceName}");
        
        Debug.Log("");
        Debug.Log("=== Diagnostic Complete ===");
        
        if (loaders.Count == 0)
        {
            Debug.LogError("🚨 ACTION REQUIRED: Enable OpenXR in XR Plug-in Management and restart Unity!");
        }
    }
} 