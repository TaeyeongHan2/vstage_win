using UnityEngine;
using RootMotion.FinalIK;

namespace FinalIKFix
{
    /// <summary>
    /// GrounderIK 문제를 해결하는 유틸리티
    /// </summary>
    public class GrounderIKFixer : MonoBehaviour
    {
        [Header("GrounderIK Fix Settings")]
        public bool autoFixOnStart = true;
        
        void Start()
        {
            if (autoFixOnStart)
            {
                FixGrounderIKIssues();
            }
        }
        
        [ContextMenu("Fix GrounderIK Issues")]
        public void FixGrounderIKIssues()
        {
            Debug.Log("Starting GrounderIK fix process...");
            
            // 1. 모든 GrounderIK 컴포넌트 찾기
            GrounderIK[] grounderIKs = FindObjectsByType<GrounderIK>(FindObjectsSortMode.None);
            
            foreach (var grounderIK in grounderIKs)
            {
                Debug.Log($"Checking GrounderIK on: {grounderIK.gameObject.name}");
                
                // 2. VRIK 컴포넌트 확인
                VRIK vrik = grounderIK.GetComponent<VRIK>();
                if (vrik != null)
                {
                    Debug.LogWarning($"GrounderIK on {grounderIK.gameObject.name} is used with VRIK. This combination can cause issues.");
                    
                    // 3. 해결 방법 제시
                    Debug.Log("Recommended solutions:");
                    Debug.Log("1. Use GrounderFBBIK instead of GrounderIK for VRIK");
                    Debug.Log("2. Or use GrounderIK with FBBIK instead of VRIK");
                    Debug.Log("3. Or disable GrounderIK if not needed");
                    
                    // 4. 자동 수정 옵션
                    if (Application.isPlaying)
                    {
                        // 플레이 모드에서는 임시로 비활성화
                        grounderIK.enabled = false;
                        Debug.Log($"Temporarily disabled GrounderIK on {grounderIK.gameObject.name} to prevent errors");
                    }
                }
                
                // 5. solver null 체크
                if (grounderIK.solver == null)
                {
                    Debug.LogError($"GrounderIK solver is null on {grounderIK.gameObject.name}");
                    
                    // solver 재초기화 시도
                    try
                    {
                        // GrounderIK는 자체적으로 solver를 초기화해야 함
                        Debug.Log("Attempting to reinitialize GrounderIK...");
                        grounderIK.enabled = false;
                        grounderIK.enabled = true;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to reinitialize GrounderIK: {e.Message}");
                    }
                }
            }
            
            Debug.Log("GrounderIK fix process completed.");
        }
        
        [ContextMenu("Replace GrounderIK with GrounderFBBIK")]
        public void ReplaceWithGrounderFBBIK()
        {
            GrounderIK[] grounderIKs = FindObjectsByType<GrounderIK>(FindObjectsSortMode.None);
            
            foreach (var grounderIK in grounderIKs)
            {
                VRIK vrik = grounderIK.GetComponent<VRIK>();
                if (vrik != null)
                {
                    Debug.Log($"Replacing GrounderIK with GrounderFBBIK on {grounderIK.gameObject.name}");
                    
                    // GrounderFBBIK 추가 (VRIK와 호환됨)
                    GrounderFBBIK grounderFBBIK = grounderIK.gameObject.AddComponent<GrounderFBBIK>();
                    
                    // 설정 복사 (가능한 경우)
                    if (grounderIK.solver != null)
                    {
                        // 기본 설정들 복사
                        grounderFBBIK.weight = grounderIK.weight;
                    }
                    
                    // 기존 GrounderIK 제거
                    DestroyImmediate(grounderIK);
                    
                    Debug.Log($"Successfully replaced GrounderIK with GrounderFBBIK on {grounderFBBIK.gameObject.name}");
                }
            }
        }
        
        [ContextMenu("Remove All GrounderIK Components")]
        public void RemoveAllGrounderIKs()
        {
            GrounderIK[] grounderIKs = FindObjectsByType<GrounderIK>(FindObjectsSortMode.None);
            
            foreach (var grounderIK in grounderIKs)
            {
                Debug.Log($"Removing GrounderIK from {grounderIK.gameObject.name}");
                DestroyImmediate(grounderIK);
            }
            
            Debug.Log($"Removed {grounderIKs.Length} GrounderIK components");
        }
    }
}