using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;

public class SimpleShinanoFacialTracking : MonoBehaviour
{
    [Header("필수 설정")]
    public SkinnedMeshRenderer targetMesh;
    
    [Header("조정값")]
    [Range(0f, 2f)] public float intensity = 1.0f;
    [Range(0f, 1f)] public float smoothing = 0.1f;
    
    [Header("디버그")]
    public bool showDebug = false;
    
    // VIVE 페이셜 트래킹
    private ViveFacialTracking facialTracking;
    private float[] lipData;
    
    // 블렌드셰이프 인덱스 캐시
    private int jawOpenIndex = -1;
    private int smileIndex = -1;
    private int mouthWideIndex = -1;
    private int mouthOIndex = -1;
    private int sadIndex = -1;
    private int tongueIndex = -1;
    
    // 스무딩용 값들
    private float smoothJaw = 0f;
    private float smoothSmile = 0f;
    private float smoothWide = 0f;
    private float smoothO = 0f;
    private float smoothSad = 0f;
    private float smoothTongue = 0f;
    
    void Start()
    {
        // VIVE 페이셜 트래킹 초기화
        facialTracking = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTracking == null || !facialTracking.enabled)
        {
            Debug.LogError("VIVE Facial Tracking이 활성화되지 않았습니다!");
            enabled = false;
            return;
        }
        
        // 타겟 메시 자동 찾기
        if (targetMesh == null)
        {
            // Body라는 이름의 메시를 먼저 찾기
            var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var mesh in meshes)
            {
                if (mesh.name == "Body")
                {
                    targetMesh = mesh;
                    break;
                }
            }
            
