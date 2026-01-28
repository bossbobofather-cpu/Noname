# Domain Layer

Unity 의존성이 없는 순수 C# 모델 계층입니다.  
Host 시뮬레이션에서 직접 사용할 수 있도록 설계되었습니다.

## 구성
```
Domain/
├── Models/
│   ├── Ability/          # Ability/Effect/Modifier 모델
│   ├── Attribute/        # AttributeSet/AttributeValue
│   ├── Tag/              # Tag 모델
│   └── AbilitySystemModel.cs
└── Snapshots/            # 스냅샷 모델
```

## 설계 원칙
- UnityEngine 참조 금지
- Thread-safe 처리 (lock 기반)
- Snapshot은 불변 복사본

## 사용 예시
```csharp
var model = new AbilitySystemModel();
model.Set(AttributeId.Health, 100f);
model.Add(AttributeId.Health, -10f);
```
