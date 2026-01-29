# Domain Layer

Unity 의존성이 없는 순수 C# 모델 계층입니다.
Host/서버 환경에서도 그대로 사용할 수 있도록 설계했습니다.

## 구성
```
Domain/
├── Models/
│   ├── Ability/          # Ability/Effect/Modifier 모델
│   ├── Attribute/        # AttributeSet/AttributeValue
│   ├── Tag/              # Tag 모델
│   ├── Target/           # 타게팅 데이터/전략 모델
│   └── AbilitySystemComponent.cs
└── Snapshots/            # 스냅샷 모델
```

## 설계 원칙
- UnityEngine 참조 금지
- 스레드 안전 처리 (lock 기반)
- 스냅샷은 불변 복사본
- 모델은 상태 소스로만 동작 (표현/입력 책임 없음)

## 사용 예시
```csharp
var model = new AbilitySystemComponent();
model.Set(AttributeId.Health, 100f);
model.Add(AttributeId.Health, -10f);
```

## 참고
- Presentation 레이어는 Domain 모델을 읽어 표시/연결만 담당합니다.
- Host 시뮬레이션에서 Tick/Effect 만료 등을 처리할 때 Domain이 기준이 됩니다.
