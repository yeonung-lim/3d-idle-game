# Utils 모듈

## 개요

게임 개발에 필요한 공통 유틸리티 함수들과 헬퍼 클래스들을 제공하는 모듈입니다.

## 주요 기능 카테고리

### 확장 메소드 (Extension Methods)

-   Unity 기본 클래스의 기능 확장
-   Transform, GameObject, Vector3 등의 편의 메소드
-   자주 사용되는 패턴의 간소화

### 수학 유틸리티

-   게임에서 자주 사용되는 수학 계산
-   확률 계산, 보간, 곡선 계산
-   랜덤 값 생성 및 분포 함수

### 문자열 처리

-   텍스트 포맷팅 및 변환
-   다국어 지원을 위한 문자열 처리
-   숫자를 읽기 쉬운 형태로 변환 (1K, 1M 등)

### 시간 관리

-   게임 내 시간 계산 유틸리티
-   쿨다운, 타이머 관리
-   날짜/시간 포맷팅

### UI 헬퍼

-   UI 요소들의 애니메이션 헬퍼
-   레이아웃 자동 조정
-   색상 및 그라데이션 유틸리티

### 디버그 도구

-   개발 중 디버깅을 위한 도구들
-   성능 측정 및 모니터링
-   로그 시스템 강화

## 구현된 클래스

### TransformExtensions

Transform 클래스의 확장 메소드를 제공합니다.

**주요 기능:**

-   개별 축 위치 설정 (SetPositionX, SetPositionY, SetPositionZ)
-   로컬 위치 설정 (SetLocalPositionX, SetLocalPositionY, SetLocalPositionZ)
-   스케일 설정 및 Transform 리셋
-   Transform 간 거리 계산

### NumberUtils

숫자 관련 유틸리티 함수들을 제공합니다.

**주요 기능:**

-   큰 숫자 포맷팅 (FormatLargeNumber)
-   천 단위 구분 표시 (FormatWithCommas)
-   퍼센트 포맷팅 (FormatPercentage)
-   시간 포맷팅 (FormatTime)
-   값 제한 (Clamp)

### MathUtils

수학 관련 계산과 확률 처리를 담당합니다.

**주요 기능:**

-   확률 판정 (RollProbability)
-   가중치 기반 랜덤 선택 (WeightedRandomChoice)
-   보간 함수들 (Lerp, SmoothLerp)
-   범위 매핑 (MapRange)
-   각도 처리 (NormalizeAngle, AngleDifference)
-   가우시안 분포 랜덤 (GaussianRandom)

## 특징

-   **재사용성**: 프로젝트 전반에서 사용 가능한 범용 기능
-   **성능 최적화**: 자주 사용되는 기능의 최적화된 구현
-   **개발 효율성**: 반복적인 코드 작성 시간 단축
-   **안정성**: 검증된 알고리즘과 예외 처리

## 사용 예시

### Transform 확장 메소드

```csharp
using Utils;

// Transform 위치 조정
transform.SetPositionX(10f);
transform.SetPositionY(5f);
transform.SetLocalPositionZ(-2f);

// Transform 리셋
transform.ResetTransform();

// 거리 계산
float distance = transform.DistanceTo(targetTransform);
```

### 숫자 포맷팅

```csharp
using Utils;

// 큰 숫자 포맷팅
string formatted = NumberUtils.FormatLargeNumber(1500); // "1.5K"
string million = NumberUtils.FormatLargeNumber(2500000); // "2.5M"

// 퍼센트 표시
string percent = NumberUtils.FormatPercentage(0.75f); // "75%"

// 시간 포맷팅
string time = NumberUtils.FormatTime(125); // "2:05"

// 천 단위 구분
string commas = NumberUtils.FormatWithCommas(1234567); // "1,234,567"
```

### 수학 및 확률 계산

```csharp
using Utils;

// 확률 판정
bool success = MathUtils.RollProbability(75f); // 75% 확률

// 가중치 기반 선택
float[] weights = {10f, 30f, 60f};
int selected = MathUtils.WeightedRandomChoice(weights);

// 범위 매핑
float mapped = MathUtils.MapRange(5f, 0f, 10f, 0f, 100f); // 50f

// 각도 처리
float normalized = MathUtils.NormalizeAngle(-45f); // 315f
float diff = MathUtils.AngleDifference(10f, 350f); // -20f
```

## 네임스페이스

모든 유틸리티 클래스는 `Utils` 네임스페이스를 사용합니다.

```csharp
using Utils;
```

## 파일 구조

```
Utils/
├── README.md
├── TransformExtensions.cs
├── NumberUtils.cs
├── MathUtils.cs
├── Editor/
└── Async/
```
