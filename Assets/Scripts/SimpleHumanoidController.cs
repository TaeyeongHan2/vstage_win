using UnityEngine;
using System.Collections.Generic;
using Mediapipe;

/// <summary>
/// 간단한 휴머노이드 포즈 컨트롤러
/// Unity-chan 같은 휴머노이드 아바타를 MediaPipe로 제어
/// </summary>
public class SimpleHumanoidController : MonoBehaviour
{
    [Header("설정")]
    public Animator targetAnimator;
    public float smoothing = 5f; // 스무딩 값 감소
    public bool enableDebugLog = false;
    
    [Header("회전 설정")]
    public float armRotationMultiplier = 150f; // 팔 회전 배율
    public float maxArmRotation = 120f; // 최대 팔 회전 각도
    
    // 본 캐시
    private Transform headBone;
    private Transform leftArmBone;
    private Transform rightArmBone;
    private Transform leftElbowBone;
    private Transform rightElbowBone;
    private Transform spineBone;
    
    // 초기 회전값 저장
    private Quaternion initialHeadRotation;
    private Quaternion initialLeftArmRotation;
    private Quaternion initialRightArmRotation;
    private Quaternion initialLeftElbowRotation;
    private Quaternion initialRightElbowRotation;
    private Quaternion initialSpineRotation;
    
    // 포즈 데이터 저장용
    private NormalizedLandmarkList currentLandmarks;
    private bool hasNewPoseData = false;
    
    /// <summary>
    /// 초기화 및 본 정보 캐싱
    /// </summary>
    void Start()
    {
        if (targetAnimator == null)
        {
            Debug.LogError("Target Animator가 없습니다!");
            return;
        }
        
        // Humanoid 타입인지 확인
        if (targetAnimator.avatar == null || !targetAnimator.avatar.isHuman)
        {
            Debug.LogError("Animator가 Humanoid 타입이 아닙니다!");
            return;
        }
        
        // 본 찾기
        headBone = targetAnimator.GetBoneTransform(HumanBodyBones.Head);
        leftArmBone = targetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        rightArmBone = targetAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        leftElbowBone = targetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        rightElbowBone = targetAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        spineBone = targetAnimator.GetBoneTransform(HumanBodyBones.Spine);
        
        // 초기 회전값 저장
        if (headBone) initialHeadRotation = headBone.localRotation;
        if (leftArmBone) initialLeftArmRotation = leftArmBone.localRotation;
        if (rightArmBone) initialRightArmRotation = rightArmBone.localRotation;
        if (leftElbowBone) initialLeftElbowRotation = leftElbowBone.localRotation;
        if (rightElbowBone) initialRightElbowRotation = rightElbowBone.localRotation;
        if (spineBone) initialSpineRotation = spineBone.localRotation;
        
        // 본 확인 로그
        Debug.Log($"SimpleHumanoidController 준비 완료!");
        Debug.Log($"찾은 본 - Head: {headBone != null}, LeftArm: {leftArmBone != null}, RightArm: {rightArmBone != null}");
        Debug.Log($"LeftElbow: {leftElbowBone != null}, RightElbow: {rightElbowBone != null}, Spine: {spineBone != null}");
        
        // Animator 설정
        if (targetAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("Animator Controller가 없습니다. 본 회전만 적용됩니다.");
        }
    }
    
    /// <summary>
    /// MediaPipe 포즈 데이터 적용
    /// </summary>
    public void ApplyPose(NormalizedLandmarkList landmarks)
    {
        if (landmarks?.Landmark == null || landmarks.Landmark.Count < 33) 
        {
            if (enableDebugLog)
                Debug.LogWarning($"유효하지 않은 랜드마크 데이터: {landmarks?.Landmark?.Count ?? 0}개");
            return;
        }
        
        currentLandmarks = landmarks;
        hasNewPoseData = true;
        
        if (enableDebugLog)
        {
            Debug.Log($"포즈 데이터 수신: {landmarks.Landmark.Count}개 포인트");
        }
    }
    
