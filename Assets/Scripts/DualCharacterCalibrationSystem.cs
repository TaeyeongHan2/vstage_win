using UnityEngine;
using RootMotion.FinalIK;
using RootMotion.Demos;

public class DualCharacterCalibrationSystem : MonoBehaviour
{
    [Header("Characters")]
    [Tooltip("트래커 위치를 표시하는 참조 캐릭터")]
    public GameObject referenceCharacter;
    
    [Tooltip("VRIK가 적용될 실제 캐릭터")]
    public GameObject vrikCharacter;
    
    [Header("Settings")]
    public bool showReferenceCharacter = true;
    public float referenceCharacterAlpha = 0.5f;
    public bool syncReferenceToTrackers = true;
    
    [Header("Comparison")]
    public bool showComparison = true;
    public Color matchColor = Color.green;
    public Color mismatchColor = Color.red;
    public float mismatchThreshold = 0.1f;
    
    private VRIKCalibrationController calibrationController;
    private Animator referenceAnimator;
    private Animator vrikAnimator;
    private Material[] referenceMaterials;
    
    void Start()
    {
        calibrationController = FindObjectOfType<VRIKCalibrationController>();
        
        if (referenceCharacter == null)
        {
            // 참조 캐릭터 생성 (VRIK 캐릭터 복제)
            CreateReferenceCharacter();
        }
        
        SetupCharacters();
    }
    
    void CreateReferenceCharacter()
    {
        if (vrikCharacter == null) return;
        
        referenceCharacter = Instantiate(vrikCharacter);
        referenceCharacter.name = "Reference Character (Tracker Positions)";
        
        // VRIK 컴포넌트 제거
        var vrik = referenceCharacter.GetComponent<VRIK>();
        if (vrik != null) Destroy(vrik);
        
        // 위치 조정
        referenceCharacter.transform.position += Vector3.right * 2f;
    }
    
    void SetupCharacters()
    {
        if (referenceCharacter != null)
        {
            referenceAnimator = referenceCharacter.GetComponent<Animator>();
            
            // 반투명하게 만들기
            var renderers = referenceCharacter.GetComponentsInChildren<Renderer>();
            referenceMaterials = new Material[renderers.Length];
            
            for (int i = 0; i < renderers.Length; i++)
            {
                referenceMaterials[i] = new Material(renderers[i].material);
                referenceMaterials[i].SetFloat("_Mode", 3); // Transparent
                referenceMaterials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                referenceMaterials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                referenceMaterials[i].SetInt("_ZWrite", 0);
                referenceMaterials[i].DisableKeyword("_ALPHATEST_ON");
                referenceMaterials[i].EnableKeyword("_ALPHABLEND_ON");
                referenceMaterials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                referenceMaterials[i].renderQueue = 3000;
                
                Color color = referenceMaterials[i].color;
                color.a = referenceCharacterAlpha;
                referenceMaterials[i].color = color;
                
                renderers[i].material = referenceMaterials[i];
            }
        }
        
        if (vrikCharacter != null)
        {
            vrikAnimator = vrikCharacter.GetComponent<Animator>();
        }
    }
    
    void Update()
    {
        if (!syncReferenceToTrackers || calibrationController == null) return;
        
        // 참조 캐릭터의 본을 트래커 위치에 직접 매칭
        if (referenceAnimator != null)
        {
            SyncBonesWithTrackers();
        }
        
        referenceCharacter.SetActive(showReferenceCharacter);
        
        if (showComparison)
        {
            CompareCharacters();
        }
    }
    
