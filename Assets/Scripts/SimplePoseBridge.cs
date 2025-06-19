using UnityEngine;
using Mediapipe;

/// <summary>
/// MediaPipe 샘플과 SimpleHumanoidController를 연결하는 간단한 브리지
/// </summary>
public class SimplePoseBridge : MonoBehaviour
{
    [Header("연결")]
    public SimpleHumanoidController humanoidController;
    public bool enableDebugLog = false;
    
    [Header("테스트 모드")]
    public bool enableTestMode = false;
    public float testAnimationSpeed = 1f;
    
    // 정적 인스턴스 (어디서든 접근 가능)
    public static SimplePoseBridge Instance;
    
    // 테스트용 변수
    private float testTime = 0f;
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        if (humanoidController == null)
        {
            humanoidController = FindObjectOfType<SimpleHumanoidController>();
        }
        
        if (humanoidController == null)
        {
            Debug.LogError("SimpleHumanoidController를 찾을 수 없습니다!");
        }
        else
        {
            Debug.Log("SimplePoseBridge 준비 완료!");
        }
    }
    
    void Update()
    {
        // 테스트 모드에서 자동으로 포즈 애니메이션
        if (enableTestMode && humanoidController != null)
        {
            testTime += Time.deltaTime * testAnimationSpeed;
            SendTestPoseData();
        }
    }
    
    /// <summary>
    /// MediaPipe에서 포즈 데이터를 받는 함수
    /// </summary>
    public void ReceivePoseData(NormalizedLandmarkList landmarks)
    {
        if (landmarks?.Landmark == null || landmarks.Landmark.Count == 0)
        {
            if (enableDebugLog)
                Debug.LogWarning("비어있는 포즈 데이터 수신");
            return;
        }
        
        if (humanoidController == null) return;
        
        if (enableDebugLog)
        {
            Debug.Log($"포즈 데이터 수신: {landmarks.Landmark.Count}개");
        }
        
        humanoidController.ApplyPose(landmarks);
    }
    
    /// <summary>
    /// 정적 메서드 - MediaPipe 샘플에서 이걸 호출하면 됨
    /// </summary>
    public static void SendPoseData(NormalizedLandmarkList landmarks)
    {
        if (Instance != null)
        {
            Instance.ReceivePoseData(landmarks);
        }
    }
    
    /// <summary>
    /// 테스트용 애니메이션 포즈 데이터 전송
    /// </summary>
    void SendTestPoseData()
    {
        var landmarks = CreateTestLandmarks();
        ReceivePoseData(landmarks);
    }
    
    /// <summary>
    /// 테스트용 랜드마크 생성
    /// </summary>
    NormalizedLandmarkList CreateTestLandmarks()
    {
        var landmarks = new NormalizedLandmarkList();
        
        // 33개 포인트 추가
        for (int i = 0; i < 33; i++)
        {
            landmarks.Landmark.Add(new NormalizedLandmark
            {
                X = 0.5f,
                Y = 0.5f,
                Z = 0f,
                Visibility = 1.0f
            });
        }
        
        // 애니메이션 적용
        float wave = Mathf.Sin(testTime);
        float wave2 = Mathf.Sin(testTime * 0.7f);
        
        // 머리 움직임
        landmarks.Landmark[0].Y = 0.3f + wave * 0.1f;
        
        // 어깨
        landmarks.Landmark[11].X = 0.3f; landmarks.Landmark[11].Y = 0.5f; // 왼쪽 어깨
        landmarks.Landmark[12].X = 0.7f; landmarks.Landmark[12].Y = 0.5f; // 오른쪽 어깨
        
        // 팔 움직임
        landmarks.Landmark[13].X = 0.2f + wave * 0.1f; landmarks.Landmark[13].Y = 0.6f; // 왼쪽 팔꿈치
        landmarks.Landmark[14].X = 0.8f + wave2 * 0.1f; landmarks.Landmark[14].Y = 0.6f; // 오른쪽 팔꿈치
        landmarks.Landmark[15].X = 0.1f + wave * 0.15f; landmarks.Landmark[15].Y = 0.7f; // 왼쪽 손목
        landmarks.Landmark[16].X = 0.9f + wave2 * 0.15f; landmarks.Landmark[16].Y = 0.7f; // 오른쪽 손목
        
        return landmarks;
    }
    
    /// <summary>
    /// 테스트용 T포즈
    /// </summary>
    [ContextMenu("Test T-Pose")]
    public void TestTPose()
    {
        if (humanoidController == null) return;
        
        // 간단한 T포즈 데이터 생성
        var landmarks = new NormalizedLandmarkList();
        
        // 33개 포인트 추가 (기본값)
        for (int i = 0; i < 33; i++)
        {
            landmarks.Landmark.Add(new NormalizedLandmark
            {
                X = 0.5f,
                Y = 0.5f,
                Z = 0f,
                Visibility = 1.0f
            });
        }
        
        // T포즈 위치 설정
        landmarks.Landmark[0].Y = 0.3f; // 코 (머리)
        landmarks.Landmark[11].X = 0.3f; landmarks.Landmark[11].Y = 0.5f; // 왼쪽 어깨
        landmarks.Landmark[12].X = 0.7f; landmarks.Landmark[12].Y = 0.5f; // 오른쪽 어깨
        landmarks.Landmark[13].X = 0.2f; landmarks.Landmark[13].Y = 0.6f; // 왼쪽 팔꿈치
        landmarks.Landmark[14].X = 0.8f; landmarks.Landmark[14].Y = 0.6f; // 오른쪽 팔꿈치
        landmarks.Landmark[15].X = 0.1f; landmarks.Landmark[15].Y = 0.7f; // 왼쪽 손목
        landmarks.Landmark[16].X = 0.9f; landmarks.Landmark[16].Y = 0.7f; // 오른쪽 손목
        
        ReceivePoseData(landmarks);
        Debug.Log("T포즈 테스트 완료!");
    }
    
    /// <summary>
    /// 테스트 모드 토글
    /// </summary>
    [ContextMenu("Toggle Test Mode")]
    public void ToggleTestMode()
    {
        enableTestMode = !enableTestMode;
        Debug.Log($"테스트 모드: {(enableTestMode ? "활성화" : "비활성화")}");
    }
}