    /// <summary>
    /// Animator 이후 LateUpdate에서 본 회전 적용
    /// </summary>
    void LateUpdate()
    {
        if (!hasNewPoseData || currentLandmarks == null) return;
        
        var points = currentLandmarks.Landmark;
        
        // 포즈 적용
        ApplyHead(points);
        ApplyArms(points);
        ApplySpine(points);
    }
    
    /// <summary>
    /// 머리 회전 적용
    /// </summary>
    void ApplyHead(Google.Protobuf.Collections.RepeatedField<NormalizedLandmark> points)
    {
        if (headBone == null) return;
        
        var nose = points[0];
        var leftEar = points[7];
        var rightEar = points[8];
        
        if (nose.Visibility < 0.5f) return;
        
        // 고개 움직임 계산
        float headX = (nose.Y - 0.4f) * 45f; // 끄덕임
        float headY = (nose.X - 0.5f) * 45f; // 좌우 회전
        
        headX = Mathf.Clamp(headX, -30f, 30f);
        headY = Mathf.Clamp(headY, -45f, 45f);
        
        var targetRot = initialHeadRotation * Quaternion.Euler(headX, headY, 0);
        headBone.localRotation = Quaternion.Slerp(headBone.localRotation, targetRot, Time.deltaTime * smoothing);
    }
    
    /// <summary>
    /// 팔 회전 적용
    /// </summary>
    void ApplyArms(Google.Protobuf.Collections.RepeatedField<NormalizedLandmark> points)
    {
        // 왼팔
        if (leftArmBone != null)
        {
            var leftShoulder = points[11];
            var leftElbow = points[13];
            var leftWrist = points[15];
            
            if (leftShoulder.Visibility > 0.5f && leftElbow.Visibility > 0.5f)
            {
                // 3D 공간에서의 팔 방향 계산
                float armX = (leftElbow.Z - leftShoulder.Z) * armRotationMultiplier;
                float armY = (leftShoulder.Y - leftElbow.Y) * armRotationMultiplier;
                float armZ = (leftElbow.X - leftShoulder.X) * armRotationMultiplier;
                
                armX = Mathf.Clamp(armX, -maxArmRotation, maxArmRotation);
                armY = Mathf.Clamp(armY, -maxArmRotation, maxArmRotation);
                armZ = Mathf.Clamp(armZ, -maxArmRotation, maxArmRotation);
                
                var targetRot = initialLeftArmRotation * Quaternion.Euler(armY, armX, -armZ);
                leftArmBone.localRotation = Quaternion.Slerp(leftArmBone.localRotation, targetRot, Time.deltaTime * smoothing);
            }
        }
        
        // 오른팔
        if (rightArmBone != null)
        {
            var rightShoulder = points[12];
            var rightElbow = points[14];
            var rightWrist = points[16];
            
            if (rightShoulder.Visibility > 0.5f && rightElbow.Visibility > 0.5f)
            {
                // 3D 공간에서의 팔 방향 계산
                float armX = (rightElbow.Z - rightShoulder.Z) * armRotationMultiplier;
                float armY = (rightShoulder.Y - rightElbow.Y) * armRotationMultiplier;
                float armZ = (rightElbow.X - rightShoulder.X) * armRotationMultiplier;
                
                armX = Mathf.Clamp(armX, -maxArmRotation, maxArmRotation);
                armY = Mathf.Clamp(armY, -maxArmRotation, maxArmRotation);
                armZ = Mathf.Clamp(armZ, -maxArmRotation, maxArmRotation);
                
                var targetRot = initialRightArmRotation * Quaternion.Euler(armY, armX, armZ);
                rightArmBone.localRotation = Quaternion.Slerp(rightArmBone.localRotation, targetRot, Time.deltaTime * smoothing);
            }
        }
        
        // 팔꿈치
        ApplyElbows(points);
    }
    