    void SyncBonesWithTrackers()
    {
        // 머리
        if (calibrationController.headTracker != null)
        {
            var head = referenceAnimator.GetBoneTransform(HumanBodyBones.Head);
            if (head != null)
            {
                // 트래커 위치에서 오프셋을 고려한 위치 계산
                Vector3 headPos = calibrationController.headTracker.position;
                if (calibrationController.settings != null)
                {
                    headPos += calibrationController.headTracker.rotation * calibrationController.settings.headOffset;
                }
                head.position = headPos;
            }
        }
        
        // 골반
        if (calibrationController.bodyTracker != null)
        {
            var pelvis = referenceAnimator.GetBoneTransform(HumanBodyBones.Hips);
            if (pelvis != null)
            {
                pelvis.position = calibrationController.bodyTracker.position;
            }
        }
        
        // 손
        if (calibrationController.leftHandTracker != null)
        {
            var leftHand = referenceAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
            if (leftHand != null)
            {
                leftHand.position = calibrationController.leftHandTracker.position;
                if (calibrationController.settings != null)
                {
                    leftHand.position += calibrationController.leftHandTracker.rotation * calibrationController.settings.handOffset;
                }
            }
        }
        
        if (calibrationController.rightHandTracker != null)
        {
            var rightHand = referenceAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            if (rightHand != null)
            {
                rightHand.position = calibrationController.rightHandTracker.position;
                if (calibrationController.settings != null)
                {
                    rightHand.position += calibrationController.rightHandTracker.rotation * calibrationController.settings.handOffset;
                }
            }
        }
        
        // 발
        if (calibrationController.leftFootTracker != null)
        {
            var leftFoot = referenceAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            if (leftFoot != null)
            {
                // 발의 경우 Y 위치는 캐릭터 발 높이 유지
                Vector3 footPos = calibrationController.leftFootTracker.position;
                footPos.y = leftFoot.position.y;
                leftFoot.position = footPos;
            }
        }
        
        if (calibrationController.rightFootTracker != null)
        {
            var rightFoot = referenceAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
            if (rightFoot != null)
            {
                Vector3 footPos = calibrationController.rightFootTracker.position;
                footPos.y = rightFoot.position.y;
                rightFoot.position = footPos;
            }
        }
    }
    
    void CompareCharacters()
    {
        if (referenceAnimator == null || vrikAnimator == null) return;
        
        // 주요 본들의 위치 차이 비교
        CompareBone(HumanBodyBones.Head);
        CompareBone(HumanBodyBones.Hips);
        CompareBone(HumanBodyBones.LeftHand);
        CompareBone(HumanBodyBones.RightHand);
        CompareBone(HumanBodyBones.LeftFoot);
        CompareBone(HumanBodyBones.RightFoot);
    }
    
    void CompareBone(HumanBodyBones bone)
    {
        var refBone = referenceAnimator.GetBoneTransform(bone);
        var vrikBone = vrikAnimator.GetBoneTransform(bone);
        
        if (refBone != null && vrikBone != null)
        {
            float distance = Vector3.Distance(refBone.position, vrikBone.position);
            Color color = distance < mismatchThreshold ? matchColor : mismatchColor;
            
            Debug.DrawLine(refBone.position, vrikBone.position, color);
        }
    }
    
    void OnGUI()
    {
        if (!showComparison) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 200));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("<b>Character Comparison</b>");
        
        if (referenceAnimator != null && vrikAnimator != null)
        {
            ShowBoneComparison(HumanBodyBones.Head, "Head");
            ShowBoneComparison(HumanBodyBones.Hips, "Hips");
            ShowBoneComparison(HumanBodyBones.LeftHand, "Left Hand");
            ShowBoneComparison(HumanBodyBones.RightHand, "Right Hand");
            ShowBoneComparison(HumanBodyBones.LeftFoot, "Left Foot");
            ShowBoneComparison(HumanBodyBones.RightFoot, "Right Foot");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void ShowBoneComparison(HumanBodyBones bone, string name)
    {
        var refBone = referenceAnimator.GetBoneTransform(bone);
        var vrikBone = vrikAnimator.GetBoneTransform(bone);
        
        if (refBone != null && vrikBone != null)
        {
            float distance = Vector3.Distance(refBone.position, vrikBone.position);
            string color = distance < mismatchThreshold ? "green" : "red";
            GUILayout.Label($"<color={color}>{name}: {distance:F3}m</color>");
        }
    }
} 