using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using VIVE.OpenXR.Samples.FacialTracking;
using System.Linq;

public class DetailedFacialTrackingTest : MonoBehaviour
{
    private ViveFacialTracking facialTrackingFeature;
    private float[] eyeBlendShapes;
    private float[] lipBlendShapes;
    
    [Header("Debug Settings")]
    public bool showAllValues = false;  // 모든 값 표시
    public float threshold = 0.1f;      // 이 값 이상만 표시
    
    void Start()
    {
        facialTrackingFeature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null)
        {
            Debug.LogError("ViveFacialTracking feature is not enabled! Please enable it in OpenXR Settings.");
            return;
        }
        
        Debug.Log("✅ ViveFacialTracking feature is enabled and ready!");
    }
    
    void Update()
    {
        if (facialTrackingFeature == null) return;
        
        // Eye Tracking 테스트
        TestEyeTracking();
        
        // Lip Tracking 테스트
        TestLipTracking();
    }
    
    void TestEyeTracking()
    {
        if (!facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeBlendShapes))
            return;
        
        // 주요 눈 표정만 체크
        LogIfActive("👁️ Left Blink", eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC]);
        LogIfActive("👁️ Right Blink", eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC]);
        LogIfActive("👁️ Left Wide", eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC]);
        LogIfActive("👁️ Right Wide", eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_WIDE_HTC]);
        
        if (showAllValues)
        {
            Debug.Log($"[Eye Tracking] Active values: {CountActiveValues(eyeBlendShapes)}");
        }
    }
    
    void TestLipTracking()
    {
        if (!facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipBlendShapes))
            return;
        
        // 주요 입 표정만 체크
        LogIfActive("👄 Jaw Open", lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC]);
        LogIfActive("👄 Mouth Raiser Right", lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_RAISER_RIGHT_HTC]);
        LogIfActive("👄 Mouth Raiser Left", lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_RAISER_LEFT_HTC]);
        LogIfActive("👄 Mouth Pout", lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_POUT_HTC]);
        
        if (showAllValues)
        {
            Debug.Log($"[Lip Tracking] Active values: {CountActiveValues(lipBlendShapes)}");
        }
    }
    
    void LogIfActive(string name, float value)
    {
        if (value > threshold)
        {
            Debug.Log($"{name}: {value:F3}");
        }
    }
    
    int CountActiveValues(float[] values)
    {
        return values.Count(v => v > threshold);
    }
} 