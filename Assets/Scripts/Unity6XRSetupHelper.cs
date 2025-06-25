using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Unity 6 XR 설정 도우미
/// </summary>
public class Unity6XRSetupHelper : MonoBehaviour
{
    [Header("Setup Status")]
    public bool isXRActive = false;
    public string currentStatus = "Not Started";
    
    void Start()
    {
        StartCoroutine(CheckAndInitializeXR());
    }
    
    IEnumerator CheckAndInitializeXR()
    {
        currentStatus = "Checking XR setup...";
        Debug.Log("🔍 Unity6 XR Setup Helper Started");
        Debug.Log($"Unity Version: {Application.unityVersion}");
        
        // 1. XR Settings 확인
        CheckXRSettings();
        
        // 2. XR 초기화 시도
        yield return AttemptXRInitialization();
        
        // 3. 결과 출력
        PrintDiagnostics();
    }
    
    void CheckXRSettings()
    {
        Debug.Log("\n=== XR Settings Check ===");
        
        // XR 활성화 상태
        Debug.Log($"XR Enabled: {UnityEngine.XR.XRSettings.enabled}");
        Debug.Log($"Loaded Device Name: {UnityEngine.XR.XRSettings.loadedDeviceName}");
        
        // Build Target 확인
        #if UNITY_EDITOR
        Debug.Log($"Current Build Target: {EditorUserBuildSettings.activeBuildTarget}");
        #endif
        
        // XR Management 확인
        if (XRGeneralSettings.Instance == null)
        {
            Debug.LogError("❌ XRGeneralSettings.Instance is null!");
            Debug.Log("Solution: Project Settings > XR Plug-in Management > Install XR Plugin Management");
            return;
        }
        
        if (XRGeneralSettings.Instance.Manager == null)
        {
            Debug.LogError("❌ XR Manager is null!");
            Debug.Log("Solution: Project Settings > XR Plug-in Management > PC tab > Check 'OpenXR'");
            return;
        }
        
        // Active Loader 확인
        var activeLoader = XRGeneralSettings.Instance.Manager.activeLoader;
        if (activeLoader == null)
        {
            Debug.LogWarning("⚠️ No active XR loader!");
            
            // 사용 가능한 로더 목록
            var loaders = XRGeneralSettings.Instance.Manager.activeLoaders;
            Debug.Log($"Available loaders count: {loaders.Count}");
            foreach (var loader in loaders)
            {
                Debug.Log($"  - {loader.GetType().Name}");
            }
        }
        else
        {
            Debug.Log($"✅ Active Loader: {activeLoader.GetType().Name}");
        }
    }
    
    IEnumerator AttemptXRInitialization()
    {
        currentStatus = "Attempting XR initialization...";
        Debug.Log("\n=== XR Initialization ===");
        
        var xrManager = XRGeneralSettings.Instance?.Manager;
        if (xrManager == null)
        {
            Debug.LogError("❌ Cannot initialize - XR Manager is null");
            yield break;
        }
        
        // 이미 초기화되어 있는지 확인
        if (xrManager.isInitializationComplete)
        {
            Debug.Log("✅ XR is already initialized");
            isXRActive = true;
            yield break;
        }
        
        // 수동 초기화 시도
        Debug.Log("Attempting manual XR initialization...");
        yield return xrManager.InitializeLoader();
        
        if (xrManager.activeLoader != null)
        {
            Debug.Log("✅ XR Loader initialized successfully!");
            xrManager.StartSubsystems();
            isXRActive = true;
            
            // OpenXR 특정 정보
            if (OpenXRSettings.Instance != null)
            {
                Debug.Log("\n=== OpenXR Info ===");
                Debug.Log($"Render Mode: {OpenXRSettings.Instance.renderMode}");
                Debug.Log($"Depth Submission Mode: {OpenXRSettings.Instance.depthSubmissionMode}");
            }
        }
        else
        {
            Debug.LogError("❌ Failed to initialize XR loader");
            Debug.Log("\nPossible solutions:");
            Debug.Log("1. Check if SteamVR is running");
            Debug.Log("2. Ensure VIVE headset is connected");
            Debug.Log("3. Project Settings > XR Plug-in Management > PC > Enable 'OpenXR'");
            Debug.Log("4. Edit > Project Settings > Player > XR Settings > Virtual Reality Supported (if available)");
        }
        
        currentStatus = isXRActive ? "XR Active" : "XR Failed";
    }
    
    void PrintDiagnostics()
    {
        Debug.Log("\n=== XR Diagnostics ===");
        
        // 현재 XR 장치 목록
        var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(devices);
        
        Debug.Log($"Connected XR Devices: {devices.Count}");
        foreach (var device in devices)
        {
            Debug.Log($"  - {device.name} ({device.characteristics})");
        }
        
        // OpenXR Runtime 정보 (간단 버전)
        #if UNITY_EDITOR
        Debug.Log("\nTo check OpenXR Runtime:");
        Debug.Log("1. Open SteamVR Settings");
        Debug.Log("2. Go to Developer tab");
        Debug.Log("3. Check 'Current OpenXR Runtime'");
        Debug.Log("4. Should be set to SteamVR");
        #endif
    }
    
    [ContextMenu("Force XR Initialization")]
    void ForceInitXR()
    {
        StartCoroutine(AttemptXRInitialization());
    }
    
    [ContextMenu("Print Current Status")]
    void PrintStatus()
    {
        PrintDiagnostics();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Unity6XRSetupHelper))]
public class Unity6XRSetupHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Open XR Plug-in Management"))
        {
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        }
        
        if (GUILayout.Button("Open Player Settings"))
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }
        
        if (GUILayout.Button("Check Windows OpenXR Runtime"))
        {
            CheckOpenXRRuntime();
        }
    }
    
    void CheckOpenXRRuntime()
    {
        EditorUtility.DisplayDialog("OpenXR Runtime Check",
            "To check/set OpenXR Runtime:\n\n" +
            "1. Make sure SteamVR is running\n" +
            "2. Right-click SteamVR tray icon\n" +
            "3. Select 'Settings'\n" +
            "4. Go to 'Developer' tab\n" +
            "5. Click 'Set SteamVR as OpenXR Runtime'\n\n" +
            "Current status can be seen in the same tab.",
            "OK");
        
        // 추가로 SteamVR 설정 열기 시도
        #if UNITY_STANDALONE_WIN
        try
        {
            System.Diagnostics.Process.Start("steam://rungameid/250820");
            Debug.Log("Attempting to open SteamVR...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not open SteamVR: {e.Message}");
        }
        #endif
    }
}
#endif 