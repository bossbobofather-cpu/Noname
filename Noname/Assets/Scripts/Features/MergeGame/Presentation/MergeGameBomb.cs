using UnityEngine;
using Noname.GameAbilitySystem;
using MyProject.GameplayAbilitySystem.Define;
using MyProject.GameplayAbilitySystem.Target;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 몬스터가 밟으면 폭발하는 폭발물입니다.
    /// </summary>
    public sealed class MergeGameBomb : MonoBehaviour
    {
        [SerializeField] private float _radius = 2f;
        [SerializeField] private LayerMask _targetLayers = ~0;
        [SerializeField] private GameplayEffectConfig _damageEffect;
        [SerializeField] private float _lifeTime = 6f;

        private AbilitySystemComponent _source;
        private bool _exploded;

        /// <summary>
        /// 폭발물에 공격 주체와 효과를 설정합니다.
        /// </summary>
        public void Initialize(AbilitySystemComponent source, GameplayEffectConfig effect)
        {
            _source = source;
            if (effect != null)
            {
                _damageEffect = effect;
            }
        }

        private void Start()
        {
            if (_lifeTime > 0f)
            {
                // 일정 시간이 지나면 자동 제거한다.
                Destroy(gameObject, _lifeTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_exploded)
            {
                return;
            }

            if (!IsValidTarget(other))
            {
                return;
            }

            // 몬스터가 밟으면 즉시 폭발한다.
            Explode();
        }

        private bool IsValidTarget(Collider other)
        {
            var targetable = other.GetComponentInParent<Targetable>();
            return targetable != null && targetable.Group == TargetGroup.Opponent;
        }

        private void Explode()
        {
            if (_exploded)
            {
                return;
            }

            _exploded = true;

            // 범위 내 모든 대상에게 GAS 효과를 적용한다.
            var hits = Physics.OverlapSphere(transform.position, _radius, _targetLayers);
            for (var i = 0; i < hits.Length; i++)
            {
                var targetable = hits[i].GetComponentInParent<Targetable>();
                if (targetable == null || targetable.Group != TargetGroup.Opponent)
                {
                    continue;
                }

                var targetAbility = targetable.AbilitySystem;
                if (targetAbility == null || _damageEffect == null)
                {
                    continue;
                }

                var context = new GameplayEffectContext(_source, targetAbility, default);
                targetAbility.ApplyGameplayEffect(_damageEffect, context);
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            // 범위를 확인하기 위한 기즈모.
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
