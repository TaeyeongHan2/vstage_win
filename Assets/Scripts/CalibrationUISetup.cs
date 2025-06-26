using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RootMotion.Demos;
using System.Collections;

public class CalibrationUISetup : MonoBehaviour
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
        if (calibrateButton != null && simpleCalibration != null)
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
        
        // 캘리브레이션 실행
        simpleCalibration.CalibrateNow();
        
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
}