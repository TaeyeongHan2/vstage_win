using UnityEngine;
using RootMotion.FinalIK;
using RootMotion.Demos;

public class ScaleRecalibrator : MonoBehaviour
{
    public VRIKCalibrationController calibrationController;
    
    // Unity Button에서 바로 보이는 메서드
    public void RecalibrateNow()
    {
        if (calibrationController == null)
        {
            calibrationController = FindObjectOfType<VRIKCalibrationController>();
        }
        
        if (calibrationController != null && calibrationController.data.scale > 0)
        {
            VRIKCalibrator.RecalibrateScale(
                calibrationController.ik,
                calibrationController.data,
                calibrationController.settings
            );
            
            Debug.Log($"Scale Recalibrated! New scale: {calibrationController.data.scale}");
        }
        else
        {
            Debug.LogWarning("Cannot recalibrate: No calibration data found!");
        }
    }
    
    // 특정 스케일로 설정
    public void SetScale(float newScale)
    {
        if (calibrationController != null)
        {
            calibrationController.settings.scaleMlp = newScale;
            RecalibrateNow();
        }
    }
    
    // 스케일 증가
    public void IncreaseScale()
    {
        if (calibrationController != null)
        {
            calibrationController.settings.scaleMlp *= 1.1f;
            RecalibrateNow();
        }
    }
    
    // 스케일 감소
    public void DecreaseScale()
    {
        if (calibrationController != null)
        {
            calibrationController.settings.scaleMlp *= 0.9f;
            RecalibrateNow();
        }
    }
}