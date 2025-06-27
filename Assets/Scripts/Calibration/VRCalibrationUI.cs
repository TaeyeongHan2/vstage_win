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
        
        [Tooltip("신체 측정 시스템")]
        public VRBodyMeasurementSystem bodyMeasurement;

        [Header("UI 요소들")]
        [Tooltip("메인 캔버스")]
        public Canvas mainCanvas;
        
        [Tooltip("완전 자동 캘리브레이션 시작 버튼")]
        public Button startFullCalibrationButton;
        
        [Tooltip("신체 측정만 시작 버튼")]
        public Button startMeasurementButton;
        
        [Tooltip("중단 버튼")]
        public Button stopButton;
        
        [Tooltip("리셋 버튼")]
        public Button resetButton;
        
        [Tooltip("스케일 증가 버튼")]
        public Button scaleUpButton;
        
        [Tooltip("스케일 감소 버튼")]
        public Button scaleDownButton;

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

        [Header("패널들")]
        [Tooltip("메인 메뉴 패널")]
        public GameObject mainMenuPanel;
        
        [Tooltip("진행 중 패널")]
        public GameObject progressPanel;
        
        [Tooltip("결과 패널")]
        public GameObject resultsPanel;
        
        [Tooltip("카운트다운 패널")]
        public GameObject countdownPanel;

        [Header("시각적 피드백")]
        [Tooltip("진행률 바")]
        public Slider progressBar;
        
        [Tooltip("상태에 따른 색상")]
        public Image statusColorIndicator;

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
            ShowMainMenu();
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

            // 초기 상태 설정
            if (progressBar != null)
                progressBar.value = 0f;
        }

        // 버튼 이벤트 연결
        void SetupButtonListeners()
        {
            if (startFullCalibrationButton != null)
                startFullCalibrationButton.onClick.AddListener(OnStartFullCalibration);
            
            if (startMeasurementButton != null)
                startMeasurementButton.onClick.AddListener(OnStartMeasurement);
            
            if (stopButton != null)
                stopButton.onClick.AddListener(OnStop);
            
            if (resetButton != null)
                resetButton.onClick.AddListener(OnReset);
            
            if (scaleUpButton != null)
                scaleUpButton.onClick.AddListener(OnScaleUp);
            
            if (scaleDownButton != null)
                scaleDownButton.onClick.AddListener(OnScaleDown);
        }

        // UI 상태 업데이트
        void UpdateUIState()
        {
            if (fullCalibration != null)
            {
                UpdateFullCalibrationUI();
            }
            else if (bodyMeasurement != null)
            {
                UpdateMeasurementUI();
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
                        ShowMainMenu();
                        UpdateStatusText("준비 완료", readyColor);
                        UpdateInstructionText("완전 자동 캘리브레이션을 시작하거나 신체 측정만 할 수 있습니다.");
                    }
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.Countdown:
                    ShowCountdown();
                    UpdateCountdownText(countdownTimer.ToString());
                    UpdateStatusText("시작 준비 중", countdownColor);
                    UpdateInstructionText("T-포즈를 준비하세요!\n• 팔을 양옆으로 수평하게\n• 똑바로 서기\n• 발은 어깨너비로");
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.DetectingFloor:
                    ShowProgress();
                    UpdateStatusText("바닥 높이 감지 중", progressColor);
                    UpdateProgressText("1/6 단계: 바닥 높이 자동 감지");
                    UpdateProgressBar(1, 6);
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.MeasuringUser:
                    ShowProgress();
                    UpdateStatusText("사용자 측정 중", progressColor);
                    UpdateProgressText("3/6 단계: 사용자 신체 측정");
                    UpdateInstructionText("T-포즈를 유지하세요! 움직이지 마세요.");
                    UpdateProgressBar(3, 6);
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.CalculatingAvatar:
                    ShowProgress();
                    UpdateStatusText("아바타 분석 중", progressColor);
                    UpdateProgressText("4/6 단계: 아바타 측정값 계산");
                    UpdateProgressBar(4, 6);
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.ComputingScale:
                    ShowProgress();
                    UpdateStatusText("스케일 계산 중", progressColor);
                    UpdateProgressText("5/6 단계: 최적 비율 계산");
                    UpdateProgressBar(5, 6);
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.ApplyingCalibration:
                    ShowProgress();
                    UpdateStatusText("캘리브레이션 적용 중", progressColor);
                    UpdateProgressText("6/6 단계: VRIK 타겟 위치 조정");
                    UpdateProgressBar(6, 6);
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.Completed:
                    ShowResults();
                    UpdateStatusText("완료!", successColor);
                    UpdateResultsText();
                    UpdateInstructionText("캘리브레이션이 완료되었습니다!\n🔒 아바타 크기는 원본 유지됩니다.\n🎯 VRIK 타겟 위치만 미세조정 가능합니다.");
                    break;

                case FullyAutomatedVRCalibration.CalibrationState.Error:
                    ShowMainMenu();
                    UpdateStatusText("오류 발생", errorColor);
                    UpdateInstructionText("오류가 발생했습니다. 다시 시도해주세요.");
                    break;
            }
        }

        // 신체 측정 UI 업데이트
        void UpdateMeasurementUI()
        {
            if (bodyMeasurement.isMeasuring)
            {
                ShowProgress();
                UpdateStatusText("신체 측정 중", progressColor);
                UpdateProgressText("T-포즈 측정 진행 중...");
                UpdateInstructionText("T-포즈를 유지하세요!");
            }
            else if (bodyMeasurement.measurementComplete)
            {
                ShowResults();
                UpdateStatusText("측정 완료", successColor);
                UpdateMeasurementResults();
            }
            else
            {
                ShowMainMenu();
                UpdateStatusText("측정 준비", readyColor);
                UpdateInstructionText("신체 측정을 시작할 수 있습니다.");
            }
        }

        // 패널 표시 함수들
        void ShowMainMenu()
        {
            SetPanelActive(mainMenuPanel, true);
            SetPanelActive(progressPanel, false);
            SetPanelActive(resultsPanel, false);
            SetPanelActive(countdownPanel, false);
        }

        void ShowProgress()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(progressPanel, true);
            SetPanelActive(resultsPanel, false);
            SetPanelActive(countdownPanel, false);
        }

        void ShowResults()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(progressPanel, false);
            SetPanelActive(resultsPanel, true);
            SetPanelActive(countdownPanel, false);
        }

        void ShowCountdown()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(progressPanel, false);
            SetPanelActive(resultsPanel, false);
            SetPanelActive(countdownPanel, true);
        }

        void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        // 텍스트 업데이트 함수들
        void UpdateStatusText(string text, Color color)
        {
            if (statusText != null)
            {
                statusText.text = text;
                statusText.color = color;
            }
            
            if (statusColorIndicator != null)
                statusColorIndicator.color = color;
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

        void UpdateProgressBar(int current, int total)
        {
            if (progressBar != null)
                progressBar.value = (float)current / total;
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
                results += $"🔒 아바타 크기: 원본 유지\n";
                results += $"🎯 VRIK 조정: {fullCalibration.scaleAdjustment:F3}";
                
                resultsText.text = results;
            }
        }

        void UpdateMeasurementResults()
        {
            if (resultsText != null && bodyMeasurement != null)
            {
                var data = bodyMeasurement.GetMeasuredBodyData();
                string results = $"=== 신체 측정 결과 ===\n";
                results += $"키: {data.height:F2}m\n";
                results += $"팔길이: {data.armLength:F2}m\n";
                results += $"다리길이: {data.legLength:F2}m\n";
                results += $"어깨너비: {data.shoulderWidth:F2}m\n";
                results += $"측정 정확도: {data.accuracy:F1}%";
                
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

        public void OnStartMeasurement()
        {
            if (bodyMeasurement != null)
            {
                bodyMeasurement.StartAutoMeasurement();
                Debug.Log("UI: 신체 측정 시작");
            }
        }

        public void OnStop()
        {
            if (fullCalibration != null)
            {
                fullCalibration.StopCalibration();
            }
            
            if (bodyMeasurement != null)
            {
                bodyMeasurement.StopMeasurement();
            }
            
            ShowMainMenu();
            Debug.Log("UI: 작업 중단");
        }

        public void OnReset()
        {
            if (fullCalibration != null)
            {
                fullCalibration.ResetCalibration();
            }
            
            ShowMainMenu();
            Debug.Log("UI: 시스템 리셋");
        }

        public void OnScaleUp()
        {
            if (fullCalibration != null)
            {
                fullCalibration.scaleAdjustment += 0.02f;
                fullCalibration.ApplyScaleAdjustment();
                UpdateResultsText();
                Debug.Log($"UI: VRIK 타겟 위치 조정 증가 - {fullCalibration.scaleAdjustment:F3} (아바타 크기는 변경 안됨)");
            }
        }

        public void OnScaleDown()
        {
            if (fullCalibration != null)
            {
                fullCalibration.scaleAdjustment -= 0.02f;
                fullCalibration.ApplyScaleAdjustment();
                UpdateResultsText();
                Debug.Log($"UI: VRIK 타겟 위치 조정 감소 - {fullCalibration.scaleAdjustment:F3} (아바타 크기는 변경 안됨)");
            }
        }

        // 공개 필드들을 접근 가능하게 만들기 위한 프로퍼티들
        public FullyAutomatedVRCalibration.CalibrationState CurrentCalibrationState
        {
            get { return fullCalibration != null ? fullCalibration.currentState : FullyAutomatedVRCalibration.CalibrationState.Ready; }
        }

        public bool IsCalibrationInProgress
        {
            get { return fullCalibration != null ? fullCalibration.calibrationInProgress : false; }
        }

        public bool IsMeasurementInProgress
        {
            get { return bodyMeasurement != null ? bodyMeasurement.isMeasuring : false; }
        }
    }
} 