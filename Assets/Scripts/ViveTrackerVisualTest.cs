using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ViveTrackerVisualTest : MonoBehaviour
{
    [Header("Visual Settings")]
    public GameObject trackerPrefab;
    public float sphereSize = 0.1f;
    public Color[] trackerColors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
    
    private Dictionary<int, GameObject> trackerVisuals = new Dictionary<int, GameObject>();
    private List<TrackedDevice> activeTrackers = new List<TrackedDevice>();
    
    void Start()
    {
        // Create default prefab if none assigned
        if (trackerPrefab == null)
        {
            CreateDefaultPrefab();
        }
        
        InvokeRepeating(nameof(UpdateTrackerList), 0f, 2f);
    }
    
    void CreateDefaultPrefab()
    {
        // Create a simple sphere prefab
        trackerPrefab = new GameObject("Tracker Prefab");
        
        // Add sphere
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(trackerPrefab.transform);
        sphere.transform.localScale = Vector3.one * sphereSize;
        
        // Add axis indicators
        CreateAxis(trackerPrefab.transform, Vector3.forward, Color.blue, "Z"); // Forward
        CreateAxis(trackerPrefab.transform, Vector3.right, Color.red, "X");   // Right
        CreateAxis(trackerPrefab.transform, Vector3.up, Color.green, "Y");    // Up
        
        // Remove collider
        Destroy(sphere.GetComponent<Collider>());
        
        // Hide the prefab
        trackerPrefab.SetActive(false);
    }
    
    void CreateAxis(Transform parent, Vector3 direction, Color color, string label)
    {
        var axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        axis.name = $"Axis_{label}";
        axis.transform.SetParent(parent);
        axis.transform.localPosition = direction * 0.1f;
        axis.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
        axis.transform.localScale = new Vector3(0.01f, 0.1f, 0.01f);
        
        var renderer = axis.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = color;
        
        Destroy(axis.GetComponent<Collider>());
    }
    
    void UpdateTrackerList()
    {
        activeTrackers.Clear();
        
        foreach (var device in InputSystem.devices)
        {
            if (device is TrackedDevice trackedDevice && 
                (device.name.Contains("Tracker") || device.name.Contains("XRTracker")))
            {
                activeTrackers.Add(trackedDevice);
                
                // Create visual if it doesn't exist
                if (!trackerVisuals.ContainsKey(device.deviceId))
                {
                    var visual = Instantiate(trackerPrefab);
                    visual.SetActive(true);
                    visual.name = $"Tracker_{device.deviceId}_{device.name}";
                    
                    // Assign color
                    int colorIndex = trackerVisuals.Count % trackerColors.Length;
                    var renderer = visual.GetComponentInChildren<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = trackerColors[colorIndex];
                    }
                    
                    trackerVisuals[device.deviceId] = visual;
                }
            }
        }
        
        Debug.Log($"Active trackers: {activeTrackers.Count}");
    }
    
    void Update()
    {
        foreach (var tracker in activeTrackers)
        {
            if (trackerVisuals.TryGetValue(tracker.deviceId, out GameObject visual))
            {
                bool isTracked = tracker.isTracked.ReadValue() > 0.5f;
                visual.SetActive(isTracked);
                
                if (isTracked)
                {
                    visual.transform.position = tracker.devicePosition.ReadValue();
                    visual.transform.rotation = tracker.deviceRotation.ReadValue();
                }
            }
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 300));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("<b>Tracker Visual Test</b>");
        GUILayout.Label($"Active Trackers: {activeTrackers.Count}");
        
        foreach (var tracker in activeTrackers)
        {
            bool isTracked = tracker.isTracked.ReadValue() > 0.5f;
            string status = isTracked ? "<color=green>●</color>" : "<color=red>●</color>";
            GUILayout.Label($"{status} {tracker.name}");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}