using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using Mediapipe.Tasks.Vision.PoseLandmarker;

public class OptimizedMediaPipeAdapter : MonoBehaviour
{
    [SerializeField] private PoseLandmarkerRunner poseLandmarkerRunner;
    [SerializeField] private float smoothingFactor = 0.5f;
    
    [Header("Debug Infos")]
    [SerializeField] private List<Avatar> avatars = new List<Avatar>();
    
    // Direct landmark transforms without visual representation
    private Transform[] bonePositions = new Transform[LandmarkCount];
    private Transform virtualNeck;
    private Transform virtualHip;
    
    // Smoothing buffers
    private Vector3[] landmarkPositions = new Vector3[LandmarkCount];
    
    private const int LandmarkCount = 33;

    private void Start()
    {
        // Create invisible landmark transforms
        var landmarkParent = new GameObject("LandmarkParent").transform;
        for (int i = 0; i < LandmarkCount; i++)
        {
            var landmark = new GameObject($"Landmark_{i}");
            landmark.transform.parent = landmarkParent;
            bonePositions[i] = landmark.transform;
        }
        
        virtualNeck = new GameObject("VirtualNeck").transform;
        virtualNeck.parent = landmarkParent;
        virtualHip = new GameObject("VirtualHip").transform;
        virtualHip.parent = landmarkParent;
        
        // Subscribe to pose results
        if (poseLandmarkerRunner != null)
        {
            poseLandmarkerRunner.OnPoseResult += OnPoseResult;
        }
        
        // Auto-find avatars if not assigned
        if (avatars.Count == 0)
        {
            avatars.AddRange(FindObjectsOfType<Avatar>());
        }
    }
    
    public Transform GetLandmark(Landmark mark)
    {
        return bonePositions[(int)mark];
    }
    
    public Transform GetVirtualNeck() => virtualNeck;
    public Transform GetVirtualHip() => virtualHip;
    
    private void OnPoseResult(PoseLandmarkerResult result)
    {
        if (result.poseLandmarks == null || result.poseLandmarks.Count == 0)
            return;
            
        var landmarks = result.poseLandmarks[0];
        
        // Update target positions
        for (int i = 0; i < landmarks.landmarks.Count && i < LandmarkCount; i++)
        {
            var landmark = landmarks.landmarks[i];
            landmarkPositions[i] = new Vector3(
                -landmark.x * 10f,   
                landmark.y * 10f,   
                landmark.z * 10f
            );
        }
    }
    
    private void Update()
    {
        // Smooth landmark positions
        for (int i = 0; i < LandmarkCount; i++)
        {
            bonePositions[i].localPosition = Vector3.Lerp(
                bonePositions[i].localPosition, landmarkPositions[i],
                smoothingFactor * Time.deltaTime * 60f
            );
        }
        
        // Update virtual joints
        virtualNeck.position = (bonePositions[11].position + bonePositions[12].position) / 2f;
        virtualHip.position = (bonePositions[23].position + bonePositions[24].position) / 2f;
    }
    
    public void SetVisible(bool visible)
    {
        // No visual elements to hide in optimized version
    }
} 