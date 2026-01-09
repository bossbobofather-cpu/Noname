using MergeGame.Define;
using MergeGame.Target;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MergeGame.Target
{
    [DisallowMultipleComponent]
    public sealed class Targetable : MonoBehaviour
    {
        [SerializeField] private TargetGroup _group = TargetGroup.Player;
        [SerializeField] private AbilitySystemComponent _abilitySystem;

        //위치로 잡히고 싶은 트랜스폼을 별도로 지정하려면 별도 지정
        [SerializeField] private Transform _transform = null;


        public TargetGroup Group => _group;
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

        public Transform GetTransform()
        {
            return _transform ? _transform : transform;
        }
    }
}
