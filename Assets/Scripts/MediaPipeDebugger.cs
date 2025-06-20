using UnityEngine;

public class MediaPipeDebugger : MonoBehaviour
{
    [Header("References")]
    public OptimizedMediaPipeAdapter mediaPipeAdapter;
    public Avatar[] avatars;
    
    [Header("Debug Info")]
    public bool isCalibrated;
    public bool hasVirtualJoints;
    public Vector3 virtualNeckPosition;
    public Vector3 virtualHipPosition;
    
    [Header("Humanoid Bones")]
    public Transform humanoidNeck;
    public Transform humanoidHips;
    
    void Start()
    {
        // Auto-find components if not assigned
        if (mediaPipeAdapter == null)
            mediaPipeAdapter = FindObjectOfType<OptimizedMediaPipeAdapter>();
            
        if (avatars == null || avatars.Length == 0)
            avatars = FindObjectsOfType<Avatar>();
            
        Debug.Log($"[MediaPipeDebugger] Found {avatars.Length} avatars");
        
        // Get humanoid bones from first avatar
        if (avatars.Length > 0 && avatars[0].animator != null)
        {
            var animator = avatars[0].animator;
            humanoidNeck = animator.GetBoneTransform(HumanBodyBones.Neck);
            humanoidHips = animator.GetBoneTransform(HumanBodyBones.Hips);
            
            Debug.Log($"[MediaPipeDebugger] Humanoid Neck: {humanoidNeck?.name}");
            Debug.Log($"[MediaPipeDebugger] Humanoid Hips: {humanoidHips?.name}");
        }
    }
    
    void Update()
    {
        if (mediaPipeAdapter == null) return;
        
        // Check virtual joints
        var virtualNeck = mediaPipeAdapter.GetVirtualNeck();
        var virtualHip = mediaPipeAdapter.GetVirtualHip();
        
        hasVirtualJoints = virtualNeck != null && virtualHip != null;
        
        if (hasVirtualJoints)
        {
            virtualNeckPosition = virtualNeck.position;
            virtualHipPosition = virtualHip.position;
        }
        
        // Check calibration status
        isCalibrated = avatars.Length > 0 && avatars[0].Calibrated;
        
        // Draw debug lines
        if (hasVirtualJoints && isCalibrated && humanoidNeck != null && humanoidHips != null)
        {
            // Draw lines between virtual joints and humanoid bones
            Debug.DrawLine(virtualNeck.position, humanoidNeck.position, Color.cyan);
            Debug.DrawLine(virtualHip.position, humanoidHips.position, Color.magenta);
            
            // Draw virtual spine
            Debug.DrawLine(virtualNeck.position, virtualHip.position, Color.yellow);
            
            // Draw humanoid spine
            Debug.DrawLine(humanoidNeck.position, humanoidHips.position, Color.green);
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 200, 300, 200));
        GUILayout.Label($"MediaPipe Connected: {mediaPipeAdapter != null}");
        GUILayout.Label($"Has Virtual Joints: {hasVirtualJoints}");
        GUILayout.Label($"Avatar Calibrated: {isCalibrated}");
        
        if (hasVirtualJoints)
        {
            GUILayout.Label($"Virtual Neck: {virtualNeckPosition:F2}");
            GUILayout.Label($"Virtual Hip: {virtualHipPosition:F2}");
        }
        
        if (humanoidNeck != null && humanoidHips != null)
        {
            GUILayout.Label($"Humanoid Neck: {humanoidNeck.position:F2}");
            GUILayout.Label($"Humanoid Hips: {humanoidHips.position:F2}");
        }
        
        GUILayout.EndArea();
    }
} 