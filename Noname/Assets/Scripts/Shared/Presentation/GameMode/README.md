# Shared GameMode

역할
- 게임 모드와 모듈의 공통 수명 주기를 관리합니다.
- Initialize → Startup → Shutdown 순서를 강제합니다.

포함 예시
- GameMode
- ModuleBase, IModule

금지
- 특정 게임 규칙/콘텐츠 구현
운영 규칙
- Initialize/Startup/Shutdown은 Bootstrapper가 명시적으로 호출
- 모듈은 Unity 생명주기 대신 모듈 수명주기를 사용

