using UnityEngine;
using UnityEngine.XR.Management;
using System.Collections;

/// <summary>
/// 간단한 XR 초기화 테스트
/// </summary>
public class SimpleXRInitTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Simple XR Init Test ===");
        StartCoroutine(InitXR());
    }
    
    IEnumerator InitXR()
    {
        Debug.Log("1. Checking XR Settings...");
        
        if (XRGeneralSettings.Instance == null)
        {
            Debug.LogError("❌ XR Plugin Management not installed!");
            Debug.Log("👉 Window > Package Manager > XR Plugin Management 설치");
            yield break;
        }
        
        if (XRGeneralSettings.Instance.Manager == null)
        {
            Debug.LogError("❌ XR Manager is null!");
            Debug.Log("👉 Project Settings > XR Plug-in Management > PC > OpenXR 체크");
            yield break;
        }
        
        Debug.Log("2. Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("❌ No XR Loader active!");
            Debug.Log("👉 Possible causes:");
            Debug.Log("   - SteamVR not running");
            Debug.Log("   - Headset not connected");
            Debug.Log("   - OpenXR not enabled in XR Plug-in Management");
            yield break;
        }
        
        Debug.Log("3. Starting XR...");
        XRGeneralSettings.Instance.Manager.StartSubsystems();
        
        Debug.Log("✅ XR Initialized Successfully!");
        
        // 장치 확인
        yield return new WaitForSeconds(1f);
        
        var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(devices);
        
        Debug.Log($"4. Detected {devices.Count} XR devices:");
        foreach (var device in devices)
        {
            Debug.Log($"   - {device.name}");
        }
    }
} 