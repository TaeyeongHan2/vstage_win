using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class ViveTrackerTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool showDetailedInfo = true;
    public float updateInterval = 1f;
    
    private float lastUpdateTime;
    private List<InputDevice> allTrackers = new List<InputDevice>();
    private Dictionary<string, TrackerInfo> trackerData = new Dictionary<string, TrackerInfo>();
    
    public class TrackerInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isTracked;
        public string deviceName;
        public int deviceId;
    }
    
    void Start()
    {
        Debug.Log("=== VIVE Tracker Test Started ===");
        Debug.Log("Looking for VIVE Trackers...");
        
        // Initial device scan
        ScanForTrackers();
    }
    
    void Update()
    {
        // Periodic device scan
        if (Time.time - lastUpdateTime > updateInterval)
        {
            ScanForTrackers();
            lastUpdateTime = Time.time;
        }
        
        // Update tracker data
        UpdateTrackerData();
        
        // Test key bindings
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PrintTrackerStatus();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            ScanForTrackers();
        }
    }
    
    void ScanForTrackers()
    {
        allTrackers.Clear();
        trackerData.Clear();
        
        var devices = InputSystem.devices;
        Debug.Log($"Total devices found: {devices.Count}");
        
        foreach (var device in devices)
        {
            Debug.Log($"Device: {device.name}, Type: {device.GetType().Name}, ID: {device.deviceId}");
            
            // Check for tracker devices
            if (device.name.ToLower().Contains("tracker") || 
                device.name.ToLower().Contains("vive") ||
                device is TrackedDevice)
            {
                allTrackers.Add(device);
                
                var info = new TrackerInfo
                {
                    deviceName = device.name,
                    deviceId = device.deviceId
                };
                
                trackerData[device.name] = info;
                Debug.Log($"<color=green>Found Tracker: {device.name} (ID: {device.deviceId})</color>");
            }
        }
        
        if (allTrackers.Count == 0)
        {
            Debug.LogWarning("No VIVE Trackers found! Make sure:");
            Debug.LogWarning("1. SteamVR is running");
            Debug.LogWarning("2. Trackers are paired and turned on");
            Debug.LogWarning("3. OpenXR settings are correct");
        }
        else
        {
            Debug.Log($"<color=cyan>Found {allTrackers.Count} tracker(s)</color>");
        }
    }
    
    void UpdateTrackerData()
    {
        foreach (var device in allTrackers)
        {
            if (device is TrackedDevice trackedDevice)
            {
                var info = trackerData[device.name];
                
                // Read tracking state
                var isTracked = trackedDevice.isTracked.ReadValue() > 0.5f;
                info.isTracked = isTracked;
                
                if (isTracked)
                {
                    // Read position
                    var position = trackedDevice.devicePosition.ReadValue();
                    info.position = position;
                    
                    // Read rotation
                    var rotation = trackedDevice.deviceRotation.ReadValue();
                    info.rotation = rotation;
                }
            }
        }
    }
    
    void PrintTrackerStatus()
    {
        Debug.Log("=== Current Tracker Status ===");
        
        if (trackerData.Count == 0)
        {
            Debug.Log("No trackers found.");
            return;
        }
        
        foreach (var kvp in trackerData)
        {
            var info = kvp.Value;
            if (info.isTracked)
            {
                Debug.Log($"<color=green>✓ {info.deviceName}</color>");
                Debug.Log($"  Position: {info.position}");
                Debug.Log($"  Rotation: {info.rotation.eulerAngles}");
            }
            else
            {
                Debug.Log($"<color=red>✗ {info.deviceName} - Not Tracked</color>");
            }
        }
    }
    
    void OnGUI()
    {
        if (!showDetailedInfo) return;
        
        // GUI Box
        GUILayout.BeginArea(new Rect(10, 10, 400, 500));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("<b>VIVE Tracker Test</b>", GUI.skin.label);
        GUILayout.Space(10);
        
        // Status
        GUILayout.Label($"Trackers Found: {allTrackers.Count}");
        GUILayout.Label($"Press SPACE to print status");
        GUILayout.Label($"Press R to rescan devices");
        GUILayout.Space(10);
        
        // Tracker details
        foreach (var kvp in trackerData)
        {
            var info = kvp.Value;
            
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"<b>{info.deviceName}</b>");
            
            if (info.isTracked)
            {
                GUILayout.Label($"Status: <color=green>TRACKED</color>");
                GUILayout.Label($"Pos: {info.position.ToString("F2")}");
                GUILayout.Label($"Rot: {info.rotation.eulerAngles.ToString("F1")}");
            }
            else
            {
                GUILayout.Label($"Status: <color=red>NOT TRACKED</color>");
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}