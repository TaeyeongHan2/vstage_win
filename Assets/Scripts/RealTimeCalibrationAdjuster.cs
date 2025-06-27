using UnityEngine;
using RootMotion.FinalIK;
using RootMotion.Demos;

public class RealTimeCalibrationAdjuster : MonoBehaviour
{
    [Header("References")]
    public VRIKCalibrationController calibrationController;
    
    [Header("Real-Time Adjustments")]
    [Range(0.5f, 1.5f)]
    public float scaleMultiplier = 1f;
    
    [Header("Foot Tracker Offsets")]
    [Range(-0.2f, 0.2f)]
    public float footHeightOffset = 0f;
    [Range(-0.2f, 0.2f)]
    public float footForwardOffset = 0f;
    [Range(-0.2f, 0.2f)]
    public float footInwardOffset = 0f;
    
    [Header("Body Tracker Offsets")]
    [Range(-0.2f, 0.2f)]
    public float bodyHeightOffset = 0f;
    public Vector3 bodyRotationOffset = Vector3.zero;
    
    [Header("Auto-Detection")]
    public bool autoDetectFootHeight = false;
    public float groundDetectionDistance = 0.5f;
    public LayerMask groundLayers = -1;
    
    private VRIKCalibrator.Settings originalSettings;
    private float detectedFootOffset;
    private bool hasOriginalSettings = false;
    
    void Start()
    {
        if (calibrationController == null)
            calibrationController = FindObjectOfType<VRIKCalibrationController>();
            
        // 원본 설정 저장
        if (calibrationController != null && calibrationController.settings != null)
        {
            originalSettings = new VRIKCalibrator.Settings();
            CopySettings(calibrationController.settings, originalSettings);
            hasOriginalSettings = true;
        }
    }
    
    void CopySettings(VRIKCalibrator.Settings from, VRIKCalibrator.Settings to)
    {
        to.scaleMlp = from.scaleMlp;
        to.headTrackerForward = from.headTrackerForward;
        to.headTrackerUp = from.headTrackerUp;
        to.handTrackerForward = from.handTrackerForward;
        to.handTrackerUp = from.handTrackerUp;
        to.footTrackerForward = from.footTrackerForward;
        to.footTrackerUp = from.footTrackerUp;
        to.headOffset = from.headOffset;
        to.handOffset = from.handOffset;
        to.footForwardOffset = from.footForwardOffset;
        to.footInwardOffset = from.footInwardOffset;
        to.footHeadingOffset = from.footHeadingOffset;
        to.pelvisPositionWeight = from.pelvisPositionWeight;
        to.pelvisRotationWeight = from.pelvisRotationWeight;
    }
    
    void Update()
    {
        if (calibrationController == null || calibrationController.settings == null) return;
        
        // 자동 발 높이 감지
        if (autoDetectFootHeight)
        {
            DetectFootHeight();
        }
        
        // 실시간 설정 적용
        ApplyRealtimeSettings();
        
        // 변경사항이 있을 때만 재캘리브레이션
        if (HasSettingsChanged() && calibrationController.data.scale > 0)
        {
            // 스케일만 재조정
            if (Mathf.Abs(scaleMultiplier - 1f) > 0.01f)
            {
                VRIKCalibrator.RecalibrateScale(
                    calibrationController.ik,
                    calibrationController.data,
                    originalSettings.scaleMlp * scaleMultiplier
                );
            }
        }
    }
    
    void DetectFootHeight()
    {
        if (calibrationController.leftFootTracker == null || 
            calibrationController.rightFootTracker == null) return;
        
        float leftFootGround = GetGroundHeight(calibrationController.leftFootTracker.position);
        float rightFootGround = GetGroundHeight(calibrationController.rightFootTracker.position);
        
        float leftOffset = calibrationController.leftFootTracker.position.y - leftFootGround;
        float rightOffset = calibrationController.rightFootTracker.position.y - rightFootGround;
        
        detectedFootOffset = (leftOffset + rightOffset) / 2f;
        
        // 자동으로 적용
        if (autoDetectFootHeight)
        {
            footHeightOffset = -detectedFootOffset;
        }
    }
    
