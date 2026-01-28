using System;
using Noname.GameAbilitySystem.Domain;
using MyProject.DefenseGame.Domain.AI;

namespace MyProject.DefenseGame.Domain
{
    /// <summary>
    /// 디펜스 게임 공통 속성 ID입니다.
    /// </summary>
    public static class DefenseAttributeIds
    {
        // 공통
        public static readonly AttributeId Hp = new("Hp");
        public static readonly AttributeId MaxHp = new("MaxHp");
        public static readonly AttributeId Attack = new("Attack");
        public static readonly AttributeId Defense = new("Defense");
        public static readonly AttributeId Level = new("Level");
    }

    /// <summary>
    /// 플레이어 전용 속성 ID입니다.
    /// </summary>
    public static class PlayerAttributeIds
    {
        public static readonly AttributeId Experience = new("Experience");
    }

    /// <summary>
    /// 몬스터 전용 속성 ID입니다.
    /// </summary>
    public static class MonsterAttributeIds
    {
        public static readonly AttributeId ExpReward = new("ExpReward");
    }

    /// <summary>
    /// 디펜스 게임 엔티티의 기본 클래스입니다.
    /// </summary>
    public abstract class DefenseEntity : IAbilitySystemOwner
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

        protected DefenseEntity(long uid, Point2D position, AbilitySystemComponent asc)
        {
            Uid = uid;
            Position = position;
            ASC = asc ?? throw new ArgumentNullException(nameof(asc));
        }

        /// <summary>
        /// 현재 HP를 반환합니다.
        /// </summary>
        public float GetHp() => ASC.Get(DefenseAttributeIds.Hp);

        /// <summary>
        /// 최대 HP를 반환합니다.
        /// </summary>
        public float GetMaxHp() => ASC.Get(DefenseAttributeIds.MaxHp);

        /// <summary>
        /// 공격력을 반환합니다.
        /// </summary>
        public float GetAttack() => ASC.Get(DefenseAttributeIds.Attack);

        /// <summary>
        /// 방어력을 반환합니다.
        /// </summary>
        public float GetDefense() => ASC.Get(DefenseAttributeIds.Defense);

        /// <summary>
        /// 레벨을 반환합니다.
        /// </summary>
        public int GetLevel() => (int)ASC.Get(DefenseAttributeIds.Level);

        /// <summary>
        /// 데미지를 받습니다.
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            if (damage <= 0) return;

            var currentHp = GetHp();
            var newHp = Math.Max(0, currentHp - damage);
            ASC.Set(DefenseAttributeIds.Hp, newHp);
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
            ASC.Set(DefenseAttributeIds.Hp, newHp);
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
    }
}
