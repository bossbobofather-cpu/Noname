using UnityEngine;
using Noname.GameAbilitySystem;
using MyProject.GameplayAbilitySystem.Define;

namespace MyProject.GameplayAbilitySystem.Target
{
    /// <summary>
    /// 타겟팅 시스템(TargetRegistry)에 등록되어 식별될 수 있는 컴포넌트입니다.
    /// 진영(Group) 정보를 가지며 활성화 시 자동으로 등록됩니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Targetable : MonoBehaviour
    {
        [SerializeField] private TargetGroup _group = TargetGroup.Player;
        [SerializeField] private AbilitySystemComponent _abilitySystem;

        /// <summary>
        /// 위치로 잡히고 싶은 트랜스폼을 별도로 지정하려면 사용합니다.
        /// </summary>
        [SerializeField] private Transform _transform = null;


        /// <summary>
        /// 이 객체의 소속 그룹입니다.
        /// </summary>
        public TargetGroup Group => _group;
        
        /// <summary>
        /// 연동된 AbilitySystemComponent입니다.
        /// </summary>
        public AbilitySystemComponent AbilitySystem => _abilitySystem;

        private void Awake()
        {
            if (_abilitySystem == null)
            {
                _abilitySystem = GetComponentInParent<AbilitySystemComponent>();
            }
        }

        private void OnEnable()
        {
            if (TargetRegistry.TryGet(out var registry))
            {
                registry.Register(this);
            }
        }

        private void OnDisable()
        {
            if (TargetRegistry.TryGet(out var registry))
            {
                registry.Unregister(this);
            }
        }

        /// <summary>
        /// 타겟팅 기준이 되는 트랜스폼을 반환합니다.
        /// </summary>
        public Transform GetTransform()
        {
            return _transform ? _transform : transform;
        }
    }
}
