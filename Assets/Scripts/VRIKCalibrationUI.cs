using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using RootMotion.FinalIK;
using RootMotion.Demos;

public class VRIKCalibrationUI : MonoBehaviour
{
    [Header("VRIK References")]
    public VRIKCalibrationController calibrationController;
    
    [Header("UI References")]
    public Button calibrateButton;
    public Text countdownText;
    public Text statusText;
    
    [Header("Settings")]
    public int countdownSeconds = 3;
    public bool showDebugLogs = true;
    
    void Start()
    {
        // 자동으로 컴포넌트 찾기
        if (calibrationController == null)
        {
            calibrationController = FindObjectOfType<VRIKCalibrationController>();
            if (calibrationController == null)
            {
                Debug.LogError("VRIKCalibrationController not found in scene!");
                return;
            }
        }
        
        // 버튼 이벤트 연결
        if (calibrateButton != null)
        {
            calibrateButton.onClick.AddListener(OnCalibrateButtonClick);
        }
        
        // UI 초기화
        UpdateUI("Ready to calibrate", "");
    }
    
    // UI 버튼 클릭 시 호출
    public void OnCalibrateButtonClick()
    {
        if (calibrationController == null)
        {
            UpdateUI("Error: No calibration controller!", "");
            return;
        }
        
        // T-Pose 카운트다운 시작
        StartCoroutine(CalibrationCountdown());
    }
    
    // 카운트다운 코루틴
    IEnumerator CalibrationCountdown()
    {
        // 버튼 비활성화
        if (calibrateButton != null)
            calibrateButton.interactable = false;
        
        UpdateUI("Get ready for T-Pose!", "");
        yield return new WaitForSeconds(1f);
        
        // 카운트다운
        for (int i = countdownSeconds; i > 0; i--)
        {
            UpdateUI($"T-Pose in...", i.ToString());
            
            if (showDebugLogs)
                Debug.Log($"Calibration in {i}...");
                
            yield return new WaitForSeconds(1f);
        }
        
        // 캘리브레이션 실행
        UpdateUI("Calibrating...", "HOLD POSE!");
        PerformCalibration();
        
        yield return new WaitForSeconds(1f);
        
        // 완료
        UpdateUI("Calibration Complete!", "✓");
        
        // 버튼 재활성화
        if (calibrateButton != null)
            calibrateButton.interactable = true;
    }
    
    // 실제 캘리브레이션 수행
    void PerformCalibration()
    {
        if (calibrationController == null) return;
        
        // VRIKCalibrator.Calibrate 호출
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
        
        if (showDebugLogs)
        {
            Debug.Log("=== Calibration Complete ===");
            Debug.Log($"Scale: {calibrationController.data.scale}");
            Debug.Log($"Head: {(calibrationController.headTracker != null ? "Connected" : "Not Connected")}");
            Debug.Log($"Body: {(calibrationController.bodyTracker != null ? "Connected" : "Not Connected")}");
            Debug.Log($"Hands: L-{(calibrationController.leftHandTracker != null ? "✓" : "✗")} R-{(calibrationController.rightHandTracker != null ? "✓" : "✗")}");
            Debug.Log($"Feet: L-{(calibrationController.leftFootTracker != null ? "✓" : "✗")} R-{(calibrationController.rightFootTracker != null ? "✓" : "✗")}");
        }
    }
    
    // UI 업데이트
    void UpdateUI(string status, string countdown)
    {
        if (statusText != null)
            statusText.text = status;
            
        if (countdownText != null)
            countdownText.text = countdown;
    }
    
    // 추가 유틸리티 메서드들
    public void RecalibrateScale()
    {
        if (calibrationController != null && calibrationController.data.scale > 0)
        {
            VRIKCalibrator.RecalibrateScale(calibrationController.ik, calibrationController.data, calibrationController.settings);
            UpdateUI("Scale recalibrated!", "");
        }
    }
    
    public void ApplySavedCalibration()
    {
        if (calibrationController != null && calibrationController.data.scale > 0)
        {
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
            UpdateUI("Saved calibration applied!", "");
        }
        else
        {
            UpdateUI("No saved calibration!", "");
        }
    }
}