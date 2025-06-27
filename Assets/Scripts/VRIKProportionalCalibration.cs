using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

// 사용자의 실제 신체 비율을 측정하여 더 정확한 캘리브레이션을 수행하는 스크립트
public class VRIKProportionalCalibration : MonoBehaviour
{
    [Header("References")]
    public VRIK vrik;
    public VRIKCalibrator.Settings calibrationSettings;
    
    [Header("Trackers")]
    public Transform headTracker;
    public Transform bodyTracker;
    public Transform leftHandTracker;
    public Transform rightHandTracker;
    public Transform leftFootTracker;
    public Transform rightFootTracker;
    
    [Header("Proportional Calibration")]
    [Tooltip("신체 비율 기반 캘리브레이션 사용")]
    public bool useProportionalCalibration = true;
    
    [Tooltip("캘리브레이션 시 측정할 포즈")]
    public CalibrationPose calibrationPose = CalibrationPose.TPose;
    
    [Header("측정된 신체 비율 (읽기 전용)")]
    [SerializeField] private float measuredHeight;
    [SerializeField] private float measuredArmLength;
    [SerializeField] private float measuredLegLength;
    [SerializeField] private float measuredTorsoLength;
    [SerializeField] private float measuredShoulderWidth;
    
    [Header("캐릭터 원본 비율 (읽기 전용)")]
    [SerializeField] private float characterHeight;
    [SerializeField] private float characterArmLength;
    [SerializeField] private float characterLegLength;
    [SerializeField] private float characterTorsoLength;
    [SerializeField] private float characterShoulderWidth;
    
    [Header("계산된 스케일 팩터")]
    [SerializeField] private float heightScale = 1f;
    [SerializeField] private float armScale = 1f;
    [SerializeField] private float legScale = 1f;
    [SerializeField] private float torsoScale = 1f;
    
    public enum CalibrationPose
    {
        TPose,      // 팔을 옆으로 뻗은 자세
        APose,      // 팔을 45도 내린 자세
        Standing    // 팔을 내린 자세
    }
    
    // 캘리브레이션 데이터
    private VRIKCalibrator.CalibrationData calibrationData;
    
    void Start()
    {
        if (vrik == null)
            vrik = GetComponent<VRIK>();
            
        if (calibrationSettings == null)
            calibrationSettings = new VRIKCalibrator.Settings();
            
        // 캐릭터 원본 비율 측정
        MeasureCharacterProportions();
    }
    
    // 캐릭터의 원본 비율 측정
    void MeasureCharacterProportions()
    {
        if (vrik == null || vrik.references.root == null) return;
        
        var refs = vrik.references;
        
        // 전체 키 (발에서 머리까지)
        characterHeight = Vector3.Distance(refs.leftFoot.position, refs.head.position);
        
        // 팔 길이 (어깨에서 손까지)
        if (refs.leftShoulder != null && refs.leftHand != null)
        {
            characterArmLength = Vector3.Distance(refs.leftShoulder.position, refs.leftHand.position);
        }
        
        // 다리 길이 (골반에서 발까지)
        characterLegLength = Vector3.Distance(refs.pelvis.position, refs.leftFoot.position);
        
        // 몸통 길이 (골반에서 머리까지)
        characterTorsoLength = Vector3.Distance(refs.pelvis.position, refs.head.position);
        
        // 어깨 너비
        if (refs.leftShoulder != null && refs.rightShoulder != null)
        {
            characterShoulderWidth = Vector3.Distance(refs.leftShoulder.position, refs.rightShoulder.position);
        }
        
        Debug.Log($"캐릭터 비율 측정 완료 - 키: {characterHeight:F2}m");
    }
    
    // 사용자의 실제 신체 비율 측정
    void MeasureUserProportions()
    {
        // 전체 키 (발 트래커에서 머리 트래커까지)
        if (leftFootTracker != null && headTracker != null)
        {
            measuredHeight = headTracker.position.y - leftFootTracker.position.y;
        }
        else if (headTracker != null)
        {
            // 발 트래커가 없으면 바닥에서 머리까지로 추정
            measuredHeight = headTracker.position.y;
        }
        
        // 팔 길이 (T-Pose에서만 정확히 측정 가능)
        if (calibrationPose == CalibrationPose.TPose && leftHandTracker != null && rightHandTracker != null)
        {
            // 양팔 벌린 길이의 절반을 팔 길이로 추정
            float armSpan = Vector3.Distance(leftHandTracker.position, rightHandTracker.position);
            measuredArmLength = armSpan / 2f;
            
            // 어깨 너비 추정 (전체 armspan의 약 25%)
            measuredShoulderWidth = armSpan * 0.25f;
        }
        
        // 다리 길이 (골반 트래커가 있는 경우)
        if (bodyTracker != null && leftFootTracker != null)
        {
            measuredLegLength = bodyTracker.position.y - leftFootTracker.position.y;
        }
        else if (leftFootTracker != null && headTracker != null)
        {
            // 골반 트래커가 없으면 전체 키의 45%로 추정 (일반적인 인체 비율)
            measuredLegLength = measuredHeight * 0.45f;
        }
        
        // 몸통 길이
        if (bodyTracker != null && headTracker != null)
        {
            measuredTorsoLength = headTracker.position.y - bodyTracker.position.y;
        }
        else
        {
            // 골반 트래커가 없으면 전체 키의 35%로 추정
            measuredTorsoLength = measuredHeight * 0.35f;
        }
        
        Debug.Log($"사용자 비율 측정 완료 - 키: {measuredHeight:F2}m, 팔: {measuredArmLength:F2}m, 다리: {measuredLegLength:F2}m");
    }
    
