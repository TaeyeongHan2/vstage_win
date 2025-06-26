using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RootMotion.Demos;
using RootMotion.FinalIK;
using System.Collections;

public class CalibrationUISetupFixed : MonoBehaviour
{
    [Header("UI References")]
    public GameObject calibrationPanel;
    public Button calibrateButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI countdownText;
    
    [Header("Tracker Status")]
    public TextMeshProUGUI headStatus;
    public TextMeshProUGUI bodyStatus;
    public TextMeshProUGUI leftHandStatus;
    public TextMeshProUGUI rightHandStatus;
    public TextMeshProUGUI leftFootStatus;
    public TextMeshProUGUI rightFootStatus;
    
    [Header("Calibration")]
    public VRIKCalibrationController calibrationController;
    public SimpleVRIKCalibration simpleCalibration;
    
    void Start()
    {
        SetupUIReferences();
        UpdateTrackerStatus();
    }
    
    void SetupUIReferences()
    {
        // 자동으로 컴포넌트 찾기
        if (calibrationController == null)
            calibrationController = FindObjectOfType<VRIKCalibrationController>();
            
        if (simpleCalibration == null)
            simpleCalibration = FindObjectOfType<SimpleVRIKCalibration>();
            
        // SimpleVRIKCalibration에 controller 연결
        if (simpleCalibration != null && calibrationController != null)
            simpleCalibration.calibrationController = calibrationController;
            
        // 버튼 이벤트 연결
        if (calibrateButton != null)
        {
            calibrateButton.onClick.RemoveAllListeners();
            calibrateButton.onClick.AddListener(() => StartCoroutine(CalibrateWithUI()));
        }
    }
    
    IEnumerator CalibrateWithUI()
    {
        calibrateButton.interactable = false;
        
        // 카운트다운 UI 업데이트
        for (int i = 3; i > 0; i--)
        {
            UpdateStatus("Get ready for T-Pose!", i.ToString());
            yield return new WaitForSeconds(1f);
        }
        
        UpdateStatus("Calibrating...", "HOLD!");
        
        // 직접 캘리브레이션 실행 (SimpleVRIKCalibration 사용하지 않고)
        if (calibrationController != null)
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
            Debug.Log("Calibration Complete!");
        }
        
        yield return new WaitForSeconds(1f);
        
        UpdateStatus("Calibration Complete!", "✓");
        UpdateTrackerStatus();
        
        calibrateButton.interactable = true;
    }
    
    void UpdateStatus(string status, string countdown)
    {
        if (statusText != null) statusText.text = status;
        if (countdownText != null) countdownText.text = countdown;
    }
    
    void UpdateTrackerStatus()
    {
        if (calibrationController == null) return;
        
        // Head
        UpdateTrackerText(headStatus, "Head", calibrationController.headTracker != null);
        
        // Body
        UpdateTrackerText(bodyStatus, "Body", calibrationController.bodyTracker != null);
        
        // Hands
        UpdateTrackerText(leftHandStatus, "L Hand", calibrationController.leftHandTracker != null);
        UpdateTrackerText(rightHandStatus, "R Hand", calibrationController.rightHandTracker != null);
        
        // Feet
        UpdateTrackerText(leftFootStatus, "L Foot", calibrationController.leftFootTracker != null);
        UpdateTrackerText(rightFootStatus, "R Foot", calibrationController.rightFootTracker != null);
    }
    
    void UpdateTrackerText(TextMeshProUGUI text, string name, bool connected)
    {
        if (text == null) return;
        
        if (connected)
            text.text = $"{name}: <color=green>Connected</color>";
        else
            text.text = $"{name}: <color=red>Not Connected</color>";
    }
    
    // 추가 메서드 - 즉시 캘리브레이션
    public void InstantCalibrate()
    {
        if (calibrationController != null)
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
            
            UpdateStatus("Instant Calibration Complete!", "✓");
            UpdateTrackerStatus();
        }
    }
    
    // 스케일 재조정
    public void RecalibrateScale()
    {
        if (calibrationController != null && calibrationController.data.scale > 0)
        {
            VRIKCalibrator.RecalibrateScale(
                calibrationController.ik,
                calibrationController.data,
                calibrationController.settings
            );
            
            UpdateStatus("Scale Recalibrated!", "✓");
        }
    }
}