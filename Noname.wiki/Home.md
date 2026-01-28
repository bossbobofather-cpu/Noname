# GameAbilitySystem Portfolio

> Unity용 Gameplay Ability System - 10년차 게임 개발자의 아키텍처 포트폴리오

## 프로젝트 개요

Unreal Engine의 Gameplay Ability System을 Unity 환경에 최적화하여 구현한 플러그인입니다. **Clean Architecture**, **멀티스레드 안전성**, **확장 가능한 설계**를 핵심으로 하며, 실무에서 요구되는 고품질 코드베이스를 시연합니다.

---

## 🎯 핵심 기술 역량

### 1. Clean Architecture
- **Domain/Presentation 계층 완전 분리**
  - Domain: 순수 C# (.NET Standard 2.1) - Unity 의존성 0%
  - Presentation: Unity MonoBehaviour/ScriptableObject
  - Bridge: 양방향 변환 레이어

- **의존성 역전 원칙 준수**
  - Domain → Presentation ❌ (컴파일 에러로 강제)
  - Presentation → Domain ✅ (올바른 방향)

### 2. Multithreading & Concurrency
- **Thread-Safe State Management**
  - `lock` 기반 동기화로 모든 public 메서드 보호
  - Snapshot Pattern으로 스레드 간 안전한 데이터 전달
  - Host 환경에서 별도 스레드 실행 가능

- **Zero Race Condition**
  - 불변 데이터 구조 (Snapshot)
  - Immutable 컬렉션 복사

### 3. Performance Optimization
- **Zero Allocation Paths**
  - Struct 기반 태그/이벤트 (`FGameplayTagModel`, `GameplayEventData`)
  - String 해시 캐싱 (`_hash` 필드)
  - Object Pool 패턴 (Task, Context)

- **O(1) Tag Lookup**
  - Dictionary 기반 해시 검색
  - 계층 구조 사전 계산 및 캐싱

### 4. Scalable System Design
- **Tag-Based Architecture**
  - 계층적 Gameplay Tag (`Ability.Attack.Melee`)
  - 런타임 조건 제어 (Required/Blocked Tags)

- **Data-Driven Workflow**
  - ScriptableObject로 비프로그래머 편집 지원
  - JSON 직렬화로 네트워크/저장 시스템 연동 가능

- **Async Task System**
  - `AbilityTask`로 비동기 능력 실행
  - Coroutine 기반 애니메이션/타겟팅/이벤트 대기

---

## 📁 프로젝트 구성

| 디렉토리 | 설명 |
|---|---|
| `UpmPackages/com.noname.gameabilitysystem.plugin` | UPM 플러그인 (재사용 가능) |
| `Noname/Assets/Scripts/Features/ExploreGame` | 플러그인 사용 예제 (던전 자동 탐방 게임) |
| `Noname.wiki` | 기술 문서 및 가이드 |
| `docs` | DocFX 생성 API 문서 |

---

## 🚀 빠른 시작

### 환경
- **Unity**: 6000.3.1f1 이상
- **C#**: 12.0 (.NET Standard 2.1)

### 설치
```bash
git clone [repository]
cd Noname
# Unity Hub에서 프로젝트 열기
```

로컬 UPM 패키지는 `Packages/manifest.json`에 자동 연결됨:
```json
{
  "dependencies": {
    "com.noname.gameabilitysystem.plugin": "file:../UpmPackages/com.noname.gameabilitysystem.plugin"
  }
}
```

---

## 📚 문서

- **[Getting Started](Getting-Started)** - 설치 및 초기 설정
- **[GameAbilitySystem Plugin](GameAbilitySystem-Plugin)** - 플러그인 상세 가이드
- **[Sample Scene](1.Sample)** - 능력 시스템 데모
- **[API Reference](https://[your-domain]/docs)** - DocFX 생성 API 문서

---

## 🎬 미디어

### 플레이 영상
[플레이 영상 링크 예정]

### 아키텍처 다이어그램
```
┌──────────────────────┐
│   Presentation       │
│  (Unity Dependent)   │
└──────────┬───────────┘
           │
     ┌─────▼──────┐
     │   Bridge   │
     └─────┬──────┘
           │
┌──────────▼───────────┐
│      Domain          │
│  (Pure C#, Unity-Free)│
└──────────────────────┘
```

---

## 💼 포트폴리오 하이라이트

### 설계 능력
- ✅ **Clean Architecture 실무 적용**: 계층 분리로 테스트 가능성 및 유지보수성 향상
- ✅ **SOLID 원칙 준수**: 단일 책임, 의존성 역전, 인터페이스 분리
- ✅ **Design Patterns**: Snapshot, Bridge, Observer, Command, Factory

### 성능 최적화
- ✅ **Zero GC Allocation**: Struct 기반 설계
- ✅ **Lock-Free 구조**: 가능한 곳은 Immutable 구조 활용
- ✅ **Profiler-Driven Optimization**: Unity Profiler 마커 활용 (예정)

### 협업 역량
- ✅ **디자이너 친화적**: ScriptableObject 워크플로우
- ✅ **문서화**: DocFX + Wiki로 체계적 문서 관리
- ✅ **코드 리뷰 가능**: 명확한 네이밍, 주석, 구조

---

## 🛠 기술 스택

| Category | Technologies |
|---|---|
| Language | C# 12.0, .NET Standard 2.1 |
| Engine | Unity 6000.3.1f1 |
| Architecture | Clean Architecture, SOLID, DDD |
| Patterns | Snapshot, Bridge, Observer, Command |
| Threading | lock, Immutable Collections |
| Documentation | DocFX, Markdown, XML Comments |
| Testing | Unity Test Framework (예정) |

---

## 📞 Contact

**10+ Years Game Developer**
- **LinkedIn**: [링크 예정]
- **GitHub**: [링크 예정]
- **Email**: [이메일 예정]

**Specialized in:**
- Gameplay Systems Architecture
- Multiplayer/Network Systems
- Performance Optimization
- Technical Leadership

---

## 📄 License

MIT License
