using UnityEngine;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using System.Collections.Generic;

/// <summary>
/// 가장 단순한 포즈 컨트롤러 - PoseLandmarkerRunner에서 직접 캐릭터로 연결
/// </summary>
public class DirectPoseController : MonoBehaviour
{
    [Header("필수 설정")]
    [SerializeField] private PoseLandmarkerRunner poseLandmarkerRunner;
    [SerializeField] private Animator targetAnimator;
    
    [Header("스무딩")]
    [Range(1f, 20f)]
    public float smoothing = 10f;
    
    // 스레드 안전을 위한 큐
    private readonly Queue<Mediapipe.Tasks.Components.Containers.NormalizedLandmarks> poseQueue = new Queue<Mediapipe.Tasks.Components.Containers.NormalizedLandmarks>();
    private readonly object queueLock = new object();
    
    // 휴머노이드 본 캐시
    private Transform headBone;
    private Transform neckBone;
    private Transform hipsBone;
    private Transform spineBone;
    private Transform chestBone;
    
    private Transform leftShoulderBone;
    private Transform leftArmBone;
    private Transform leftElbowBone;
    private Transform leftWristBone;
    
    private Transform rightShoulderBone;
    private Transform rightArmBone;
    private Transform rightElbowBone;
    private Transform rightWristBone;
    
    private Transform leftThighBone;
    private Transform leftShinBone;
    private Transform leftFootBone;
    
    private Transform rightThighBone;
    private Transform rightShinBone;
    private Transform rightFootBone;
    
    // 초기 회전값 저장
    private Quaternion[] initialRotations;
    private Transform[] bones;
    
    void Start()
    {
        // 자동으로 찾기
        if (poseLandmarkerRunner == null)
            poseLandmarkerRunner = FindObjectOfType<PoseLandmarkerRunner>();
            
        if (targetAnimator == null)
            targetAnimator = GetComponent<Animator>();
            
        if (poseLandmarkerRunner == null || targetAnimator == null || !targetAnimator.isHuman)
        {
            Debug.LogError("필수 컴포넌트가 없습니다!");
            enabled = false;
            return;
        }
        
        // 본 캐시
        CacheBones();
        SaveInitialRotations();
        
        // 이벤트 연결
        poseLandmarkerRunner.OnPoseResult += OnPoseResult;
        
        Debug.Log("DirectPoseController 준비 완료!");
    }
    
    void OnDestroy()
    {
        if (poseLandmarkerRunner != null)
            poseLandmarkerRunner.OnPoseResult -= OnPoseResult;
    }
    
    void CacheBones()
    {
        // 머리/목
        headBone = targetAnimator.GetBoneTransform(HumanBodyBones.Head);
        neckBone = targetAnimator.GetBoneTransform(HumanBodyBones.Neck);
        
        // 척추
        hipsBone = targetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        spineBone = targetAnimator.GetBoneTransform(HumanBodyBones.Spine);
        chestBone = targetAnimator.GetBoneTransform(HumanBodyBones.Chest);
        
        // 왼팔
        leftShoulderBone = targetAnimator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        leftArmBone = targetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        leftElbowBone = targetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        leftWristBone = targetAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
        
        // 오른팔
        rightShoulderBone = targetAnimator.GetBoneTransform(HumanBodyBones.RightShoulder);
        rightArmBone = targetAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        rightElbowBone = targetAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        rightWristBone = targetAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        
        // 왼다리
        leftThighBone = targetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        leftShinBone = targetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        leftFootBone = targetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        
        // 오른다리
        rightThighBone = targetAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        rightShinBone = targetAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        rightFootBone = targetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        
        // 배열로 저장
        bones = new Transform[] {
            headBone, neckBone, hipsBone, spineBone, chestBone,
            leftShoulderBone, leftArmBone, leftElbowBone, leftWristBone,
            rightShoulderBone, rightArmBone, rightElbowBone, rightWristBone,
            leftThighBone, leftShinBone, leftFootBone,
            rightThighBone, rightShinBone, rightFootBone
        };
    }
    
    void SaveInitialRotations()
    {
        initialRotations = new Quaternion[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
                initialRotations[i] = bones[i].localRotation;
        }
    }
    
    void OnPoseResult(PoseLandmarkerResult result)
    {
        if (result.poseLandmarks == null || result.poseLandmarks.Count == 0)
            return;
            
        var landmarks = result.poseLandmarks[0];
        if (landmarks.landmarks.Count < 33)
            return;
        
        // 큐에 추가 (스레드 안전)
        lock (queueLock)
        {
            poseQueue.Enqueue(landmarks);
            // 큐가 너무 커지지 않도록 제한
            while (poseQueue.Count > 3)
                poseQueue.Dequeue();
        }
    }
    
    void Update()
    {
        // 메인 스레드에서 포즈 적용
        lock (queueLock)
        {
            while (poseQueue.Count > 0)
            {
                var landmarks = poseQueue.Dequeue();
                ApplyPose(landmarks);
            }
        }
    }
    
