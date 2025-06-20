using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avatar : MonoBehaviour
{
    public Camera previewCamera; // OPTIONAL
    public Animator animator;
    public LayerMask ground;
    public bool footTracking = true;
    public float footGroundOffset = .1f;
    [Header("Calibration")]
    public bool useCalibrationData = false;
    public PersistentCalibrationData calibrationData;

    public bool Calibrated { get; private set; }

    private MediaPipeToAvatarAdapter adapter;
    private OptimizedMediaPipeAdapter optimizedAdapter;

    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private Quaternion targetRot;

    private Dictionary<HumanBodyBones, CalibrationData> parentCalibrationData = new Dictionary<HumanBodyBones, CalibrationData>();
    private CalibrationData spineUpDown, hipsTwist,chest,head;

    private void Start()
    {
        initialRotation = transform.rotation;
        initialPosition = transform.position;

        if (calibrationData && useCalibrationData)
        {
            CalibrateFromPersistent();
        }

        // Try to find either adapter
        adapter = FindObjectOfType<MediaPipeToAvatarAdapter>();
        optimizedAdapter = FindObjectOfType<OptimizedMediaPipeAdapter>();
        
        if (adapter == null && optimizedAdapter == null)
        {
            Debug.LogError("You must have either MediaPipeToAvatarAdapter or OptimizedMediaPipeAdapter in the scene!");
        }
    }

    public void CalibrateFromPersistent()
    {
        parentCalibrationData.Clear();

        if (calibrationData)
        {
            foreach (PersistentCalibrationData.CalibrationEntry d in calibrationData.parentCalibrationData)
            {
                parentCalibrationData.Add(d.bone, d.data.ReconstructReferences());
            }
            spineUpDown = calibrationData.spineUpDown.ReconstructReferences();
            hipsTwist = calibrationData.hipsTwist.ReconstructReferences();
            chest = calibrationData.chest.ReconstructReferences();
            head = calibrationData.head.ReconstructReferences();
        }

        animator.enabled = false; // disable animator to stop interference.
        Calibrated = true;
    }
    public void Calibrate()
    {
        // Here we store the values of variables required to do the correct rotations at runtime.
        print("Calibrating on " + gameObject.name);

        parentCalibrationData.Clear();

        // Manually setting calibration data for the spine chain as we want really specific control over that.
        spineUpDown = new CalibrationData(animator.transform, animator.GetBoneTransform(HumanBodyBones.Spine), animator.GetBoneTransform(HumanBodyBones.Neck),
            GetVirtualHipTransform(), GetVirtualNeckTransform());
        hipsTwist = new CalibrationData(animator.transform, animator.GetBoneTransform(HumanBodyBones.Hips), animator.GetBoneTransform(HumanBodyBones.Hips),
            GetLandmarkTransform(Landmark.RIGHT_HIP), GetLandmarkTransform(Landmark.LEFT_HIP));
        chest = new CalibrationData(animator.transform, animator.GetBoneTransform(HumanBodyBones.Chest), animator.GetBoneTransform(HumanBodyBones.Chest),
            GetLandmarkTransform(Landmark.RIGHT_HIP), GetLandmarkTransform(Landmark.LEFT_HIP));
        head = new CalibrationData(animator.transform, animator.GetBoneTransform(HumanBodyBones.Neck), animator.GetBoneTransform(HumanBodyBones.Head),
            GetVirtualNeckTransform(), GetLandmarkTransform(Landmark.NOSE));

        // Adding calibration data automatically for the rest of the bones.
        AddCalibration(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm,
            GetLandmarkTransform(Landmark.RIGHT_SHOULDER), GetLandmarkTransform(Landmark.RIGHT_ELBOW));
        AddCalibration(HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
            GetLandmarkTransform(Landmark.RIGHT_ELBOW), GetLandmarkTransform(Landmark.RIGHT_WRIST));

        AddCalibration(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg,
            GetLandmarkTransform(Landmark.RIGHT_HIP), GetLandmarkTransform(Landmark.RIGHT_KNEE));
        AddCalibration(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot,
            GetLandmarkTransform(Landmark.RIGHT_KNEE), GetLandmarkTransform(Landmark.RIGHT_ANKLE));

        AddCalibration(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm,
            GetLandmarkTransform(Landmark.LEFT_SHOULDER), GetLandmarkTransform(Landmark.LEFT_ELBOW));
        AddCalibration(HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
            GetLandmarkTransform(Landmark.LEFT_ELBOW), GetLandmarkTransform(Landmark.LEFT_WRIST));

        AddCalibration(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg,
            GetLandmarkTransform(Landmark.LEFT_HIP), GetLandmarkTransform(Landmark.LEFT_KNEE));
        AddCalibration(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
            GetLandmarkTransform(Landmark.LEFT_KNEE), GetLandmarkTransform(Landmark.LEFT_ANKLE));

        if (footTracking)
        {
            AddCalibration(HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes,
                GetLandmarkTransform(Landmark.LEFT_ANKLE), GetLandmarkTransform(Landmark.LEFT_FOOT_INDEX));
            AddCalibration(HumanBodyBones.RightFoot, HumanBodyBones.RightToes,
                GetLandmarkTransform(Landmark.RIGHT_ANKLE), GetLandmarkTransform(Landmark.RIGHT_FOOT_INDEX));
        }

        animator.enabled = false; // disable animator to stop interference.
        Calibrated = true;
    }

    public void StoreCalibration()
    {
        if (!calibrationData)
        {
            Debug.LogError("Optional calibration data must be assigned to store into.");
            return;
        }

        List<PersistentCalibrationData.CalibrationEntry> calibrations = new List<PersistentCalibrationData.CalibrationEntry>();
        foreach (KeyValuePair<HumanBodyBones, CalibrationData> k in parentCalibrationData)
        {
            calibrations.Add(new PersistentCalibrationData.CalibrationEntry() { bone = k.Key, data = k.Value });
        }
        calibrationData.parentCalibrationData = calibrations.ToArray();

        calibrationData.spineUpDown = spineUpDown;
        calibrationData.hipsTwist = hipsTwist;
        calibrationData.chest = chest;
        calibrationData.head = head;

        calibrationData.Dirty();

        print("Completed storing calibration data "+calibrationData.name);
    }
    private void AddCalibration(HumanBodyBones parent, HumanBodyBones child, Transform trackParent,Transform trackChild)
    {
        parentCalibrationData.Add(parent,
            new CalibrationData(animator.transform, animator.GetBoneTransform(parent), animator.GetBoneTransform(child),
            trackParent, trackChild));
    }

    private void Update()
    {
        // Adjust the vertical position of the avatar to keep it approximately grounded.
        if(parentCalibrationData.Count > 0)
        {
            float displacement = 0;
            RaycastHit h1;
            if (Physics.Raycast(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, Vector3.down, out h1, 100f, ground, QueryTriggerInteraction.Ignore)){
                displacement = (h1.point - animator.GetBoneTransform(HumanBodyBones.LeftFoot).position).y;
            }
            if (Physics.Raycast(animator.GetBoneTransform(HumanBodyBones.RightFoot).position, Vector3.down, out h1, 100f, ground, QueryTriggerInteraction.Ignore)){
                float displacement2 = (h1.point - animator.GetBoneTransform(HumanBodyBones.RightFoot).position).y;
                if (Mathf.Abs(displacement2) < Mathf.Abs(displacement))
                {
                    displacement = displacement2;
                }
            }
            transform.position = Vector3.Lerp(transform.position,initialPosition+ Vector3.up * displacement + Vector3.up * footGroundOffset,
                Time.deltaTime*5f);
        }

        // Compute the new rotations for each limbs of the avatar using the calibration datas we created before.
        foreach(var i in parentCalibrationData)
        {
            Quaternion deltaRotTracked = Quaternion.FromToRotation(i.Value.initialDir, i.Value.CurrentDirection);
            i.Value.parent.rotation = deltaRotTracked * i.Value.initialRotation;
        }

        // Deal with spine chain as a special case.
        if(parentCalibrationData.Count > 0)
        {
            Vector3 hd = head.CurrentDirection;
            // Some are partial rotations which we can stack together to specify how much we should rotate.
            Quaternion headr = Quaternion.FromToRotation(head.initialDir, hd);
            Quaternion twist = Quaternion.FromToRotation(hipsTwist.initialDir, 
                Vector3.Slerp(hipsTwist.initialDir,hipsTwist.CurrentDirection,.25f));
            Quaternion updown = Quaternion.FromToRotation(spineUpDown.initialDir,
                Vector3.Slerp(spineUpDown.initialDir, spineUpDown.CurrentDirection, .25f));

            // Compute the final rotations.
            Quaternion h = updown * updown * updown * twist * twist;
            Quaternion s = h * twist * updown;
            Quaternion c = s * twist * twist;
            float speed = 10f;
            hipsTwist.Tick(h * hipsTwist.initialRotation, speed);
            spineUpDown.Tick(s * spineUpDown.initialRotation, speed);
            chest.Tick(c * chest.initialRotation, speed);
            head.Tick(updown * twist * headr * head.initialRotation, speed);

            // For additional responsiveness, we rotate the entire transform slightly based on the hips.
            Vector3 d = Vector3.Slerp(hipsTwist.initialDir, hipsTwist.CurrentDirection, .25f);
            d.y *= 0.5f;
            Quaternion deltaRotTracked = Quaternion.FromToRotation(hipsTwist.initialDir, d);
            targetRot = deltaRotTracked * initialRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * speed);
        }

    }

    // Helper method to get landmarks from either adapter
    private Transform GetLandmarkTransform(Landmark landmark)
    {
        if (adapter != null)
            return adapter.GetLandmark(landmark);
        else if (optimizedAdapter != null)
            return optimizedAdapter.GetLandmark(landmark);
        else
            return null;
    }
    
    private Transform GetVirtualNeckTransform()
    {
        if (adapter != null)
            return adapter.GetVirtualNeck();
        else if (optimizedAdapter != null)
            return optimizedAdapter.GetVirtualNeck();
        else
            return null;
    }
    
    private Transform GetVirtualHipTransform()
    {
        if (adapter != null)
            return adapter.GetVirtualHip();
        else if (optimizedAdapter != null)
            return optimizedAdapter.GetVirtualHip();
        else
            return null;
    }
}
