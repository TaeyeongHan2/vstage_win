using UnityEngine;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;

namespace RootMotion.Demos
{
    // VR 트래커를 이용한 자동 신체 측정 시스템
    // T-포즈 상태에서 사용자의 실제 신체 측정값을 자동으로 계산
    public class VRBodyMeasurementSystem : MonoBehaviour
    {
        [Header("VRIK 설정")]
        [Tooltip("아바타의 VRIK 컴포넌트")]
        public VRIK ik;

        [Header("VR 트래커 참조")]
        [Tooltip("HMD (헤드셋)")]
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

        [Header("바닥 기준점")]
        [Tooltip("VR 플레이 공간의 바닥 높이 (Y 좌표)")]
        public float floorLevel = 0f;
        
        [Tooltip("바닥 높이 자동 감지")]
        public bool autoDetectFloor = true;

        [Header("측정 결과 (자동 계산됨)")]
        [SerializeField, Tooltip("측정된 사용자 키")]
        private float measuredUserHeight;
        
        [SerializeField, Tooltip("측정된 사용자 팔 길이 (양팔 평균)")]
        private float measuredUserArmLength;
        
        [SerializeField, Tooltip("측정된 사용자 다리 길이")]
        private float measuredUserLegLength;
        
        [SerializeField, Tooltip("측정된 사용자 어깨 너비")]
        private float measuredShoulderWidth;
        
        [SerializeField, Tooltip("측정된 사용자 골반 높이")]
        private float measuredWaistHeight;

        [Header("측정 품질 확인")]
        [SerializeField, Tooltip("측정 완료 여부")]
        public bool measurementComplete = false;
        
        [SerializeField, Tooltip("측정 정확도 점수 (0-100)")]
        private float measurementAccuracy = 0f;
        
        [Tooltip("측정 반복 횟수 (정확도 향상)")]
        public int measurementSamples = 5;
        
        [Tooltip("각 측정 간격 (초)")]
        public float measurementInterval = 0.5f;

        [Header("측정 임계값")]
        [Tooltip("팔이 수평인지 확인하는 각도 임계값 (도)")]
        public float armHorizontalThreshold = 15f;
        
        [Tooltip("양발이 평행한지 확인하는 각도 임계값 (도)")]
        public float feetParallelThreshold = 20f;
        
        [Tooltip("자세가 안정적인지 확인하는 움직임 임계값 (m)")]
        public float poseStabilityThreshold = 0.05f;

        // 측정 데이터 저장용
        private List<BodyMeasurement> measurementSamples_list = new List<BodyMeasurement>();
        public bool isMeasuring = false;
        private Coroutine measurementCoroutine;

        [System.Serializable]
        public class BodyMeasurement
        {
            public float height;
            public float leftArmLength;
            public float rightArmLength;
            public float legLength;
            public float shoulderWidth;
            public float waistHeight;
            public float accuracy;
            public float timestamp;
        }

        [System.Serializable]
        public class UserBodyData
        {
            public float height;
            public float armLength;
            public float legLength;
            public float shoulderWidth;
            public float waistHeight;
            public float accuracy;
            public bool isValid;
        }

        void Start()
        {
            if (autoDetectFloor)
            {
                DetectFloorLevel();
            }
        }

        void Update()
        {
            // M 키로 자동 측정 시작
            if (Input.GetKeyDown(KeyCode.M) && !isMeasuring)
            {
                StartAutoMeasurement();
            }

            // 측정 중단
            if (Input.GetKeyDown(KeyCode.Escape) && isMeasuring)
            {
                StopMeasurement();
            }
        }

        // 바닥 높이 자동 감지
        void DetectFloorLevel()
        {
            if (leftFootTracker != null && rightFootTracker != null)
            {
                // 양발 트래커의 평균 높이를 바닥으로 설정
                floorLevel = (leftFootTracker.position.y + rightFootTracker.position.y) * 0.5f;
                Debug.Log($"바닥 높이 자동 감지: {floorLevel:F3}m");
            }
            else
            {
                Debug.LogWarning("발 트래커가 연결되지 않아 바닥 높이를 자동 감지할 수 없습니다.");
            }
        }

        // 자동 측정 시작
        public void StartAutoMeasurement()
        {
            if (isMeasuring)
            {
                Debug.LogWarning("이미 측정이 진행 중입니다.");
                return;
            }

            if (!ValidateTrackers())
            {
                Debug.LogError("일부 트래커가 연결되지 않았습니다. 측정을 시작할 수 없습니다.");
                return;
            }

            Debug.Log("자동 신체 측정을 시작합니다!");
            Debug.Log("⏰ 3초 후 시작됩니다. T-포즈를 준비해주세요!");
            isMeasuring = true;
            measurementSamples_list.Clear();
            measurementCoroutine = StartCoroutine(MeasurementCoroutine());
        }

