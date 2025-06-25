using UnityEngine;
using VIVE.OpenXR.Samples.FacialTracking;

/// <summary>
/// HybridFacialTrackingTest의 데이터를 아바타에 적용하는 간단한 예제
/// </summary>
public class SimpleFacialTrackingApplier : MonoBehaviour
{
    [Header("Source")]
    public HybridFacialTrackingTest facialTrackingSource;
    
    [Header("Avatar")]
    public SkinnedMeshRenderer targetMeshRenderer;
    
    [Header("Blend Shape Mapping")]
    [SerializeField] private int leftBlinkIndex = -1;
    [SerializeField] private int rightBlinkIndex = -1;
    [SerializeField] private int jawOpenIndex = -1;
    
    void Start()
    {
        if (facialTrackingSource == null)
        {
            facialTrackingSource = FindObjectOfType<HybridFacialTrackingTest>();
        }
        
        if (targetMeshRenderer != null)
        {
            FindBlendShapeIndices();
        }
    }
    
    void FindBlendShapeIndices()
    {
        // 블렌드셰이프 이름으로 인덱스 찾기 (예시)
        for (int i = 0; i < targetMeshRenderer.sharedMesh.blendShapeCount; i++)
        {
            string shapeName = targetMeshRenderer.sharedMesh.GetBlendShapeName(i);
            Debug.Log($"BlendShape [{i}]: {shapeName}");
            
            // 이름에 따라 매핑 (아바타에 따라 수정 필요)
            if (shapeName.Contains("Blink") && shapeName.Contains("Left"))
                leftBlinkIndex = i;
            else if (shapeName.Contains("Blink") && shapeName.Contains("Right"))
                rightBlinkIndex = i;
            else if (shapeName.Contains("Jaw") || shapeName.Contains("Open"))
                jawOpenIndex = i;
        }
    }
    
    void Update()
    {
        if (facialTrackingSource == null || targetMeshRenderer == null) return;
        
        // 얼굴 트래킹 데이터 가져오기
        var eyeData = facialTrackingSource.GetCurrentEyeBlendShapes();
        var lipData = facialTrackingSource.GetCurrentLipBlendShapes();
        
        // 아바타에 적용 (0-100 범위로 변환)
        if (leftBlinkIndex >= 0)
            targetMeshRenderer.SetBlendShapeWeight(leftBlinkIndex, 
                eyeData[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC] * 100f);
                
        if (rightBlinkIndex >= 0)
            targetMeshRenderer.SetBlendShapeWeight(rightBlinkIndex, 
                eyeData[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC] * 100f);
                
        if (jawOpenIndex >= 0)
            targetMeshRenderer.SetBlendShapeWeight(jawOpenIndex, 
                lipData[(int)XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC] * 100f);
    }
} 