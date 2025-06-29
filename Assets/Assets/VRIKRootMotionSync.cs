using UnityEngine;
using RootMotion.FinalIK;

public class VRIKRootMotionSync : MonoBehaviour
{
    [Header("Components")]
    public VRIK vrik;
    public Animator animator;
    public Transform vrCamera;
    
    [Header("Root Motion Settings")]
    [Tooltip("Root Motion과 VR 위치 동기화")]
    public bool syncRootMotionWithVR = true;
    
    [Tooltip("애니메이션 이동량 제한")]
    [Range(0f, 1f)]
    public float rootMotionInfluence = 0.3f;
    
    private Vector3 lastVRPosition;
    private Vector3 accumulatedMotion;
    
    void Start()
    {
        if (vrik == null) vrik = GetComponent<VRIK>();
        if (animator == null) animator = GetComponent<Animator>();
        
        if (vrCamera != null)
        {
            lastVRPosition = vrCamera.position;
        }
        
        // Root Motion을 수동으로 처리
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }
    
    void OnAnimatorMove()
    {
        if (!syncRootMotionWithVR || animator == null) return;
        
        // 애니메이션의 Root Motion 가져오기
        Vector3 animationDelta = animator.deltaPosition;
        
        // VR 카메라 이동량 계산
        Vector3 vrDelta = Vector3.zero;
        if (vrCamera != null)
        {
            vrDelta = vrCamera.position - lastVRPosition;
            lastVRPosition = vrCamera.position;
        }
        
        // Root Motion과 VR 이동량 혼합
        Vector3 finalDelta = Vector3.Lerp(vrDelta, animationDelta, rootMotionInfluence);
        
        // 캐릭터 이동 적용
        transform.position += finalDelta;
        
        // VRIK 타겟 업데이트 (필요시)
        if (vrik != null && vrik.solver != null)
        {
            // 몸체 위치를 VR 카메라와 동기화
            Vector3 bodyTarget = new Vector3(vrCamera.position.x, transform.position.y, vrCamera.position.z);
            vrik.solver.spine.pelvisTarget = null; // 자동 위치 조정 허용
        }
    }
    
    [ContextMenu("Reset VR Position Sync")]
    public void ResetVRPositionSync()
    {
        if (vrCamera != null)
        {
            lastVRPosition = vrCamera.position;
            accumulatedMotion = Vector3.zero;
            Debug.Log("VR position sync reset");
        }
    }
}