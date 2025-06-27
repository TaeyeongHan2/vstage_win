using UnityEngine;
using RootMotion.FinalIK;
using RootMotion.Demos;

public class VRIKLegLengthFix : MonoBehaviour
{
    [Header("References")]
    public VRIK vrik;
    public VRIKCalibrationController calibrationController;
    
    [Header("Leg Length Adjustment")]
    [Range(0.5f, 1.5f)]
    public float legLengthMultiplier = 1f;
    
    [Header("Foot Tracker Offsets")]
    [Tooltip("발 트래커가 실제 발보다 높이 있을 때 음수값 사용")]
    public float footHeightOffset = -0.05f;
    public float footForwardOffset = 0f;
    public float footInwardOffset = 0f;
    
    [Header("Solver Settings")]
    public bool disablePlantFeet = true;
    public float legSwivelOffset = 0f;
    public float bendGoalWeight = 0f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    private float leftLegLength;
    private float rightLegLength;
    
    void Start()
    {
        if (vrik == null) vrik = GetComponent<VRIK>();
        if (calibrationController == null) calibrationController = GetComponent<VRIKCalibrationController>();
        
        ApplyFixes();
    }
    
    public void ApplyFixes()
    {
        if (vrik == null || calibrationController == null) return;
        
        // 1. PlantFeet 비활성화 (다리가 늘어나는 주요 원인)
        if (disablePlantFeet)
        {
            vrik.solver.plantFeet = false;
        }
        
        // 2. 발 트래커 오프셋 조정
        if (calibrationController.settings != null)
        {
            calibrationController.settings.footForwardOffset = footForwardOffset;
            calibrationController.settings.footInwardOffset = footInwardOffset;
            calibrationController.settings.footHeadingOffset = footHeightOffset;
        }
        
        // 3. 다리 solver 설정
        vrik.solver.leftLeg.swivelOffset = legSwivelOffset;
        vrik.solver.rightLeg.swivelOffset = legSwivelOffset;
        vrik.solver.leftLeg.bendGoalWeight = bendGoalWeight;
        vrik.solver.rightLeg.bendGoalWeight = bendGoalWeight;
        
        // 4. Locomotion 설정 조정
        vrik.solver.locomotion.weight = 1f;
        vrik.solver.locomotion.footDistance = 0.3f;
        vrik.solver.locomotion.stepThreshold = 0.4f;
        vrik.solver.locomotion.velocityFactor = 0.4f;
        vrik.solver.locomotion.maxVelocity = 0.4f;
        vrik.solver.locomotion.rootSpeed = 20f;
        vrik.solver.locomotion.stepSpeed = 3f;
    }
    
    public void RecalibrateWithOffset()
    {
        // 발 트래커 위치를 임시로 조정한 후 캘리브레이션
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
        
        // 위치 복원
        if (calibrationController.leftFootTracker != null)
        {
            calibrationController.leftFootTracker.position -= Vector3.up * footHeightOffset;
        }
        if (calibrationController.rightFootTracker != null)
        {
            calibrationController.rightFootTracker.position -= Vector3.up * footHeightOffset;
        }
        
        Debug.Log("Recalibrated with foot offset!");
    }
    
    public void AdjustLegLength()
    {
        if (calibrationController.data.scale > 0)
        {
            // 스케일 조정
            float newScale = calibrationController.data.scale * legLengthMultiplier;
            calibrationController.settings.scaleMlp = newScale;
            
            VRIKCalibrator.RecalibrateScale(
                calibrationController.ik,
                calibrationController.data,
                calibrationController.settings
            );
            
            Debug.Log($"Leg length adjusted with multiplier: {legLengthMultiplier}");
        }
    }
    
    void Update()
    {
        if (showDebugInfo && vrik != null && vrik.references != null)
        {
            // 다리 길이 계산
            leftLegLength = CalculateLegLength(true);
            rightLegLength = CalculateLegLength(false);
        }
    }
    
    float CalculateLegLength(bool isLeft)
    {
        var refs = vrik.references;
        if (refs == null) return 0f;
        
        Transform thigh = isLeft ? refs.leftThigh : refs.rightThigh;
        Transform calf = isLeft ? refs.leftCalf : refs.rightCalf;
        Transform foot = isLeft ? refs.leftFoot : refs.rightFoot;
        
        if (thigh == null || calf == null || foot == null) return 0f;
        
        float thighLength = Vector3.Distance(thigh.position, calf.position);
        float calfLength = Vector3.Distance(calf.position, foot.position);
        
        return thighLength + calfLength;
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("<b>VRIK Leg Debug Info</b>");
        GUILayout.Label($"Left Leg Length: {leftLegLength:F3}");
        GUILayout.Label($"Right Leg Length: {rightLegLength:F3}");
        GUILayout.Label($"PlantFeet: {(vrik != null ? vrik.solver.plantFeet.ToString() : "N/A")}");
        
        if (GUILayout.Button("Apply Fixes"))
        {
            ApplyFixes();
        }
        
        if (GUILayout.Button("Recalibrate with Offset"))
        {
            RecalibrateWithOffset();
        }
        
        if (GUILayout.Button("Adjust Leg Length"))
        {
            AdjustLegLength();
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
} 