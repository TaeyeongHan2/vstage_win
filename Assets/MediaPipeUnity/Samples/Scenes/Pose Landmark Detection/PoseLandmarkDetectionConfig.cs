// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.ComponentModel;
using Mediapipe.Tasks.Vision.PoseLandmarker;

namespace Mediapipe.Unity.Sample.PoseLandmarkDetection
{
  /// 포즈 검출 모델 종류를 구분하는 열거형(enum)입니다.
  public enum ModelType : int
  {
    /// 경량 버전의 포즈 랜드마커 타입
    [Description("Pose landmarker (lite)")]
    BlazePoseLite = 0,
    /// 기본(Full) 버전의 포즈 랜드마커 타입
    [Description("Pose landmarker (Full)")]
    BlazePoseFull = 1,
    /// 무거운(Heavy) 버전의 포즈 랜드마커 타입
    [Description("Pose landmarker (Heavy)")]
    BlazePoseHeavy = 2,
  }
  /// 포즈 랜드마크 인식 관련 설정을 담는 클래스입니다.
  public class PoseLandmarkDetectionConfig
  {
    /// 모델 구동 방식을 지정합니다. CPU/GPU 등 플랫폼 상황에 따라 달라질 수 있습니다.
    public Tasks.Core.BaseOptions.Delegate Delegate { get; set; } =
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
      Tasks.Core.BaseOptions.Delegate.CPU;
#else
    Tasks.Core.BaseOptions.Delegate.GPU;
#endif
    /// 영상을 어떤 방식으로 읽어오는지 설정. CPU 비동기 모드가 기본값
    public ImageReadMode ImageReadMode { get; set; } = ImageReadMode.CPUAsync;
    /// 사용할 모델 타입(라이트, 풀, 헤비)
    public ModelType Model { get; set; } = ModelType.BlazePoseHeavy;
    /// 라이브 스트림 등 동작 방식을 정의
    public Tasks.Vision.Core.RunningMode RunningMode { get; set; } = Tasks.Vision.Core.RunningMode.LIVE_STREAM;
    /// 감지할 포즈의 최대 개수
    public int NumPoses { get; set; } = 1;
    /// 포즈를 감지할 때 필요한 최소 신뢰도
    public float MinPoseDetectionConfidence { get; set; } = 0.5f;
    /// 포즈가 존재한다고 판단하기 위한 최소 신뢰도
    public float MinPosePresenceConfidence { get; set; } = 0.5f;
    /// 추적 성공으로 간주하기 위한 최소 신뢰도
    public float MinTrackingConfidence { get; set; } = 0.5f;
    /// 세그멘테이션 마스크 이미지를 출력할지 여부
    public bool OutputSegmentationMasks { get; set; } = false;
    /// 모델의 Display용 이름(Description 혹은 해당 enum 값) 가져오기
    public string ModelName => Model.GetDescription() ?? Model.ToString();
    /// 현재 설정된 모델 타입에 대응하는 실제 모델 파일 경로를 반환
    public string ModelPath
    {
      get
      {
        switch (Model)
        {
          case ModelType.BlazePoseLite:
            return "pose_landmarker_lite.bytes";
          case ModelType.BlazePoseFull:
            return "pose_landmarker_full.bytes";
          case ModelType.BlazePoseHeavy:
            return "pose_landmarker_heavy.bytes";
          default:
            return null;
        }
      }
    }
    ///PoseLandmarkerOptions 인스턴스를 생성하여 반환
    public PoseLandmarkerOptions GetPoseLandmarkerOptions(PoseLandmarkerOptions.ResultCallback resultCallback = null)
    {
      return new PoseLandmarkerOptions(
        new Tasks.Core.BaseOptions(Delegate, modelAssetPath: ModelPath),
        runningMode: RunningMode,
        numPoses: NumPoses,
        minPoseDetectionConfidence: MinPoseDetectionConfidence,
        minPosePresenceConfidence: MinPosePresenceConfidence,
        minTrackingConfidence: MinTrackingConfidence,
        outputSegmentationMasks: OutputSegmentationMasks,
        resultCallback: resultCallback
      );
    }
  }
}
