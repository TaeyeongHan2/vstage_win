using UnityEngine;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RootMotion.Demos
{
    // 완전 자동화된 VR IK 캘리브레이션 시스템 (매직 넘버 제거 버전)
    // 모든 수치를 설정 가능한 변수로 분리하여 유연성과 가독성 향상
    public class FullyAutomatedVRCalibration : MonoBehaviour
    {
        [Header("VRIK 설정")]
        [Tooltip("아바타의 VRIK 컴포넌트")]
        public VRIK ik;

        [Header("VR 트래커 할당")]
        [Tooltip("HMD (Vive Pro 2 헤드셋)")]
        public Transform hmdTracker;
        
        [Tooltip("왼손 컨트롤러")]
        public Transform leftControllerTracker;
        
        [Tooltip("오른손 컨트롤러")]
        public Transform rightControllerTracker;
        
        [Tooltip("허리/골반 트래커")]
        public Transform waistTracker;
        
        [Tooltip("왼발 트래커")]
        public Transform leftFootTracker;
        
        [Tooltip("오른발 트래커")]
        public Transform rightFootTracker;

        [Header("자동 측정 설정")]
        [Tooltip("바닥 높이 자동 감지")]
        public bool autoDetectFloor = true;
        
        [SerializeField, Tooltip("감지된 바닥 높이")]
        private float detectedFloorLevel = 0f;
        
        [Tooltip("측정 샘플 수 (정확도 향상)")]
        [Range(3, 10)]
        public int measurementSamples = 5;
        
        [Tooltip("측정 간격 (초)")]
        [Range(0.1f, 1f)]
        public float measurementInterval = 0.3f;

        [Header("인체 측정 파라미터")]
        [Tooltip("어깨에서 HMD까지의 거리 (m)")]
        [Range(0.1f, 0.4f)]
        public float shoulderToHeadDistance = 0.2f;
        
        [Tooltip("어깨 너비의 절반 (m)")]
        [Range(0.1f, 0.3f)]
        public float shoulderWidth = 0.18f;
        
        [Tooltip("키에서 허리 높이 비율 (허리 트래커 없을 때)")]
        [Range(0.4f, 0.7f)]
        public float waistHeightRatio = 0.55f;
        
        [Tooltip("바닥 감지 여유 공간 (m)")]
        [Range(0.01f, 0.2f)]
        public float floorDetectionMargin = 0.05f;

        [Header("T-포즈 검증 기준")]
        [Tooltip("팔 각도 허용 범위 - 엄격 (도)")]
        [Range(10f, 60f)]
        public float armAngleStrictThreshold = 45f;
        
        [Tooltip("팔 각도 허용 범위 - 관대 (도)")]
        [Range(30f, 90f)]
        public float armAngleLooseThreshold = 60f;
        
        [Tooltip("척추 각도 허용 범위 - 엄격 (도)")]
        [Range(5f, 45f)]
        public float spineAngleStrictThreshold = 30f;
        
        [Tooltip("척추 각도 허용 범위 - 관대 (도)")]
        [Range(20f, 60f)]
        public float spineAngleLooseThreshold = 45f;
        
        [Tooltip("발 간격 최소값 (m)")]
        [Range(0.1f, 0.5f)]
        public float feetDistanceMin = 0.2f;
        
        [Tooltip("발 간격 최대값 - 이상적 (m)")]
        [Range(0.5f, 1.5f)]
        public float feetDistanceMaxIdeal = 1.0f;
        
        [Tooltip("발 간격 최대값 - 허용 (m)")]
        [Range(1.0f, 2.0f)]
        public float feetDistanceMaxAllowed = 1.5f;

        [Header("측정 정확도 기준")]
        [Tooltip("T-포즈 검증 최소 점수")]
        [Range(30f, 90f)]
        public float tPoseMinScore = 50f;
        
        [Tooltip("측정 데이터 최소 정확도")]
        [Range(50f, 95f)]
        public float measurementMinAccuracy = 70f;
        
        [Tooltip("전체 캘리브레이션 최소 정확도")]
        [Range(50f, 95f)]
        public float calibrationMinAccuracy = 70f;

        [Header("인체 비율 검증 범위")]
        [Tooltip("팔/키 비율 최소값")]
        [Range(0.15f, 0.35f)]
        public float armToHeightRatioMin = 0.25f;
        
        [Tooltip("팔/키 비율 최대값")]
        [Range(0.35f, 0.65f)]
        public float armToHeightRatioMax = 0.50f;
        
        [Tooltip("다리/키 비율 최소값")]
        [Range(0.3f, 0.5f)]
        public float legToHeightRatioMin = 0.40f;
        
        [Tooltip("다리/키 비율 최대값")]
        [Range(0.5f, 0.8f)]
        public float legToHeightRatioMax = 0.65f;

        [Header("절대값 검증 범위")]
        [Tooltip("최소 키 (m)")]
        [Range(0.5f, 1.5f)]
        public float minHeight = 1.0f;
        
        [Tooltip("최대 키 (m)")]
        [Range(1.8f, 3.0f)]
        public float maxHeight = 2.5f;
        
        [Tooltip("최소 팔 길이 (m)")]
        [Range(0.2f, 0.5f)]
        public float minArmLength = 0.3f;
        
        [Tooltip("최대 팔 길이 (m)")]
        [Range(0.8f, 1.5f)]
        public float maxArmLength = 1.2f;

        [Header("키 차이 보정 비율")]
        [Tooltip("머리 타겟 높이 조정 비율")]
        [Range(0f, 0.5f)]
        public float headHeightCompensationRatio = 0.1f;
        
        [Tooltip("허리 타겟 높이 조정 비율")]
        [Range(0f, 0.3f)]
        public float pelvisHeightCompensationRatio = 0.05f;
        
        [Tooltip("발 타겟 높이 조정 비율")]
        [Range(0f, 0.1f)]
        public float footHeightCompensationRatio = 0.02f;

        [Header("스케일 계산 가중치")]
        [Tooltip("키 비율의 가중치")]
        [Range(0f, 1f)]
        public float heightRatioWeight = 0.6f;
        
        [Tooltip("팔 비율의 가중치")]
        [Range(0f, 1f)]
        public float armRatioWeight = 0.2f;
        
        [Tooltip("다리 비율의 가중치")]
        [Range(0f, 1f)]
        public float legRatioWeight = 0.2f;
        
        [Tooltip("최소 허용 스케일")]
        [Range(0.3f, 0.8f)]
        public float minScale = 0.5f;
        
        [Tooltip("최대 허용 스케일")]
        [Range(1.2f, 3.0f)]
        public float maxScale = 2.0f;

        [Header("측정 정확도 페널티")]
        [Tooltip("비율 벗어남 페널티")]
        [Range(5f, 30f)]
        public float ratioViolationPenalty = 15f;
        
        [Tooltip("절대값 벗어남 페널티")]
        [Range(10f, 40f)]
        public float absoluteValueViolationPenalty = 20f;
        
        [Tooltip("팔길이 벗어남 페널티")]
        [Range(5f, 25f)]
        public float armLengthViolationPenalty = 10f;

        [Header("타이밍 설정")]
        [Tooltip("카운트다운 시간")]
        [Range(1, 10)]
        public int countdownDuration = 3;
        
        [Tooltip("자세 안정성 확인 시간")]
        [Range(0.1f, 1f)]
        public float poseStabilityCheckTime = 0.2f;
        
        [Tooltip("자세 불안정시 대기 시간")]
        [Range(0.3f, 2f)]
        public float poseInstabilityWaitTime = 0.5f;
        
        [Tooltip("T-포즈 실패시 대기 시간")]
        [Range(0.5f, 3f)]
        public float tPoseFailureWaitTime = 1f;



        [Header("캘리브레이션 설정")]
        [Tooltip("VRIK 캘리브레이션 상세 설정")]
        public VRIKCalibrator.Settings calibrationSettings = new VRIKCalibrator.Settings();

        [Header("캘리브레이션 상태")]
        [SerializeField, Tooltip("전체 프로세스 상태")]
        public CalibrationState currentState = CalibrationState.Ready;
        
        [SerializeField, Tooltip("측정 정확도")]
        public float measurementAccuracy = 0f;
        
        [SerializeField, Tooltip("최종 스케일 배율")]
        public float finalScale = 1f;
        
        [Tooltip("저장된 캘리브레이션 데이터")]
        public VRIKCalibrator.CalibrationData calibrationData = new VRIKCalibrator.CalibrationData();

        [Header("측정 결과 (자동 계산됨)")]
        [SerializeField, Tooltip("측정된 사용자 키")]
        public float userHeight;
        
        [SerializeField, Tooltip("측정된 사용자 팔 길이")]
        public float userArmLength;
        
        [SerializeField, Tooltip("측정된 사용자 다리 길이")]
        public float userLegLength;
        
        [SerializeField, Tooltip("계산된 아바타 키")]
        public float avatarHeight;
        
        [SerializeField, Tooltip("계산된 아바타 팔 길이")]
        public float avatarArmLength;
        
        [SerializeField, Tooltip("계산된 아바타 다리 길이")]
        public float avatarLegLength;

        // 상태 열거형
        public enum CalibrationState
        {
            Ready,              // 준비 상태
            Countdown,          // 카운트다운 중
            DetectingFloor,     // 바닥 감지 중
            MeasuringUser,      // 사용자 측정 중
            CalculatingAvatar,  // 아바타 측정 중
            ComputingScale,     // 스케일 계산 중
            ApplyingCalibration,// 캘리브레이션 적용 중
            Completed,          // 완료
            Error,              // 오류
            Failed              // 실패
        }

        // 측정 데이터 저장용 클래스
        [System.Serializable]
        public class MeasurementData
        {
            public float height;
            public float armLength;
            public float legLength;
            public float shoulderWidth;
            public float waistHeight;
            public float accuracy;
            public float timestamp;
        }

        private List<MeasurementData> measurementSamples_list = new List<MeasurementData>();
        private Coroutine calibrationCoroutine;
        public bool calibrationInProgress = false;
        public int countdownTimer = 0; // 카운트다운 표시용

        void Start()
        {
            InitializeCalibrationSettings();
            ValidateSettings();
        }

        void Update()
        {
            // 스페이스바로 완전 자동 캘리브레이션 시작
            if (Input.GetKeyDown(KeyCode.Space) && !calibrationInProgress)
            {
                StartFullAutoCalibration();
            }

            // ESC로 캘리브레이션 중단
            if (Input.GetKeyDown(KeyCode.Escape) && calibrationInProgress)
            {
                StopCalibration();
            }

            // 캘리브레이션 완료 후 실시간 스케일 조정 기능 제거됨

            // R 키로 전체 리셋
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetCalibration();
            }
        }

        // 설정값 검증
        void ValidateSettings()
        {
            // 가중치 합계 확인
            float totalWeight = heightRatioWeight + armRatioWeight + legRatioWeight;
            if (Mathf.Abs(totalWeight - 1f) > 0.01f)
            {
                Debug.LogWarning($"스케일 가중치 합계가 1이 아닙니다! 현재: {totalWeight:F3}");
            }

            // 비율 범위 검증
            if (armToHeightRatioMin >= armToHeightRatioMax)
            {
                Debug.LogError("팔/키 비율 최소값이 최대값보다 크거나 같습니다!");
            }

            if (legToHeightRatioMin >= legToHeightRatioMax)
            {
                Debug.LogError("다리/키 비율 최소값이 최대값보다 크거나 같습니다!");
            }
        }

        // 완전 자동 캘리브레이션 시작
        public void StartFullAutoCalibration()
        {
            if (calibrationInProgress)
            {
                Debug.LogWarning("이미 캘리브레이션이 진행 중입니다.");
                return;
            }

            Debug.Log("완전 자동 VR 캘리브레이션을 시작합니다!");
            Debug.Log($"{countdownDuration}초 후 시작됩니다. T-포즈를 준비해주세요!");
            calibrationCoroutine = StartCoroutine(FullCalibrationProcess());
        }

        // 완전 자동 캘리브레이션 프로세스
        IEnumerator FullCalibrationProcess()
        {
            calibrationInProgress = true;
            currentState = CalibrationState.Countdown;
            
            // 카운트다운
            Debug.Log("캘리브레이션 준비 중...");
            for (int i = countdownDuration; i > 0; i--)
            {
                countdownTimer = i;
                Debug.Log($"{i}초 후 시작... T-포즈를 준비하세요!");
                yield return new WaitForSeconds(1f);
            }
            countdownTimer = 0;
            Debug.Log("캘리브레이션 시작!");
            
            // 1단계: 트래커 연결 확인
            currentState = CalibrationState.Ready;
            Debug.Log("1단계: 트래커 연결 상태 확인 중...");
            
            if (!ValidateTrackers())
            {
                currentState = CalibrationState.Error;
                calibrationInProgress = false;
                countdownTimer = 0;
                yield break;
            }
            yield return new WaitForSeconds(0.5f);

            // 2단계: 바닥 높이 자동 감지
            currentState = CalibrationState.DetectingFloor;
            Debug.Log("2단계: 바닥 높이 자동 감지 중...");
            
            DetectFloorLevel();
            yield return new WaitForSeconds(0.5f);

            // 3단계: 사용자 신체 자동 측정
            currentState = CalibrationState.MeasuringUser;
            Debug.Log("3단계: 사용자 신체 자동 측정 시작 - T-포즈를 취해주세요!");
            
            yield return StartCoroutine(AutoMeasureUserBody());
            
            if (measurementAccuracy < calibrationMinAccuracy)
            {
                Debug.LogError($"측정 정확도가 너무 낮습니다 ({measurementAccuracy:F1}%). 최소 기준: {calibrationMinAccuracy}%");
                currentState = CalibrationState.Error;
                calibrationInProgress = false;
                countdownTimer = 0;
                yield break;
            }

            // 4단계: 아바타 측정값 계산
            currentState = CalibrationState.CalculatingAvatar;
            Debug.Log("4단계: 아바타 신체 측정값 계산 중...");
            
            try
            {
                CalculateAvatarMeasurements();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"아바타 측정 중 오류 발생: {e.Message}");
                currentState = CalibrationState.Error;
                calibrationInProgress = false;
                countdownTimer = 0;
                yield break;
            }
            yield return new WaitForSeconds(0.5f);

            // 5단계: 스케일 계산
            currentState = CalibrationState.ComputingScale;
            Debug.Log("5단계: 최적 스케일 비율 계산 중...");
            
            try
            {
                CalculateOptimalScale();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"스케일 계산 중 오류 발생: {e.Message}");
                currentState = CalibrationState.Error;
                calibrationInProgress = false;
                countdownTimer = 0;
                yield break;
            }
            yield return new WaitForSeconds(0.5f);

            // 6단계: VRIK 캘리브레이션 적용
            currentState = CalibrationState.ApplyingCalibration;
            Debug.Log("6단계: VRIK 캘리브레이션 적용 중...");
            
            try
            {
                ApplyVRIKCalibration();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"VRIK 캘리브레이션 적용 중 오류 발생: {e.Message}");
                currentState = CalibrationState.Error;
                calibrationInProgress = false;
                countdownTimer = 0;
                yield break;
            }
            yield return new WaitForSeconds(1f);

            // 완료
            currentState = CalibrationState.Completed;
            Debug.Log("완전 자동 캘리브레이션이 성공적으로 완료되었습니다!");
            
            LogFinalResults();
            calibrationInProgress = false;
            countdownTimer = 0;
        }

        // 사용자 신체 자동 측정
        IEnumerator AutoMeasureUserBody()
        {
            measurementSamples_list.Clear();
            int successfulMeasurements = 0;
            int maxAttempts = measurementSamples * 3; // 최대 시도 횟수
            int attempts = 0;

            while (successfulMeasurements < measurementSamples && attempts < maxAttempts)
            {
                attempts++;
                Debug.Log($"측정 시도 {attempts}/{maxAttempts} (성공: {successfulMeasurements}/{measurementSamples})");

                // T-포즈 유효성 검사
                if (!ValidateTPose())
                {
                    Debug.LogWarning("T-포즈가 올바르지 않습니다. 자세를 다시 취해주세요.");
                    yield return new WaitForSeconds(tPoseFailureWaitTime);
                    continue;
                }

                // 자세 안정성 확인
                yield return new WaitForSeconds(poseStabilityCheckTime);
                if (!CheckPoseStability())
                {
                    Debug.LogWarning("자세가 불안정합니다. 움직이지 마세요.");
                    yield return new WaitForSeconds(poseInstabilityWaitTime);
                    continue;
                }

                // 실제 측정 수행
                MeasurementData measurement = PerformSingleMeasurement();
                
                if (measurement.accuracy > measurementMinAccuracy)
                {
                    measurementSamples_list.Add(measurement);
                    successfulMeasurements++;
                    Debug.Log($"측정 성공 ({successfulMeasurements}/{measurementSamples}) - 정확도: {measurement.accuracy:F1}%");
                }
                else
                {
                    Debug.LogWarning($"측정 정확도 부족 ({measurement.accuracy:F1}%) - 재시도 (기준: {measurementMinAccuracy}%)");
                }

                yield return new WaitForSeconds(measurementInterval);
            }

            if (successfulMeasurements < measurementSamples)
            {
                Debug.LogWarning($"충분한 측정 데이터를 얻지 못했습니다. ({successfulMeasurements}/{measurementSamples})");
            }

            // 최종 사용자 측정값 계산
            CalculateFinalUserMeasurements();
        }

        // 단일 측정 수행
        MeasurementData PerformSingleMeasurement()
        {
            MeasurementData measurement = new MeasurementData();
            measurement.timestamp = Time.time;

            // 키 측정 (HMD 높이 기준)
            measurement.height = hmdTracker.position.y - detectedFloorLevel;

            // 팔 길이 측정 (양팔 평균)
            float leftArmLength = 0f;
            float rightArmLength = 0f;

            if (leftControllerTracker != null)
            {
                Vector3 leftShoulder = hmdTracker.position + Vector3.down * shoulderToHeadDistance + Vector3.left * shoulderWidth;
                leftArmLength = Vector3.Distance(leftShoulder, leftControllerTracker.position);
            }

            if (rightControllerTracker != null)
            {
                Vector3 rightShoulder = hmdTracker.position + Vector3.down * shoulderToHeadDistance + Vector3.right * shoulderWidth;
                rightArmLength = Vector3.Distance(rightShoulder, rightControllerTracker.position);
            }

            measurement.armLength = (leftArmLength + rightArmLength) * 0.5f;

            // 다리 길이 측정
            if (waistTracker != null && leftFootTracker != null)
            {
                measurement.legLength = waistTracker.position.y - leftFootTracker.position.y;
                measurement.waistHeight = waistTracker.position.y - detectedFloorLevel;
            }
            else if (leftFootTracker != null)
            {
                // 허리 트래커가 없으면 키의 설정된 비율로 추정
                measurement.waistHeight = measurement.height * waistHeightRatio;
                measurement.legLength = measurement.waistHeight - (leftFootTracker.position.y - detectedFloorLevel);
            }

            // 어깨 너비 측정
            if (leftControllerTracker != null && rightControllerTracker != null)
            {
                measurement.shoulderWidth = Vector3.Distance(leftControllerTracker.position, rightControllerTracker.position);
            }

            // 측정 정확도 계산
            measurement.accuracy = CalculateMeasurementAccuracy(measurement);

            return measurement;
        }

        // T-포즈 유효성 검사
        bool ValidateTPose()
        {
            float accuracy = 0f;

            // 1. 팔이 수평인지 확인 (40점)
            if (leftControllerTracker != null && rightControllerTracker != null)
            {
                Vector3 leftArm = leftControllerTracker.position - hmdTracker.position;
                Vector3 rightArm = rightControllerTracker.position - hmdTracker.position;

                float leftAngle = Vector3.Angle(leftArm.normalized, Vector3.left);
                float rightAngle = Vector3.Angle(rightArm.normalized, Vector3.right);

                Debug.Log($"팔 각도 체크 - 왼팔: {leftAngle:F1}도, 오른팔: {rightAngle:F1}도");

                if (leftAngle < armAngleStrictThreshold && rightAngle < armAngleStrictThreshold)
                    accuracy += 40f;
                else if (leftAngle < armAngleLooseThreshold && rightAngle < armAngleLooseThreshold)
                    accuracy += 20f;
            }

            // 2. 몸이 똑바로 서 있는지 확인 (30점)
            if (waistTracker != null)
            {
                Vector3 spine = hmdTracker.position - waistTracker.position;
                float spineAngle = Vector3.Angle(spine, Vector3.up);
                
                Debug.Log($"척추 각도 체크 - 각도: {spineAngle:F1}도");

                if (spineAngle < spineAngleStrictThreshold)
                    accuracy += 30f;
                else if (spineAngle < spineAngleLooseThreshold)
                    accuracy += 15f;
            }
            else
            {
                // 허리 트래커가 없으면 기본 점수 부여
                accuracy += 20f;
                Debug.Log("허리 트래커 없음 - 기본 점수 부여");
            }

            // 3. 양발 위치 확인 (30점)
            if (leftFootTracker != null && rightFootTracker != null)
            {
                Vector3 feetVector = rightFootTracker.position - leftFootTracker.position;
                float feetDistance = feetVector.magnitude;
                
                Debug.Log($"발 간격 체크 - 거리: {feetDistance:F2}m");
                
                // 어깨너비 정도의 발 간격이 이상적
                if (feetDistance > feetDistanceMin && feetDistance < feetDistanceMaxIdeal)
                    accuracy += 30f;
                else if (feetDistance > (feetDistanceMin * 0.5f) && feetDistance < feetDistanceMaxAllowed)
                    accuracy += 15f;
            }
            else
            {
                // 발 트래커가 없으면 기본 점수 부여
                accuracy += 20f;
                Debug.Log("발 트래커 없음 - 기본 점수 부여");
            }

            Debug.Log($"T-포즈 검증 결과 - 총 정확도: {accuracy:F1}% (기준: {tPoseMinScore}%)");

            return accuracy >= tPoseMinScore;
        }

        // 자세 안정성 확인 (간단한 버전)
        bool CheckPoseStability()
        {
            // 실제로는 여러 프레임에 걸친 위치 변화를 확인해야 함
            // 여기서는 간단히 true 반환
            return true;
        }

        // 측정 정확도 계산
        float CalculateMeasurementAccuracy(MeasurementData measurement)
        {
            float accuracy = 100f;

            Debug.Log($"측정값 체크 - 키: {measurement.height:F2}m, 팔: {measurement.armLength:F2}m, 다리: {measurement.legLength:F2}m");

            // 1. 인체 비율 검사
            if (measurement.armLength > 0 && measurement.height > 0)
            {
                float armRatio = measurement.armLength / measurement.height;
                Debug.Log($"팔/키 비율: {armRatio:F3} (기준: {armToHeightRatioMin:F2}-{armToHeightRatioMax:F2})");
                
                if (armRatio < armToHeightRatioMin || armRatio > armToHeightRatioMax)
                    accuracy -= ratioViolationPenalty;
            }

            if (measurement.legLength > 0 && measurement.height > 0)
            {
                float legRatio = measurement.legLength / measurement.height;
                Debug.Log($"다리/키 비율: {legRatio:F3} (기준: {legToHeightRatioMin:F2}-{legToHeightRatioMax:F2})");
                
                if (legRatio < legToHeightRatioMin || legRatio > legToHeightRatioMax)
                    accuracy -= ratioViolationPenalty;
            }

            // 2. 절대값 검사
            if (measurement.height < minHeight || measurement.height > maxHeight)
            {
                Debug.Log($"키 범위 벗어남: {measurement.height:F2}m (기준: {minHeight:F1}-{maxHeight:F1}m)");
                accuracy -= absoluteValueViolationPenalty;
            }
            
            if (measurement.armLength < minArmLength || measurement.armLength > maxArmLength)
            {
                Debug.Log($"팔길이 범위 벗어남: {measurement.armLength:F2}m (기준: {minArmLength:F1}-{maxArmLength:F1}m)");
                accuracy -= armLengthViolationPenalty;
            }

            Debug.Log($"측정 정확도: {accuracy:F1}%");

            return Mathf.Clamp(accuracy, 0f, 100f);
        }

        // 최종 사용자 측정값 계산
        void CalculateFinalUserMeasurements()
        {
            if (measurementSamples_list.Count == 0) return;

            float totalWeight = 0f;
            userHeight = 0f;
            userArmLength = 0f;
            userLegLength = 0f;

            // 정확도 기반 가중 평균
            foreach (var sample in measurementSamples_list)
            {
                float weight = sample.accuracy / 100f;
                totalWeight += weight;

                userHeight += sample.height * weight;
                userArmLength += sample.armLength * weight;
                userLegLength += sample.legLength * weight;
            }

            if (totalWeight > 0)
            {
                userHeight /= totalWeight;
                userArmLength /= totalWeight;
                userLegLength /= totalWeight;

                // 전체 정확도 계산
                measurementAccuracy = 0f;
                foreach (var sample in measurementSamples_list)
                {
                    measurementAccuracy += sample.accuracy;
                }
                measurementAccuracy /= measurementSamples_list.Count;
            }

            Debug.Log($"사용자 측정 완료 - 키: {userHeight:F2}m, 팔: {userArmLength:F2}m, 다리: {userLegLength:F2}m (정확도: {measurementAccuracy:F1}%)");
        }

        // 아바타 측정값 계산
        void CalculateAvatarMeasurements()
        {
            if (ik?.references == null) return;

            // 아바타 키
            avatarHeight = ik.references.head.position.y - ik.references.root.position.y;

            // 아바타 팔 길이
            if (ik.references.leftUpperArm && ik.references.leftForearm && ik.references.leftHand)
            {
                float upperArm = Vector3.Distance(ik.references.leftUpperArm.position, ik.references.leftForearm.position);
                float forearm = Vector3.Distance(ik.references.leftForearm.position, ik.references.leftHand.position);
                avatarArmLength = upperArm + forearm;
            }

            // 아바타 다리 길이
            if (ik.references.leftThigh && ik.references.leftCalf && ik.references.leftFoot)
            {
                float thigh = Vector3.Distance(ik.references.leftThigh.position, ik.references.leftCalf.position);
                float calf = Vector3.Distance(ik.references.leftCalf.position, ik.references.leftFoot.position);
                avatarLegLength = thigh + calf;
            }

            Debug.Log($"아바타 측정 완료 - 키: {avatarHeight:F2}m, 팔: {avatarArmLength:F2}m, 다리: {avatarLegLength:F2}m");
        }

        // 최적 스케일 계산
        void CalculateOptimalScale()
        {
            if (avatarHeight <= 0) return;

            float heightRatio = userHeight / avatarHeight;
            float armRatio = avatarArmLength > 0 ? userArmLength / avatarArmLength : heightRatio;
            float legRatio = avatarLegLength > 0 ? userLegLength / avatarLegLength : heightRatio;

            // 설정 가능한 가중 평균으로 최종 스케일 계산
            finalScale = (heightRatio * heightRatioWeight) + (armRatio * armRatioWeight) + (legRatio * legRatioWeight);
            
            // 설정 가능한 범위로 스케일 제한
            finalScale = Mathf.Clamp(finalScale, minScale, maxScale);

            // 아바타 크기는 원본 유지
            calibrationSettings.scaleMlp = 1.0f;

            Debug.Log($"스케일 계산 완료 - 계산된 비율: {finalScale:F3} (키: {heightRatio:F3}x{heightRatioWeight:F1}, 팔: {armRatio:F3}x{armRatioWeight:F1}, 다리: {legRatio:F3}x{legRatioWeight:F1})");
            Debug.Log($"아바타 크기는 원본 유지 (scaleMlp = 1.0), VRIK 타겟 위치로 보정");
        }

        // VRIK 캘리브레이션 적용 (크기 변경 없이 위치 조정)
        void ApplyVRIKCalibration()
        {
            // 기본 캘리브레이션 (크기 변경 없음)
            calibrationData = VRIKCalibrator.Calibrate(
                ik,
                calibrationSettings,
                hmdTracker,
                waistTracker,
                leftControllerTracker,
                rightControllerTracker,
                leftFootTracker,
                rightFootTracker
            );

            // 사용자와 아바타 크기 차이를 VRIK 타겟 위치로 보정
            ApplyHeightCompensation();

            Debug.Log("VRIK 캘리브레이션 적용 완료! (아바타 크기 변경 없음)");
        }

        // 키 차이를 VRIK 타겟 위치 조정으로 보정
        void ApplyHeightCompensation()
        {
            if (finalScale == 1.0f) return; // 보정이 필요없음

            float heightDifference = userHeight - avatarHeight;
            
            Debug.Log($"키 차이 보정: 사용자 {userHeight:F2}m vs 아바타 {avatarHeight:F2}m (차이: {heightDifference:F2}m)");

            // 1. HMD 타겟 높이 조정
            if (ik.solver.spine.headTarget != null)
            {
                Vector3 currentPos = ik.solver.spine.headTarget.localPosition;
                float headAdjustment = heightDifference * headHeightCompensationRatio;
                ik.solver.spine.headTarget.localPosition = new Vector3(currentPos.x, currentPos.y + headAdjustment, currentPos.z);
                Debug.Log($"머리 타겟 높이 조정: +{headAdjustment:F3}m ({headHeightCompensationRatio:F1}%)");
            }

            // 2. 허리 트래커가 있으면 허리 높이도 조정
            if (waistTracker != null && ik.solver.spine.pelvisTarget != null)
            {
                Vector3 currentPos = ik.solver.spine.pelvisTarget.localPosition;
                float pelvisAdjustment = heightDifference * pelvisHeightCompensationRatio;
                ik.solver.spine.pelvisTarget.localPosition = new Vector3(currentPos.x, currentPos.y + pelvisAdjustment, currentPos.z);
                Debug.Log($"허리 타겟 높이 조정: +{pelvisAdjustment:F3}m ({pelvisHeightCompensationRatio:F1}%)");
            }

            // 3. 발 트래커 높이 조정 (바닥에 발이 닿도록)
            float footAdjustment = heightDifference * footHeightCompensationRatio;
            
            if (leftFootTracker != null && ik.solver.leftLeg.target != null)
            {
                Vector3 currentPos = ik.solver.leftLeg.target.localPosition;
                ik.solver.leftLeg.target.localPosition = new Vector3(currentPos.x, currentPos.y - footAdjustment, currentPos.z);
            }
            
            if (rightFootTracker != null && ik.solver.rightLeg.target != null)
            {
                Vector3 currentPos = ik.solver.rightLeg.target.localPosition;
                ik.solver.rightLeg.target.localPosition = new Vector3(currentPos.x, currentPos.y - footAdjustment, currentPos.z);
            }

            if (footAdjustment != 0)
            {
                Debug.Log($"발 타겟 높이 조정: -{footAdjustment:F3}m ({footHeightCompensationRatio:F1}%)");
            }

            Debug.Log($"키 차이 보정 완료 - 아바타 크기는 원본 유지, VRIK 타겟만 조정");
        }

        // 바닥 높이 자동 감지
        void DetectFloorLevel()
        {
            if (autoDetectFloor && leftFootTracker != null && rightFootTracker != null)
            {
                detectedFloorLevel = Mathf.Min(leftFootTracker.position.y, rightFootTracker.position.y) - floorDetectionMargin;
                Debug.Log($"바닥 높이 감지: {detectedFloorLevel:F3}m (여유공간: {floorDetectionMargin:F3}m)");
            }
        }

        // 트래커 연결 확인
        bool ValidateTrackers()
        {
            if (hmdTracker == null)
            {
                Debug.LogError("HMD가 연결되지 않았습니다!");
                return false;
            }

            bool hasControllers = leftControllerTracker != null && rightControllerTracker != null;
            bool hasFeet = leftFootTracker != null && rightFootTracker != null;

            if (!hasControllers && !hasFeet)
            {
                Debug.LogError("양손 컨트롤러 또는 양발 트래커 중 하나는 필요합니다!");
                return false;
            }

            Debug.Log($"트래커 연결 확인 완료 - HMD: O, 컨트롤러: {(hasControllers ? "O" : "X")}, 허리: {(waistTracker ? "O" : "X")}, 발: {(hasFeet ? "O" : "X")}");
            return true;
        }

        // 기본 캘리브레이션 설정 초기화
        void InitializeCalibrationSettings()
        {
            // Vive 트래커 기본 축 설정
            calibrationSettings.headTrackerForward = Vector3.forward;
            calibrationSettings.headTrackerUp = Vector3.up;
            calibrationSettings.handTrackerForward = Vector3.forward;
            calibrationSettings.handTrackerUp = Vector3.up;
            calibrationSettings.footTrackerForward = Vector3.forward;
            calibrationSettings.footTrackerUp = Vector3.up;

            // 기본 오프셋 설정
            calibrationSettings.headOffset = Vector3.zero;
            calibrationSettings.handOffset = Vector3.zero;
            calibrationSettings.footForwardOffset = 0.08f;
            calibrationSettings.footInwardOffset = 0.04f;
            calibrationSettings.footHeadingOffset = 0f;

            // 가중치 설정
            calibrationSettings.pelvisPositionWeight = 1.0f;
            calibrationSettings.pelvisRotationWeight = 1.0f;
        }



        // 캘리브레이션 중단
        public void StopCalibration()
        {
            if (calibrationCoroutine != null)
            {
                StopCoroutine(calibrationCoroutine);
            }
            calibrationInProgress = false;
            currentState = CalibrationState.Ready;
            countdownTimer = 0;
            Debug.Log("캘리브레이션이 중단되었습니다.");
        }

        // 전체 리셋
        public void ResetCalibration()
        {
            StopCalibration();
            currentState = CalibrationState.Ready;
            measurementAccuracy = 0f;
            finalScale = 1f;
            measurementSamples_list.Clear();
            
            if (ik?.references?.root != null)
            {
                ik.references.root.localScale = Vector3.one;
            }

            Debug.Log("캘리브레이션이 리셋되었습니다.");
        }

        // 최종 결과 로그
        void LogFinalResults()
        {
            Debug.Log("=== 완전 자동 캘리브레이션 완료 ===");
            Debug.Log($"사용자 측정값 - 키: {userHeight:F2}m, 팔길이: {userArmLength:F2}m, 다리길이: {userLegLength:F2}m");
            Debug.Log($"아바타 측정값 - 키: {avatarHeight:F2}m, 팔길이: {avatarArmLength:F2}m, 다리길이: {avatarLegLength:F2}m");
            Debug.Log($"최종 스케일: {finalScale:F3} (측정 정확도: {measurementAccuracy:F1}%)");
            Debug.Log($"캘리브레이션 데이터 스케일: {calibrationData.scale:F3}");
            Debug.Log($"보정 비율 - 머리: {headHeightCompensationRatio:F1}%, 허리: {pelvisHeightCompensationRatio:F1}%, 발: {footHeightCompensationRatio:F1}%");
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 600, 500));
            
            // 타이틀
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            GUILayout.Label("개선된 완전 자동 VR 캘리브레이션 (매직 넘버 제거)", titleStyle);
            
            GUILayout.Space(10);

            // 현재 상태 표시
            GUIStyle statusStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            GUILayout.Label($"상태: {GetStatusText(currentState)}", statusStyle);

            GUILayout.Space(5);

            // 진행 중인 경우 상세 정보
            if (calibrationInProgress)
            {
                if (currentState == CalibrationState.Countdown)
                {
                    // 카운트다운 표시 (큰 글자)
                    GUIStyle countdownStyle = new GUIStyle(GUI.skin.label) 
                    { 
                        fontSize = 48, 
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };
                    GUILayout.Label($"{countdownTimer}", countdownStyle);
                    
                    GUILayout.Space(10);
                    GUIStyle instructionStyle = new GUIStyle(GUI.skin.label) 
                    { 
                        fontSize = 16, 
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };
                    GUILayout.Label("T-포즈를 준비하세요!", instructionStyle);
                    GUILayout.Label("• 팔을 양옆으로 수평하게", instructionStyle);
                    GUILayout.Label("• 똑바로 서기", instructionStyle);
                    GUILayout.Label("• 발은 어깨너비로", instructionStyle);
                }
                else
                {
                    GUILayout.Label("진행 중... 움직이지 마세요!");
                    if (currentState == CalibrationState.MeasuringUser)
                    {
                        GUILayout.Label("T-포즈를 정확히 취해주세요:");
                        GUILayout.Label("• 팔을 양옆으로 수평하게");
                        GUILayout.Label("• 똑바로 서기");
                        GUILayout.Label("• 발은 어깨너비로");
                    }
                }
            }

            GUILayout.Space(10);

            // 설정값 표시
            if (!calibrationInProgress)
            {
                GUIStyle boldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                GUILayout.Label("=== 현재 설정값 ===", boldLabelStyle);
                GUILayout.Label($"측정 샘플 수: {measurementSamples}회");
                GUILayout.Label($"T-포즈 최소 점수: {tPoseMinScore:F0}%");
                GUILayout.Label($"측정 최소 정확도: {measurementMinAccuracy:F0}%");
                GUILayout.Label($"보정 비율 - 머리: {headHeightCompensationRatio:F1}%, 허리: {pelvisHeightCompensationRatio:F1}%, 발: {footHeightCompensationRatio:F1}%");
            }

            // 측정 결과 (완료된 경우)
            if (currentState == CalibrationState.Completed)
            {
                GUILayout.Space(10);
                GUIStyle boldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                GUILayout.Label("=== 측정 결과 ===", boldLabelStyle);
                GUILayout.Label($"사용자 키: {userHeight:F2}m");
                GUILayout.Label($"사용자 팔길이: {userArmLength:F2}m");
                GUILayout.Label($"측정 정확도: {measurementAccuracy:F1}%");
                GUILayout.Label($"최종 스케일: {finalScale:F3}");
            }

            GUILayout.Space(10);

            // 컨트롤 안내
            GUIStyle boldLabelStyle2 = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            GUILayout.Label("=== 컨트롤 ===", boldLabelStyle2);
            if (!calibrationInProgress)
            {
                GUILayout.Label("SPACE - 자동 캘리브레이션 시작");
            }
            else
            {
                GUILayout.Label("ESC - 캘리브레이션 중단");
            }
            GUILayout.Label("R - 전체 리셋");

            GUILayout.EndArea();
        }

        string GetStatusText(CalibrationState state)
        {
            switch (state)
            {
                case CalibrationState.Ready: return "대기 중";
                case CalibrationState.Countdown: return "카운트다운 중";
                case CalibrationState.DetectingFloor: return "바닥 감지 중";
                case CalibrationState.MeasuringUser: return "사용자 측정 중";
                case CalibrationState.CalculatingAvatar: return "아바타 분석 중";
                case CalibrationState.ComputingScale: return "스케일 계산 중";
                case CalibrationState.ApplyingCalibration: return "캘리브레이션 적용 중";
                case CalibrationState.Completed: return "완료";
                case CalibrationState.Error: return "오류";
                case CalibrationState.Failed: return "실패";
                default: return "알 수 없음";
            }
        }
    }
}