using UnityEngine;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe;

/// <summary>
/// MediaPipe 포즈 결과를 Humanoid 캐릭터에 연결
/// </summary>
public class SimplePoseToHumanoid : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PoseLandmarkerRunner poseRunner;
    [SerializeField] private SimpleHumanoidController humanoidController;
    
    [Header("디버그")]
    [SerializeField] private bool enableDebugLog = true;
    
    void Start()
    {
        // PoseLandmarkerRunner 찾기
        if (poseRunner == null)
        {
            var solution = GameObject.Find("Solution");
            if (solution != null)
            {
                poseRunner = solution.GetComponent<PoseLandmarkerRunner>();
            }
        }
        
        // SimpleHumanoidController 찾기
        if (humanoidController == null)
        {
            var character = GameObject.Find("wonjin_Shinano_kisekae");
            if (character != null)
            {
                humanoidController = character.GetComponent<SimpleHumanoidController>();
            }
        }
        
        // 검증
        if (poseRunner == null)
        {
            Debug.LogError("PoseLandmarkerRunner를 찾을 수 없습니다!");
            return;
        }
        
        if (humanoidController == null)
        {
            Debug.LogError("SimpleHumanoidController를 찾을 수 없습니다!");
            return;
        }
        
        // 이벤트 구독
        poseRunner.OnPoseResult += OnPoseResult;
        
        Debug.Log("SimplePoseToHumanoid 초기화 완료!");
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (poseRunner != null)
        {
            poseRunner.OnPoseResult -= OnPoseResult;
        }
    }
    
    /// <summary>
    /// 포즈 결과 처리
    /// </summary>
    void OnPoseResult(PoseLandmarkerResult result)
    {
        if (result.poseLandmarks == null || result.poseLandmarks.Count == 0)
            return;
        
        // 첫 번째 포즈만 사용
        var poseData = result.poseLandmarks[0];
        
        // MediaPipe 형식을 Unity 형식으로 변환
        var convertedData = ConvertToNormalizedLandmarkList(poseData);
        
        if (convertedData != null && humanoidController != null)
        {
            humanoidController.ApplyPose(convertedData);
            
            if (enableDebugLog)
            {
                Debug.Log($"포즈 적용됨: {convertedData.Landmark.Count}개 랜드마크");
            }
        }
    }
    
    /// <summary>
    /// MediaPipe 형식을 기존 형식으로 변환
    /// </summary>
    NormalizedLandmarkList ConvertToNormalizedLandmarkList(Mediapipe.Tasks.Components.Containers.NormalizedLandmarks landmarks)
    {
        var list = new NormalizedLandmarkList();
        
        foreach (var landmark in landmarks.landmarks)
        {
            list.Landmark.Add(new NormalizedLandmark
            {
                X = landmark.x,
                Y = landmark.y,
                Z = landmark.z,
                Visibility = landmark.visibility ?? 1.0f
            });
        }
        
        return list;
    }
} 