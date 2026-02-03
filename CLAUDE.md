# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**애니멀 키친 타이쿤** - 모바일 캐주얼 레스토랑 경영 시뮬레이션 게임
- **엔진**: Unity 6
- **플랫폼**: Android/iOS
- **시점**: 2D 아이소메트릭

동물 캐릭터들이 운영하는 레스토랑을 성장시키는 게임. 손님 → 주문 → 요리 → 서빙 → 수익의 핵심 루프.

## Build & Run

Unity Hub에서 프로젝트 열기. 스크립트 변경 시 자동 컴파일.

### Mobile Build
```bash
# Android
Unity.exe -batchmode -projectPath "d:\NG_Workspace\VibeProj" -buildTarget Android -quit

# iOS (Mac only)
Unity.exe -batchmode -projectPath "d:\NG_Workspace\VibeProj" -buildTarget iOS -quit
```

## Architecture

모든 게임 코드는 `AnimalKitchen` 네임스페이스 사용.

### Core Systems (`Scripts/Core/`)
- **GameManager**: 게임 상태 관리 싱글톤
- **ResourceManager**: 재화(골드, 젬) 관리
- **SaveManager**: PlayerPrefs 기반 저장/로드, 자동 저장(60초마다), 오프라인 수익 계산
- **StaffManager**: 직원 고용, 스폰, 관리 (싱글톤)
- **GameEnums**: 상태 enum 정의 (GameState, CustomerState, StaffState, Rarity 등)
- **GameSetup**: 에디터 씬 자동 생성 도구

### Data (`Scripts/Data/`)
ScriptableObject 기반 데이터 설계:
- **RecipeData**: 메뉴 정보 (이름, 조리시간, 가격, 해금비용)
- **StaffData**: 직원 정보 (동물타입, 역할, 능력치, 레벨업 공식)
- **CustomerData**: 손님 정보 (인내심, 이동속도, 선호메뉴, 팁 계산)

### Restaurant (`Scripts/Restaurant/`)
- **Restaurant**: 레스토랑 상태, 테이블/직원/메뉴 관리
- **Table**: 테이블 점유 상태, 손님 할당
- **Kitchen**: 요리 주문 처리, 슬롯 관리, 진행률 추적

### Characters (`Scripts/Characters/`)
- **Customer**: 손님 AI 상태머신 (입장→착석→주문→대기→식사→결제→퇴장)
- **CustomerSpawner**: 손님 자동 스폰, 테이블 할당, OnCustomerSpawned 이벤트
- **Staff** (abstract): 직원 베이스 클래스 (이동, 레벨업, TryLevelUp 메서드)
- **Chef**: 요리 담당 직원 AI (우선순위 시스템: 인내심 낮은 손님 우선)
- **Waiter**: 서빙 담당 직원 AI (우선순위 서빙)
- **Cashier**: 결제 담당 직원 AI (결제 속도 가속)

### UI (`Scripts/UI/`)
**Core UI:**
- **UIManager**: HUD 업데이트, 패널 관리, ShowEarningsPopup

**Cooking UI:**
- **CookingSlotUI**: 개별 요리 진행 상황 표시 (진행 바, 완료 알림)
- **KitchenUI**: 주방 UI 관리자 (슬롯 관리, 완료 알림)

**Payment & Effects:**
- **PaymentEffectManager**: 결제 이펙트 관리 (골드 파티클, 팁 표시)
- **FloatingText**: 팁/골드 획득 텍스트 애니메이션

**Staff UI:**
- **StaffCardUI**: 고용 가능한 직원 카드 (능력치, 가격, 고용 버튼)
- **StaffSlotUI**: 고용된 직원 슬롯 (레벨, 능력치, 레벨업 버튼)
- **StaffHirePanel**: 직원 고용 패널 (필터, 카드 목록)
- **StaffCollectionPanel**: 직원 수집 도감 (수집률, 상세 정보)

**Expansion UI:**
- **RestaurantExpansionPanel**: 레스토랑 확장 UI (테이블 추가, 주방 업그레이드)
- **RecipeUnlockPanel**: 메뉴 해금 UI (RecipeCardUI 포함)

### Utilities (`Scripts/Utils/`)
- **ObjectPool**: 오브젝트 풀링
- **IsometricHelper**: 아이소메트릭 좌표 변환, 스프라이트 정렬
- **TouchInputManager**: 모바일 터치 입력 (드래그, 핀치 줌)

## Key Patterns

### Singleton
GameManager, ResourceManager, SaveManager, UIManager는 싱글톤 패턴 사용:
```csharp
public static GameManager Instance { get; private set; }
```

### Event System
상태 변경은 이벤트로 통지:
```csharp
public event Action<int> OnGoldChanged;
ResourceManager.Instance.OnGoldChanged += UpdateGoldDisplay;
```

### State Machine
Customer, Staff는 상태 기반 동작:
```csharp
public enum CustomerState { Entering, WaitingForSeat, WalkingToSeat, Ordering, WaitingForFood, Eating, Paying, Leaving }
```

## Save System

SaveManager는 다음 데이터를 저장:
- 재화 (골드, 젬)
- 레스토랑 레벨, 테이블 개수, 주방 슬롯
- 해금된 레시피 목록
- 고용된 직원 정보 (이름, 레벨, 타입)
- 마지막 플레이 시간 (오프라인 수익 계산용)

자동 저장: 60초마다, 앱 일시정지/종료 시

## ScriptableObject 생성

Unity Editor에서 우클릭 → Create → Animal Kitchen → Recipe/Staff/Customer Data

## 개발 진행 상황

- ✅ **Phase 1**: 기본 구조 (GameManager, ResourceManager, SaveManager)
- ✅ **Phase 2**: 핵심 게임플레이 (손님, 요리, 서빙, 결제)
- ✅ **Phase 3**: 직원 시스템 (고용, AI, 레벨업)
- ✅ **Phase 4**: 확장 시스템 (레스토랑, 메뉴, 도감)
- 🔄 **Phase 5**: 폴리싱 (UI/UX, 사운드, 튜토리얼)

## 기획 문서

상세 기획서: [기획.md](기획.md)
