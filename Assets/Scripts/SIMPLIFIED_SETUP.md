# 간소화된 설정 가이드

## 실제로 필요한 것들만!

### 1. Scene 설정 (3단계)

```
1. Pose Landmark Detection Scene 열기
2. 빈 GameObject 생성 → "MediaPipeAdapter"
3. OptimizedMediaPipeAdapter 컴포넌트 추가
```

### 2. Inspector 설정 (2개만!)

```
OptimizedMediaPipeAdapter:
- Pose Landmarker Runner: Scene에서 찾아서 드래그
- Smoothing Factor: 0.5 (기본값 유지)
```

### 3. Avatar 연결

```
1. Humanoid 캐릭터를 Scene에 추가
2. Avatar.cs 컴포넌트 추가
3. Start()에서 자동으로 OptimizedMediaPipeAdapter 찾음
```

## 완료! 🎉

프리팹 만들 필요 없이 바로 작동합니다.

### 장점:
- ✅ 설정 시간 5분
- ✅ 불필요한 시각적 요소 없음
- ✅ 성능 최적화됨
- ✅ 코드만으로 완전히 작동

### 디버깅이 필요한 경우에만:
MediaPipeToAvatarAdapter를 사용하고 프리팹을 만들어서 시각적으로 확인 