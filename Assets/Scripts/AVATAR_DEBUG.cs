using UnityEngine;
using System.Collections;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;

public class AvatarDebug : MonoBehaviour
{
    private float checkInterval = 2f;
    
    void Start()
    {
        StartCoroutine(DiagnoseAvatarSystem());
    }
    
    IEnumerator DiagnoseAvatarSystem()
    {
        yield return new WaitForSeconds(1f);
        
        Debug.Log("========== 🔍 AVATAR SYSTEM DIAGNOSIS START ==========");
        
        // 1. PoseLandmarkerRunner 확인
        var runner = FindObjectOfType<PoseLandmarkerRunner>();
        if (runner == null)
        {
            Debug.LogError("❌ [1] PoseLandmarkerRunner NOT FOUND! - MediaPipe 포즈 검출이 없습니다");
            yield break;
        }
        else
        {
            Debug.Log("✅ [1] PoseLandmarkerRunner found");
        }
        
        // 2. MediaPipe 어댑터 확인
        var adapter = FindObjectOfType<MediaPipeToAvatarAdapter>();
        var optimizedAdapter = FindObjectOfType<OptimizedMediaPipeAdapter>();
        
        if (adapter == null && optimizedAdapter == null)
        {
            Debug.LogError("❌ [2] NO ADAPTER FOUND! 다음을 수행하세요:");
            Debug.LogError("    1) 빈 GameObject 생성");
            Debug.LogError("    2) 이름을 'MediaPipeAdapter'로 변경");
            Debug.LogError("    3) OptimizedMediaPipeAdapter 컴포넌트 추가");
            Debug.LogError("    4) PoseLandmarkerRunner를 Inspector에서 연결");
            yield break;
        }
        else
        {
            var adapterName = adapter != null ? "MediaPipeToAvatarAdapter" : "OptimizedMediaPipeAdapter";
            Debug.Log($"✅ [2] Adapter found: {adapterName}");
            
            // 어댑터의 PoseLandmarkerRunner 연결 확인
            if (optimizedAdapter != null && optimizedAdapter.GetComponent<OptimizedMediaPipeAdapter>().GetType().GetField("poseLandmarkerRunner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(optimizedAdapter) == null)
            {
                Debug.LogError("❌ [2-1] Adapter의 PoseLandmarkerRunner가 연결되지 않았습니다!");
            }
        }
        
        // 3. Avatar 컴포넌트 확인
        var avatars = FindObjectsOfType<Avatar>();
        if (avatars.Length == 0)
        {
            Debug.LogError("❌ [3] Avatar 컴포넌트가 없습니다! 캐릭터에 Avatar.cs를 추가하세요");
            yield break;
        }
        else
        {
            Debug.Log($"✅ [3] Found {avatars.Length} Avatar(s):");
            foreach (var avatar in avatars)
            {
                Debug.Log($"    - {avatar.gameObject.name} (Calibrated: {avatar.Calibrated})");
                
                // Animator 확인
                if (avatar.animator == null)
                {
                    Debug.LogError($"    ❌ {avatar.gameObject.name}의 Animator가 없습니다!");
                }
                else if (!avatar.animator.isHuman)
                {
                    Debug.LogError($"    ❌ {avatar.gameObject.name}의 Animator가 Humanoid가 아닙니다!");
                }
            }
        }
        
        // 4. 캘리브레이션 상태 확인
        yield return new WaitForSeconds(1f);
        
        bool anyCalibrated = false;
        foreach (var avatar in avatars)
        {
            if (avatar.Calibrated)
            {
                anyCalibrated = true;
                Debug.Log($"✅ [4] {avatar.gameObject.name} is calibrated");
            }
        }
        
        if (!anyCalibrated)
        {
            Debug.LogWarning("⚠️ [4] 캘리브레이션이 안되어 있습니다!");
            Debug.LogWarning("    → 'C' 키를 눌러 캘리브레이션을 시작하세요");
            Debug.LogWarning("    → T-포즈를 취하고 5초 기다리세요");
        }
        
        // 5. WebCam/ImageSource 상태 확인
        var imageSource = Mediapipe.Unity.Sample.ImageSourceProvider.ImageSource;
        if (imageSource != null)
        {
            Debug.Log($"✅ [5] ImageSource: {imageSource.sourceName}");
            Debug.Log($"    - Prepared: {imageSource.isPrepared}");
            Debug.Log($"    - Playing: {imageSource.isPlaying}");
            
            if (!imageSource.isPrepared || !imageSource.isPlaying)
            {
                Debug.LogError("❌ [5-1] WebCam이 시작되지 않았습니다!");
                Debug.LogError("    → 화면 좌측 상단 메뉴에서 'HD Webcam' 선택");
            }
        }
        
        // 6. 실시간 포즈 검출 확인
        yield return new WaitForSeconds(2f);
        
        Debug.Log("========== 📊 REAL-TIME CHECK (5초간) ==========");
        
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1f);
            
            // 랜드마크 위치 확인
            if (optimizedAdapter != null || adapter != null)
            {
                var landmark = optimizedAdapter != null ? 
                    optimizedAdapter.GetLandmark(Landmark.NOSE) : 
                    adapter.GetLandmark(Landmark.NOSE);
                    
                if (landmark != null)
                {
                    Debug.Log($"[{i+1}/5] Nose position: {landmark.position}");
                    
                    if (landmark.position == Vector3.zero)
                    {
                        Debug.LogWarning("    ⚠️ 랜드마크가 (0,0,0)입니다 - 포즈가 검출되지 않음");
                    }
                }
            }
        }
        
        Debug.Log("========== 🏁 DIAGNOSIS COMPLETE ==========");
        
        // 최종 권장사항
        Debug.Log("\n📌 권장 조치사항:");
        Debug.Log("1. 위 체크리스트에서 ❌ 표시된 항목 수정");
        Debug.Log("2. WebCam을 'HD Webcam'으로 설정 (OBS Virtual Camera X)");
        Debug.Log("3. 'C' 키로 캘리브레이션 실행");
        Debug.Log("4. 카메라 앞에서 몸 전체가 보이도록 위치 조정");
    }
} 