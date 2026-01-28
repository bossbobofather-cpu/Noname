using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 타겟팅 결과를 담는 데이터입니다.
    /// </summary>
    public sealed class TargetData
    {
        private readonly HashSet<AbilitySystemComponent> _targetSet = new();
        private readonly List<AbilitySystemComponent> _targets = new();

        public TargetData(AbilitySystemComponent source, Point2D? hitLocation = null)
        {
            Source = source;
            HitLocation = hitLocation;
        }

        /// <summary>
        /// 타겟팅을 수행한 주체입니다.
        /// </summary>
        public AbilitySystemComponent Source { get; }

        /// <summary>
        /// 타겟 목록입니다.
        /// </summary>
        public IReadOnlyList<AbilitySystemComponent> Targets => _targets;

        /// <summary>
        /// 범위 공격 등에서 사용할 기준 위치입니다.
        /// </summary>
        public Point2D? HitLocation { get; }

        /// <summary>
        /// 타겟을 추가합니다.
        /// </summary>
        public void AddTarget(AbilitySystemComponent target)
        {
            if (target == null)
            {
                return;
            }

            if (_targetSet.Add(target))
            {
                _targets.Add(target);
            }
        }
    }
}
