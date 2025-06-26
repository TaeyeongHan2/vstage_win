using UnityEngine;
using System.Collections;
using RootMotion.Demos;
using RootMotion.FinalIK;

public class SimpleVRIKCalibration : MonoBehaviour
{
    public VRIKCalibrationController calibrationController;
    
    // 버튼에서 직접 호출
    public void CalibrateNow()
    {
        StartCoroutine(Calibrate());
    }
    
    IEnumerator Calibrate()
    {
        // 3초 카운트다운
        for (int i = 3; i > 0; i--)
        {
            Debug.Log($"T-Pose in {i}...");
            yield return new WaitForSeconds(1f);
        }
        
        // 캘리브레이션 실행
        Debug.Log("Calibrating NOW!");
        calibrationController.data = VRIKCalibrator.Calibrate(
            calibrationController.ik,
            calibrationController.settings,
            calibrationController.headTracker,
            calibrationController.bodyTracker,
            calibrationController.leftHandTracker,
            calibrationController.rightHandTracker,
            calibrationController.leftFootTracker,
            calibrationController.rightFootTracker
        );
        
        Debug.Log("Calibration Complete!");
    }
    
    // 카운트다운 없이 즉시 캘리브레이션
    public void CalibrateInstant()
    {
        calibrationController.data = VRIKCalibrator.Calibrate(
            calibrationController.ik,
            calibrationController.settings,
            calibrationController.headTracker,
            calibrationController.bodyTracker,
            calibrationController.leftHandTracker,
            calibrationController.rightHandTracker,
            calibrationController.leftFootTracker,
            calibrationController.rightFootTracker
        );
        Debug.Log("Instant Calibration Complete!");
    }
    
    // 스케일만 재조정
    public void RecalibrateScale()
    {
        if (calibrationController == null)
        {
            Debug.LogError("Calibration Controller is null!");
            return;
        }
        
        if (calibrationController.data == null || calibrationController.data.scale <= 0)
        {
            Debug.LogError("No calibration data available! Calibrate first.");
            return;
        }
        
        VRIKCalibrator.RecalibrateScale(
            calibrationController.ik, 
            calibrationController.data, 
            calibrationController.settings
        );
        
        Debug.Log("Scale Recalibrated!");
    }
    
    // Unity Button을 위한 대체 메서드
    public void RecalibrateScaleButton()
    {
        RecalibrateScale();
    }
    
    // Force recompile
} 