// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.PoseLandmarkDetection
{
  /// PoseLandmarker를 실행하고, 결과를 받아 화면에 표시하는 역할
  public class PoseLandmarkerRunner : VisionTaskApiRunner<PoseLandmarker>
  {
    /// 포즈 랜드마커 결과를 시각화할 Annotation Controller를 할당
    [SerializeField] private PoseLandmarkerResultAnnotationController _poseLandmarkerResultAnnotationController;
    /// 텍스처를 효율적으로 관리하기 위한 풀(Pool) 클래스
    private Experimental.TextureFramePool _textureFramePool;
    /// 포즈 랜드마크 검출에 필요한 설정값을 저장한 Config 객체
    public readonly PoseLandmarkDetectionConfig config = new PoseLandmarkDetectionConfig();
    /// 런너를 중지할 때 호출되는 메서드입니다. 리소스를 정리합니다
    public override void Stop()
    {
      // 부모 클래스의 Stop 로직 호출
      base.Stop();
      // TextureFramePool이 있으면 Dispose 처리
      _textureFramePool?.Dispose();
      _textureFramePool = null;
    }
    /// 실제 PoseLandmarker를 실행하고, 이미지를 입력받아 반복적으로 처리하는 코루틴
    protected override IEnumerator Run()
    {
      // 설정값 확인용 디버그 출력
      Debug.Log($"Delegate = {config.Delegate}");
      Debug.Log($"Image Read Mode = {config.ImageReadMode}");
      Debug.Log($"Model = {config.ModelName}");
      Debug.Log($"Running Mode = {config.RunningMode}");
      Debug.Log($"NumPoses = {config.NumPoses}");
      Debug.Log($"MinPoseDetectionConfidence = {config.MinPoseDetectionConfidence}");
      Debug.Log($"MinPosePresenceConfidence = {config.MinPosePresenceConfidence}");
      Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");
      Debug.Log($"OutputSegmentationMasks = {config.OutputSegmentationMasks}");
      // 모델 파일 등 필요한 Asset을 로드
      yield return AssetLoader.PrepareAssetAsync(config.ModelPath);
      // PoseLandmarker 생성 옵션을 가져옴
      var options = config.GetPoseLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnPoseLandmarkDetectionOutput : null);
      // PoseLandmarker 인스턴스 생성
      taskApi = PoseLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
      // 입력 영상 소스를 가져와 재생 시작
      var imageSource = ImageSourceProvider.ImageSource;

      yield return imageSource.Play();
      // 영상 소스가 준비되지 않으면 종료
      if (!imageSource.isPrepared)
      {
        Logger.LogError(TAG, "Failed to start ImageSource, exiting...");
        yield break;
      }
      // RGBA32 형식으로 텍스처 풀 생성
      // Use RGBA32 as the input format.
      // (GPUBuffer 사용 시 BGRA가 기본이므로 필요한 경우 수정이 필요)
      // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
      _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

      // 화면 초기화 (가로세로 비율 유지해서 정렬)
      // NOTE: The screen will be resized later, keeping the aspect ratio.
      screen.Initialize(imageSource);
      
      // Annotation Controller 설정
      SetupAnnotationController(_poseLandmarkerResultAnnotationController, imageSource);
      _poseLandmarkerResultAnnotationController.InitScreen(imageSource.textureWidth, imageSource.textureHeight);

      // 화면 뒤집기(flip) 및 회전 각도 옵션 설정
      var transformationOptions = imageSource.GetTransformationOptions();
      var flipHorizontally = transformationOptions.flipHorizontally;
      var flipVertically = transformationOptions.flipVertically;

      // 회전을 0도 고정 (회전 시 검출 성능 문제가 생길 수 있음)
      // Always setting rotationDegrees to 0 to avoid the issue that the detection becomes unstable when the input image is rotated.
      // https://github.com/homuler/MediaPipeUnityPlugin/issues/1196
      var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: 0);

      AsyncGPUReadbackRequest req = default;
      var waitUntilReqDone = new WaitUntil(() => req.done);
      var waitForEndOfFrame = new WaitForEndOfFrame();
      // PoseLandmarkerResult 객체 생성 (segmentationMasks 사용 여부에 따라 다름)
      var result = PoseLandmarkerResult.Alloc(options.numPoses, options.outputSegmentationMasks);
      
      // Android 환경에서만 GL context를 공유 가능
      // NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
      var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && GpuManager.GpuResources != null;
      using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;
      
      // 계속해서 이미지 프레임을 입력받아 처리
      while (true)
      {
        // 일시 정지 상태면 대기
        if (isPaused)
        {
          yield return new WaitWhile(() => isPaused);
        }

        // TextureFramePool에서 사용 가능한 프레임 가져옴
        if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
        {
          yield return new WaitForEndOfFrame();
          continue;
        }

        // 이미지 입력 모드에 따라 CPU/GPU 이미지를 빌드
        // Build the input Image
        Image image;
        switch (config.ImageReadMode)
        {
          case ImageReadMode.GPU:
            if (!canUseGpuImage)
            {
              throw new System.Exception("ImageReadMode.GPU is not supported");
            }
            textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            image = textureFrame.BuildGPUImage(glContext);
            // TODO: 완전히 복사가 보장되지 않으므로 1프레임 대기
            // TODO: Currently we wait here for one frame to make sure the texture is fully copied to the TextureFrame before sending it to MediaPipe.
            // This usually works but is not guaranteed. Find a proper way to do this. See: https://github.com/homuler/MediaPipeUnityPlugin/pull/1311
            yield return waitForEndOfFrame;
            break;
          case ImageReadMode.CPU:
            // CPU 모드에서는 프레임을 읽고 바로 CPU에 복사해 사용
            yield return waitForEndOfFrame;
            textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            image = textureFrame.BuildCPUImage();
            textureFrame.Release();
            break;
          case ImageReadMode.CPUAsync:
          default:
            // 비동기 방식으로 CPU에 텍스처를 읽음
            req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            yield return waitUntilReqDone;

            if (req.hasError)
            {
              Debug.LogWarning($"Failed to read texture from the image source");
              continue;
            }
            image = textureFrame.BuildCPUImage();
            textureFrame.Release();
            break;
        }

        // PoseLandmarker의 실행 모드(IMAGE, VIDEO, LIVE_STREAM)에 따라 처리
        switch (taskApi.runningMode)
        {
          case Tasks.Vision.Core.RunningMode.IMAGE:
            // 단일 이미지에 대한 검출
            if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
            {
              _poseLandmarkerResultAnnotationController.DrawNow(result);
            }
            else
            {
              _poseLandmarkerResultAnnotationController.DrawNow(default);
            }
            DisposeAllMasks(result);
            break;
          case Tasks.Vision.Core.RunningMode.VIDEO:
            // 영상(프레임 단위) 검출
            if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result))
            {
              _poseLandmarkerResultAnnotationController.DrawNow(result);
            }
            else
            {
              _poseLandmarkerResultAnnotationController.DrawNow(default);
            }
            DisposeAllMasks(result);
            break;
          case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
            // 라이브 스트림 - 비동기 검출
            taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
            break;
        }
      }
    }

    /// 라이브 스트림 모드에서 랜드마크 검출 완료 시 호출되는 콜백 메서드
    private void OnPoseLandmarkDetectionOutput(PoseLandmarkerResult result, Image image, long timestamp)
    {
      // 비동기로 처리된 결과를 그려줌
      _poseLandmarkerResultAnnotationController.DrawLater(result);
      DisposeAllMasks(result);
    }

    /// 결과 객체에 포함된 세그멘테이션 마스크를 모두 Dispose 처리
    private void DisposeAllMasks(PoseLandmarkerResult result)
    {
      if (result.segmentationMasks != null)
      {
        foreach (var mask in result.segmentationMasks)
        {
          mask.Dispose();
        }
      }
    }
  }
}
