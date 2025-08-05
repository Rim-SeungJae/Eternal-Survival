# 유키 차오름 효과 시스템 사용 가이드

## 개요

유키의 화무십일홍 차오름 효과를 스크립트 기반으로 구현한 새로운 시스템입니다. 기존의 애니메이션 대신 어두운/밝은 스프라이트를 겹쳐놓고 시계방향으로 점진적으로 채워지는 효과를 제공합니다.

## 파일 구조

- `YukiChargeEffect.cs` - 차오름 효과 메인 컴포넌트
- `RadialFill.shader` - 방사형 채우기 셰이더
- `YukiWeaponEvoEffect.cs` - 기존 파일에 차오름 효과 통합

## 프리팹 설정 방법

### 1. 프리팹 구조 만들기

유키 진화 무기 프리팹에 다음과 같은 구조를 만드세요:

```
Yuki Weapon evo (Root)
├── Effect (기존)
│   ├── DarkSprite (새로 추가)
│   └── BrightSprite (새로 추가)
├── AttackArea (기존)
└── Afterimage (기존)
```

### 2. 스프라이트 설정

1. **DarkSprite** (어두운 버전):
   - 어두운 색상의 반원 스프라이트 할당
   - SpriteRenderer의 Sorting Order: 0
   - Color: 원하는 어두운 색상 (예: 회색, 검은색)

2. **BrightSprite** (밝은 버전):
   - 밝은 색상의 동일한 반원 스프라이트 할당
   - SpriteRenderer의 Sorting Order: 1
   - Material: RadialFill 셰이더를 사용하는 머티리얼
   - Color: 원하는 밝은 색상 (예: 흰색, 노란색)

### 3. YukiChargeEffect 컴포넌트 설정

Effect 오브젝트에 `YukiChargeEffect` 컴포넌트를 추가하고 다음과 같이 설정:

```csharp
[Header("# Sprite Settings")]
public SpriteRenderer darkSprite;      // DarkSprite 할당
public SpriteRenderer brightSprite;    // BrightSprite 할당

[Header("# Fill Settings")]
public float startAngle = -90f;        // 12시 방향부터 시작 (-90도)
public bool clockwise = true;          // 시계방향으로 채우기

[Header("# Animation Settings")]
public float chargeDuration = 2f;      // 차오름 지속 시간
public float holdDuration = 0.5f;      // 완료 후 유지 시간

[Header("# Material")]
public Material brightSpriteMaterial;  // RadialFill 머티리얼 할당
```

## 머티리얼 설정

### RadialFill 머티리얼 생성

1. Project 창에서 우클릭 → Create → Material
2. 생성된 머티리얼의 Shader를 "Custom/RadialFill"로 설정
3. 프로퍼티 설정:
   - **Main Tex**: 밝은 스프라이트 텍스처
   - **Color**: 원하는 색상 (예: 흰색, 노란색)  
   - **Fill Amount**: 0 (런타임에서 자동 제어됨)
   - **Start Angle**: 0 (반원의 오른쪽 끝에서 시작)
   - **Clockwise**: 1 (시계방향)
   - **Center Point**: (0.5, 0, 0, 0) - 반원의 bottom center
4. YukiChargeEffect 컴포넌트의 `brightSpriteMaterial` 필드에 이 머티리얼을 할당

### 셰이더 프로퍼티

```hlsl
_MainTex ("Texture", 2D) = "white" {}       // 스프라이트 텍스처
_Color ("Color", Color) = (1,1,1,1)         // 색상
_FillAmount ("Fill Amount", Range(0, 1)) = 0 // 채우기 진행도 (0~1)
_StartAngle ("Start Angle", Float) = 0       // 시작 각도 (반원 기준)
_Clockwise ("Clockwise", Float) = 1          // 방향 (1=시계방향, 0=반시계방향)
_CenterPoint ("Center Point", Vector) = (0.5, 0, 0, 0) // 중심점 (반원의 bottom center)
```

## 사용법

### 코드에서 사용

```csharp
// 차오름 효과 시작
chargeEffect.StartCharging(2.0f); // 2초 동안 차오름

// 즉시 중단
chargeEffect.StopCharging();

// 상태 확인
bool isCharging = chargeEffect.IsCharging();
float progress = chargeEffect.GetFillAmount(); // 0~1 진행도
```

### YukiWeaponEvoEffect에서 자동 사용

`YukiWeaponEvoEffect.cs`에서 자동으로 새로운 차오름 효과를 감지하고 사용합니다:

1. `YukiChargeEffect` 컴포넌트가 있으면 스크립트 기반 효과 사용
2. 없으면 기존 애니메이션 시스템 사용

## 커스터마이징

### 시작 각도 변경

```csharp
chargeEffect.startAngle = 0f;    // 3시 방향부터 시작
chargeEffect.startAngle = 90f;   // 6시 방향부터 시작
chargeEffect.startAngle = 180f;  // 9시 방향부터 시작
chargeEffect.startAngle = -90f;  // 12시 방향부터 시작 (기본값)
```

### 방향 변경

```csharp
chargeEffect.clockwise = true;   // 시계방향 (기본값)
chargeEffect.clockwise = false;  // 반시계방향
```

### 색상 효과

```csharp
// 런타임에서 색상 변경
brightSprite.color = Color.red;    // 빨간색
brightSprite.color = Color.yellow; // 노란색
darkSprite.color = Color.gray;     // 회색 배경
```

## 디버깅

### 테스트 메뉴

에디터에서 컴포넌트 우클릭 → "Test Charge Effect"로 테스트 가능

### 로그 메시지

```
"스크립트 기반 차오름 효과 시작! 지속 시간: 2초"
"기존 애니메이션 사용: 길이 0.83초, 속도: 0.415"
```

### 문제 해결

1. **효과가 보이지 않음**: 
   - BrightSprite의 Sorting Order가 DarkSprite보다 높은지 확인
   - RadialFill 머티리얼이 올바르게 적용되었는지 확인

2. **방향이 이상함**:
   - startAngle 값 확인 (-90도가 12시 방향)
   - clockwise 값 확인

3. **셰이더 오류**:
   - RadialFill.shader 파일이 올바른 위치에 있는지 확인
   - Unity에서 셰이더가 컴파일되었는지 확인

## 성능 최적화

- 머티리얼 인스턴스는 자동으로 생성/정리됩니다
- 여러 개의 차오름 효과가 동시에 실행되어도 문제없습니다
- 셰이더는 가벼운 연산만 사용하므로 성능 영향이 최소화됩니다

## 향후 확장

이 시스템을 다른 무기의 차오름 효과에도 적용할 수 있습니다:
- 혜진의 흡령부 차오름
- 별조각의 메테오 예고 효과
- 번개의 충전 효과 등