            // 못 찾으면 첫 번째 메시 사용
            if (targetMesh == null && meshes.Length > 0)
            {
                targetMesh = meshes[0];
            }
        }
        
        if (targetMesh == null)
        {
            Debug.LogError("SkinnedMeshRenderer를 찾을 수 없습니다!");
            enabled = false;
            return;
        }
        
        // 립 데이터 배열 초기화
        lipData = new float[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC];
        
        // 블렌드셰이프 인덱스 캐시
        CacheBlendshapeIndices();
        
        Debug.Log("SimpleShinanoFacialTracking 초기화 완료!");
    }
    
    void CacheBlendshapeIndices()
    {
        if (targetMesh?.sharedMesh == null) return;
        
        // 블렌드셰이프 이름으로 인덱스 찾기
        for (int i = 0; i < targetMesh.sharedMesh.blendShapeCount; i++)
        {
            string name = targetMesh.sharedMesh.GetBlendShapeName(i);
            
            switch (name)
            {
                case "mouth_a1":
                    jawOpenIndex = i;
                    break;
                case "mouth_smile":
                    smileIndex = i;
                    break;
                case "mouth_wide":
                    mouthWideIndex = i;
                    break;
                case "mouth_o1":
                    mouthOIndex = i;
                    break;
                case "mouth_sad":
                    sadIndex = i;
                    break;
                case "tongue_pero":
                    tongueIndex = i;
                    break;
            }
        }
        
        if (showDebug)
        {
            Debug.Log($"블렌드셰이프 인덱스 - Jaw: {jawOpenIndex}, Smile: {smileIndex}, Wide: {mouthWideIndex}, O: {mouthOIndex}, Sad: {sadIndex}, Tongue: {tongueIndex}");
        }
    }
    
    void Update()
    {
        if (targetMesh == null || facialTracking == null) return;
        
        // VIVE 페이셜 데이터 가져오기
        bool success = facialTracking.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC,
            out lipData
        );
        
        if (!success || lipData == null) return;
        
        // 주요 표정들 처리
        ProcessFacialExpressions();
    }
    
    void ProcessFacialExpressions()
    {
        // 입 벌리기 (Jaw Open)
        if (jawOpenIndex >= 0)
        {
            float jawValue = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC];
            smoothJaw = Mathf.Lerp(smoothJaw, jawValue * intensity, smoothing);
            targetMesh.SetBlendShapeWeight(jawOpenIndex, smoothJaw * 100f);
            
            if (showDebug && smoothJaw > 0.01f)
                Debug.Log($"Jaw Open: {smoothJaw:F2}");
        }
        
        // 웃음 (Smile - 좌우 합산)
        if (smileIndex >= 0)
        {
            float smileLeft = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_LEFT_HTC];
            float smileRight = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_RIGHT_HTC];
            float smileValue = Mathf.Max(smileLeft, smileRight);
            
            smoothSmile = Mathf.Lerp(smoothSmile, smileValue * intensity, smoothing);
            targetMesh.SetBlendShapeWeight(smileIndex, smoothSmile * 100f);
            
            if (showDebug && smoothSmile > 0.01f)
                Debug.Log($"Smile: {smoothSmile:F2}");
        }
        
        // 입 넓히기 (Wide Mouth)
        if (mouthWideIndex >= 0)
        {
            float wideLeft = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_LEFT_HTC];
            float wideRight = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_RIGHT_HTC];
            float wideValue = Mathf.Max(wideLeft, wideRight);
            
            smoothWide = Mathf.Lerp(smoothWide, wideValue * intensity * 0.8f, smoothing);
            targetMesh.SetBlendShapeWeight(mouthWideIndex, smoothWide * 100f);
            
            if (showDebug && smoothWide > 0.01f)
                Debug.Log($"Wide: {smoothWide:F2}");
        }
        
        // O모양 입 (Pout)
        if (mouthOIndex >= 0)
        {
            float oValue = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC];
            smoothO = Mathf.Lerp(smoothO, oValue * intensity, smoothing);
            targetMesh.SetBlendShapeWeight(mouthOIndex, smoothO * 100f);
            
            if (showDebug && smoothO > 0.01f)
                Debug.Log($"O Shape: {smoothO:F2}");
        }
        
        // 슬픈 표정 (Sad - 아래쪽 입술 움직임)
        if (sadIndex >= 0)
        {
            float sadLeft = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNLEFT_HTC];
            float sadRight = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNRIGHT_HTC];
            float sadValue = Mathf.Max(sadLeft, sadRight);
            
            smoothSad = Mathf.Lerp(smoothSad, sadValue * intensity * 0.7f, smoothing);
            targetMesh.SetBlendShapeWeight(sadIndex, smoothSad * 100f);
            
            if (showDebug && smoothSad > 0.01f)
                Debug.Log($"Sad: {smoothSad:F2}");
        }
        
        // 혀 내밀기 (Tongue Out)
        if (tongueIndex >= 0)
        {
            float tongueValue = lipData[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_LONGSTEP1_HTC];
            smoothTongue = Mathf.Lerp(smoothTongue, tongueValue * intensity, smoothing);
            targetMesh.SetBlendShapeWeight(tongueIndex, smoothTongue * 100f);
            
            if (showDebug && smoothTongue > 0.01f)
                Debug.Log($"Tongue: {smoothTongue:F2}");
        }
    }
    
    // 수동으로 표정 설정하는 함수 (테스트용)
    public void SetExpression(string expressionName, float value)
    {
        int index = -1;
        
        switch (expressionName.ToLower())
        {
            case "jaw":
            case "mouth_a1":
                index = jawOpenIndex;
                break;
            case "smile":
            case "mouth_smile":
                index = smileIndex;
                break;
            case "wide":
            case "mouth_wide":
                index = mouthWideIndex;
                break;
            case "o":
            case "mouth_o1":
                index = mouthOIndex;
                break;
            case "sad":
            case "mouth_sad":
                index = sadIndex;
                break;
            case "tongue":
            case "tongue_pero":
                index = tongueIndex;
                break;
        }
        
        if (index >= 0 && targetMesh != null)
        {
            targetMesh.SetBlendShapeWeight(index, Mathf.Clamp01(value) * 100f);
        }
    }
    
    // 간단한 GUI 디버깅
    void OnGUI()
    {
        if (!showDebug) return;
        
        int y = 10;
        GUI.Box(new Rect(10, y, 200, 150), "");
        
        y += 5;
        GUI.Label(new Rect(15, y, 190, 20), "=== 페이셜 트래킹 ===");
        y += 25;
        
        GUI.Label(new Rect(15, y, 190, 20), $"입 벌림: {(smoothJaw * 100f):F0}%");
        y += 20;
        GUI.Label(new Rect(15, y, 190, 20), $"미소: {(smoothSmile * 100f):F0}%");
        y += 20;
        GUI.Label(new Rect(15, y, 190, 20), $"입 넓힘: {(smoothWide * 100f):F0}%");
        y += 20;
        GUI.Label(new Rect(15, y, 190, 20), $"O 모양: {(smoothO * 100f):F0}%");
        y += 20;
        GUI.Label(new Rect(15, y, 190, 20), $"슬픔: {(smoothSad * 100f):F0}%");
        y += 20;
        GUI.Label(new Rect(15, y, 190, 20), $"혀: {(smoothTongue * 100f):F0}%");
    }
} 