    void ApplyPose(Mediapipe.Tasks.Components.Containers.NormalizedLandmarks landmarks)
    {
        var points = landmarks.landmarks;
        
        // 머리 회전
        if (headBone != null && points[0].visibility > 0.5f)
        {
            Vector3 nose = ToVector3(points[0]);
            Vector3 leftEar = ToVector3(points[7]);
            Vector3 rightEar = ToVector3(points[8]);
            
            Vector3 headForward = nose - (leftEar + rightEar) * 0.5f;
            Vector3 headUp = Vector3.Cross(rightEar - leftEar, headForward);
            
            Quaternion headRot = Quaternion.LookRotation(headForward, headUp);
            headBone.rotation = Quaternion.Slerp(headBone.rotation, headRot * initialRotations[0], Time.deltaTime * smoothing);
        }
        
        // 척추/가슴
        if (spineBone != null && points[11].visibility > 0.5f && points[12].visibility > 0.5f)
        {
            Vector3 leftShoulder = ToVector3(points[11]);
            Vector3 rightShoulder = ToVector3(points[12]);
            Vector3 leftHip = ToVector3(points[23]);
            Vector3 rightHip = ToVector3(points[24]);
            
            Vector3 shoulderCenter = (leftShoulder + rightShoulder) * 0.5f;
            Vector3 hipCenter = (leftHip + rightHip) * 0.5f;
            
            Vector3 spineDir = shoulderCenter - hipCenter;
            Vector3 spineRight = rightShoulder - leftShoulder;
            Vector3 spineForward = Vector3.Cross(spineRight, spineDir);
            
            Quaternion spineRot = Quaternion.LookRotation(spineForward, spineDir);
            spineBone.rotation = Quaternion.Slerp(spineBone.rotation, spineRot * initialRotations[3], Time.deltaTime * smoothing);
        }
        
        // 팔 - 간단한 2본 IK
        ApplyArm(points[11], points[13], points[15], leftArmBone, leftElbowBone, 6, 7, true);
        ApplyArm(points[12], points[14], points[16], rightArmBone, rightElbowBone, 10, 11, false);
        
        // 다리 - 간단한 2본 IK
        ApplyLeg(points[23], points[25], points[27], leftThighBone, leftShinBone, 13, 14);
        ApplyLeg(points[24], points[26], points[28], rightThighBone, rightShinBone, 16, 17);
        
        // 엉덩이 위치
        if (hipsBone != null && points[23].visibility > 0.5f && points[24].visibility > 0.5f)
        {
            Vector3 leftHip = ToVector3(points[23]);
            Vector3 rightHip = ToVector3(points[24]);
            Vector3 hipCenter = (leftHip + rightHip) * 0.5f;
            
            hipsBone.position = Vector3.Lerp(hipsBone.position, transform.position + hipCenter * 2f, Time.deltaTime * smoothing);
        }
    }
    
    void ApplyArm(Mediapipe.Tasks.Components.Containers.NormalizedLandmark shoulder,
                  Mediapipe.Tasks.Components.Containers.NormalizedLandmark elbow,
                  Mediapipe.Tasks.Components.Containers.NormalizedLandmark wrist,
                  Transform upperArm, Transform lowerArm, int upperIdx, int lowerIdx, bool isLeft)
    {
        if (upperArm == null || lowerArm == null) return;
        if (shoulder.visibility < 0.5f || elbow.visibility < 0.5f || wrist.visibility < 0.5f) return;
        
        Vector3 shoulderPos = ToVector3(shoulder);
        Vector3 elbowPos = ToVector3(elbow);
        Vector3 wristPos = ToVector3(wrist);
        
        // 상완 방향
        Vector3 upperDir = elbowPos - shoulderPos;
        Quaternion upperRot = Quaternion.LookRotation(upperDir, Vector3.up);
        upperArm.rotation = Quaternion.Slerp(upperArm.rotation, upperRot * initialRotations[upperIdx], Time.deltaTime * smoothing);
        
        // 팔꿈치 각도
        Vector3 forearmDir = wristPos - elbowPos;
        float angle = Vector3.Angle(upperDir, forearmDir);
        Quaternion elbowRot = initialRotations[lowerIdx] * Quaternion.AngleAxis(angle - 180f, isLeft ? Vector3.right : Vector3.left);
        lowerArm.localRotation = Quaternion.Slerp(lowerArm.localRotation, elbowRot, Time.deltaTime * smoothing);
    }
    
    void ApplyLeg(Mediapipe.Tasks.Components.Containers.NormalizedLandmark hip,
                  Mediapipe.Tasks.Components.Containers.NormalizedLandmark knee,
                  Mediapipe.Tasks.Components.Containers.NormalizedLandmark ankle,
                  Transform thigh, Transform shin, int thighIdx, int shinIdx)
    {
        if (thigh == null || shin == null) return;
        if (hip.visibility < 0.5f || knee.visibility < 0.5f || ankle.visibility < 0.5f) return;
        
        Vector3 hipPos = ToVector3(hip);
        Vector3 kneePos = ToVector3(knee);
        Vector3 anklePos = ToVector3(ankle);
        
        // 허벅지 방향
        Vector3 thighDir = kneePos - hipPos;
        Quaternion thighRot = Quaternion.LookRotation(thighDir, Vector3.up);
        thigh.rotation = Quaternion.Slerp(thigh.rotation, thighRot * initialRotations[thighIdx], Time.deltaTime * smoothing);
        
        // 무릎 각도
        Vector3 shinDir = anklePos - kneePos;
        float angle = Vector3.Angle(thighDir, shinDir);
        Quaternion kneeRot = initialRotations[shinIdx] * Quaternion.AngleAxis(180f - angle, Vector3.right);
        shin.localRotation = Quaternion.Slerp(shin.localRotation, kneeRot, Time.deltaTime * smoothing);
    }
    
    Vector3 ToVector3(Mediapipe.Tasks.Components.Containers.NormalizedLandmark landmark)
    {
        return new Vector3(
            (landmark.x - 0.5f) * 2f,  // 중앙 정렬
            (0.5f - landmark.y) * 2f,  // Y축 반전
            landmark.z * 0.5f          // Z축 스케일
        );
    }
    
    [ContextMenu("Reset Pose")]
    public void ResetPose()
    {
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null && i < initialRotations.Length)
                bones[i].localRotation = initialRotations[i];
        }
        
        if (hipsBone != null)
            hipsBone.localPosition = Vector3.zero;
    }
} 