using UnityEngine;
using RootMotion.FinalIK;
using RootMotion.Demos;

public class TrackerVisualizationSystem : MonoBehaviour
{
    [Header("References")]
    public VRIKCalibrationController calibrationController;
    
    [Header("Visualization Settings")]
    public bool showTrackers = true;
    public float trackerSize = 0.07f;
    public Color headColor = Color.blue;
    public Color handColor = Color.green;
    public Color footColor = Color.red;
    public Color bodyColor = Color.yellow;
    
    [Header("Debug Visuals")]
    public bool showBoneConnections = true;
    public bool showExpectedPositions = true;
    
    private GameObject[] trackerVisuals;
    private GameObject[] expectedPositions;
    
    void Start()
    {
        if (calibrationController == null)
            calibrationController = FindObjectOfType<VRIKCalibrationController>();
            
        CreateTrackerVisuals();
    }
    
    void CreateTrackerVisuals()
    {
        trackerVisuals = new GameObject[6];
        expectedPositions = new GameObject[6];
        
        // Head Tracker
        trackerVisuals[0] = CreateTrackerVisual("Head Tracker Visual", headColor);
        expectedPositions[0] = CreateExpectedPositionVisual("Head Expected", headColor);
        
        // Hand Trackers
        trackerVisuals[1] = CreateTrackerVisual("Left Hand Tracker Visual", handColor);
        trackerVisuals[2] = CreateTrackerVisual("Right Hand Tracker Visual", handColor);
        expectedPositions[1] = CreateExpectedPositionVisual("Left Hand Expected", handColor);
        expectedPositions[2] = CreateExpectedPositionVisual("Right Hand Expected", handColor);
        
        // Body Tracker
        trackerVisuals[3] = CreateTrackerVisual("Body Tracker Visual", bodyColor);
        expectedPositions[3] = CreateExpectedPositionVisual("Body Expected", bodyColor);
        
        // Foot Trackers
        trackerVisuals[4] = CreateTrackerVisual("Left Foot Tracker Visual", footColor);
        trackerVisuals[5] = CreateTrackerVisual("Right Foot Tracker Visual", footColor);
        expectedPositions[4] = CreateExpectedPositionVisual("Left Foot Expected", footColor);
        expectedPositions[5] = CreateExpectedPositionVisual("Right Foot Expected", footColor);
    }
    
    GameObject CreateTrackerVisual(string name, Color color)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = name;
        visual.transform.localScale = Vector3.one * trackerSize;
        visual.GetComponent<Renderer>().material.color = color;
        visual.GetComponent<Collider>().enabled = false;
        return visual;
    }
    
    GameObject CreateExpectedPositionVisual(string name, Color color)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = name;
        visual.transform.localScale = Vector3.one * (trackerSize * 0.7f);
        var mat = visual.GetComponent<Renderer>().material;
        mat.color = new Color(color.r, color.g, color.b, 0.5f);
        visual.GetComponent<Collider>().enabled = false;
        return visual;
    }
    
    void Update()
    {
        if (!showTrackers || calibrationController == null) return;
        
        UpdateTrackerVisual(0, calibrationController.headTracker);
        UpdateTrackerVisual(1, calibrationController.leftHandTracker);
        UpdateTrackerVisual(2, calibrationController.rightHandTracker);
        UpdateTrackerVisual(3, calibrationController.bodyTracker);
        UpdateTrackerVisual(4, calibrationController.leftFootTracker);
        UpdateTrackerVisual(5, calibrationController.rightFootTracker);
        
        if (showExpectedPositions && calibrationController.ik != null)
        {
            UpdateExpectedPositions();
        }
        
        if (showBoneConnections)
        {
            DrawBoneConnections();
        }
    }
    
    void UpdateTrackerVisual(int index, Transform tracker)
    {
        if (tracker != null && trackerVisuals[index] != null)
        {
            trackerVisuals[index].SetActive(true);
            trackerVisuals[index].transform.position = tracker.position;
            trackerVisuals[index].transform.rotation = tracker.rotation;
        }
        else if (trackerVisuals[index] != null)
        {
            trackerVisuals[index].SetActive(false);
        }
    }
    
    void UpdateExpectedPositions()
    {
        var refs = calibrationController.ik.references;
        if (refs == null) return;
        
        // Head
        if (refs.head != null && expectedPositions[0] != null)
        {
            expectedPositions[0].transform.position = refs.head.position;
            expectedPositions[0].SetActive(showExpectedPositions);
        }
        
        // Hands
        if (refs.leftHand != null && expectedPositions[1] != null)
        {
            expectedPositions[1].transform.position = refs.leftHand.position;
            expectedPositions[1].SetActive(showExpectedPositions);
        }
        if (refs.rightHand != null && expectedPositions[2] != null)
        {
            expectedPositions[2].transform.position = refs.rightHand.position;
            expectedPositions[2].SetActive(showExpectedPositions);
        }
        
        // Body
        if (refs.pelvis != null && expectedPositions[3] != null)
        {
            expectedPositions[3].transform.position = refs.pelvis.position;
            expectedPositions[3].SetActive(showExpectedPositions);
        }
        
        // Feet
        if (refs.leftFoot != null && expectedPositions[4] != null)
        {
            expectedPositions[4].transform.position = refs.leftFoot.position;
            expectedPositions[4].SetActive(showExpectedPositions);
        }
        if (refs.rightFoot != null && expectedPositions[5] != null)
        {
            expectedPositions[5].transform.position = refs.rightFoot.position;
            expectedPositions[5].SetActive(showExpectedPositions);
        }
    }
    
    void DrawBoneConnections()
    {
        // 트래커와 예상 위치 간의 연결선 그리기
        for (int i = 0; i < 6; i++)
        {
            if (trackerVisuals[i] != null && trackerVisuals[i].activeSelf &&
                expectedPositions[i] != null && expectedPositions[i].activeSelf)
            {
                Debug.DrawLine(
                    trackerVisuals[i].transform.position,
                    expectedPositions[i].transform.position,
                    Color.white
                );
            }
        }
    }
    
    void OnGUI()
    {
        if (!showTrackers) return;
        
        GUILayout.BeginArea(new Rect(10, 200, 300, 400));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("<b>Tracker Offsets</b>");
        
        // 각 트래커의 오프셋 표시
        for (int i = 0; i < 6; i++)
        {
            if (trackerVisuals[i] != null && trackerVisuals[i].activeSelf &&
                expectedPositions[i] != null && expectedPositions[i].activeSelf)
            {
                float distance = Vector3.Distance(
                    trackerVisuals[i].transform.position,
                    expectedPositions[i].transform.position
                );
                
                string trackerName = GetTrackerName(i);
                GUILayout.Label($"{trackerName}: {distance:F3}m");
            }
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    string GetTrackerName(int index)
    {
        switch (index)
        {
            case 0: return "Head";
            case 1: return "Left Hand";
            case 2: return "Right Hand";
            case 3: return "Body";
            case 4: return "Left Foot";
            case 5: return "Right Foot";
            default: return "Unknown";
        }
    }
} 