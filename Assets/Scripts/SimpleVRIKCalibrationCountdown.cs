using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using RootMotion.Demos;

// UI 없이 간단하게 3초 카운트다운 후 캘리브레이션을 실행하는 스크립트
public class SimpleVRIKCalibrationCountdown : MonoBehaviour
{
    [Header("References")]
    public VRIK vrik;
    public VRIKCalibrator.Settings calibrationSettings;
    
    [Header("Trackers")]
    public Transform headTracker;
    public Transform bodyTracker;
    public Transform leftHandTracker;
    public Transform rightHandTracker;
    public Transform leftFootTracker;
    public Transform rightFootTracker;
    
    [Header("Settings")]
    [Tooltip("캘리브레이션 전 카운트다운 시간")]
    public float countdownTime = 3f;
    
    [Tooltip("캘리브레이션 후 PlantFeet 비활성화")]
    public bool disablePlantFeetAfterCalibration = true;
    
    // 저장된 캘리브레이션 데이터
    private VRIKCalibrator.CalibrationData calibrationData;
    private bool isCalibrating = false;
    
    void Start()
    {
        // VRIK가 없으면 찾기
        if (vrik == null)
            vrik = GetComponent<VRIK>();
            
        // 캘리브레이션 설정이 없으면 기본값 사용
        if (calibrationSettings == null)
            calibrationSettings = new VRIKCalibrator.Settings();
    }
    
    void Update()
    {
        // C키로 캘리브레이션 시작
        if (Input.GetKeyDown(KeyCode.C) && !isCalibrating)
        {
            StartCoroutine(CalibrationWithCountdown());
        }
        
        // S키로 스케일만 재조정 (즉시)
        if (Input.GetKeyDown(KeyCode.S) && calibrationData != null && calibrationData.scale > 0f)
        {
            VRIKCalibrator.RecalibrateScale(vrik, calibrationData, calibrationSettings);
            Debug.Log("✅ 스케일 재조정 완료");
        }
        
        // R키로 즉시 캘리브레이션 (카운트다운 없이)
        if (Input.GetKeyDown(KeyCode.R) && !isCalibrating)
        {
            PerformCalibration();
        }
    }
    
    // 카운트다운과 함께 캘리브레이션 실행
    IEnumerator CalibrationWithCountdown()
    {
        isCalibrating = true;
        
        Debug.Log("⚠️ T-Pose 자세를 취하세요!");
        Debug.Log($"🔄 {countdownTime}초 후 캘리브레이션이 시작됩니다...");
        
        // 카운트다운
        float timeLeft = countdownTime;
        int lastSecond = Mathf.CeilToInt(timeLeft);
        
        while (timeLeft > 0)
        {
            int currentSecond = Mathf.CeilToInt(timeLeft);
            
            // 초가 바뀔 때마다 로그 출력
            if (currentSecond != lastSecond)
            {
                Debug.Log($"⏰ {currentSecond}...");
                lastSecond = currentSecond;
            }
            
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("🎯 캘리브레이션 시작!");
        
        // 캘리브레이션 실행
        PerformCalibration();
        
        isCalibrating = false;
    }
    
    // 실제 캘리브레이션 수행
    void PerformCalibration()
    {
        if (vrik == null)
        {
            Debug.LogError("❌ VRIK 컴포넌트를 찾을 수 없습니다!");
            return;
        }
        
        if (headTracker == null)
        {
            Debug.LogError("❌ Head Tracker가 설정되지 않았습니다!");
            return;
        }
        
        // 캘리브레이션 실행
        calibrationData = VRIKCalibrator.Calibrate(
            vrik,
            calibrationSettings,
            headTracker,
            bodyTracker,
            leftHandTracker,
            rightHandTracker,
            leftFootTracker,
            rightFootTracker
        );
        
        // 캘리브레이션 후 추가 설정
        if (disablePlantFeetAfterCalibration)
        {
            vrik.solver.plantFeet = false;
            Debug.Log("📌 PlantFeet 비활성화됨 (다리 굽힘 방지)");
        }
        
        // 결과 로그
        Debug.Log($"✅ VRIK 캘리브레이션 완료!");
        Debug.Log($"   - 스케일: {calibrationData.scale:F3}");
        Debug.Log($"   - Head Tracker: ✓");
        Debug.Log($"   - Body Tracker: {(bodyTracker != null ? "✓" : "✗")}");
        Debug.Log($"   - Hand Trackers: L={(leftHandTracker != null ? "✓" : "✗")} R={(rightHandTracker != null ? "✓" : "✗")}");
        Debug.Log($"   - Foot Trackers: L={(leftFootTracker != null ? "✓" : "✗")} R={(rightFootTracker != null ? "✓" : "✗")}");
        Debug.Log("💡 단축키: C=재캘리브레이션, S=스케일조정, R=즉시캘리브레이션");
    }
    
    // 외부에서 호출 가능한 메서드
    public void StartCalibration()
    {
        if (!isCalibrating)
            StartCoroutine(CalibrationWithCountdown());
    }
    
    public void CalibrateImmediately()
    {
        PerformCalibration();
    }
} 