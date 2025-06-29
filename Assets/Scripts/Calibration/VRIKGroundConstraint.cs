using UnityEngine;
using RootMotion.FinalIK;

public class VRIKGroundConstraint : MonoBehaviour
{
    [Header("VRIK 컴포넌트")]
    public VRIK vrik;
    
    [Header("바닥 감지 설정")]
    public LayerMask groundLayerMask = 1;
    public float minHeightAboveGround = 0.05f;
    public float raycastDistance = 3f;
    
    [Header("런타임 조정 설정")]
    [Space]
    [Tooltip("캐릭터가 바닥 아래로 내려갈 수 있는 최대 거리")]
    public float maxBodyYOffset = 0.01f;
    
    [Tooltip("캐릭터가 서 있을 때의 Y 오프셋")]
    public float standOffsetY = 0.05f;
    
    [Tooltip("두 발 사이의 거리")]
    public float footDistance = 0.3f;
    
    [Tooltip("루트 움직임 속도")]
    public float rootSpeed = 20f;
    
    [Header("실시간 테스트 키")]
    [Space]
    public KeyCode increaseHeightKey = KeyCode.Q;
    public KeyCode decreaseHeightKey = KeyCode.E;
    public KeyCode resetSettingsKey = KeyCode.R;
    
    [Header("디버그")]
    public bool showDebugRays = true;
    public bool enableRuntimeAdjustment = true;
    
    // 초기값 저장
    private float originalMaxBodyYOffset;
    private Vector3 originalStandOffset;
    private float originalFootDistance;
    private float originalRootSpeed;
    
    void Start()
    {
        // VRIK 컴포넌트 자동 찾기
        if (vrik == null)
            vrik = GetComponent<VRIK>();
            
        if (vrik == null)
        {
            Debug.LogError("VRIK 컴포넌트를 찾을 수 없습니다!");
            enabled = false;
            return;
        }
        
        // 초기값 저장
        SaveOriginalSettings();
        
        // 초기 설정 적용
        ApplySettings();
    }
    
    void SaveOriginalSettings()
    {
        // Locomotion 관련 초기값 저장
        if (vrik.solver.locomotion != null)
        {
            originalFootDistance = vrik.solver.locomotion.footDistance;
            originalRootSpeed = vrik.solver.locomotion.rootSpeed;
            originalStandOffset = vrik.solver.locomotion.offset;
        }
        
        // 현재는 maxBodyYOffset에 대한 직접적인 API를 찾지 못했으므로 
        // 다른 방법으로 접근할 예정
        originalMaxBodyYOffset = maxBodyYOffset;
    }
    
    void ApplySettings()
    {
        if (vrik.solver.locomotion != null)
        {
            // Locomotion 설정 적용
            vrik.solver.locomotion.footDistance = footDistance;
            vrik.solver.locomotion.rootSpeed = rootSpeed;
            vrik.solver.locomotion.offset = new Vector3(0, standOffsetY, 0);
            
            // 기타 바닥 뚫림 방지 설정들
            vrik.solver.locomotion.weight = 1f; // 전체 가중치
        }
    }
    
    void Update()
    {
        // 런타임 조정이 활성화되어 있으면
        if (enableRuntimeAdjustment)
        {
            HandleKeyboardInput();
            PerformGroundCheck();
        }
        
        // 설정이 변경되었으면 실시간 적용
        ApplySettings();
    }
    
    void HandleKeyboardInput()
    {
        // Q키: 높이 증가
        if (Input.GetKey(increaseHeightKey))
        {
            standOffsetY += Time.deltaTime * 0.1f;
            standOffsetY = Mathf.Clamp(standOffsetY, -0.2f, 0.5f);
            Debug.Log($"Stand Offset Y: {standOffsetY:F3}");
        }
        
        // E키: 높이 감소
        if (Input.GetKey(decreaseHeightKey))
        {
            standOffsetY -= Time.deltaTime * 0.1f;
            standOffsetY = Mathf.Clamp(standOffsetY, -0.2f, 0.5f);
            Debug.Log($"Stand Offset Y: {standOffsetY:F3}");
        }
        
        // R키: 설정 리셋
        if (Input.GetKeyDown(resetSettingsKey))
        {
            ResetToOriginalSettings();
            Debug.Log("VRIK 설정이 초기값으로 리셋되었습니다.");
        }
    }
    
    void PerformGroundCheck()
    {
        // 캐릭터 발 위치에서 바닥까지의 거리 체크
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastDistance, groundLayerMask))
        {
            float distanceToGround = hit.distance - 0.5f; // 레이캐스트 시작점 보정
            
            // 바닥에 너무 가까우면 자동으로 높이 조정
            if (distanceToGround < minHeightAboveGround)
            {
                float requiredOffset = minHeightAboveGround - distanceToGround + standOffsetY;
                vrik.solver.locomotion.offset = new Vector3(0, requiredOffset, 0);
                
                if (showDebugRays)
                {
                    Debug.Log($"바닥 감지: 자동 높이 조정 - {requiredOffset:F3}");
                }
            }
            
            // 디버그 레이 표시
            if (showDebugRays)
            {
                Debug.DrawRay(rayStart, Vector3.down * hit.distance, Color.green);
                Debug.DrawRay(hit.point, Vector3.up * minHeightAboveGround, Color.red);
            }
        }
        else if (showDebugRays)
        {
            Debug.DrawRay(rayStart, Vector3.down * raycastDistance, Color.red);
        }
    }
    
    void LateUpdate()
    {
        // 추가적인 물리적 제약 (필요시)
        if (enableRuntimeAdjustment)
        {
            EnforceGroundConstraint();
        }
    }
    
    void EnforceGroundConstraint()
    {
        // 최후의 수단: 캐릭터가 바닥 아래로 가지 않도록 강제 조정
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 2f, groundLayerMask))
        {
            float groundY = hit.point.y;
            Vector3 currentPos = transform.position;
            
            // 캐릭터가 바닥 아래에 있으면 강제로 위로 이동
            if (currentPos.y < groundY + maxBodyYOffset)
            {
                currentPos.y = groundY + maxBodyYOffset;
                transform.position = currentPos;
                
                if (showDebugRays)
                {
                    Debug.LogWarning($"강제 높이 조정: {currentPos.y:F3}");
                }
            }
        }
    }
    
    public void ResetToOriginalSettings()
    {
        standOffsetY = originalStandOffset.y;
        footDistance = originalFootDistance;
        rootSpeed = originalRootSpeed;
        maxBodyYOffset = originalMaxBodyYOffset;
        
        ApplySettings();
    }
    
    public void SetStandOffsetY(float newOffset)
    {
        standOffsetY = Mathf.Clamp(newOffset, -0.2f, 0.5f);
        ApplySettings();
    }
    
    public void SetFootDistance(float newDistance)
    {
        footDistance = Mathf.Clamp(newDistance, 0.1f, 1f);
        ApplySettings();
    }
    
    // VR 컨트롤러에서 호출할 수 있는 메서드들
    public void AdjustHeightUp()
    {
        SetStandOffsetY(standOffsetY + 0.05f);
        Debug.Log($"높이 증가: {standOffsetY:F3}");
    }
    
    public void AdjustHeightDown()
    {
        SetStandOffsetY(standOffsetY - 0.05f);
        Debug.Log($"높이 감소: {standOffsetY:F3}");
    }
}