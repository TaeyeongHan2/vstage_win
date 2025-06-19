# Prefab 설정 가이드

## 1. Landmark Prefab
```
GameObject 구조:
- Empty GameObject
  └─ Sphere (Scale: 0.1, 0.1, 0.1)
     └─ Material: 밝은 색상
```

## 2. Line Prefab
```
GameObject 구조:
- Empty GameObject
  └─ LineRenderer Component
     - Width: 0.02
     - Material: 기본 Line Material
     - Use World Space: true
```

## 3. Head Prefab (선택사항)
```
GameObject 구조:
- Empty GameObject
  └─ 머리 모델 또는 구체
     - Position: (0, 0.1, 0)
     - Scale: 적절히 조정
```

## Scene Hierarchy 예시
```
Scene
├── Main Camera
├── Directional Light
├── Bootstrap (MediaPipe)
├── Screen (UI)
├── PoseLandmarkerRunner
├── MediaPipeAdapter
│   └── BodyParent
│       ├── Landmark_0 (NOSE)
│       ├── Landmark_1 (LEFT_EYE_INNER)
│       ├── ... (자동 생성됨)
│       └── VirtualJoints
├── Shinano (Avatar)
│   └── Avatar Component
└── UI
    └── CalibrationTimer
``` 