    /// <summary>
    /// 팔꿈치 회전 적용
    /// </summary>
    void ApplyElbows(Google.Protobuf.Collections.RepeatedField<NormalizedLandmark> points)
    {
        // 왼쪽 팔꿈치
        if (leftElbowBone != null)
        {
            var leftElbow = points[13];
            var leftWrist = points[15];
            
            if (leftElbow.Visibility > 0.5f && leftWrist.Visibility > 0.5f)
            {
                float elbowX = (leftWrist.Z - leftElbow.Z) * armRotationMultiplier * 0.5f;
                float elbowY = (leftElbow.Y - leftWrist.Y) * armRotationMultiplier * 0.5f;
                float elbowZ = (leftWrist.X - leftElbow.X) * armRotationMultiplier * 0.5f;
                
                elbowX = Mathf.Clamp(elbowX, -90f, 90f);
                elbowY = Mathf.Clamp(elbowY, -90f, 90f);
                elbowZ = Mathf.Clamp(elbowZ, -90f, 90f);
                
                var targetRot = initialLeftElbowRotation * Quaternion.Euler(elbowY, elbowX, -elbowZ);
                leftElbowBone.localRotation = Quaternion.Slerp(leftElbowBone.localRotation, targetRot, Time.deltaTime * smoothing);
            }
        }
        
        // 오른쪽 팔꿈치
        if (rightElbowBone != null)
        {
            var rightElbow = points[14];
            var rightWrist = points[16];
            
            if (rightElbow.Visibility > 0.5f && rightWrist.Visibility > 0.5f)
            {
                float elbowX = (rightWrist.Z - rightElbow.Z) * armRotationMultiplier * 0.5f;
                float elbowY = (rightElbow.Y - rightWrist.Y) * armRotationMultiplier * 0.5f;
                float elbowZ = (rightWrist.X - rightElbow.X) * armRotationMultiplier * 0.5f;
                
                elbowX = Mathf.Clamp(elbowX, -90f, 90f);
                elbowY = Mathf.Clamp(elbowY, -90f, 90f);
                elbowZ = Mathf.Clamp(elbowZ, -90f, 90f);
                
                var targetRot = initialRightElbowRotation * Quaternion.Euler(elbowY, elbowX, elbowZ);
                rightElbowBone.localRotation = Quaternion.Slerp(rightElbowBone.localRotation, targetRot, Time.deltaTime * smoothing);
            }
        }
    }
    
    /// <summary>
    /// 상체(Spine) 회전 적용
    /// </summary>
    void ApplySpine(Google.Protobuf.Collections.RepeatedField<NormalizedLandmark> points)
    {
        if (spineBone == null) return;
        
        var leftShoulder = points[11];
        var rightShoulder = points[12];
        var leftHip = points[23];
        var rightHip = points[24];
        
        if (leftShoulder.Visibility < 0.5f || rightShoulder.Visibility < 0.5f) return;
        
        // 상체 기울기 계산
        float spineX = ((leftShoulder.Y + rightShoulder.Y) / 2f - 0.5f) * 45f;
        float spineZ = (rightShoulder.Y - leftShoulder.Y) * 45f;
        
        spineX = Mathf.Clamp(spineX, -30f, 30f);
        spineZ = Mathf.Clamp(spineZ, -30f, 30f);
        
        var targetRot = initialSpineRotation * Quaternion.Euler(spineX, 0, spineZ);
        spineBone.localRotation = Quaternion.Slerp(spineBone.localRotation, targetRot, Time.deltaTime * smoothing);
    }
    
    /// <summary>
    /// 포즈 초기화 (초기 회전값으로 복원)
    /// </summary>
    [ContextMenu("Reset Pose")]
    public void ResetPose()
    {
        if (headBone) headBone.localRotation = initialHeadRotation;
        if (leftArmBone) leftArmBone.localRotation = initialLeftArmRotation;
        if (rightArmBone) rightArmBone.localRotation = initialRightArmRotation;
        if (leftElbowBone) leftElbowBone.localRotation = initialLeftElbowRotation;
        if (rightElbowBone) rightElbowBone.localRotation = initialRightElbowRotation;
        if (spineBone) spineBone.localRotation = initialSpineRotation;
        
        Debug.Log("포즈 초기화 완료!");
    }
}