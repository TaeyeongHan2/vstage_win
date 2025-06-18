using UnityEngine;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Tasks.Components.Containers;

namespace Mediapipe.Unity.Sample.PoseLandmarkDetection
{
    public class SpineAndLimbIK : MonoBehaviour
    {
        [Header("Target References")]
        [SerializeField] private Animator targetAnimator;
        [SerializeField] private SimplePoseToHumanoid simplePoseToHumanoid;
        
        [Header("Spine IK Settings")]
        [SerializeField] private Transform[] spineChain;
        [SerializeField] private float spineIKWeight = 0.5f;
        [SerializeField] private bool useSpineIK = true;
        
        [Header("Limb IK Settings")]
        [SerializeField] private bool useElbowHints = true;
        [SerializeField] private bool useKneeHints = true;
        [SerializeField] private float limbIKWeight = 0.8f;
        
        [Header("Natural Movement")]
        [SerializeField] private float breathingAmplitude = 0.02f;
        [SerializeField] private float breathingSpeed = 1.5f;
        [SerializeField] private bool simulateNaturalSway = true;
        [SerializeField] private float smoothingFactor = 0.1f;
        
        // Spine tracking
        private Vector3[] spineTargets;
        private Vector3 shoulderCenter;
        private Vector3 hipCenter;
        private Vector3 headPosition;
        
        // Natural movement simulation
        private float breathingTimer;
        private Vector3 naturalSwayOffset;
        
        // MediaPipe landmark positions (cached for spine calculation)
        private Vector3 leftShoulder, rightShoulder;
        private Vector3 leftElbow, rightElbow;
        private Vector3 leftHip, rightHip;
        private Vector3 leftKnee, rightKnee;
        private Vector3 nose;
        
        private void Start()
        {
            if (!targetAnimator)
                targetAnimator = GetComponent<Animator>();
            
            if (!simplePoseToHumanoid)
                simplePoseToHumanoid = GetComponent<SimplePoseToHumanoid>();
            
            if (!targetAnimator || !targetAnimator.isHuman)
            {
                Debug.LogError("SpineAndLimbIK requires a humanoid Animator!");
                enabled = false;
                return;
            }
            
            InitializeSpineChain();
        }
        
        private void InitializeSpineChain()
        {
            if (targetAnimator && targetAnimator.isHuman)
            {
                List<Transform> spine = new List<Transform>();
                
                // Build spine chain from humanoid following MediaPipe bone structure
                Transform hips = targetAnimator.GetBoneTransform(HumanBodyBones.Hips);
                Transform spine1 = targetAnimator.GetBoneTransform(HumanBodyBones.Spine);
                Transform chest = targetAnimator.GetBoneTransform(HumanBodyBones.Chest);
                Transform upperChest = targetAnimator.GetBoneTransform(HumanBodyBones.UpperChest);
                Transform neck = targetAnimator.GetBoneTransform(HumanBodyBones.Neck);
                Transform head = targetAnimator.GetBoneTransform(HumanBodyBones.Head);
                
                if (hips) spine.Add(hips);
                if (spine1) spine.Add(spine1);
                if (chest) spine.Add(chest);
                if (upperChest) spine.Add(upperChest);
                if (neck) spine.Add(neck);
                if (head) spine.Add(head);
                
                spineChain = spine.ToArray();
                spineTargets = new Vector3[spineChain.Length];
                
                Debug.Log($"SpineAndLimbIK: Initialized spine chain with {spineChain.Length} joints");
            }
        }
        
        public void UpdateFromMediaPipeLandmarks(PoseLandmarkerResult result)
        {
            if (result.poseLandmarks?.Count == 0) return;
            
            var landmarks = result.poseLandmarks[0].landmarks;
            if (landmarks.Count < 33) return; // MediaPipe pose has 33 landmarks
            
            // Extract key landmarks following MediaPipe pose landmark indices
            // Reference: https://github.com/homuler/MediaPipeUnityPlugin/wiki/API-Overview
            nose = ConvertLandmarkToWorld(landmarks[0]);           // 0: NOSE
            leftShoulder = ConvertLandmarkToWorld(landmarks[11]);  // 11: LEFT_SHOULDER
            rightShoulder = ConvertLandmarkToWorld(landmarks[12]); // 12: RIGHT_SHOULDER
            leftElbow = ConvertLandmarkToWorld(landmarks[13]);     // 13: LEFT_ELBOW
            rightElbow = ConvertLandmarkToWorld(landmarks[14]);    // 14: RIGHT_ELBOW
            leftHip = ConvertLandmarkToWorld(landmarks[23]);       // 23: LEFT_HIP
            rightHip = ConvertLandmarkToWorld(landmarks[24]);      // 24: RIGHT_HIP
            leftKnee = ConvertLandmarkToWorld(landmarks[25]);      // 25: LEFT_KNEE
            rightKnee = ConvertLandmarkToWorld(landmarks[26]);     // 26: RIGHT_KNEE
            
            // Calculate center points
            shoulderCenter = (leftShoulder + rightShoulder) * 0.5f;
            hipCenter = (leftHip + rightHip) * 0.5f;
            headPosition = nose;
            
            // Update spine and limb IK
            if (useSpineIK)
            {
                UpdateSpineIK();
            }
            
            UpdateLimbHints();
        }
        
