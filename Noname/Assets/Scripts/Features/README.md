# Features

프로젝트 기능(Feature)을 기준으로 묶고, 각 기능 내부는 Clean 레이어로 나눕니다.

구성 원칙
- Domain: 순수 규칙/상태/정책. Unity 의존 없음.
- Application: 유스케이스/서비스. Domain에만 의존.
- Presentation: MonoBehaviour, 입력, UI, 씬 연결.
- Data: ScriptableObject/프리팹/에셋 참조.

예시
- MergeGame 기능은 `Assets/Features/MergeGame` 아래에 배치합니다.
- 공용 로직은 `Assets/Shared` 아래에 배치합니다.
