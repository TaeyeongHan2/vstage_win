using UnityEngine;
using Mediapipe;
using System.Reflection;

/// <summary>
/// MediaPipe와 연결하는 커넥터 - 새 버전
/// </summary>
public class SimplePoseConnector : MonoBehaviour
{
    [Header("설정")]
    public bool enableDebugLog = true;
    
    // MediaPipe 컴포넌트 참조
    private MonoBehaviour poseRunner;
    private bool isConnected = false;
    
    void Start()
    {
        // MediaPipe PoseRunner 찾기
        ConnectToMediaPipe();
    }
    
    /// <summary>
    /// MediaPipe PoseRunner를 씬에서 찾아 연결 시도
    /// </summary>
    void ConnectToMediaPipe()
    {
        var allComponents = FindObjectsOfType<MonoBehaviour>();
        
        foreach (var component in allComponents)
        {
            var type = component.GetType();
            
            // 이름에 PoseLandmarker와 Runner가 모두 포함된 컴포넌트 탐색
            if (type.Name.Contains("PoseLandmarker") && type.Name.Contains("Runner"))
            {
                poseRunner = component;
                if (enableDebugLog)
                {
                    Debug.Log($"✅ PoseRunner 발견: {type.Name}");
                }
                
                SetupConnection(component);
                break;
            }
        }
        
        if (poseRunner == null && enableDebugLog)
        {
            Debug.LogWarning("PoseRunner를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// PoseRunner의 OnPoseLandmarksOutput 이벤트에 핸들러 연결
    /// </summary>
    void SetupConnection(MonoBehaviour runner)
    {
        try
        {
            var type = runner.GetType();
            var eventField = type.GetEvent("OnPoseLandmarksOutput");
            
            if (eventField != null)
            {
                var handler = System.Delegate.CreateDelegate(
                    eventField.EventHandlerType, 
                    this, 
                    "OnPoseDataReceived"
                );
                eventField.AddEventHandler(runner, handler);
                
                isConnected = true;
                Debug.Log("✅ MediaPipe 연결 성공!");
            }
            else if (enableDebugLog)
            {
                Debug.LogWarning("OnPoseLandmarksOutput 이벤트가 없습니다.");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLog)
            {
                Debug.LogError($"연결 실패: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// MediaPipe 포즈 데이터 수신 핸들러
    /// </summary>
    public void OnPoseDataReceived(object sender, Mediapipe.Tasks.Vision.PoseLandmarker.PoseLandmarkerResult result)
    {
        try
        {
            if (result.poseLandmarks?.Count > 0)
            {
                var poseData = result.poseLandmarks[0];
                var convertedData = ConvertPoseData(poseData);
                
                if (convertedData != null)
                {
                    SimplePoseBridge.SendPoseData(convertedData);
                    
                    if (enableDebugLog)
                    {
                        Debug.Log($"포즈 데이터 전송됨: {convertedData.Landmark.Count}개 포인트");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLog)
            {
                Debug.LogError($"포즈 처리 오류: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// MediaPipe 타입을 기존 타입으로 변환
    /// </summary>
    NormalizedLandmarkList ConvertPoseData(Mediapipe.Tasks.Components.Containers.NormalizedLandmarks input)
    {
        try
        {
            var output = new NormalizedLandmarkList();
            
            for (int i = 0; i < input.landmarks.Count; i++)
            {
                var landmark = input.landmarks[i];
                output.Landmark.Add(new NormalizedLandmark
                {
                    X = landmark.x,
                    Y = landmark.y,
                    Z = landmark.z,
                    Visibility = landmark.visibility ?? 1.0f
                });
            }
            
            return output;
        }
        catch (System.Exception e)
        {
            if (enableDebugLog)
            {
                Debug.LogError($"변환 오류: {e.Message}");
            }
            return null;
        }
    }
    
    /// <summary>
    /// 연결 상태 확인용 ContextMenu
    /// </summary>
    [ContextMenu("연결 상태 확인")]
    public void CheckConnection()
    {
        Debug.Log($"연결됨: {isConnected}");
        if (poseRunner != null)
        {
            Debug.Log($"Runner: {poseRunner.GetType().Name}");
        }
    }
} 