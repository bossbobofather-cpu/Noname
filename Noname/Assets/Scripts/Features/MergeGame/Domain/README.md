# MergeGame Domain

역할
- 게임 규칙/상태/정책을 정의합니다.
- UnityEngine 없이 동작 가능한 순수 로직입니다.

포함 예시
- 규칙 컨텍스트/상태 구조체
- 승리 조건, 스폰 정책, 배치 정책
- 값 객체(스펙, 범위, 등급)

금지
- MonoBehaviour, ScriptableObject
- UnityEngine 네임스페이스 참조
- 씬/프리팹 직접 접근
