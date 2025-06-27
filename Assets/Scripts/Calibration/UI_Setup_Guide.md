# VR 캘리브레이션 UI 설정 가이드

## 📋 개요
VR 환경에서 사용할 수 있는 캘리브레이션 UI를 설정하는 방법을 설명합니다.

## ⚠️ 중요 변경사항
**🔒 아바타 크기 보존**: 이 시스템은 아바타의 실제 크기를 변경하지 않습니다. 대신 VRIK 타겟 위치만 조정하여 바닥 뚫림이나 충돌 문제를 방지합니다.

**🎯 안전한 캘리브레이션**: 
- 아바타 Transform.localScale은 원본 유지
- 물리 시뮬레이션 안전성 보장
- 애니메이션 호환성 유지
- VRIK 타겟 위치만 미세 조정

## 🏗️ UI 구조

### 1. 메인 Canvas 설정
```
GameObject: "VR Calibration UI"
├── Canvas (World Space)
│   ├── Main Menu Panel
│   ├── Progress Panel  
│   ├── Results Panel
│   └── Countdown Panel
```

## 🎨 UI 설정 단계

### Step 1: 메인 Canvas 생성
1. **GameObject 생성**: `UI > Canvas` 
2. **이름 변경**: "VR Calibration UI"
3. **Canvas 설정**:
   - Render Mode: `World Space`
   - Canvas Scaler: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`

### Step 2: 패널들 생성

#### 📱 Main Menu Panel
```
GameObject: "Main Menu Panel"
├── Background Image
├── Title Text: "VR 캘리브레이션 시스템"
├── Status Text: "준비 완료"
├── Status Color Indicator (Image)
├── Instruction Text: "시작할 기능을 선택하세요"
├── Start Full Calibration Button: "🚀 완전 자동 캘리브레이션"
├── Start Measurement Button: "📏 신체 측정만"
└── Reset Button: "🔄 리셋"
```

#### ⏳ Progress Panel
```
GameObject: "Progress Panel"  
├── Background Image
├── Status Text: "진행 중..."
├── Progress Text: "X/6 단계: 현재 작업"
├── Progress Bar (Slider)
├── Instruction Text: "안내 메시지"
└── Stop Button: "⏹️ 중단"
```

#### 🎯 Results Panel
```
GameObject: "Results Panel"
├── Background Image  
├── Status Text: "완료!"
├── Results Text: "측정 결과 표시"
├── VRIK Adjust Up Button: "🔼 VRIK 조정 +"
├── VRIK Adjust Down Button: "🔽 VRIK 조정 -"
├── Reset Button: "🔄 다시하기"
└── Back Button: "↩️ 메인메뉴"
```

#### ⏰ Countdown Panel
```
GameObject: "Countdown Panel"
├── Background Image
├── Countdown Text: "3" (큰 글자)
├── Status Text: "시작 준비 중"
├── Instruction Text: "T-포즈 준비!"
└── Stop Button: "⏹️ 중단"
```

## 🎯 컴포넌트 설정

### VRCalibrationUI 스크립트 설정
```
캘리브레이션 시스템 참조:
- Full Calibration: FullyAutomatedVRCalibration 컴포넌트
- Body Measurement: VRBodyMeasurementSystem 컴포넌트

UI 요소들:
- Main Canvas: 메인 Canvas
- 각종 Button들: 해당 버튼 컴포넌트들
- 각종 Text들: TextMeshProUGUI 컴포넌트들

패널들:
- Main Menu Panel: 메인 메뉴 GameObject
- Progress Panel: 진행 중 GameObject  
- Results Panel: 결과 GameObject
- Countdown Panel: 카운트다운 GameObject

