# 3D 방치형 게임

## 게임 개요

Unity로 개발된 3D 방치형(Idle) 게임입니다. 플레이어가 직접 조작하지 않아도 자동으로 진행되는 게임플레이를 특징으로 합니다.

## 게임 시스템 구성요소

### 스테이지 매니저 (StageManager)

-   게임의 스테이지 진행을 관리하는 핵심 시스템
-   주요 기능:
    -   스테이지 초기화 및 진행 관리
    -   몬스터 처치 수 추적
    -   스테이지 클리어 조건 확인
    -   다음 스테이지로의 자동 진행
-   설정 가능한 요소:
    -   스테이지당 처치해야 할 몬스터 수 (기본값: 10)
-   디버그 기능:
    -   'N' 키를 눌러 다음 스테이지로 즉시 진행 가능

## 주요 모듈 구조

### [Game](Assets/Scripts/Game/README.md)

게임의 핵심 로직과 시스템을 담당하는 모듈

-   캐릭터, 몬스터, 전투, UI 등 게임플레이 핵심 요소
-   방치형 게임에 최적화된 자동 전투 시스템

### [View](Assets/Scripts/View/README.md)

UI 시스템 관리 모듈

-   화면 전환, 팝업 관리, UI 컴포넌트 제어
-   MVC 패턴을 적용한 UI 아키텍처

### [Sound](Assets/Scripts/Sound/README.md)

오디오 시스템 관리 모듈

-   BGM, 효과음, UI 사운드 제어

### [BackEnd](Assets/Scripts/BackEnd/README.md)

백엔드 서비스 통신 모듈

-   서버 연동, 데이터 동기화, 클라우드 저장

### [Utils](Assets/Scripts/Utils/README.md)

공통 유틸리티 모듈

-   확장 메소드, 수학 계산, 문자열 처리 등
-   개발 효율성 향상을 위한 헬퍼 함수들

### [IAP](Assets/Scripts/IAP/README.md)

인앱 구매 시스템 모듈

-   iOS/Android 결제 처리 및 상품 관리
-   보안 강화된 결제 검증 시스템

### [Ads](Assets/Scripts/Ads/README.md)

광고 시스템 모듈

-   다양한 광고 네트워크 연동 및 수익화
-   사용자 경험을 고려한 광고 최적화

## 추가 모듈

### GPGS (Google Play Games Services)

Google Play 게임 서비스 연동

### AppleSignIn

Apple 로그인 서비스 연동

### AsyncInitialize

비동기 초기화 시스템

### Boot

게임 부팅 및 초기화

### Effects

게임 내 시각적 효과 시스템

### Editor

Unity 에디터 확장 도구

### Sequence

게임 시퀀스 및 타임라인 관리

### Singleton

싱글톤 패턴 구현체

## 개발 환경

-   **엔진**: Unity 2022.3 LTS
-   **언어**: C#
-   **플랫폼**: iOS, Android
-   **아키텍처**: 모듈형 설계 패턴
