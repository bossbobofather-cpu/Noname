# Features

프로젝트의 기능(Feature) 단위로 코드를 묶고, 각 기능 내부는 Clean 레이어로 분리합니다.

구성 원칙
- Domain: 순수 규칙/상태/정책. Unity 의존 없음.
- Application: 유스케이스/호스트 로직. Domain에만 의존.
- Presentation: MonoBehaviour, 입력, UI, 씬 연결.
- Data: ScriptableObject/프리팹/에셋 참조.

예시
- ExploreGame 기능은 `Assets/Scripts/Features/ExploreGame` 아래에 배치합니다.
- Host 시뮬레이션 로직은 Application/Host 폴더에서 관리합니다.
- 공용 로직은 `Assets/Scripts/Shared` 아래에 배치합니다.
현재 Feature
- ExploreGame: 텍스트 로그 기반 탐방 게임
  - 문서: Assets/Scripts/Features/ExploreGame/README.md`r

