# Shared Domain

역할
- 공통 값 객체/인터페이스/규칙을 정의합니다.
- Host/Client가 공유하는 DTO나 타입을 둡니다.

포함 예시
- 공용 인터페이스, 값 타입
- GameHostTypes (Command/Result/Event/Snapshot)
- 세션/스냅샷 관련 데이터 모델

금지
- MonoBehaviour, ScriptableObject
- UnityEngine 참조
사용 예
- ExploreGame Host/클라이언트가 공유하는 타입 정의

