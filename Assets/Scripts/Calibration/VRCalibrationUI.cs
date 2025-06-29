using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RootMotion.Demos;

namespace RootMotion.Demos
{
    // VR 캘리브레이션을 위한 UI 컨트롤러
    // 버튼으로 캘리브레이션 시작, 상태 표시, 진행상황 안내
    public class VRCalibrationUI : MonoBehaviour
    {
        [Header("캘리브레이션 시스템 참조")]
        [Tooltip("완전 자동 캘리브레이션 시스템")]
        public FullyAutomatedVRCalibration fullCalibration;
        
        [Header("UI 요소들")]
        [Tooltip("메인 캔버스")]
        public Canvas mainCanvas;
        
        [Tooltip("완전 자동 캘리브레이션 시작 버튼")]
        public Button startFullCalibrationButton;

        [Header("상태 표시")]
        [Tooltip("상태 텍스트")]
        public TextMeshProUGUI statusText;
        
        [Tooltip("진행상황 텍스트")]
        public TextMeshProUGUI progressText;
        
        [Tooltip("카운트다운 텍스트")]
        public TextMeshProUGUI countdownText;
        
        [Tooltip("측정 결과 텍스트")]
        public TextMeshProUGUI resultsText;
        
        [Tooltip("안내 텍스트")]
        public TextMeshProUGUI instructionText;
        
        // 상태 색상
        private Color readyColor = Color.yellow;
        private Color progressColor = Color.blue;
        private Color successColor = Color.green;
        private Color errorColor = Color.red;
        private Color countdownColor = Color.orange;

        void Start()
        {
            InitializeUI();
            SetupButtonListeners();
        }

        void Update()
        {
            UpdateUIState();
        }

        // UI 초기화
        void InitializeUI()
        {
            // 캔버스가 World Space로 설정되어 있는지 확인
            if (mainCanvas != null && mainCanvas.renderMode != RenderMode.WorldSpace)
            {
                mainCanvas.renderMode = RenderMode.WorldSpace;
                mainCanvas.worldCamera = Camera.main;
            }
        }

        // 버튼 이벤트 연결
        void SetupButtonListeners()
        {
            if (startFullCalibrationButton != null)
                startFullCalibrationButton.onClick.AddListener(OnStartFullCalibration);
        }

        // UI 상태 업데이트
        void UpdateUIState()
        {
            if (fullCalibration != null)
            {
                UpdateFullCalibrationUI();
            }
        }

        // 완전 자동 캘리브레이션 UI 업데이트
        void UpdateFullCalibrationUI()
        {
            var state = fullCalibration.currentState;
            var isInProgress = fullCalibration.calibrationInProgress;
            var countdownTimer = fullCalibration.countdownTimer;

            // 상태별 UI 표시
            switch (state)
            {
                case FullyAutomatedVRCalibration.CalibrationState.Ready:
                    if (!isInProgress)
                    {
                        UpdateStatusText("준비 완료", readyColor);
                        UpdateInstructionText("완전 자동 캘리브레이션을 시작하거나 신체 측정만 할 수 있습니다.");
                    }
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.Countdown:
                    UpdateCountdownText(countdownTimer.ToString());
                    UpdateStatusText("시작 준비 중", countdownColor);
                    UpdateInstructionText("T-포즈를 준비하세요!\n• 팔을 양옆으로 수평하게\n• 똑바로 서기\n• 발은 어깨너비로");
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.DetectingFloor:
                    UpdateStatusText("바닥 높이 감지 중", progressColor);
                    UpdateProgressText("1/6 단계: 바닥 높이 자동 감지");
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.MeasuringUser:
                    UpdateStatusText("사용자 측정 중", progressColor);
                    UpdateProgressText("3/6 단계: 사용자 신체 측정");
                    UpdateInstructionText("T-포즈를 유지하세요! 움직이지 마세요.");
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.CalculatingAvatar:
                    UpdateStatusText("아바타 분석 중", progressColor);
                    UpdateProgressText("4/6 단계: 아바타 측정값 계산");
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.ComputingScale:
                    UpdateStatusText("스케일 계산 중", progressColor);
                    UpdateProgressText("5/6 단계: 최적 비율 계산");
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.ApplyingCalibration:
                    UpdateStatusText("캘리브레이션 적용 중", progressColor);
                    UpdateProgressText("6/6 단계: VRIK 타겟 위치 조정");
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.Completed:
                    UpdateStatusText("완료!", successColor);
                    UpdateResultsText();
                    UpdateInstructionText("캘리브레이션이 완료되었습니다!\n🔒 아바타 크기는 원본 유지됩니다.\n🎯 VRIK 타겟 위치만 미세조정 가능합니다.");
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.Error:
                    UpdateStatusText("오류 발생", errorColor);
                    UpdateInstructionText("오류가 발생했습니다. 다시 시도해주세요.");
                    break;
            }
        }

        // 텍스트 업데이트 함수들
        void UpdateStatusText(string text, Color color)
        {
            if (statusText != null)
            {
                statusText.text = text;
                statusText.color = color;
            }
        }

        void UpdateProgressText(string text)
        {
            if (progressText != null)
                progressText.text = text;
        }

        void UpdateCountdownText(string text)
        {
            if (countdownText != null)
                countdownText.text = text;
        }

        void UpdateInstructionText(string text)
        {
            if (instructionText != null)
                instructionText.text = text;
        }

        void UpdateResultsText()
        {
            if (resultsText != null && fullCalibration != null)
            {
                string results = $"=== 캘리브레이션 결과 ===\n";
                results += $"사용자 키: {fullCalibration.userHeight:F2}m\n";
                results += $"사용자 팔길이: {fullCalibration.userArmLength:F2}m\n";
                results += $"측정 정확도: {fullCalibration.measurementAccuracy:F1}%\n";
                results += $"계산된 비율: {fullCalibration.finalScale:F3}\n";
                resultsText.text = results;
            }
        }

        // 버튼 이벤트 핸들러들
        public void OnStartFullCalibration()
        {
            if (fullCalibration != null)
            {
                fullCalibration.StartFullAutoCalibration();
                Debug.Log("UI: 완전 자동 캘리브레이션 시작");
            }
        }

        // 공개 필드들을 접근 가능하게 만들기 위한 프로퍼티들
        public FullyAutomatedVRCalibration.CalibrationState CurrentCalibrationState
        {
            get { return fullCalibration != null ? fullCalibration.currentState : FullyAutomatedVRCalibration.CalibrationState.Ready; }
        }
    }
} 