using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using RootMotion.FinalIK;
using RootMotion.Demos;

public class VRIKCalibrationWithCountdown : MonoBehaviour
{
    [Header("VRIK References")]
    public VRIKCalibrationController calibrationController; // 기존 캘리브레이션 컨트롤러
    
    [Header("Countdown Settings")]
    [Tooltip("캘리브레이션 시작 전 카운트다운 시간")]
    public float countdownTime = 3f;
    
    [Header("UI References (Optional)")]
    [Tooltip("카운트다운을 표시할 UI 텍스트")]
    public Text countdownText;
    
    [Tooltip("캘리브레이션 안내 메시지 텍스트")]
    public Text instructionText;
    
    [Header("Audio (Optional)")]
    [Tooltip("카운트다운 사운드")]
    public AudioSource countdownSound;
    
    [Tooltip("캘리브레이션 시작 사운드")]
    public AudioSource calibrationStartSound;
    
    // 캘리브레이션 진행 중 여부
    private bool isCalibrating = false;
    private Coroutine calibrationCoroutine;
    
    void Start()
    {
        // 캘리브레이션 컨트롤러가 없으면 찾기
        if (calibrationController == null)
        {
            calibrationController = GetComponent<VRIKCalibrationController>();
            if (calibrationController == null)
            {
                calibrationController = FindObjectOfType<VRIKCalibrationController>();
            }
        }
        
        // UI 텍스트 초기화
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (instructionText != null) instructionText.text = "C키를 눌러 캘리브레이션 시작";
    }
    
    void Update()
    {
        // C키를 누르면 카운트다운 시작
        if (Input.GetKeyDown(KeyCode.C) && !isCalibrating)
        {
            StartCalibrationCountdown();
        }
        
        // ESC키로 캘리브레이션 취소
        if (Input.GetKeyDown(KeyCode.Escape) && isCalibrating)
        {
            CancelCalibration();
        }
        
        // S키로 스케일만 재조정 (카운트다운 없이)
        if (Input.GetKeyDown(KeyCode.S) && calibrationController.data.scale > 0f)
        {
            VRIKCalibrator.RecalibrateScale(
                calibrationController.ik, 
                calibrationController.data, 
                calibrationController.settings
            );
            Debug.Log("스케일 재조정 완료");
        }
    }
    
    // 캘리브레이션 카운트다운 시작
    public void StartCalibrationCountdown()
    {
        if (calibrationCoroutine != null)
        {
            StopCoroutine(calibrationCoroutine);
        }
        calibrationCoroutine = StartCoroutine(CalibrationCountdownCoroutine());
    }
    
    // 캘리브레이션 카운트다운 코루틴
    IEnumerator CalibrationCountdownCoroutine()
    {
        isCalibrating = true;
        
        // 안내 메시지 표시
        if (instructionText != null)
        {
            instructionText.text = "T-Pose 자세를 취하세요!";
        }
        
        // 카운트다운 UI 활성화
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }
        
        // 카운트다운 진행
        float timeLeft = countdownTime;
        while (timeLeft > 0)
        {
            // UI 업데이트
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(timeLeft).ToString();
                
                // 마지막 1초는 크게 표시
                if (timeLeft <= 1f)
                {
                    countdownText.fontSize = 100;
                    countdownText.color = Color.red;
                }
                else
                {
                    countdownText.fontSize = 72;
                    countdownText.color = Color.white;
                }
            }
            
            // 카운트다운 사운드 재생
            if (countdownSound != null && Mathf.Ceil(timeLeft) != Mathf.Ceil(timeLeft - Time.deltaTime))
            {
                countdownSound.Play();
            }
            
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        
        // 캘리브레이션 실행
        if (countdownText != null)
        {
            countdownText.text = "캘리브레이션!";
            countdownText.color = Color.green;
        }
        
        // 캘리브레이션 시작 사운드
        if (calibrationStartSound != null)
        {
            calibrationStartSound.Play();
        }
        
        // 실제 캘리브레이션 실행
        PerformCalibration();
        
        // 2초 후 UI 정리
        yield return new WaitForSeconds(2f);
        
        // UI 숨기기
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
            countdownText.fontSize = 72;
            countdownText.color = Color.white;
        }
        
        if (instructionText != null)
        {
            instructionText.text = "캘리브레이션 완료! (C: 재시작, S: 스케일 조정)";
        }
        
        isCalibrating = false;
    }
    
    // 실제 캘리브레이션 수행
    void PerformCalibration()
    {
        if (calibrationController == null)
        {
            Debug.LogError("VRIKCalibrationController를 찾을 수 없습니다!");
            return;
        }
        
        // 캘리브레이션 실행
        calibrationController.data = VRIKCalibrator.Calibrate(
            calibrationController.ik,
            calibrationController.settings,
            calibrationController.headTracker,
            calibrationController.bodyTracker,
            calibrationController.leftHandTracker,
            calibrationController.rightHandTracker,
            calibrationController.leftFootTracker,
            calibrationController.rightFootTracker
        );
        
        Debug.Log("VRIK 캘리브레이션 완료!");
        
        // 캘리브레이션 후 추가 설정 (선택사항)
        ApplyPostCalibrationSettings();
    }
    
    // 캘리브레이션 후 추가 설정
    void ApplyPostCalibrationSettings()
    {
        var vrik = calibrationController.ik;
        
        // PlantFeet 비활성화 (다리 굽힘 문제 방지)
        vrik.solver.plantFeet = false;
        
        // Locomotion 설정
        if (calibrationController.leftFootTracker == null && calibrationController.rightFootTracker == null)
        {
            vrik.solver.locomotion.weight = 1f;
        }
        else
        {
            vrik.solver.locomotion.weight = 0f;
        }
    }
    
    // 캘리브레이션 취소
    void CancelCalibration()
    {
        if (calibrationCoroutine != null)
        {
            StopCoroutine(calibrationCoroutine);
        }
        
        isCalibrating = false;
        
        // UI 정리
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        if (instructionText != null)
        {
            instructionText.text = "캘리브레이션 취소됨 (C키로 다시 시작)";
        }
        
        Debug.Log("캘리브레이션이 취소되었습니다.");
    }
    
    // 외부에서 호출 가능한 캘리브레이션 메서드
    public void StartCalibration(float customCountdown = -1f)
    {
        if (customCountdown > 0)
        {
            countdownTime = customCountdown;
        }
        StartCalibrationCountdown();
    }
} 