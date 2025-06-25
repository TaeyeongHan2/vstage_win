using UnityEngine;
using UnityEngine.XR.Management;

public class XRCheck : MonoBehaviour
{
    void Start()
    {
        var mgr = XRGeneralSettings.Instance?.Manager;
        
        if (mgr == null)
        {
            Debug.LogError("❌ XR Manager null - Enable OpenXR in Project Settings > XR Plug-in Management");
            return;
        }
        
        Debug.Log($"✅ XR Manager exists");
        Debug.Log($"Active Loaders: {mgr.activeLoaders.Count}");
        
        if (mgr.activeLoaders.Count == 0)
        {
            Debug.LogError("❌ No loaders - Check OpenXR in XR Plug-in Management and restart Unity");
        }
    }
} 