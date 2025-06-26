using UnityEngine;
using RootMotion.FinalIK;
using RootMotion.Demos;

public class VRIKCalibrationExample : MonoBehaviour
{
    [Header("References")]
    public VRIKCalibrationController calibrationController;
    
    [Header("Custom Calibration")]
    public bool autoCalibrate = false;
    public float calibrationDelay = 3f;
    
    void Start()
    {
        if (autoCalibrate)
        {
            Invoke(nameof(PerformAutoCalibration), calibrationDelay);
        }
    }
    
    // 자동 캘리브레이션
    void PerformAutoCalibration()
    {
        if (calibrationController == null) return;
        
        // Settings 확인
        if (calibrationController.settings == null)
        {
            Debug.LogError("Calibration settings not configured!");
            return;
        }
        
        // 모든 트래커가 연결되었는지 확인
        if (calibrationController.headTracker == null)
        {
            Debug.LogError("Head tracker not assigned!");
            return;
        }
        
        // 캘리브레이션 실행
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
        
        Debug.Log("Auto calibration completed!");
    }
    
    // 수동 캘리브레이션 메서드들
    public void CalibrateWithTPose()
    {
        // C키와 동일한 동작
        PerformAutoCalibration();
    }
    
    public void ApplySavedCalibration()
    {
        // D키와 동일한 동작
        if (calibrationController.data.scale == 0f)
        {
            Debug.LogError("No calibration data available!");
            return;
        }
        
        VRIKCalibrator.Calibrate(
            calibrationController.ik,
            calibrationController.data,
            calibrationController.headTracker,
            calibrationController.bodyTracker,
            calibrationController.leftHandTracker,
            calibrationController.rightHandTracker,
            calibrationController.leftFootTracker,
            calibrationController.rightFootTracker
        );
    }
    
    public void RecalibrateScale(float newScaleMlp)
    {
        // 스케일 변경
        calibrationController.settings.scaleMlp = newScaleMlp;
        VRIKCalibrator.RecalibrateScale(
            calibrationController.ik,
            calibrationController.data,
            calibrationController.settings
        );
    }
    
    // 캘리브레이션 데이터 저장/로드
    public void SaveCalibrationData()
    {
        if (calibrationController.data.scale == 0f)
        {
            Debug.LogError("No calibration data to save!");
            return;
        }
        
        // JSON으로 저장 (예시)
        string json = JsonUtility.ToJson(calibrationController.data);
        PlayerPrefs.SetString("VRIKCalibrationData", json);
        PlayerPrefs.Save();
        
        Debug.Log("Calibration data saved!");
    }
    
    public void LoadCalibrationData()
    {
        string json = PlayerPrefs.GetString("VRIKCalibrationData", "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("No saved calibration data found!");
            return;
        }
        
        calibrationController.data = JsonUtility.FromJson<VRIKCalibrator.CalibrationData>(json);
        Debug.Log("Calibration data loaded!");
    }
}