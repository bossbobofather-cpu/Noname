# Getting Started

## 요구 사항

- **Unity**: 6000.3.1f1 이상
- **Git**: 선택 사항 (zip 다운로드 가능)
- **.NET SDK**: .NET Standard 2.1 호환

---

## 설치

### 1. 프로젝트 클론

```bash
git clone [repository-url]
cd Noname
```

### 2. Unity에서 프로젝트 열기

1. Unity Hub 실행
2. **Add** > `Noname` 폴더 선택
3. Unity 버전: **6000.3.1f1** 선택
4. 프로젝트 열기

### 3. 로컬 UPM 패키지 확인

프로젝트는 로컬 UPM 패키지를 자동으로 참조합니다:

**`Noname/Packages/manifest.json`**
```json
{
  "dependencies": {
    "com.noname.gameabilitysystem.plugin": "file:../UpmPackages/com.noname.gameabilitysystem.plugin"
  }
}
```

Package Manager에서 확인:
- Window > Package Manager
- Packages: **In Project**
- `GameAbilitySystem Plugin` 패키지 확인

---

## 빠른 테스트

### Sample Scene 실행

1. **Scenes/Sample.unity** 열기
2. Play 버튼 클릭
3. **F1** 키로 Debug UI Panel 열기
4. Ability 버튼 클릭하여 테스트

**조작법:**
- **WASD**: 이동
- **Space**: 점프
- **마우스 왼쪽 버튼**: 공격
- **F1**: Debug UI 토글

---

## 첫 Ability 만들기

### 1. GameplayAbility 클래스 생성

```csharp
using Noname.GameAbilitySystem.Presentation;
using UnityEngine;

public class MyFirstAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        Debug.Log("My First Ability Activated!");

        // 효과는 GameplayEffectConfig로 자동 적용됨
        EndAbility(context.Handle);
    }

    public override bool CanActivateAbility()
    {
        // 활성화 조건 체크
        return base.CanActivateAbility();
    }
}
```

### 2. ScriptableObject 생성

**Assets/Create/GameAbilitySystem/**
1. **Ability Definition** 생성
   - Name: `MyFirstAbilityDefinition`
   - Ability Type Name: `MyFirstAbility`

2. **Gameplay Tag Config** 생성
   - Ability Tags: `Ability.Custom.MyFirst`

3. **Ability Definition**에 Config 추가
   - Configs 배열에 Tag Config 추가

### 3. GameObject에 부여

```csharp
public class PlayerController : MonoBehaviour, IAbilitySystemProvider
{
    [SerializeField] private AbilitySystemComponent _abilitySystem;
    [SerializeField] private GameplayAbilityDefinition _myAbility;

    public AbilitySystemComponent AbilitySystem => _abilitySystem;

    private void Start()
    {
        _abilitySystem.GiveAbility(_myAbility);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _abilitySystem.TryActivateAbilityByType<MyFirstAbility>();
        }
    }
}
```

---

## 다음 단계

- **[GameAbilitySystem Plugin Guide](GameAbilitySystem-Plugin)** - 플러그인 상세 가이드
- **[Sample Scene](1.Sample)** - 더 많은 예제 확인
- **[API Reference](https://[your-domain]/docs)** - API 문서

---

## 문제 해결

### 패키지가 보이지 않음

1. `Packages/manifest.json` 경로 확인
2. Unity 재시작
3. **Assets > Reimport All**

### 컴파일 에러

1. **Console** 창에서 에러 확인
2. Unity 버전 확인 (6000.3.1f1 이상)
3. `.NET Standard 2.1` 호환 확인

### Ability가 활성화되지 않음

1. **F1** 키로 Debug UI 열기
2. Ability가 제대로 부여되었는지 확인
3. Tag 조건 확인 (Required/Blocked Tags)
4. `CanActivateAbility()` 반환 값 확인