        private Vector3 ConvertLandmarkToWorld(Mediapipe.Tasks.Components.Containers.NormalizedLandmark landmark)
        {
            // Convert MediaPipe normalized coordinates to Unity world space
            // Following the same conversion as SimplePoseToHumanoid
            float scaleFactor = 2.0f; // Match SimplePoseToHumanoid scale
            float x = (landmark.x - 0.5f) * scaleFactor;
            float y = (0.5f - landmark.y) * scaleFactor;
            float z = landmark.z * scaleFactor;
            
            // Mirror for natural movement
            return new Vector3(-x, y, z);
        }
        
        private void UpdateSpineIK()
        {
            if (spineChain == null || spineChain.Length == 0) return;
            
            // Calculate natural spine curve based on MediaPipe landmarks
            Vector3 spineDirection = (shoulderCenter - hipCenter).normalized;
            float spineLength = Vector3.Distance(shoulderCenter, hipCenter);
            
            // Add natural breathing movement
            if (simulateNaturalSway)
            {
                breathingTimer += Time.deltaTime * breathingSpeed;
                float breathingOffset = Mathf.Sin(breathingTimer) * breathingAmplitude;
                Vector3 breathingDirection = targetAnimator.transform.right;
                naturalSwayOffset = breathingDirection * breathingOffset;
            }
            
            // Distribute spine joints along the curve from hips to head
            for (int i = 0; i < spineChain.Length; i++)
            {
                float t = (float)i / (spineChain.Length - 1);
                
                // Create natural spine curve from hip to head
                Vector3 basePosition;
                if (i == spineChain.Length - 1) // Head
                {
                    basePosition = headPosition;
                }
                else
                {
                    basePosition = Vector3.Lerp(hipCenter, shoulderCenter, t);
                }
                
                // Add natural curvature and breathing
                float curveIntensity = Mathf.Sin(t * Mathf.PI) * 0.1f;
                Vector3 curveOffset = targetAnimator.transform.forward * curveIntensity * spineLength;
                
                spineTargets[i] = basePosition + curveOffset + naturalSwayOffset;
                
                // Apply to spine joint with smooth interpolation
                if (spineChain[i] != null)
                {
                    Vector3 currentPos = spineChain[i].position;
                    Vector3 targetPosition = spineTargets[i];
                    
                    // Smooth interpolation
                    spineChain[i].position = Vector3.Lerp(
                        currentPos, 
                        targetPosition, 
                        spineIKWeight * smoothingFactor
                    );
                    
                    // Calculate rotation for natural spine orientation
                    if (i < spineChain.Length - 1 && spineChain[i + 1] != null)
                    {
                        Vector3 nextSegmentDir = (spineTargets[i + 1] - spineTargets[i]).normalized;
                        if (nextSegmentDir.magnitude > 0.001f)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(nextSegmentDir, targetAnimator.transform.up);
                            spineChain[i].rotation = Quaternion.Slerp(
                                spineChain[i].rotation, 
                                targetRotation, 
                                spineIKWeight * smoothingFactor * 0.5f
                            );
                        }
                    }
                }
            }
        }
        
        private void UpdateLimbHints()
        {
            if (!targetAnimator) return;
            
            // Set elbow hints for natural arm bending using MediaPipe landmarks
            if (useElbowHints)
            {
                targetAnimator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbow);
                targetAnimator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, limbIKWeight);
                
                targetAnimator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbow);
                targetAnimator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, limbIKWeight);
            }
            
            // Set knee hints for natural leg bending
            if (useKneeHints)
            {
                targetAnimator.SetIKHintPosition(AvatarIKHint.LeftKnee, leftKnee);
                targetAnimator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, limbIKWeight);
                
                targetAnimator.SetIKHintPosition(AvatarIKHint.RightKnee, rightKnee);
                targetAnimator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, limbIKWeight);
            }
        }
        
        public void SetSpineIKWeight(float weight)
        {
            spineIKWeight = Mathf.Clamp01(weight);
        }
        
        public void SetLimbIKWeight(float weight)
        {
            limbIKWeight = Mathf.Clamp01(weight);
        }
        
        private void OnDrawGizmos()
        {
            if (spineTargets == null) return;
            
            // Draw spine chain visualization
            Gizmos.color = UnityEngine.Color.green;
            for (int i = 0; i < spineTargets.Length - 1; i++)
            {
                Gizmos.DrawLine(spineTargets[i], spineTargets[i + 1]);
                Gizmos.DrawSphere(spineTargets[i], 0.02f);
            }
            
            // Draw MediaPipe landmark positions
            Gizmos.color = UnityEngine.Color.blue;
            Gizmos.DrawSphere(shoulderCenter, 0.03f);
            Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawSphere(hipCenter, 0.03f);
            Gizmos.color = UnityEngine.Color.yellow;
            Gizmos.DrawSphere(headPosition, 0.025f);
            
            // Draw limb hint positions
            Gizmos.color = UnityEngine.Color.cyan;
            Gizmos.DrawSphere(leftElbow, 0.02f);
            Gizmos.DrawSphere(rightElbow, 0.02f);
            Gizmos.DrawSphere(leftKnee, 0.02f);
            Gizmos.DrawSphere(rightKnee, 0.02f);
        }
    }
} 