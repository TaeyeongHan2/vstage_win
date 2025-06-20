using UnityEngine;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using System.Collections;

public class DebugHelper : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DebugSequence());
    }
    
    IEnumerator DebugSequence()
    {
        // PoseLandmarkerRunner 찾기
        var runner = FindObjectOfType<PoseLandmarkerRunner>();
        if (runner != null)
        {
            // CPU 모드로 변경 (더 안정적)
            runner.config.ImageReadMode = Mediapipe.Unity.ImageReadMode.CPU;
            Debug.Log("Changed ImageReadMode to CPU for stability");
        }
        
        // WebCam 상태 확인
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No webcam detected!");
        }
        else
        {
            Debug.Log($"Found {WebCamTexture.devices.Length} webcam(s):");
            foreach (var device in WebCamTexture.devices)
            {
                Debug.Log($"- {device.name}");
            }
        }
        
        // 잠시 대기 후 어댑터 확인
        yield return new WaitForSeconds(2f);
        
        // MediaPipe 어댑터 확인
        var adapter = FindObjectOfType<MediaPipeToAvatarAdapter>();
        var optimizedAdapter = FindObjectOfType<OptimizedMediaPipeAdapter>();
        
        if (adapter == null && optimizedAdapter == null)
        {
            Debug.LogError("❌ No MediaPipe adapter found! You need to add MediaPipeToAvatarAdapter or OptimizedMediaPipeAdapter to the scene!");
        }
        else
        {
            Debug.Log($"✅ Found adapter: {(adapter != null ? "MediaPipeToAvatarAdapter" : "OptimizedMediaPipeAdapter")}");
        }
        
        // Avatar 확인
        var avatars = FindObjectsOfType<Avatar>();
        if (avatars.Length == 0)
        {
            Debug.LogWarning("⚠️ No Avatar components found in scene");
        }
        else
        {
            Debug.Log($"✅ Found {avatars.Length} Avatar(s)");
            foreach (var avatar in avatars)
            {
                Debug.Log($"  - {avatar.gameObject.name}");
            }
        }
        
        // WebCam 권한 문제 해결 시도
        yield return new WaitForSeconds(1f);
        
        // ImageSource 확인
        var imageSource = Mediapipe.Unity.Sample.ImageSourceProvider.ImageSource;
        if (imageSource != null && imageSource is Mediapipe.Unity.WebCamSource webCamSource)
        {
            Debug.Log($"ImageSource type: {imageSource.GetType().Name}");
            Debug.Log($"ImageSource prepared: {imageSource.isPrepared}");
            Debug.Log($"ImageSource playing: {imageSource.isPlaying}");
            
            // OBS Virtual Camera 문제 해결
            if (!imageSource.isPrepared)
            {
                Debug.LogWarning("Trying to switch to HD Webcam instead of OBS Virtual Camera...");
                // HD Webcam 선택 (index 0)
                webCamSource.SelectSource(0);
            }
        }
    }
} 