시각적 피드백:
- Progress Bar: Slider 컴포넌트
- Status Color Indicator: Image 컴포넌트
```

## 🎨 스타일 가이드

### 색상 팔레트
- **준비**: 노란색 (#FFFF00)
- **진행중**: 파란색 (#0080FF) 
- **성공**: 초록색 (#00FF00)
- **오류**: 빨간색 (#FF0000)
- **카운트다운**: 주황색 (#FFA500)

### 텍스트 크기
- **제목**: 24px, Bold
- **상태**: 18px, Bold  
- **일반**: 14px, Regular
- **카운트다운**: 48px, Bold

### 버튼 크기
- **주요 버튼**: 200x60px
- **보조 버튼**: 120x40px
- **아이콘 버튼**: 60x60px

## 🔧 VR 최적화 설정

### Canvas 설정
```
Canvas:
- Render Mode: World Space
- Position: (0, 2, 2) - 사용자 앞 2m, 높이 2m
- Rotation: (0, 0, 0)
- Scale: (0.001, 0.001, 0.001) - VR 환경에 맞게 축소
```

### GraphicRaycaster 설정
```
GraphicRaycaster:
- Ignore Reversed Graphics: true
- Blocking Objects: All
- Blocking Mask: Everything
```

### 상호작용 설정
- **XR Interaction Toolkit** 사용 권장
- **Canvas에 TrackedDeviceGraphicRaycaster** 추가
- **버튼에 XR Simple Interactable** 추가

## 📱 사용 흐름

### 1. 메인 메뉴
```
사용자 선택:
→ "완전 자동 캘리브레이션" 버튼 클릭
→ "신체 측정만" 버튼 클릭  
→ "리셋" 버튼 클릭
```

### 2. 카운트다운 (3초)
```
표시 내용:
- 큰 숫자 카운트다운: "3", "2", "1"
- 상태: "시작 준비 중"
- 안내: "T-포즈를 준비하세요!"
```

### 3. 진행 중
```
표시 내용:  
- 현재 단계: "3/6 단계: 사용자 신체 측정"
- 진행률 바: 50% 완료
- 상태별 안내 메시지
- 중단 버튼
```

### 4. 완료
```
표시 내용:
- 측정 결과 상세 정보
- 🔒 아바타 크기: 원본 유지 표시
- 🎯 VRIK 타겟 조정 버튼 (+/-)
- 다시하기 버튼
- 메인메뉴 버튼
```

## 🚀 빠른 설정 체크리스트

### ✅ 필수 설정
- [ ] Canvas를 World Space로 설정
- [ ] VRCalibrationUI 스크립트 추가
- [ ] 모든 UI 요소 연결 완료
- [ ] 캘리브레이션 시스템 참조 연결
- [ ] 버튼 이벤트 연결 확인

### ✅ VR 최적화
- [ ] Canvas 크기 및 위치 조정
- [ ] VR 컨트롤러로 상호작용 가능
- [ ] 텍스트 크기가 VR에서 읽기 쉬움
- [ ] 버튼 크기가 VR에서 클릭하기 쉬움

### ✅ 테스트
- [ ] 모든 버튼 동작 확인
- [ ] 상태 변화 시 UI 업데이트 확인
- [ ] 카운트다운 표시 확인
- [ ] 진행률 바 동작 확인
- [ ] 결과 표시 확인

## 🎯 완성된 UI의 특징

### 📊 실시간 상태 표시
- 현재 캘리브레이션 단계 표시
- 진행률 바로 시각적 피드백
- 색상으로 상태 구분

### 🎮 직관적 조작
- 큰 버튼으로 VR에서 쉬운 클릭
- 명확한 아이콘과 텍스트
- 단계별 안내 메시지

### 🔄 완전한 제어
- 언제든 중단 가능
- 리셋 기능
- 🔒 아바타 크기 보존하면서 VRIK 타겟만 미세 조정

### 🛡️ 안전성 보장
- 바닥 뚫림 방지
- 충돌 시스템 안정성 유지
- 애니메이션 호환성 보장
- 물리 시뮬레이션 정상 동작

이제 이 가이드에 따라 UI를 설정하면 VR 환경에서 안전하고 편리하게 캘리브레이션을 사용할 수 있습니다! 🎉 