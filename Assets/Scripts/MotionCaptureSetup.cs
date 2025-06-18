using UnityEngine;

namespace Mediapipe.Unity.Sample.PoseLandmarkDetection
{
    [System.Serializable]
    public class MotionCaptureSetup : MonoBehaviour
    {
        [Header("Setup Guide")]
        [TextArea(5, 10)]
        public string setupInstructions = @"MediaPipe 모션캡처 설정 가이드:

1. 휴머노이드 모델 준비:
   - Avatar 설정을 'Humanoid'로 변경
   - Animator 컴포넌트 추가

2. 스크립트 연결:
   - AdvancedPoseToHumanoid 컴포넌트를 휴머노이드 모델에 추가
   - Target Animator 필드에 Animator 컴포넌트 할당

3. PoseLandmarkerRunner 연결:
   - Solution GameObject의 PoseLandmarkerRunner 스크립트에서
   - Simple Pose To Humanoid 필드에 SimplePoseToHumanoid 컴포넌트 할당

4. 고급 기능 (선택사항):
   - SpineAndLimbIK 컴포넌트 추가하여 척추 및 사지 세밀 제어
   - MediaPipe 33개 랜드마크를 활용한 전신 추적

5. 설정 최적화:
   - Hand/Foot Weight: 손목/발목 추적 강도 조절
   - Smoothing Factor: 0.1 (부드러운 움직임)
   - Scale Factor: 2.0 (움직임 크기)";
        
        [Header("Quick Setup")]
        [SerializeField] private bool autoSetup = false;
        [SerializeField] private Animator targetHumanoid;
        [SerializeField] private PoseLandmarkerRunner poseLandmarkerRunner;
        
        private void Start()
        {
            if (autoSetup)
            {
                PerformAutoSetup();
            }
        }
        
        [ContextMenu("Auto Setup Motion Capture")]
        public void PerformAutoSetup()
        {
            if (targetHumanoid == null)
            {
                Debug.LogError("Target Humanoid Animator not assigned!");
                return;
            }
            
            if (poseLandmarkerRunner == null)
            {
                Debug.LogError("PoseLandmarkerRunner not assigned!");
                return;
            }
            
            // Add SimplePoseToHumanoid component if not exists
            SimplePoseToHumanoid poseToHumanoid = targetHumanoid.GetComponent<SimplePoseToHumanoid>();
            if (poseToHumanoid == null)
            {
                poseToHumanoid = targetHumanoid.gameObject.AddComponent<SimplePoseToHumanoid>();
                Debug.Log("Added SimplePoseToHumanoid component to " + targetHumanoid.name);
            }
            
            Debug.Log("Motion Capture setup completed successfully!");
            Debug.Log("Please manually connect the SimplePoseToHumanoid component to PoseLandmarkerRunner in the Inspector.");
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new UnityEngine.Rect(UnityEngine.Screen.width - 250, 10, 240, 200));
            GUILayout.Box("Motion Capture Setup");
            
            if (GUILayout.Button("Auto Setup"))
            {
                PerformAutoSetup();
            }
            
            GUILayout.Space(10);
            
            if (targetHumanoid != null)
            {
                GUILayout.Label("✓ Humanoid: " + targetHumanoid.name);
            }
            else
            {
                GUILayout.Label("✗ Humanoid: Not Assigned");
            }
            
            if (poseLandmarkerRunner != null)
            {
                GUILayout.Label("✓ Runner: Found");
            }
            else
            {
                GUILayout.Label("✗ Runner: Not Found");
            }
            
            var poseComponent = targetHumanoid?.GetComponent<SimplePoseToHumanoid>();
            if (poseComponent != null)
            {
                GUILayout.Label("✓ Pose Component: Active");
            }
            else
            {
                GUILayout.Label("✗ Pose Component: Missing");
            }
            
            GUILayout.EndArea();
        }
    }
} 