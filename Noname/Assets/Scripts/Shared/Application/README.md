# Shared Application

역할
- 공용 유스케이스/서비스를 정의합니다.
- Host 시뮬레이션과 이벤트 디스패치의 공통 흐름을 둡니다.

포함 예시
- GameHostBase, GameCommandOutcome
- GameEventBus, GameEventHub

금지
- UI/씬 종속 코드
주의
- Host 스레드에서는 Unity API를 호출하지 않습니다.
- 이벤트는 큐에 넣고 메인 스레드에서 Flush합니다.

