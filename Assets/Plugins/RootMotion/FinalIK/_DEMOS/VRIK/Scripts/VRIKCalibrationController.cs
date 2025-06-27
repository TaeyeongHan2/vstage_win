using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos
{
    // VRIK 캘리브레이션을 제어하는 컨트롤러 클래스
    public class VRIKCalibrationController : MonoBehaviour
    {
        // 아바타의 VRIK 컴포넌트 참조
        [Tooltip("Reference to the VRIK component on the avatar.")] public VRIK ik;
        
        // VRIK 캘리브레이션 설정 값
        [Tooltip("The settings for VRIK calibration.")] public VRIKCalibrator.Settings settings;
        
        // HMD (머리 추적기)
        [Tooltip("The HMD.")] public Transform headTracker;
        
        // (선택) 몸통 트래커 - 골반 근처 벨트 영역에 위치하는 것이 좋음
        [Tooltip("(Optional) A tracker placed anywhere on the body of the player, preferrably close to the pelvis, on the belt area.")] public Transform bodyTracker;
        
        // (선택) 왼손 트래커 또는 컨트롤러
        [Tooltip("(Optional) A tracker or hand controller device placed anywhere on or in the player's left hand.")] public Transform leftHandTracker;
        
        // (선택) 오른손 트래커 또는 컨트롤러
        [Tooltip("(Optional) A tracker or hand controller device placed anywhere on or in the player's right hand.")] public Transform rightHandTracker;
        
        // (선택) 왼발 트래커 - 발목이나 발가락에 위치
        [Tooltip("(Optional) A tracker placed anywhere on the ankle or toes of the player's left leg.")] public Transform leftFootTracker;
        
        // (선택) 오른발 트래커 - 발목이나 발가락에 위치
        [Tooltip("(Optional) A tracker placed anywhere on the ankle or toes of the player's right leg.")] public Transform rightFootTracker;

        [Header("Data stored by Calibration")]
        // 캘리브레이션으로 저장되는 데이터
        public VRIKCalibrator.CalibrationData data = new VRIKCalibrator.CalibrationData();
        
        [Header("Countdown Settings")]
        // 카운트다운 사용 여부
        [Tooltip("캘리브레이션 전 카운트다운 사용 여부")]
        public bool useCountdown = true;
        
        // 카운트다운 시간
        [Tooltip("캘리브레이션 전 카운트다운 시간 (초)")]
        public float countdownTime = 3f;
        
        // 캘리브레이션 진행 중 플래그
        private bool isCalibrating = false;
        private Coroutine calibrationCoroutine;

        void LateUpdate()
        {
            // C 키를 누르면 캘리브레이션 실행
            if (Input.GetKeyDown(KeyCode.C) && !isCalibrating)
            {
                if (useCountdown)
                {
                    // 카운트다운과 함께 캘리브레이션
                    if (calibrationCoroutine != null) StopCoroutine(calibrationCoroutine);
                    calibrationCoroutine = StartCoroutine(CalibrateWithCountdown());
                }
                else
                {
                    // 즉시 캘리브레이션
                    PerformCalibration();
                }
            }

            /*
             * settings로 Calibrate를 호출하면 VRIKCalibrator.CalibrationData를 반환합니다.
             * 이 데이터는 다른 씬에서 동일한 캐릭터를 정확히 같은 방식으로 캘리브레이션하는 데 사용할 수 있습니다.
             * (settings 대신 data를 전달하면 됨) 캘리브레이션 시점의 플레이어 자세에 의존하지 않습니다.
             * 
             * 하지만 캘리브레이션 데이터는 여전히 본(bone) 방향에 의존하므로, 
             * 이 데이터는 캘리브레이션된 캐릭터 또는 동일한 본 구조를 가진 캐릭터에만 유효합니다.
             * 여러 캐릭터를 사용하려면 모두 한 번에 캘리브레이션하고 각각의 CalibrationData를 저장하는 것이 좋습니다.
             * */
            
            // D 키를 누르면 저장된 데이터로 재캘리브레이션
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (data.scale == 0f)
                {
                    Debug.LogError("No Calibration Data to calibrate to, please calibrate with settings first.");
                }
                else
                {
                    // 이전 캘리브레이션의 데이터를 사용하여 동일한 캐릭터를 다시 캘리브레이션
                    VRIKCalibrator.Calibrate(ik, data, headTracker, bodyTracker, leftHandTracker, rightHandTracker, leftFootTracker, rightFootTracker);
                }
            }

            // S 키를 누르면 아바타 스케일만 재캘리브레이션
            // 아바타가 이미 캘리브레이션된 경우에만 호출 가능
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (data.scale == 0f)
                {
                    Debug.LogError("Avatar needs to be calibrated before RecalibrateScale is called.");
                }
                VRIKCalibrator.RecalibrateScale(ik, data, settings);
            }
        }
        
        // 카운트다운과 함께 캘리브레이션 실행
        IEnumerator CalibrateWithCountdown()
        {
            isCalibrating = true;
            
            Debug.Log("T-Pose 자세를 취하세요!");
            Debug.Log($"{countdownTime}초 후 캘리브레이션이 시작됩니다...");
            
            // 카운트다운
            float timeLeft = countdownTime;
            int lastSecond = Mathf.CeilToInt(timeLeft);
            
            while (timeLeft > 0)
            {
                int currentSecond = Mathf.CeilToInt(timeLeft);
                
                // 초가 바뀔 때마다 로그 출력
                if (currentSecond != lastSecond)
                {
                    Debug.Log($"{currentSecond}...");
                    lastSecond = currentSecond;
                }
                
                timeLeft -= Time.deltaTime;
                yield return null;
            }
            
            Debug.Log("캘리브레이션 시작!");
            
            // 캘리브레이션 실행
            PerformCalibration();
            
            isCalibrating = false;
        }
        
        // 실제 캘리브레이션 수행
        void PerformCalibration()
        {
            // 캐릭터를 캘리브레이션하고, 캘리브레이션 데이터를 저장
            data = VRIKCalibrator.Calibrate(ik, settings, headTracker, bodyTracker, leftHandTracker, rightHandTracker, leftFootTracker, rightFootTracker);
            
            Debug.Log("VRIK 캘리브레이션 완료!");
        }
    }
}
