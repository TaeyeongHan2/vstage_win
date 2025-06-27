using UnityEngine;
using RootMotion.FinalIK;
using RootMotion.Demos;

public class ImprovedVRIKCalibration : MonoBehaviour
{
    [Header("References")]
    public VRIKCalibrationController calibrationController;
    
    [Header("Calibration Improvements")]
    [Tooltip("트래커가 실제 신체 부위보다 높이 있을 때 사용")]
    public float trackerHeightCompensation = -0.05f;
    
    [Tooltip("캘리브레이션 후 자동으로 스케일 조정")]
    public bool autoAdjustScale = true;
    
    [Tooltip("목표 다리 길이 비율 (0.9 = 10% 짧게)")]
    [Range(0.7f, 1.0f)]
    public float targetLegRatio = 0.9f;
    
    void Start()
    {
        if (calibrationController == null)
        {
            calibrationController = GetComponent<VRIKCalibrationController>();
        }
    }
    
    // 개선된 캘리브레이션 메서드
    public void ImprovedCalibrate()
    {
        if (calibrationController == null) return;
        
        // 1. 트래커 높이 보정
        CompensateTrackerHeight();
        
        // 2. 일반 캘리브레이션 실행
        PerformCalibration();
        
        // 3. 트래커 위치 복원
        RestoreTrackerPositions();
        
        // 4. 자동 스케일 조정
        if (autoAdjustScale)
        {
            AdjustScalePostCalibration();
        }
        
        // 5. VRIK Solver 최적화
        OptimizeVRIKSolver();
        
        Debug.Log("Improved calibration completed!");
    }
    
    void CompensateTrackerHeight()
    {
        // 발 트래커 높이 보정
        if (calibrationController.leftFootTracker != null)
        {
            calibrationController.leftFootTracker.position += Vector3.up * trackerHeightCompensation;
        }
        if (calibrationController.rightFootTracker != null)
        {
            calibrationController.rightFootTracker.position += Vector3.up * trackerHeightCompensation;
        }
        
        // 허리 트래커도 약간 보정 (선택적)
        if (calibrationController.bodyTracker != null)
        {
            calibrationController.bodyTracker.position += Vector3.up * (trackerHeightCompensation * 0.5f);
        }
    }
    
    void RestoreTrackerPositions()
    {
        // 원래 위치로 복원
        if (calibrationController.leftFootTracker != null)
        {
            calibrationController.leftFootTracker.position -= Vector3.up * trackerHeightCompensation;
        }
        if (calibrationController.rightFootTracker != null)
        {
            calibrationController.rightFootTracker.position -= Vector3.up * trackerHeightCompensation;
        }
        if (calibrationController.bodyTracker != null)
        {
            calibrationController.bodyTracker.position -= Vector3.up * (trackerHeightCompensation * 0.5f);
        }
    }
    
    void PerformCalibration()
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
    }
    
    void AdjustScalePostCalibration()
    {
        if (calibrationController.data.scale > 0)
        {
            // 현재 스케일에 목표 비율 적용
            float adjustedScale = calibrationController.data.scale * targetLegRatio;
            calibrationController.settings.scaleMlp = adjustedScale;
            
            // 스케일 재보정
            VRIKCalibrator.RecalibrateScale(
                calibrationController.ik,
                calibrationController.data,
                calibrationController.settings
            );
            
            Debug.Log($"Scale adjusted from {calibrationController.data.scale:F3} to {adjustedScale:F3}");
        }
    }
    
    void OptimizeVRIKSolver()
    {
        var vrik = calibrationController.ik;
        
        // PlantFeet 비활성화
        vrik.solver.plantFeet = false;
        
        // Spine 최적화
        vrik.solver.spine.pelvisPositionWeight = 0.8f;
        vrik.solver.spine.positionWeight = 0.8f;
        vrik.solver.spine.rotationWeight = 0.8f;
        
        // Locomotion 최적화
        vrik.solver.locomotion.weight = 0.9f;
        vrik.solver.locomotion.stepThreshold = 0.3f;
        vrik.solver.locomotion.angleThreshold = 45f;
        vrik.solver.locomotion.comAngleMlp = 1f;
        vrik.solver.locomotion.maxVelocity = 0.4f;
        vrik.solver.locomotion.velocityFactor = 0.4f;
        vrik.solver.locomotion.rootSpeed = 20f;
        vrik.solver.locomotion.stepSpeed = 3f;
        
        // 다리 무게 설정
        vrik.solver.leftLeg.positionWeight = 1f;
        vrik.solver.leftLeg.rotationWeight = 1f;
        vrik.solver.rightLeg.positionWeight = 1f;
        vrik.solver.rightLeg.rotationWeight = 1f;
    }
    
    // Inspector에서 사용할 버튼
    [ContextMenu("Perform Improved Calibration")]
    public void PerformImprovedCalibrationFromMenu()
    {
        ImprovedCalibrate();
    }
} 