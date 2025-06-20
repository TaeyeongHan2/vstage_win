# 문제 해결 가이드

## 자주 발생하는 문제들

### 1. "MediaPipeToAvatarAdapter not found" 에러
**원인:** Avatar.cs가 Adapter를 찾을 수 없음
**해결:**
- Scene에 MediaPipeAdapter GameObject가 있는지 확인
- MediaPipeToAvatarAdapter 컴포넌트가 추가되었는지 확인

### 2. 랜드마크가 보이지 않음
**원인:** Prefab 할당 누락 또는 스케일 문제
**해결:**
- Landmark Prefab이 Inspector에 할당되었는지 확인
- Multiplier 값을 10에서 시작해서 조정
- BodyParent의 위치가 (0,0,0)인지 확인

### 3. 캐릭터가 움직이지 않음
**원인:** 캘리브레이션 안됨 또는 참조 누락
**해결:**
- 'C' 키로 캘리브레이션 실행
- Console에서 "Calibration Completed" 메시지 확인
- Avatar.cs의 Adapter 참조가 연결되었는지 확인

### 4. 움직임이 부자연스러움
**원인:** 좌표계 변환 문제
**해결:**
- MediaPipeToAvatarAdapter의 좌표 변환 확인:
  - X축: 미러링 (-landmark.x)
  - Y축: 반전 (-landmark.y)
  - Z축: 그대로 (landmark.z)

### 5. NullReferenceException 발생
**원인:** 필수 컴포넌트 누락
**확인 사항:**
- PoseLandmarkerRunner가 Scene에 있고 실행 중인지
- OnPoseResult 이벤트가 연결되었는지
- 모든 Transform 참조가 유효한지

## 디버그 팁

### Console 로그 추가
```csharp
// MediaPipeToAvatarAdapter.cs의 OnPoseResult에 추가
Debug.Log($"Pose detected: {landmarks.landmarks.Count} landmarks");
```

### 시각적 디버깅
- Landmark Scale을 크게 설정해서 위치 확인
- Line Renderer의 Width를 늘려서 연결 상태 확인

### 성능 최적화
- 프로덕션에서는 OptimizedMediaPipeAdapter 사용 고려
- 불필요한 시각적 요소 제거 