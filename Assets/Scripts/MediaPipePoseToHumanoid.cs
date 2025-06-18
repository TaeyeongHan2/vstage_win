using UnityEngine;
using Mediapipe.Unity;
using NormalizedLandmark = Mediapipe.Tasks.Components.Containers.NormalizedLandmark;
using NormalizedLandmarks = Mediapipe.Tasks.Components.Containers.NormalizedLandmarks;

public class MediaPipePoseToHumanoid : MonoBehaviour
{
    [Tooltip("Animator with Humanoid Avatar to be driven by MediaPipe pose landmarks")]
    public Animator avatar;

    // Example mapping indices for key landmarks to Humanoid bones
    private readonly (int landmarkIndex, HumanBodyBones bone)[] _map =
    {
        (23, HumanBodyBones.Hips),          // left hip ≈ hips center proxy
        (24, HumanBodyBones.Hips),          // right hip – averaged later
        (11, HumanBodyBones.LeftUpperArm),  // left shoulder
        (12, HumanBodyBones.RightUpperArm), // right shoulder
        (13, HumanBodyBones.LeftLowerArm),  // left elbow
        (14, HumanBodyBones.RightLowerArm), // right elbow
        (15, HumanBodyBones.LeftHand),      // left wrist
        (16, HumanBodyBones.RightHand),     // right wrist
        (25, HumanBodyBones.LeftUpperLeg),  // left knee proxy (upper leg)
        (26, HumanBodyBones.RightUpperLeg), // right knee proxy
        (27, HumanBodyBones.LeftLowerLeg),  // left ankle proxy (lower leg)
        (28, HumanBodyBones.RightLowerLeg)  // right ankle proxy
    };

    /// <summary>
    /// Apply landmarks to avatar bones.
    /// </summary>
    public void ApplyLandmarks(NormalizedLandmarks landmarks)
    {
        if (avatar == null || landmarks.landmarks == null || landmarks.landmarks.Count == 0) return;

        foreach (var (idx, bone) in _map)
        {
            var t = avatar.GetBoneTransform(bone);
            if (t == null) continue;

            if (idx >= landmarks.landmarks.Count) continue;

            var lm = landmarks.landmarks[idx];
            Vector3 pos = new Vector3(lm.x - 0.5f, 0.5f - lm.y, -lm.z);
            const float scale = 2.0f;
            t.localPosition = Vector3.Lerp(t.localPosition, pos * scale, 0.5f);
        }
    }

    // legacy support (if proto list is supplied)
    private void OnLandmarks(Mediapipe.NormalizedLandmarkList list)
    {
        if (list == null) return;
        var container = NormalizedLandmarks.CreateFrom(list);
        ApplyLandmarks(container);
    }
}