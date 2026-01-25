# Shared

여러 기능에서 공통으로 사용하는 인프라를 모읍니다.

구성
- Domain / Application / Presentation으로 분리합니다.
- 기능 특화 로직은 여기에 두지 않습니다.

포함 예시
- GameHostBase, GameHostTypes, GameCommandOutcome
- GameEventBus, GameEventHub
- GameMode, ModuleBase, Bootstrapper
- UIManager, UIRoot, 공용 UI 베이스

금지
- 특정 게임 규칙/콘텐츠 구현
