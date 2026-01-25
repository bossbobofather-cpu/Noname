# Shared Application

역할
- 공용 유스케이스/서비스를 정의합니다.
- Host 시뮬레이션과 이벤트 디스패치의 공통 흐름을 둡니다.

포함 예시
- GameHostBase, GameCommandOutcome
- GameEventBus, GameEventHub

금지
- UI/씬 종속 코드
