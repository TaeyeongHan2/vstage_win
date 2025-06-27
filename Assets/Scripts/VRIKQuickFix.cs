using UnityEngine;
using RootMotion.FinalIK;

[RequireComponent(typeof(VRIK))]
public class VRIKQuickFix : MonoBehaviour
{
    private VRIK vrik;
    
    void Start()
    {
        vrik = GetComponent<VRIK>();
        
        // 즉시 적용되는 수정사항들
        ApplyQuickFixes();
    }
    
    void ApplyQuickFixes()
    {
        // 1. PlantFeet 비활성화 (가장 흔한 원인)
        vrik.solver.plantFeet = false;
        
        // 2. Spine 설정 조정 (허리가 과도하게 늘어나는 것 방지)
        vrik.solver.spine.pelvisPositionWeight = 0.8f; // 1에서 0.8로 감소
        vrik.solver.spine.pelvisRotationWeight = 0.8f;
        vrik.solver.spine.chestGoalWeight = 0.8f;
        
        // 3. 다리 설정
        vrik.solver.leftLeg.positionWeight = 1f;
        vrik.solver.leftLeg.rotationWeight = 1f;
        vrik.solver.rightLeg.positionWeight = 1f;
        vrik.solver.rightLeg.rotationWeight = 1f;
        
        // 4. IK 위치 가중치 조정
        vrik.solver.IKPositionWeight = 1f;
        
        // 5. Locomotion 미세 조정
        vrik.solver.locomotion.weight = 0.9f; // 1에서 0.9로 감소
        vrik.solver.locomotion.footDistance = 0.3f;
        vrik.solver.locomotion.stepThreshold = 0.3f; // 더 낮은 값으로
        
        Debug.Log("VRIK Quick Fixes Applied!");
    }
    
    // Inspector에서 버튼으로 실행 가능
    [ContextMenu("Apply Quick Fixes")]
    public void ManualApplyFixes()
    {
        if (vrik == null) vrik = GetComponent<VRIK>();
        ApplyQuickFixes();
    }
} 