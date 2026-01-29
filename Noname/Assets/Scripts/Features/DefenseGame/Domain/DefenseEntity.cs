using System;
using Noname.GameAbilitySystem.Domain;
using MyProject.DefenseGame.Domain.AI;
using UnityEditor;

namespace MyProject.DefenseGame.Domain
{
    /// <summary>
    /// 디펜스 게임 엔티티의 기본 클래스입니다.
    /// </summary>
    public abstract class DefenseEntity : IAbilitySystemOwner, IDisposable
    {
        /// <summary>
        /// 고유 ID입니다.
        /// </summary>
        public long Uid { get; }

        /// <summary>
        /// 2D 위치입니다.
        /// </summary>
        public Point2D Position { get; protected set; }

        /// <summary>
        /// AbilitySystemComponent입니다.
        /// </summary>
        public AbilitySystemComponent ASC { get; }

        /// <summary>
        /// AI(의사결정) 모듈입니다.
        /// </summary>
        public IDefenseAI AI { get; set; }

        /// <summary>
        /// 생존 여부입니다.
        /// </summary>
        public bool IsAlive => GetHp() > 0;

        /// <summary>
        /// 사망 여부입니다.
        /// </summary>
        public bool IsDead => GetHp() <= 0;

        /// <summary>
        /// 능력 활성화 이벤트 입니다.
        /// </summary>
        public event Action<GameplayAbility, TargetData> OnActivateAbility;

        /// <summary>
        /// ASC에 변경 사항(태그 추가,제거, 능력 추가)이 생기면 true가 되며 해당 정보 소비시 false로 변경됩니다.
        /// 최초에도 체크해야하기때문에 true입니다.
        /// </summary>
        private bool _ascRecheckFlag = true;

        protected DefenseEntity(long uid, Point2D position, AbilitySystemComponent asc)
        {
            Uid = uid;
            Position = position;
            ASC = asc ?? throw new ArgumentNullException(nameof(asc));

            ASC.SetOwner(this);
            ASC.OnAddedAbility += HandleAddedAbility;
            ASC.OnChangedTags += HandleChangedTags;
            ASC.OnActivateAbility += HandleActivateAbility;
        }

        /// <summary>
        /// 현재 HP를 반환합니다.
        /// </summary>
        public float GetHp() => ASC.Get(AttributeId.Health);

        /// <summary>
        /// 최대 HP를 반환합니다.
        /// </summary>
        public float GetMaxHp() => ASC.Get(AttributeId.MaxHealth);

        /// <summary>
        /// 공격력을 반환합니다.
        /// </summary>
        public float GetAttack() => ASC.Get(AttributeId.AttackDamage);

        /// <summary>
        /// 방어력을 반환합니다.
        /// </summary>
        public float GetDefense() => ASC.Get(AttributeId.Defense);

        /// <summary>
        /// 레벨을 반환합니다.
        /// </summary>
        public int GetLevel() => (int)ASC.Get(AttributeId.Level);

        /// <summary>
        /// 데미지를 받습니다.
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            if (damage <= 0) return;

            var currentHp = GetHp();
            var newHp = Math.Max(0, currentHp - damage);
            ASC.Set(AttributeId.Health, newHp);
        }

        /// <summary>
        /// 체력을 회복합니다.
        /// </summary>
        public virtual void Heal(float amount)
        {
            if (amount <= 0) return;

            var currentHp = GetHp();
            var maxHp = GetMaxHp();
            var newHp = Math.Min(maxHp, currentHp + amount);
            ASC.Set(AttributeId.Health, newHp);
        }

        /// <summary>
        /// 위치를 설정합니다.
        /// </summary>
        public void SetPosition(Point2D position)
        {
            Position = position;
        }

        /// <summary>
        /// 기본 속성을 설정합니다.
        /// </summary>
        /// <summary>
        /// 매 틱마다 호출됩니다.
        /// </summary>
        public abstract void Tick(float deltaTime);

        private void HandleAddedAbility()
        {
            _ascRecheckFlag |= true;
        }

        private void HandleChangedTags()
        {
            _ascRecheckFlag |= true;
        }

        private void HandleActivateAbility(GameplayAbility ability, TargetData targetData)
        {
            OnActivateAbility?.Invoke(ability, targetData);
        }

        public bool ConsumeASCRecheck()
        {
            var changed = _ascRecheckFlag;
            _ascRecheckFlag = false;
            return changed;
        }

        public void Dispose()
        {
            if (ASC != null)
            {
                ASC.Dispose();
            }

            OnActivateAbility = null;
        }
    }
}
