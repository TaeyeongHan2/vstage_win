using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;
using VIVE.OpenXR.Samples.FacialTracking;

public class SimpleFacialTrackingTest : MonoBehaviour
{
    private ViveFacialTracking facialTrackingFeature;
    private float[] eyeBlendShapes;
    private float[] lipBlendShapes;
    
    void Start()
    {
        // ViveFacialTracking Feature 가져오기
        facialTrackingFeature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null)
        {
            Debug.LogError("ViveFacialTracking feature is not enabled!");
            return;
        }
        
        Debug.Log("ViveFacialTracking feature found!");
    }
    
    void Update()
    {
        if (facialTrackingFeature == null) return;
        
        // Eye Tracking 데이터 가져오기
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out eyeBlendShapes))
        {
            // 눈 깜빡임 값만 출력 (테스트용)
            float leftBlink = eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC];
            float rightBlink = eyeBlendShapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC];
            
            if (leftBlink > 0.1f || rightBlink > 0.1f)
            {
                Debug.Log($"Eye Blink - Left: {leftBlink:F2}, Right: {rightBlink:F2}");
            }
        }
        
        // Lip Tracking 데이터 가져오기
        if (facialTrackingFeature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out lipBlendShapes))
        {
            // 입 열기 값만 출력 (테스트용)
            float jawOpen = lipBlendShapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC];
            
            if (jawOpen > 0.1f)
            {
                Debug.Log($"Jaw Open: {jawOpen:F2}");
            }
        }
    }
} 