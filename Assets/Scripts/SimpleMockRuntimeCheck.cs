using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

public class SimpleMockRuntimeCheck : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Mock Runtime Check ===");
        
        var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(UnityEditor.BuildTargetGroup.Standalone);
        if (settings != null)
        {
            // Check if any feature mentions "Mock" in its name
            foreach (var feature in settings.GetFeatures<OpenXRFeature>())
            {
                if (feature.GetType().Name.Contains("Mock"))
                {
                    Debug.LogError($"❌ Mock feature found: {feature.GetType().Name} - Enabled: {feature.enabled}");
                    Debug.LogError("   → Disable Mock Runtime in Project Settings > XR Plug-in Management > OpenXR");
                }
            }
        }
        
        // Check OpenXR runtime
        if (OpenXRRuntime.name.Contains("Mock") || OpenXRRuntime.name == "Unknown")
        {
            Debug.LogError($"❌ Mock Runtime active: {OpenXRRuntime.name}");
            Debug.LogError("   → Enable real OpenXR in XR Plug-in Management");
        }
        else
        {
            Debug.Log($"✅ OpenXR Runtime: {OpenXRRuntime.name}");
        }
    }
} 