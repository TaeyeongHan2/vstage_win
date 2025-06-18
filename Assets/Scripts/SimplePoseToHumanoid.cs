using UnityEngine;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Tasks.Components.Containers;

namespace Mediapipe.Unity.Sample.PoseLandmarkDetection
{
    public class SimplePoseToHumanoid : MonoBehaviour
    {
        [Header("Target Humanoid")]
        [SerializeField] private Animator targetAnimator;
        [SerializeField] private bool enableIK = true;
        
        [Header("Settings")]
        [SerializeField] private float smoothingFactor = 0.1f;
        [SerializeField] private float scaleFactor = 2.0f;
        
        [Header("IK Weights")]
        [SerializeField] private float handWeight = 1f;
        [SerializeField] private float footWeight = 0.8f;
        
        // IK Target Transforms
        private Transform leftHandTarget;
        private Transform rightHandTarget;
        private Transform leftFootTarget;
        private Transform rightFootTarget;
        
        // Optional spine and limb IK system
        private SpineAndLimbIK spineAndLimbIK;
        
        private void Start()
        {
            if (!targetAnimator || !targetAnimator.isHuman)
            {
                Debug.LogError("Target animator must be humanoid!");
                enabled = false;
                return;
            }
            
            InitializeIKTargets();
            
            // Try to find SpineAndLimbIK component
            spineAndLimbIK = GetComponent<SpineAndLimbIK>();
        }
        
        private void InitializeIKTargets()
        {
            GameObject ikParent = new GameObject("IK_Targets");
            ikParent.transform.SetParent(transform);
            
            leftHandTarget = CreateTarget("LeftHand_Target", ikParent.transform);
            rightHandTarget = CreateTarget("RightHand_Target", ikParent.transform);
            leftFootTarget = CreateTarget("LeftFoot_Target", ikParent.transform);
            rightFootTarget = CreateTarget("RightFoot_Target", ikParent.transform);
        }
        
        private Transform CreateTarget(string name, Transform parent)
        {
            GameObject target = new GameObject(name);
            target.transform.SetParent(parent);
            return target.transform;
        }
        
        public void UpdatePose(PoseLandmarkerResult result)
        {
            if (result.poseLandmarks?.Count == 0) return;
            
            var landmarks = result.poseLandmarks[0].landmarks;
            if (landmarks.Count < 33) return; // MediaPipe has 33 landmarks
            
            // Update IK targets
            UpdateHandTargets(landmarks);
            UpdateFootTargets(landmarks);
            
            // Update spine and limb IK if available
            if (spineAndLimbIK != null)
            {
                spineAndLimbIK.UpdateFromMediaPipeLandmarks(result);
            }
        }
        
        private void UpdateHandTargets(IList<Mediapipe.Tasks.Components.Containers.NormalizedLandmark> landmarks)
        {
            // Left wrist (landmark 15)
            if (landmarks.Count > 15)
            {
                Vector3 leftWrist = ConvertLandmarkToWorld(landmarks[15]);
                leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, leftWrist, smoothingFactor);
            }
            
            // Right wrist (landmark 16)
            if (landmarks.Count > 16)
            {
                Vector3 rightWrist = ConvertLandmarkToWorld(landmarks[16]);
                rightHandTarget.position = Vector3.Lerp(rightHandTarget.position, rightWrist, smoothingFactor);
            }
        }
        
        private void UpdateFootTargets(IList<Mediapipe.Tasks.Components.Containers.NormalizedLandmark> landmarks)
        {
            // Left ankle (landmark 27)
            if (landmarks.Count > 27)
            {
                Vector3 leftAnkle = ConvertLandmarkToWorld(landmarks[27]);
                leftFootTarget.position = Vector3.Lerp(leftFootTarget.position, leftAnkle, smoothingFactor);
            }
            
            // Right ankle (landmark 28)
            if (landmarks.Count > 28)
            {
                Vector3 rightAnkle = ConvertLandmarkToWorld(landmarks[28]);
                rightFootTarget.position = Vector3.Lerp(rightFootTarget.position, rightAnkle, smoothingFactor);
            }
        }
        
        private Vector3 ConvertLandmarkToWorld(Mediapipe.Tasks.Components.Containers.NormalizedLandmark landmark)
        {
            // Convert normalized coordinates to world space
            float x = (landmark.x - 0.5f) * scaleFactor;
            float y = (0.5f - landmark.y) * scaleFactor;
            float z = landmark.z * scaleFactor;
            
            // Mirror for natural movement
            return new Vector3(-x, y, z);
        }
        
        private void OnAnimatorIK(int layerIndex)
        {
            if (!enableIK || !targetAnimator) return;
            
            // Apply hand IK
            targetAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handWeight);
            targetAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            
            targetAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, handWeight);
            targetAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            
            // Apply foot IK
            targetAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, footWeight);
            targetAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTarget.position);
            
            targetAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, footWeight);
            targetAnimator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position);
        }
        
        private void OnGUI()
        {
            GUI.Label(new UnityEngine.Rect(10, 10, 300, 30), "Simple Pose to Humanoid Active");
            GUI.Label(new UnityEngine.Rect(10, 40, 300, 30), $"Scale Factor: {scaleFactor:F2}");
        }
    }
} 