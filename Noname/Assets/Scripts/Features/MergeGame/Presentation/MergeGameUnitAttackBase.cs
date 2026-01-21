using UnityEngine;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 유닛의 공격 루틴을 담당하는 베이스 클래스입니다.
    /// </summary>
    public abstract class MergeGameUnitAttackBase : MonoBehaviour
    {
        [SerializeField] private float _fallbackInterval = 1.2f;

        private MergeGameUnit _owner;
        private float _nextAttackTime;

        /// <summary>
        /// 공격 주체 유닛입니다.
        /// </summary>
        protected MergeGameUnit Owner => _owner;

        /// <summary>
        /// 공격 주체의 AbilitySystemComponent입니다.
        /// </summary>
        protected AbilitySystemComponent SourceAbility => _owner != null ? _owner.AbilitySystem : null;

        /// <summary>
        /// 공격 컴포넌트를 초기화합니다.
        /// </summary>
        public void Initialize(MergeGameUnit owner)
        {
            _owner = owner;
            _nextAttackTime = Time.time + Random.Range(0f, _fallbackInterval);
        }

        private void Update()
        {
            if (_owner == null)
            {
                return;
            }

            if (Time.time < _nextAttackTime)
            {
                return;
            }

            if (!CanAttack())
            {
                _nextAttackTime = Time.time + 0.1f;
                return;
            }

            ExecuteAttack();
            _nextAttackTime = Time.time + GetAttackInterval();
        }

        /// <summary>
        /// 공격 가능 여부를 확인합니다.
        /// </summary>
        protected virtual bool CanAttack()
        {
            return true;
        }

        /// <summary>
        /// 실제 공격 동작을 수행합니다.
        /// </summary>
        protected abstract void ExecuteAttack();

        /// <summary>
        /// 공격 간격을 계산합니다.
        /// AttackSpeed 속성이 없으면 기본값을 사용합니다.
        /// </summary>
        protected float GetAttackInterval()
        {
            var abilitySystem = SourceAbility;
            if (abilitySystem != null
                && abilitySystem.Attributes.TryGet(AttributeId.AttackSpeed, out var value)
                && value != null
                && value.CurrentValue > 0.01f)
            {
                return 1f / value.CurrentValue;
            }

            return Mathf.Max(0.1f, _fallbackInterval);
        }
    }
}