    // 비율 기반 스케일 계산
    void CalculateProportionalScales()
    {
        // 전체 높이 스케일
        heightScale = measuredHeight / characterHeight;
        
        // 팔 길이 스케일 (측정된 경우에만)
        if (measuredArmLength > 0 && characterArmLength > 0)
        {
            armScale = measuredArmLength / characterArmLength;
        }
        else
        {
            armScale = heightScale; // 측정 못한 경우 전체 스케일 사용
        }
        
        // 다리 길이 스케일
        if (measuredLegLength > 0 && characterLegLength > 0)
        {
            legScale = measuredLegLength / characterLegLength;
        }
        else
        {
            legScale = heightScale;
        }
        
        // 몸통 스케일
        if (measuredTorsoLength > 0 && characterTorsoLength > 0)
        {
            torsoScale = measuredTorsoLength / characterTorsoLength;
        }
        else
        {
            torsoScale = heightScale;
        }
        
        Debug.Log($"계산된 스케일 - 전체: {heightScale:F3}, 팔: {armScale:F3}, 다리: {legScale:F3}, 몸통: {torsoScale:F3}");
    }
    
    // 비율 기반 캘리브레이션 실행
    public void CalibrateWithProportions()
    {
        StartCoroutine(ProportionalCalibrationCoroutine());
    }
    
    IEnumerator ProportionalCalibrationCoroutine()
    {
        Debug.Log("⚠️ 비율 기반 캘리브레이션 시작!");
        Debug.Log($"📐 {calibrationPose} 자세를 취하세요!");
        
        // 3초 카운트다운
        for (int i = 3; i > 0; i--)
        {
            Debug.Log($"⏰ {i}...");
            yield return new WaitForSeconds(1f);
        }
        
        // 1. 사용자 신체 비율 측정
        MeasureUserProportions();
        
        // 2. 스케일 계산
        CalculateProportionalScales();
        
        // 3. 기본 캘리브레이션 실행
        calibrationData = VRIKCalibrator.Calibrate(
            vrik,
            calibrationSettings,
            headTracker,
            bodyTracker,
            leftHandTracker,
            rightHandTracker,
            leftFootTracker,
            rightFootTracker
        );
        
        // 4. 비율 기반 조정 적용
        if (useProportionalCalibration)
        {
            ApplyProportionalAdjustments();
        }
        
        Debug.Log("✅ 비율 기반 캘리브레이션 완료!");
    }
    
    // 비율 기반 조정 적용
    void ApplyProportionalAdjustments()
    {
        // 주의: Unity의 기본 휴머노이드 리그는 개별 본 스케일링을 지원하지 않음
        // 따라서 IK 타겟 위치를 조정하는 방식으로 구현
        
        // 다리가 짧은 경우 발 타겟을 위로 조정
        if (legScale < heightScale * 0.95f) // 다리가 상대적으로 짧다면
        {
            float legDifference = (heightScale - legScale) * characterLegLength;
            
            if (vrik.solver.leftLeg.target != null)
                vrik.solver.leftLeg.target.position += Vector3.up * legDifference * 0.5f;
                
            if (vrik.solver.rightLeg.target != null)
                vrik.solver.rightLeg.target.position += Vector3.up * legDifference * 0.5f;
                
            Debug.Log($"📏 다리가 짧아 발 타겟을 {legDifference * 0.5f:F3}m 위로 조정");
        }
        
        // PlantFeet 설정 조정
        vrik.solver.plantFeet = false; // 비율이 다른 경우 발 고정 해제
        
        // Spine 설정 조정 (몸통 비율에 따라)
        if (Mathf.Abs(torsoScale - heightScale) > 0.1f)
        {
            vrik.solver.spine.bodyPosStiffness = 0.3f; // 더 유연하게
            vrik.solver.spine.bodyRotStiffness = 0.2f;
        }
    }
    
    void Update()
    {
        // P키로 비율 기반 캘리브레이션
        if (Input.GetKeyDown(KeyCode.P))
        {
            CalibrateWithProportions();
        }
        
        // C키로 일반 캘리브레이션
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(StandardCalibrationCoroutine());
        }
    }
    
    IEnumerator StandardCalibrationCoroutine()
    {
        Debug.Log("🔄 표준 캘리브레이션 (3초 후 시작)");
        yield return new WaitForSeconds(3f);
        
        calibrationData = VRIKCalibrator.Calibrate(
            vrik,
            calibrationSettings,
            headTracker,
            bodyTracker,
            leftHandTracker,
            rightHandTracker,
            leftFootTracker,
            rightFootTracker
        );
        
        Debug.Log("✅ 표준 캘리브레이션 완료!");
    }
} 