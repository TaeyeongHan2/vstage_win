using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RootMotion.Demos;

public class QuickCalibrationUI : MonoBehaviour
{
    [Header("Quick Setup")]
    public Button mainButton;
    public TextMeshProUGUI statusText;
    
    void Start()
    {
        // 자동 설정
        AutoSetup();
    }
    
    void AutoSetup()
    {
        // VRIKCalibrationController 찾기
        var vrikController = FindObjectOfType<VRIKCalibrationController>();
        if (vrikController == null)
        {
            Debug.LogError("No VRIKCalibrationController found!");
            return;
        }
        
        // SimpleVRIKCalibration 찾기 또는 생성
        var simpleCalib = FindObjectOfType<SimpleVRIKCalibration>();
        if (simpleCalib == null)
        {
            GameObject go = new GameObject("SimpleCalibration");
            simpleCalib = go.AddComponent<SimpleVRIKCalibration>();
        }
        
        // 연결
        simpleCalib.calibrationController = vrikController;
        
        // 버튼 이벤트
        if (mainButton != null)
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() => 
            {
                simpleCalib.CalibrateNow();
                UpdateStatus("Calibrating...");
            });
        }
        
        UpdateStatus("Ready");
    }
    
    void UpdateStatus(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }
    
    // Inspector에서 수동 실행 가능
    [ContextMenu("Force Setup")]
    public void ForceSetup()
    {
        AutoSetup();
        Debug.Log("Setup completed!");
    }
}