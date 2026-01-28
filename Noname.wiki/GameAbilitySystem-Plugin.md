# GameAbilitySystem 플러그인

Unity용 Gameplay Ability System(GAS) 스타일 플러그인입니다. Domain(순수 C#)과 Presentation(Unity) 레이어를 분리해 Host 환경에서도 사용할 수 있도록 설계했습니다.

## 핵심 구조

- **Domain**: Unity 의존 없는 모델 (속성/태그/이펙트/어빌리티)
- **Presentation**: Unity 컴포넌트 및 ScriptableObject 설정
- **Bridge**: Presentation → Domain 변환

## 주요 타입

| Name | Description |
|---|---|
| AbilitySystemModel | 스레드 안전 상태 모델 (속성/태그/이펙트) |
| AttributeSetModel | 속성 값 관리 (최소/최대 포함) |
| GameplayTagContainerModel | 태그 컨테이너 (O(1) 조회) |
| AbilitySystemComponent | Unity 컴포넌트 (뷰/워크플로우) |
| GameplayAbility | 능력 베이스 클래스 |
| GameplayEffectConfig | 효과 설정(SO) |
| GameplayAbilityDefinition | 능력 정의(SO) |

## 사용 흐름

1. ScriptableObject로 능력/효과/태그 구성
2. AbilitySystemComponent에 능력 부여
3. 이벤트/입력에 따라 능력 활성화
4. Domain 모델에서 효과 처리 → 스냅샷/이벤트 전달

## 샘플 (GAS 예제)

GAS 구조를 간단히 확인할 수 있는 예제 흐름입니다.

- **능력 정의(SO)**
  - AbilityDefinition + TagConfig + EffectConfig
- **타겟팅**
  - TargetConfig 기반으로 TargetData 획득
- **이펙트 적용**
  - Ability_Hit에서 TargetConfig의 Effect 적용

> 기존 1.Sample 씬의 상세 설명(씬 정보/컨트롤)은 제거했습니다.

## 참고
- 플러그인 코드는 UPM 패키지로 관리됩니다.
- Domain 레이어는 Unity 의존성이 없습니다.

- [GameAbilitySystem API 문서](https://bossbobofather-cpu.github.io/Noname/docs/index.html)
