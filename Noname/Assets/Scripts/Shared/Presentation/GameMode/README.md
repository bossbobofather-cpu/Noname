# Shared GameMode

역할
- 게임 모드와 모듈의 공통 수명 주기를 관리합니다.
- Initialize → Startup → Shutdown 순서를 강제합니다.

포함 예시
- GameMode
- ModuleBase, IModule

금지
- 특정 게임 규칙/콘텐츠 구현
