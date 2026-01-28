using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem.Presentation
{
    /// <summary>
    /// 어빌리티 타겟팅 결과를 담는 데이터입니다.
    /// </summary>
    public sealed class AbilityTargetData
    {
        private readonly HashSet<Transform> _targetSet = new();
        private readonly List<Transform> _targets = new();
        private readonly HashSet<AbilitySystemComponentAdapter> _abilitySystemSet = new();
        private readonly List<AbilitySystemComponentAdapter> _abilitySystems = new();

        /// <summary>
        /// 타겟 데이터에 기준 위치를 설정합니다.
        /// </summary>
        /// <param name="origin">기준 위치</param>
        public AbilityTargetData(Vector3 origin)
        {
            // 타겟팅 기준 위치를 저장한다.
            Origin = origin;
        }

        /// <summary>
        /// 타겟팅 기준 위치입니다.
        /// </summary>
        public Vector3 Origin { get; }

        /// <summary>
        /// 타겟 트랜스폼 목록입니다.
        /// </summary>
        public IReadOnlyList<Transform> Targets => _targets;

        /// <summary>
        /// 타겟의 능력 시스템 목록입니다.
        /// </summary>
        public IReadOnlyList<AbilitySystemComponentAdapter> AbilitySystems => _abilitySystems;

        /// <summary>
        /// 타겟 트랜스폼을 추가합니다.
        /// </summary>
        /// <param name="target">추가할 대상</param>
        public void AddTarget(Transform target)
        {
            if (target == null || !_targetSet.Add(target))
            {
                // 비어 있거나 중복이면 무시한다.
                return;
            }

            _targets.Add(target);
        }

        /// <summary>
        /// 능력 시스템을 타겟으로 추가합니다.
        /// </summary>
        /// <param name="abilitySystem">추가할 능력 시스템</param>
        public void AddAbilitySystem(AbilitySystemComponentAdapter abilitySystem)
        {
            if (abilitySystem == null)
            {
                return;
            }

            if (_abilitySystemSet.Add(abilitySystem))
            {
                _abilitySystems.Add(abilitySystem);
            }

            // 능력 시스템의 트랜스폼도 함께 추가한다.
            AddTarget(abilitySystem.transform);
        }
    }
}
