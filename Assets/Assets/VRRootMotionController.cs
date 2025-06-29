using UnityEngine;

public class VRRootMotionController : MonoBehaviour
{
    [Header("VR Root Motion Settings")]
    [Tooltip("Animator 컴포넌트")]
    public Animator animator;
    
    [Tooltip("VR에서 Root Motion 비활성화")]
    public bool disableRootMotionForVR = true;
    
    [Tooltip("Root Motion을 수동으로 제어")]
    public bool manualRootMotionControl = false;
    
    [Header("Manual Root Motion")]
    [Tooltip("이동 스케일 (0 = 이동 없음, 1 = 원본 이동)")]
    [Range(0f, 1f)]
    public float motionScale = 0f;
    
    [Tooltip("회전 스케일 (0 = 회전 없음, 1 = 원본 회전)")]
    [Range(0f, 1f)]
    public float rotationScale = 0f;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
            return;
        }
        
        // VR에서는 기본적으로 Root Motion 비활성화
        if (disableRootMotionForVR)
        {
            animator.applyRootMotion = false;
            Debug.Log("Root Motion disabled for VR");
        }
        
        // 수동 제어 모드인 경우
        if (manualRootMotionControl)
        {
            animator.applyRootMotion = false;
            Debug.Log("Manual Root Motion control enabled");
        }
    }

    void OnAnimatorMove()
    {
        // 수동 Root Motion 제어가 활성화된 경우에만 실행
        if (!manualRootMotionControl || animator == null) return;
        
        // 애니메이션에서 Root Motion 정보 가져오기
        Vector3 deltaPosition = animator.deltaPosition;
        Quaternion deltaRotation = animator.deltaRotation;
        
        // 스케일 적용
        Vector3 scaledPosition = deltaPosition * motionScale;
        Quaternion scaledRotation = Quaternion.Lerp(Quaternion.identity, deltaRotation, rotationScale);
        
        // Transform에 적용
        transform.position += scaledPosition;
        transform.rotation *= scaledRotation;
        
        // 디버그 정보
        if (motionScale > 0 || rotationScale > 0)
        {
            Debug.Log($"Manual Root Motion - Position: {scaledPosition}, Rotation: {scaledRotation.eulerAngles}");
        }
    }
    
    [ContextMenu("Toggle Root Motion")]
    public void ToggleRootMotion()
    {
        if (animator != null)
        {
            animator.applyRootMotion = !animator.applyRootMotion;
            Debug.Log($"Root Motion: {(animator.applyRootMotion ? "Enabled" : "Disabled")}");
        }
    }
    
    [ContextMenu("Disable Root Motion (VR Mode)")]
    public void DisableRootMotionForVR()
    {
        if (animator != null)
        {
            animator.applyRootMotion = false;
            disableRootMotionForVR = true;
            Debug.Log("Root Motion disabled for VR mode");
        }
    }
    
    [ContextMenu("Enable Manual Root Motion")]
    public void EnableManualRootMotion()
    {
        if (animator != null)
        {
            animator.applyRootMotion = false;
            manualRootMotionControl = true;
            Debug.Log("Manual Root Motion control enabled");
        }
    }
}