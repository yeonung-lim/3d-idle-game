# Game 모듈

## 개요

게임의 핵심 로직과 시스템을 담당하는 모듈입니다. 캐릭터, 몬스터, 전투, UI 등 게임플레이의 핵심 요소들을 포함합니다.

## 주요 구성 요소

### Core/

-   게임의 핵심 시스템과 매니저 클래스들
-   게임 상태 관리 및 전체적인 게임 흐름 제어

### Character/

-   플레이어 캐릭터 관련 기능
-   캐릭터 스탯, 레벨링, 장비 시스템
-   캐릭터 애니메이션 및 상태 관리

### Monster/

-   몬스터 관련 시스템
-   몬스터 생성, AI, 스탯 관리
-   몬스터 타입별 특성 및 보상 시스템

### Combat/

-   전투 시스템
-   데미지 계산, 전투 로직
-   스킬 시스템 및 전투 이펙트

### UI/

-   게임 내 UI 요소들
-   HUD, 인벤토리, 스탯 창 등
-   게임플레이 관련 사용자 인터페이스

## 특징

-   방치형 게임에 최적화된 자동 전투 시스템
-   실시간 스탯 업데이트 및 진행도 추적
-   모듈화된 구조로 확장성과 유지보수성 확보
