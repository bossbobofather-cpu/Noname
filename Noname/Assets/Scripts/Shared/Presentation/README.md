# Shared Presentation

역할
- 공용 UI 프레임워크와 프레젠테이션 계층을 둡니다.
- 씬에서 사용할 공용 Bootstrap/Mode 진입점을 제공합니다.

포함 예시
- UIManager, UIRoot
- 공용 UI 베이스/툴팁/드래그
- BootstrapperBase, GameBootstrapper
- GameMode, ModuleBase, 공용 모듈 인터페이스

금지
- 특정 게임 기능에 종속된 UI/로직
사용 예
- GameBootstrapper에서 공용 Manager를 생성
- GameMode에서 모듈 수명주기 제어