    float GetGroundHeight(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * groundDetectionDistance, 
            Vector3.down, out hit, groundDetectionDistance * 2f, groundLayers))
        {
            return hit.point.y;
        }
        return 0f;
    }
    
    void ApplyRealtimeSettings()
    {
        if (!hasOriginalSettings) return;
        
        var settings = calibrationController.settings;
        
        // 발 오프셋 적용
        settings.footForwardOffset = originalSettings.footForwardOffset + footForwardOffset;
        settings.footInwardOffset = originalSettings.footInwardOffset + footInwardOffset;
        settings.footHeadingOffset = originalSettings.footHeadingOffset + footHeightOffset;
        
        // 스케일 적용
        settings.scaleMlp = originalSettings.scaleMlp * scaleMultiplier;
    }
    
    bool HasSettingsChanged()
    {
        return Mathf.Abs(footHeightOffset) > 0.001f ||
               Mathf.Abs(footForwardOffset) > 0.001f ||
               Mathf.Abs(footInwardOffset) > 0.001f ||
               Mathf.Abs(scaleMultiplier - 1f) > 0.001f;
    }
    
    public void ApplyFootHeightFix()
    {
        // 발 트래커 위치를 임시로 조정한 후 재캘리브레이션
        if (calibrationController.leftFootTracker != null)
        {
            calibrationController.leftFootTracker.position += Vector3.up * footHeightOffset;
        }
        if (calibrationController.rightFootTracker != null)
        {
            calibrationController.rightFootTracker.position += Vector3.up * footHeightOffset;
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
        
        // 원래 위치로 복원
        if (calibrationController.leftFootTracker != null)
        {
            calibrationController.leftFootTracker.position -= Vector3.up * footHeightOffset;
        }
        if (calibrationController.rightFootTracker != null)
        {
            calibrationController.rightFootTracker.position -= Vector3.up * footHeightOffset;
        }
        
        Debug.Log("Applied foot height fix and recalibrated!");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 400, 350, 300));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("<b>Real-Time Calibration Adjustments</b>");
        
        // 스케일 조정
        GUILayout.Label($"Scale Multiplier: {scaleMultiplier:F2}");
        scaleMultiplier = GUILayout.HorizontalSlider(scaleMultiplier, 0.5f, 1.5f);
        
        // 발 높이 조정
        GUILayout.Label($"Foot Height Offset: {footHeightOffset:F3}");
        footHeightOffset = GUILayout.HorizontalSlider(footHeightOffset, -0.2f, 0.2f);
        
        if (autoDetectFootHeight)
        {
            GUILayout.Label($"<color=yellow>Auto-detected offset: {detectedFootOffset:F3}</color>");
        }
        
        // 자동 감지 토글
        autoDetectFootHeight = GUILayout.Toggle(autoDetectFootHeight, "Auto-Detect Foot Height");
        
        GUILayout.Space(10);
        
        // 버튼들
        if (GUILayout.Button("Apply Foot Height Fix"))
        {
            ApplyFootHeightFix();
        }
        
        if (GUILayout.Button("Reset to Original"))
        {
            scaleMultiplier = 1f;
            footHeightOffset = 0f;
            footForwardOffset = 0f;
            footInwardOffset = 0f;
            bodyHeightOffset = 0f;
            bodyRotationOffset = Vector3.zero;
            
            if (hasOriginalSettings)
            {
                CopySettings(originalSettings, calibrationController.settings);
            }
        }
        
        if (GUILayout.Button("Save Current as Default"))
        {
            if (hasOriginalSettings)
            {
                CopySettings(calibrationController.settings, originalSettings);
            }
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
} 