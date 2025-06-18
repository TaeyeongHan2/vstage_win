using System.Collections;
using UnityEngine;
using Mediapipe;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Tasks.Core;
using CoreVision = Mediapipe.Tasks.Vision.Core;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;

/// <summary>
/// Captures webcam frames, runs MediaPipe PoseLandmarker live-stream, and sends the first detected pose landmarks
/// to a MediaPipePoseToHumanoid component.
/// </summary>
public class PoseLandmarkerHumanoidDriver : MonoBehaviour
{
    [Tooltip("Component that converts landmark positions to Humanoid bones")] public MediaPipePoseToHumanoid humanoid;

    [Tooltip("Task model filename placed under StreamingAssets")] public string modelAssetPath = "pose_landmarker_full.bytes";

    private PoseLandmarker _landmarker;
    private WebCamTexture _webcam;
    private Texture2D _frameTexture;

    private IEnumerator Start()
    {
        if (humanoid == null)
        {
            Debug.LogError("Humanoid target not assigned – please drag a GameObject with MediaPipePoseToHumanoid.");
            yield break;
        }

        // Initialize AssetLoader with StreamingAssets manager (only once per session).
        AssetLoader.Provide(new StreamingAssetsResourceManager());

        // Ensure model asset is copied to cache and its path registered.
        yield return AssetLoader.PrepareAssetAsync(modelAssetPath);

        var baseOptions = new BaseOptions(BaseOptions.Delegate.CPU, modelAssetPath: modelAssetPath);
        var options = new PoseLandmarkerOptions(
            baseOptions,
            runningMode: CoreVision.RunningMode.LIVE_STREAM,
            numPoses: 1,
            minPoseDetectionConfidence: 0.5f,
            minPosePresenceConfidence: 0.5f,
            minTrackingConfidence: 0.5f,
            resultCallback: OnLandmarkerResult);

        _landmarker = PoseLandmarker.CreateFromOptions(options);

        // Initialize webcam (first available camera)
        _webcam = new WebCamTexture();
        _webcam.Play();

        // Wait until the webcam is actually streaming frames
        while (_webcam.width <= 16)
            yield return null;

        _frameTexture = new Texture2D(_webcam.width, _webcam.height, TextureFormat.RGBA32, false);
    }

    private void Update()
    {
        if (_webcam == null || _landmarker == null) return;
        if (!_webcam.didUpdateThisFrame) return;

        // Copy current frame into Texture2D
        _frameTexture.SetPixels32(_webcam.GetPixels32());
        _frameTexture.Apply(false);

        using var image = new Image(_frameTexture);
        long timestampMillis = (long)(Time.realtimeSinceStartup * 1000);
        _landmarker.DetectAsync(image, timestampMillis, null);
    }

    private void OnLandmarkerResult(PoseLandmarkerResult result, Image image, long timestamp)
    {
        if (result.poseLandmarks != null && result.poseLandmarks.Count > 0)
        {
            humanoid.ApplyLandmarks(result.poseLandmarks[0]);
        }
    }

    private void OnDestroy()
    {
        _landmarker?.Close();
        _landmarker = null;
        if (_webcam != null)
        {
            _webcam.Stop();
            _webcam = null;
        }
        if (_frameTexture != null)
            Destroy(_frameTexture);
    }
}