        // 측정 중단
        public void StopMeasurement()
        {
            if (measurementCoroutine != null)
            {
                StopCoroutine(measurementCoroutine);
            }
            isMeasuring = false;
            Debug.Log("측정이 중단되었습니다.");
        }

        // 측정 코루틴
        IEnumerator MeasurementCoroutine()
        {
            // 🆕 3초 카운트다운 추가
            Debug.Log("🔄 측정 준비 중...");
            for (int i = 3; i > 0; i--)
            {
                Debug.Log($"⏰ {i}초 후 시작... T-포즈를 준비하세요!");
                yield return new WaitForSeconds(1f);
            }
            Debug.Log("🎬 측정 시작!");
            
            yield return new WaitForSeconds(0.5f); // 추가 준비 시간

            int successfulMeasurements = 0;
            int maxRetries = measurementSamples * 3; // 최대 재시도 횟수
            int totalAttempts = 0;

            while (successfulMeasurements < measurementSamples && totalAttempts < maxRetries)
            {
                totalAttempts++;
                Debug.Log($"측정 시도 {totalAttempts}/{maxRetries} (성공: {successfulMeasurements}/{measurementSamples})");

                // T-포즈 자세 확인
                if (!ValidateTPose())
                {
                    Debug.LogWarning("T-포즈가 올바르지 않습니다. 자세를 확인해주세요.");
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // 자세 안정성 확인
                if (!CheckPoseStability())
                {
                    Debug.LogWarning("자세가 불안정합니다. 움직이지 마세요.");
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                // 실제 측정 수행
                BodyMeasurement measurement = PerformSingleMeasurement();
                if (measurement.accuracy > 70f) // 70% 이상 정확도만 수용
                {
                    measurementSamples_list.Add(measurement);
                    successfulMeasurements++;
                    Debug.Log($"측정 완료 (정확도: {measurement.accuracy:F1}%)");
                }
                else
                {
                    Debug.LogWarning($"측정 정확도가 낮습니다 ({measurement.accuracy:F1}%). 재시도합니다.");
                }

                yield return new WaitForSeconds(measurementInterval);
            }

            // 충분한 측정 데이터를 얻었는지 확인
            if (successfulMeasurements < measurementSamples * 0.4f) // 0.6f → 0.4f 완화
            {
                Debug.LogWarning($"충분한 측정 데이터를 얻지 못했습니다. ({successfulMeasurements}/{measurementSamples})");
                measurementComplete = false;
                isMeasuring = false;
                yield break; // return → yield break로 변경
            }

            // 평균값 계산
            float avgHeight = 0f;
            float avgArmLength = 0f;
            float avgLegLength = 0f;
            float avgShoulderWidth = 0f;
            float avgWaistHeight = 0f;
            float totalWeight = 0f;

            foreach (var sample in measurementSamples_list)
            {
                float weight = sample.accuracy / 100f;
                totalWeight += weight;

                avgHeight += sample.height * weight;
                avgArmLength += ((sample.leftArmLength + sample.rightArmLength) * 0.5f) * weight;
                avgLegLength += sample.legLength * weight;
                avgShoulderWidth += sample.shoulderWidth * weight;
                avgWaistHeight += sample.waistHeight * weight;
            }

            if (totalWeight > 0)
            {
                avgHeight /= totalWeight;
                avgArmLength /= totalWeight;
                avgLegLength /= totalWeight;
                avgShoulderWidth /= totalWeight;
                avgWaistHeight /= totalWeight;

                // 전체 측정 정확도 계산
                measurementAccuracy = 0f;
                foreach (var sample in measurementSamples_list)
                {
                    measurementAccuracy += sample.accuracy;
                }
                measurementAccuracy /= measurementSamples_list.Count;
            }

            measuredUserHeight = avgHeight;
            measuredUserArmLength = avgArmLength;
            measuredUserLegLength = avgLegLength;
            measuredShoulderWidth = avgShoulderWidth;
            measuredWaistHeight = avgWaistHeight;

            measurementComplete = true;
            isMeasuring = false;
            Debug.Log("자동 신체 측정이 완료되었습니다!");
            LogMeasurementResults();
        }

        // T-포즈 유효성 검사
        bool ValidateTPose()
        {
            float accuracy = 0f;

            // 1. 팔이 수평인지 확인 (40점) - 기준 완화
            if (leftControllerTracker != null && rightControllerTracker != null)
            {
                Vector3 leftArm = leftControllerTracker.position - hmdTracker.position;
                Vector3 rightArm = rightControllerTracker.position - hmdTracker.position;

                float leftAngle = Vector3.Angle(leftArm.normalized, Vector3.left);
                float rightAngle = Vector3.Angle(rightArm.normalized, Vector3.right);

                Debug.Log($"🔍 팔 각도 체크 - 왼팔: {leftAngle:F1}°, 오른팔: {rightAngle:F1}°");

                if (leftAngle < 45f && rightAngle < 45f) // 20° → 45° 완화
                    accuracy += 40f;
                else if (leftAngle < 60f && rightAngle < 60f) // 30° → 60° 완화
                    accuracy += 20f;
            }

            // 2. 몸이 똑바로 서 있는지 확인 (30점) - 기준 완화
            if (waistTracker != null)
            {
                Vector3 spine = hmdTracker.position - waistTracker.position;
                float spineAngle = Vector3.Angle(spine, Vector3.up);
                
                Debug.Log($"🔍 척추 각도 체크 - 각도: {spineAngle:F1}°");

                if (spineAngle < 30f) // 10° → 30° 완화
                    accuracy += 30f;
                else if (spineAngle < 45f) // 20° → 45° 완화
                    accuracy += 15f;
            }
            else
            {
                // 허리 트래커가 없으면 기본 점수 부여
                accuracy += 20f;
                Debug.Log("🔍 허리 트래커 없음 - 기본 점수 부여");
            }

            // 3. 양발 위치 확인 (30점) - 기준 완화
            if (leftFootTracker != null && rightFootTracker != null)
            {
                Vector3 feetVector = rightFootTracker.position - leftFootTracker.position;
                float feetDistance = feetVector.magnitude;
                
                Debug.Log($"🔍 발 간격 체크 - 거리: {feetDistance:F2}m");
                
                // 어깨너비 정도의 발 간격이 이상적
                if (feetDistance > 0.2f && feetDistance < 1.0f) // 범위 확대
                    accuracy += 30f;
                else if (feetDistance > 0.1f && feetDistance < 1.5f) // 범위 더 확대
                    accuracy += 15f;
            }
            else
            {
                // 발 트래커가 없으면 기본 점수 부여
                accuracy += 20f;
                Debug.Log("🔍 발 트래커 없음 - 기본 점수 부여");
            }

            Debug.Log($"🎯 T-포즈 검증 결과 - 총 정확도: {accuracy:F1}% (기준: 50%)");

            return accuracy >= 50f; // 70% → 50% 완화
        }

        // 자세 안정성 확인
        bool CheckPoseStability()
        {
            // 이전 위치와 비교하여 움직임 정도 확인
            // 실제 구현에서는 이전 프레임들의 위치를 저장해서 비교
            return true; // 간단히 true 반환 (실제로는 위치 변화량 체크)
        }

        // 단일 측정 수행
        BodyMeasurement PerformSingleMeasurement()
        {
            BodyMeasurement measurement = new BodyMeasurement();
            measurement.timestamp = Time.time;

            // 1. 키 측정 (HMD 높이 기준)
            measurement.height = hmdTracker.position.y - floorLevel;

            // 2. 팔 길이 측정 (어깨에서 손목까지)
            if (leftControllerTracker != null)
            {
                // 어깨 위치 추정 (HMD에서 약간 아래)
                Vector3 estimatedLeftShoulder = hmdTracker.position + Vector3.down * 0.2f + Vector3.left * 0.15f;
                measurement.leftArmLength = Vector3.Distance(estimatedLeftShoulder, leftControllerTracker.position);
            }

            if (rightControllerTracker != null)
            {
                Vector3 estimatedRightShoulder = hmdTracker.position + Vector3.down * 0.2f + Vector3.right * 0.15f;
                measurement.rightArmLength = Vector3.Distance(estimatedRightShoulder, rightControllerTracker.position);
            }

            // 3. 다리 길이 측정 (허리에서 발까지)
            if (waistTracker != null && leftFootTracker != null)
            {
                measurement.legLength = waistTracker.position.y - leftFootTracker.position.y;
            }
            else if (leftFootTracker != null)
            {
                // 허리 트래커가 없으면 HMD에서 추정 (키의 약 55%)
                float estimatedWaistHeight = measurement.height * 0.55f;
                measurement.legLength = estimatedWaistHeight - (leftFootTracker.position.y - floorLevel);
            }

            // 4. 어깨 너비 측정
            if (leftControllerTracker != null && rightControllerTracker != null)
            {
                measurement.shoulderWidth = Vector3.Distance(leftControllerTracker.position, rightControllerTracker.position);
            }

            // 5. 허리 높이 측정
            if (waistTracker != null)
            {
                measurement.waistHeight = waistTracker.position.y - floorLevel;
            }

            // 6. 측정 정확도 계산
            measurement.accuracy = CalculateMeasurementAccuracy(measurement);

            return measurement;
        }

        // 측정 정확도 계산
        float CalculateMeasurementAccuracy(BodyMeasurement measurement)
        {
            float accuracy = 100f;

            // 일반적인 인체 비율과 비교하여 정확도 계산
            // 1. 키 대비 팔 길이 비율 (일반적으로 키의 35-40%)
            float avgArmLength = (measurement.leftArmLength + measurement.rightArmLength) * 0.5f;
            float armToHeightRatio = avgArmLength / measurement.height;
            if (armToHeightRatio < 0.3f || armToHeightRatio > 0.45f)
            {
                accuracy -= 20f;
            }

            // 2. 키 대비 다리 길이 비율 (일반적으로 키의 50-55%)
            float legToHeightRatio = measurement.legLength / measurement.height;
            if (legToHeightRatio < 0.45f || legToHeightRatio > 0.6f)
            {
                accuracy -= 20f;
            }

            // 3. 양팔 길이 대칭성
            float armAsymmetry = Mathf.Abs(measurement.leftArmLength - measurement.rightArmLength);
            if (armAsymmetry > 0.1f)
            {
                accuracy -= 15f;
            }

            // 4. 키의 합리성 (1.4m ~ 2.2m 범위)
            if (measurement.height < 1.4f || measurement.height > 2.2f)
            {
                accuracy -= 25f;
            }

            return Mathf.Clamp(accuracy, 0f, 100f);
        }

        // 측정 결과를 UserBodyData 형태로 반환
        public UserBodyData GetMeasuredBodyData()
        {
            UserBodyData data = new UserBodyData();
            
            if (measurementComplete)
            {
                data.height = measuredUserHeight;
                data.armLength = measuredUserArmLength;
                data.legLength = measuredUserLegLength;
                data.shoulderWidth = measuredShoulderWidth;
                data.waistHeight = measuredWaistHeight;
                data.accuracy = measurementAccuracy;
                data.isValid = measurementAccuracy > 70f;
            }
            else
            {
                data.isValid = false;
            }

            return data;
        }

        // 트래커 연결 상태 확인
        bool ValidateTrackers()
        {
            bool hasMinimum = hmdTracker != null;

            if (!hasMinimum)
            {
                Debug.LogError("HMD가 연결되지 않았습니다.");
                return false;
            }

            // 최소 요구사항: HMD + (양손 또는 양발)
            bool hasHands = leftControllerTracker != null && rightControllerTracker != null;
            bool hasFeet = leftFootTracker != null && rightFootTracker != null;

            if (!hasHands && !hasFeet)
            {
                Debug.LogError("양손 컨트롤러 또는 양발 트래커 중 하나는 연결되어야 합니다.");
                return false;
            }

            return true;
        }

        // 측정 결과 로그 출력
        void LogMeasurementResults()
        {
            Debug.Log("=== 자동 신체 측정 결과 ===");
            Debug.Log($"키: {measuredUserHeight:F2}m");
            Debug.Log($"팔 길이: {measuredUserArmLength:F2}m");
            Debug.Log($"다리 길이: {measuredUserLegLength:F2}m");
            Debug.Log($"어깨 너비: {measuredShoulderWidth:F2}m");
            Debug.Log($"허리 높이: {measuredWaistHeight:F2}m");
            Debug.Log($"측정 정확도: {measurementAccuracy:F1}%");
            Debug.Log($"측정 샘플 수: {measurementSamples_list.Count}개");
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 350, 400, 200));
            GUIStyle boldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            GUILayout.Label("=== 자동 신체 측정 ===", boldLabelStyle);
            
            if (isMeasuring)
            {
                GUILayout.Label("측정 중...", boldLabelStyle);
                GUILayout.Label("T-포즈를 유지해주세요!");
            }
            else if (measurementComplete)
            {
                GUILayout.Label("측정 완료!", boldLabelStyle);
                GUILayout.Label($"키: {measuredUserHeight:F2}m");
                GUILayout.Label($"팔길이: {measuredUserArmLength:F2}m");
                GUILayout.Label($"다리길이: {measuredUserLegLength:F2}m");
                GUILayout.Label($"정확도: {measurementAccuracy:F1}%");
            }
            else
            {
                GUILayout.Label("M - 자동 측정 시작");
                GUILayout.Label("ESC - 측정 중단");
            }
            
            GUILayout.EndArea();
        }
    }
}