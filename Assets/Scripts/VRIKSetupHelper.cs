using UnityEngine;
using RootMotion.FinalIK;
using RootMotion.Demos;

[RequireComponent(typeof(VRIK))]
[RequireComponent(typeof(VRIKCalibrationController))]
public class VRIKSetupHelper : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoFindTrackers = true;
    
    private VRIK vrik;
    private VRIKCalibrationController calibrationController;
    
    void Start()
    {
        SetupComponents();
        
        if (autoFindTrackers)
        {
            AutoFindTrackers();
        }
        
        // 기본 Settings 설정
        SetupDefaultSettings();
    }
    
    void SetupComponents()
    {
        vrik = GetComponent<VRIK>();
        calibrationController = GetComponent<VRIKCalibrationController>();
        
        // VRIK 참조 설정
        calibrationController.ik = vrik;
    }
    
    void SetupDefaultSettings()
    {
        // Settings가 없으면 기본값 생성
        if (calibrationController.settings == null)
        {
            calibrationController.settings = new VRIKCalibrator.Settings();
        }
        
        var settings = calibrationController.settings;
        
        // 기본 설정값
        settings.scaleMlp = 1f;
        
        // HMD 방향 설정
        settings.headTrackerForward = Vector3.forward;
        settings.headTrackerUp = Vector3.up;
        
        // 손 컨트롤러 방향 설정
        settings.handTrackerForward = Vector3.forward;
        settings.handTrackerUp = Vector3.up;
        
        // 발 트래커 방향 설정
        settings.footTrackerForward = Vector3.forward;
        settings.footTrackerUp = Vector3.up;
        
        // 오프셋 설정
        settings.headOffset = new Vector3(0f, -0.1f, -0.05f);
        settings.handOffset = Vector3.zero;
        settings.footForwardOffset = 0f;
        settings.footInwardOffset = 0f;
        settings.footHeadingOffset = 0f;
        
        // 골반 가중치
        settings.pelvisPositionWeight = 1f;
        settings.pelvisRotationWeight = 1f;
        
        Debug.Log("Default VRIK Settings configured");
    }
    
    void AutoFindTrackers()
    {
        // XR Origin 찾기
        var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin == null)
        {
            Debug.LogWarning("XR Origin not found!");
            return;
        }
        
        // HMD (Main Camera) 찾기
        var mainCamera = xrOrigin.Camera;
        if (mainCamera != null)
        {
            calibrationController.headTracker = mainCamera.transform;
            Debug.Log($"Head Tracker set to: {mainCamera.name}");
        }
        
        // 컨트롤러 찾기
        var controllers = xrOrigin.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
        foreach (var controller in controllers)
        {
            if (controller.name.ToLower().Contains("left"))
            {
                calibrationController.leftHandTracker = controller.transform;
                Debug.Log($"Left Hand set to: {controller.name}");
            }
            else if (controller.name.ToLower().Contains("right"))
            {
                calibrationController.rightHandTracker = controller.transform;
                Debug.Log($"Right Hand set to: {controller.name}");
            }
        }
        
        // 트래커 찾기 (이름으로)
        FindTrackerByName("Waist", ref calibrationController.bodyTracker);
        FindTrackerByName("Left Foot", ref calibrationController.leftFootTracker);
        FindTrackerByName("Right Foot", ref calibrationController.rightFootTracker);
    }
    
    void FindTrackerByName(string trackerName, ref Transform trackerTransform)
    {
        var trackers = GameObject.FindObjectsOfType<Transform>();
        foreach (var t in trackers)
        {
            if (t.name.ToLower().Contains(trackerName.ToLower()) && 
                t.name.ToLower().Contains("tracker"))
            {
                trackerTransform = t;
                Debug.Log($"{trackerName} Tracker set to: {t.name}");
                break;
            }
        }
    }
    
    void OnGUI()
    {
        // 캘리브레이션 가이드
        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 400, 140));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("<b>VRIK Calibration Controls</b>");
        GUILayout.Label("C - Calibrate (T-Pose required)");
        GUILayout.Label("D - Apply saved calibration data");
        GUILayout.Label("S - Recalibrate scale only");
        
        if (calibrationController.data != null && calibrationController.data.scale > 0)
        {
            GUILayout.Label($"<color=green>Calibrated - Scale: {calibrationController.data.scale:F2}</color>");
        }
        else
        {
            GUILayout.Label("<color=yellow>Not calibrated yet</color>");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}