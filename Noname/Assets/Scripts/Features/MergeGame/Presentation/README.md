# MergeGame Presentation

역할
- 씬/입력/UI 연결부입니다.
- MonoBehaviour와 Unity API 사용이 허용됩니다.

포함 예시
- MergeGameMode(공용 GameMode를 상속)
- 머지게임 전용 모듈, UI 연결
- 인풋 핸들러, 브릿지, UI 연결

금지
- 규칙/정책의 핵심 로직을 여기서 구현하지 않기
- 데이터 계산은 Application/Domain에 위임
