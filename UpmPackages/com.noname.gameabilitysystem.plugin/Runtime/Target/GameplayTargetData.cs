using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 능력 타겟팅 요청 정보
    /// </summary>
    /// <summary>
    /// 능력 타겟팅 데이터
    /// </summary>
    public sealed class AbilityTargetData
    {
        private readonly HashSet<Transform> _targetSet = new();
        private readonly List<Transform> _targets = new();
        private readonly HashSet<AbilitySystemComponent> _abilitySystemSet = new();
        private readonly List<AbilitySystemComponent> _abilitySystems = new();

        public AbilityTargetData(Vector3 origin)
        {
            Origin = origin;
        }

        public Vector3 Origin { get; }
        public IReadOnlyList<Transform> Targets => _targets;
        public IReadOnlyList<AbilitySystemComponent> AbilitySystems => _abilitySystems;

        public void AddTarget(Transform target)
        {
            if (target == null || !_targetSet.Add(target))
            {
                return;
            }

            _targets.Add(target);
        }

        public void AddAbilitySystem(AbilitySystemComponent abilitySystem)
        {
            if (abilitySystem == null)
            {
                return;
            }

            if (_abilitySystemSet.Add(abilitySystem))
            {
                _abilitySystems.Add(abilitySystem);
            }

            AddTarget(abilitySystem.transform);
        }
    }
}
